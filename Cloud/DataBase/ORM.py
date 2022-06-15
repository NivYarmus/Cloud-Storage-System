from Constants.DataBaseConstants import DATABASE_PATH
from ThreadManagement.ThreadManager import ThreadManager

from DesignPattern.Singleton import SingletonMeta

import sqlite3


class ORM(metaclass=SingletonMeta):
    def __init__(self):
        self.__conn = None
        self.__curr = None
    
    def __open_database(self):
        self.__conn = sqlite3.connect(DATABASE_PATH)
        self.__curr = self.__conn.cursor()
    
    def __close_database(self):
        self.__conn.close()
        self.__conn = None
        self.__curr = None
    
    def __save_changes(self):
        self.__conn.commit()
    
    def __run_command(self, command, args = ()):
        if args:
            return self.__curr.execute(command, args)
        return self.__curr.execute(command, args)
    
    @ThreadManager.lock_thread
    def __handle_database(function):
        def inner(self, *args):
            if self.__conn is None:
                self.__open_database()
                res = function(self, *args) if args else function(self)
                self.__close_database()
            else:
                res = function(self, *args) if args else function(self)
            return res
        return inner
    
    @__handle_database
    def create_tables(self):
        self.__run_command("""CREATE TABLE IF NOT EXISTS Users (UserID INT PRIMARY KEY, Username TEXT UNQIUE NOT NULL, Password TEXT NOT NULL, Salt TEXT NOT NULL);""")
        self.__run_command("""CREATE TABLE IF NOT EXISTS Folders (FolderID INT PRIMARY KEY, Folder INT, Name TEXT NOT NULL);""")
        self.__run_command("""CREATE TABLE IF NOT EXISTS Files (FileID INT PRIMARY KEY, Folder INT, Name TEXT NOT NULL, Extension TEXT NOT NULL, Time INT, ModifyTime INT, Updatable INT DEFAULT 1, Downloadable INT DEFAULT 1);""")
        self.__run_command("""CREATE TABLE IF NOT EXISTS UsersFolders (UserID INT, FolderID INT);""")
        self.__run_command("""CREATE TABLE IF NOT EXISTS UsersFiles (UserID INT, FileID INT);""")
        self.__run_command("""CREATE TABLE IF NOT EXISTS Shares (UserID INT, OtherUserID INT, FileID INT, Time INT);""")
        self.__save_changes()
    
    @__handle_database
    def sign_up(self, username, password, salt):
        def get_top_user_id():
            query = self.__run_command("""SELECT MAX(UserID) FROM Users;""").fetchone()[0]
            if query is None:
                return 0
            return query

        if self.__run_command("""SELECT COUNT(*) FROM Users WHERE Username = ?;""", (username,)).fetchone()[0]:
            return (False, 'Username taken',)
        
        new_user_id = get_top_user_id() + 1
        self.__run_command("""INSERT INTO Users (UserID, Username, Password, Salt) VALUES (?, ?, ?, ?);""", (new_user_id, username, password, salt))
        self.__add_folder(new_user_id, 0, username)
        self.__save_changes()

        return (True, 'Username added successfully')

    @__handle_database
    def log_in(self, username, password):
        if not self.__run_command("""SELECT COUNT(*) FROM Users WHERE Username = ? AND Password = ?;""", (username, password)).fetchone()[0]:
            return (False, 'Incorrect entry values')

        user_id = self.__run_command("""SELECT UserID FROM Users WHERE Username = ?;""", (username,)).fetchone()[0]
        main_folder_id = self.__run_command("""WITH total_main_folders AS (SELECT FolderID FROM Folders WHERE Folder = 0) SELECT UsersFolders.FolderID FROM total_main_folders LEFT JOIN UsersFolders WHERE total_main_folders.FolderID = UsersFolders.FolderID AND UsersFolders.UserID = ?;""", (user_id,)).fetchone()[0]
        return (True, '', user_id, main_folder_id)

    @__handle_database
    def add_folder(self, user_id, folder_id, new_folder_name):
        if not self.check_user_folder_connection(user_id, folder_id):
            return (False, 'Unrecognized folder')
        elif self.__run_command("""SELECT COUNT(*) FROM Folders WHERE Folder = ? AND Name = ?;""", (folder_id, new_folder_name)).fetchone()[0]:
            return (False, 'Folder name is taken')
        return self.__add_folder(user_id, folder_id, new_folder_name)
    
    @__handle_database
    def add_file(self, user_id, folder_id, new_file_name, file_extension, upload_time):
        def get_top_file_id():
            query = self.__run_command("""SELECT MAX(FileID) FROM Files;""").fetchone()[0]
            if query is None:
                return 0
            return query
        
        if not self.check_user_folder_connection(user_id, folder_id):
            return (False, 'Unrecognized folder')
        elif self.__run_command("""SELECT COUNT(*) FROM Files WHERE Folder = ? AND Name = ? AND Extension = ?;""", (folder_id, new_file_name, file_extension)).fetchone()[0]:
            return (False, 'File name is taken')

        new_file_id = get_top_file_id() + 1
        self.__run_command("""INSERT INTO Files (FileID, Folder, Name, Extension, Time, ModifyTime) VALUES (?, ?, ?, ?, ?, ?)""", (new_file_id, folder_id, new_file_name, file_extension, upload_time, upload_time))
        self.__run_command("""INSERT INTO UsersFiles (UserID, FileID) VALUES (?, ?);""", (user_id, new_file_id))
        self.__save_changes()

        return (True, 'File added successfully', new_file_id)
    
    @__handle_database
    def delete_file(self, user_id, file_id):
        if not self.check_user_file_connection(user_id, file_id):
            return (False, 'Unrecognized file')

        self.__run_command("""DELETE FROM UsersFiles WHERE UserID = ? AND FileID = ?;""", (user_id, file_id))
        self.__save_changes()

        return (True, 'File deleted successfully')
    
    @__handle_database
    def delete_absolute_file(self, file_id):
        self.__run_command("""DELETE FROM Files WHERE FileID = ?;""", (file_id,))
        self.__save_changes()
    
    @__handle_database
    def rename_file(self, user_id, file_id, new_file_name, modify_time):
        if not self.check_user_file_connection(user_id, file_id):
            return (False, 'Unrecognized file')
        
        folder_id, file_extension = self.__run_command("""SELECT Folder, Extension FROM Files WHERE FileID = ?;""", (file_id,)).fetchone()
        if self.__run_command("""SELECT COUNT(*) FROM Files WHERE Folder = ? AND Name = ? AND Extension = ?;""", (folder_id, new_file_name, file_extension)).fetchone()[0]:
            return (False, 'File name is taken')

        self.__run_command("""UPDATE Files SET Name = ?, ModifyTime = ? WHERE FileID = ?;""", (new_file_name, modify_time, file_id))
        self.__save_changes()

        return (True, 'File renamed')
    
    @__handle_database
    def get_folders(self, user_id, folder_id):
        if not self.check_user_folder_connection(user_id, folder_id):
            return (False, 'Unrecognized folder')

        folders = self.__run_command("""SELECT FolderID, Name FROM Folders WHERE Folder = ?;""", (folder_id,)).fetchall()
        folders = [{'folder': x[0], 'name': x[1]} for x in folders]
        return (True, '', folders)
    
    @__handle_database
    def get_files(self, user_id, folder_id):
        if not self.check_user_folder_connection(user_id, folder_id):
            return (False, 'Unrecognized folder')

        files = self.__run_command("""SELECT FileID, Name, Extension, Time, ModifyTime FROM Files WHERE Folder = ?;""", (folder_id,)).fetchall()
        files = [{'file': x[0], 'name': x[1], 'extension': x[2], 'upload_time': x[3], 'modify_time': x[4]} for x in files]
        return (True, '', files)
    
    @__handle_database
    def share_file(self, user_id, file_id, user, share_time):
        if not self.check_user_file_connection(user_id, file_id):
            return (False, 'Unrecognized file')
        elif not self.__run_command("""SELECT COUNT(*) FROM Users WHERE Username = ?;""", (user,)).fetchone()[0]:
            return (False, 'User does not exist',)
        
        other_user_id = self.__run_command("""SELECT UserID FROM Users WHERE Username = ?;""", (user,)).fetchone()[0]
        if other_user_id == user_id:
            return (False, 'Cannot share file with youself')
        if self.__run_command("""SELECT COUNT(*) FROM Shares WHERE (UserID = ? OR UserID = ?) AND (OtherUserID = ? OR OtherUserID = ?) AND FileID = ?;""", (user_id, other_user_id, other_user_id, user_id, file_id)).fetchone()[0]:
            return (False, 'File is already shared between users')

        self.__run_command("""INSERT INTO Shares (UserID, OtherUserID, FileID, Time) VALUES (?, ?, ?, ?);""", (user_id, other_user_id, file_id, share_time))
        self.__save_changes()

        return (True, 'File shared successfully')
    
    @__handle_database
    def get_shared_files(self, user_id):
        shared_results = self.__run_command("""SELECT UserID, FileID, Time FROM Shares WHERE OtherUserID = ?;""", (user_id,)).fetchall()

        shares = []
        for x in shared_results:
            username = self.__run_command("""SELECT Username FROM Users WHERE UserID = ?;""", (x[0],)).fetchone()[0]
            file_id, file_name, file_extension, modify_time = self.__run_command("""SELECT FileID, Name, Extension, ModifyTime FROM Files WHERE FileID = ?;""", (x[1],)).fetchone()
            shares.append({"username": username, "file": file_id, "name": file_name, 'extension': file_extension, 'share_time': x[2], 'modify_time': modify_time})
        return (True, '', shares)
    

    @__handle_database
    def get_file_shares(self, file_id):
        return self.__run_command("""SELECT COUNT(*) FROM Shares WHERE FileID = ?;""", (file_id,)).fetchone()[0]
    
    @__handle_database
    def get_salt(self, username):
        result = self.__run_command("""SELECT Salt FROM Users WHERE Username = ?;""", (username,)).fetchone()
        if result is None:
            return ''
        return result[0]
    
    @__handle_database
    def __add_folder(self, user_id, folder_id, new_folder_name):
        def get_top_folder_id():
            query = self.__run_command("""SELECT MAX(FolderID) FROM Folders;""").fetchone()[0]
            if query is None:
                return 0
            return query

        new_folder_id = get_top_folder_id() + 1
        self.__run_command("""INSERT INTO Folders (FolderID, Folder, Name) VALUES (?, ?, ?);""", (new_folder_id, folder_id, new_folder_name))
        self.__run_command("""INSERT INTO UsersFolders (UserID, FolderID) VALUES (?, ?);""", (user_id, new_folder_id))
        self.__save_changes()

        return (True, 'Folder added successfully', new_folder_id)
    
    @__handle_database
    def check_user_file_connection(self, user_id, file_id):
        return self.__run_command("""SELECT COUNT(*) FROM UsersFiles WHERE UserID = ? AND FileID = ?;""", (user_id, file_id)).fetchone()[0] or self.__run_command("""SELECT COUNT(*) FROM Shares WHERE OtherUserID = ? AND FileID = ?;""", (user_id, file_id)).fetchone()[0]
    
    @__handle_database
    def check_user_folder_connection(self, user_id, folder_id):
        return self.__run_command("""SELECT COUNT(*) FROM UsersFolders WHERE UserID = ? AND FolderID = ?;""", (user_id, folder_id)).fetchone()[0]
    
    @__handle_database
    def update_file(self, user_id, file_id, file_extension, modify_time):
        if not self.check_user_file_connection(user_id, file_id):
            return (False, 'unrecognized file')
        
        self.__run_command("""UPDATE Files SET Extension = ?, ModifyTime = ? WHERE FileID = ?;""", (file_extension, modify_time, file_id))
        self.__save_changes()
        
        return (True, 'File updated successfully')
    
    @__handle_database
    def remove_shared_file(self, user_id, file_id):
        if not self.__run_command("""SELECT COUNT(*) FROM Shares WHERE OtherUserID = ? AND FileID = ?;""", (user_id, file_id)).fetchone()[0]:
            return (False, 'File is not shared with user.')
        self.__run_command("""DELETE FROM Shares WHERE OtherUserID = ? AND FileID = ?;""", (user_id, file_id))
        self.__save_changes()
        return (True, 'File removed successfully')
    
    @__handle_database
    def change_file_download_status(self, file_id):
        if self.get_file_download_status(file_id):
            self.__run_command("""UPDATE Files SET Downloadable = 0 WHERE FileID = ?;""", (file_id,))
        else:
            self.__run_command("""UPDATE Files SET Downloadable = 1 WHERE FileID = ?;""", (file_id,))
        self.__save_changes()
    
    @__handle_database
    def change_file_update_status(self, file_id):
        if self.get_file_update_status(file_id):
            self.__run_command("""UPDATE Files SET Updatable = 0 WHERE FileID = ?;""", (file_id,))
        else:
            self.__run_command("""UPDATE Files SET Updatable = 1 WHERE FileID = ?;""", (file_id,))
        self.__save_changes()

    @__handle_database
    def get_file_download_status(self, file_id):
        return self.__run_command("""SELECT Downloadable FROM Files WHERE FileID = ?;""", (file_id,)).fetchone()[0]

    @__handle_database
    def get_file_update_status(self, file_id):
        return self.__run_command("""SELECT Updatable FROM Files WHERE FileID = ?;""", (file_id,)).fetchone()[0]