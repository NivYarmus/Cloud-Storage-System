import json
from Protocol.ProtocolHandlers.Handler import Handler
from Protocol.Types.Types import ServerCommands, ClientShortCommands, ClientLongCommands
from Protocol.MessageAssembler.Assembler import Assembler
from FileManagement.FileManager import FileManager
from ThreadManagement.ThreadManager import ThreadManager

from Constants.DebugConstants import DEBUG


class UserHandler(Handler):
    def __init__(self, user_id, security):
        super().__init__(security, {
            ClientShortCommands.get_opcode("Create_Folder"): self.__create_folder,
            ClientLongCommands.get_opcode("Upload_File"): self.__upload_file,
            ClientShortCommands.get_opcode("Delete_File"): self.__delete_file,
            ClientShortCommands.get_opcode("Rename_File"): self.__rename_file,
            ClientShortCommands.get_opcode("Get_Folders"): self.__get_folders,
            ClientShortCommands.get_opcode("Get_Files"): self.__get_files,
            ClientLongCommands.get_opcode("Download_File"): self.__download_file,
            ClientShortCommands.get_opcode("Share_File"): self.__share_file,
            ClientShortCommands.get_opcode("Get_Shared_Files"): self.__get_shared_files,
            ClientLongCommands.get_opcode("Update_File"): self.__update_file,
            ClientShortCommands.get_opcode("Remove_Shared_File"): self.__remove_shared_file
        })
        self.__user_id = user_id

    def __create_folder(self, socket, fields):
        folder_id = fields['folder']
        new_folder_name = fields['name']

        result = self.orm.add_folder(self.__user_id, folder_id, new_folder_name)

        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]
        if result[0]:
            extension = result[2].to_bytes(4, 'big')
            extension = self.security.encrypt(extension)
            command['template']['extended_length'] = len(extension)

            data = Assembler.build_message(command, self.security)
            socket.send(data + extension)
        else:
            data = Assembler.build_message(command, self.security)
            socket.send(data)


        return command['opcode'], None
    
    def __upload_file(self, socket, fields):
        folder_id = fields['folder']
        new_file_name = fields['name']
        file_extension = fields['extension']
        upload_time = fields['upload_time']

        result = self.orm.add_file(self.__user_id, folder_id, new_file_name, file_extension, upload_time)

        try:
            if result[0]:
                recv_len = fields['extended_length']
                while recv_len > 0:
                    data = socket.receive(1024)
                    recv_len -= len(data)
                    data = self.security.decrypt(data)
                    FileManager.append_data(result[2], data)
                else:
                    FileManager.append_data(result[2], b'')
            else:
                recv_len = fields['extended_length']
                while recv_len > 0:
                    recv_len -= len(socket.receive(recv_len))

            command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
            command['template']['description'] = result[1]
            if result[0]:
                file_id = result[2].to_bytes(4, 'big')
                command['template']['extended_length'] = len(file_id)

                data = Assembler.build_message(command, self.security)
                file_id = self.security.encrypt(file_id)
                socket.send(data + file_id)
            else:
                data = Assembler.build_message(command, self.security)
                socket.send(data)
        except Exception as e:
            file_id = result[2]
            result = self.orm.delete_file(self.__user_id, file_id)
            if self.orm.get_file_shares(file_id) == 0:
                self.orm.delete_absolute_file(file_id)
                try:
                    FileManager.delete_file(file_id)
                except:
                    pass
            
            if DEBUG:
                print(f'Exception at file upload: {e}.')

        return command['opcode'], None
    
    def __delete_file(self, socket, fields):
        file_id = fields['file']

        result = self.orm.delete_file(self.__user_id, file_id)
        if self.orm.get_file_shares(file_id) == 0:
            self.orm.delete_absolute_file(file_id)
            FileManager.delete_file(file_id)
        
        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]

        data = Assembler.build_message(command, self.security)
        socket.send(data)

        return command['opcode'], None
    
    def __rename_file(self, socket, fields):
        file_id = fields['file']
        new_file_name = fields['name']
        modify_time = fields['modify_time']

        result = self.orm.rename_file(self.__user_id, file_id, new_file_name, modify_time)

        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]

        data = Assembler.build_message(command, self.security)
        socket.send(data)

        return command['opcode'], None
    
    def __get_folders(self, socket, fields):
        folder_id = fields['folder']

        result = self.orm.get_folders(self.__user_id, folder_id)

        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]

        if result[0]:
            extension = json.dumps(result[2]).encode()
            command['template']['extended_length'] = len(extension)
            
            data = Assembler.build_message(command, self.security)
            extension = self.security.encrypt(extension)

            socket.send(data + extension)

        else:
            data = Assembler.build_message(command, self.security)
            socket.send(data)

        return command['opcode'], None
    
    def __get_files(self, socket, fields):
        folder_id = fields['folder']

        result = self.orm.get_files(self.__user_id, folder_id)

        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]

        if result[0]:
            for file in result[2]:
                file['size'] = FileManager.get_file_length(file['file'])
        
            extension = json.dumps(result[2]).encode()
            command['template']['extended_length'] = len(extension)
            
            data = Assembler.build_message(command, self.security)
            extension = self.security.encrypt(extension)

            socket.send(data + extension)

        else:
            data = Assembler.build_message(command, self.security)
            socket.send(data)

        return command['opcode'], None
    
    def __download_file(self, socket, fields):
        file_id = fields['file']

        result = self.orm.check_user_file_connection(self.__user_id, file_id)
        if result:
            ThreadManager.LOCK.acquire()
            result = self.orm.get_file_download_status(file_id)
            if result:
                self.orm.change_file_update_status(file_id)
            ThreadManager.LOCK.release()
        
        try:
            if result:
                command = ServerCommands.get_command("Query_Success")
                command['template']['description'] = 'File downloaded successfully'
                command['template']['extended_length'] = FileManager.get_file_length(file_id)
            else:
                command = ServerCommands.get_command("Query_Failed")
                command['template']['description'] = 'File not found/Undownloadable'
            data = Assembler.build_message(command, self.security)
            socket.send(data)

            if result:
                data_iter = FileManager.read_file_data(file_id)
                try:
                    while True:
                        data = data_iter.__next__()
                        data = self.security.encrypt(data)
                        socket.send(data)
                except StopIteration:
                    pass
        except Exception as e:
            if DEBUG:
                print(f'Exception at file download: {e}.')
        
        if result:
            ThreadManager.LOCK.acquire()
            self.orm.change_file_update_status(file_id)
            ThreadManager.LOCK.release()

        return command['opcode'], None
    
    def __share_file(self, socket, fields):
        file_id = fields['file']
        user = fields['user']
        time = fields['share_time']

        result = self.orm.share_file(self.__user_id, file_id, user, time)

        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]

        data = Assembler.build_message(command, self.security)
        socket.send(data)

        return command['opcode'], None
    
    def __get_shared_files(self, socket, fields):
        result = self.orm.get_shared_files(self.__user_id)

        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]

        if result[0]:
            for file in result[2]:
                file['size'] = FileManager.get_file_length(file['file'])

            extension = json.dumps(result[2]).encode()
            extension = self.security.encrypt(extension)
            command['template']['extended_length'] = len(extension)
            
            data = Assembler.build_message(command, self.security)

            socket.send(data + extension)

        else:
            data = Assembler.build_message(command, self.security)
            socket.send(data)

        return command['opcode'], None
    
    def __update_file(self, socket, fields):
        file_id = fields['file']
        file_extension = fields['extension']
        modify_time = fields['modify_time']

        ThreadManager.LOCK.acquire()
        if self.orm.get_file_update_status(file_id):
            result = self.orm.update_file(self.__user_id, file_id, file_extension, modify_time)
            if result[0]:
                self.orm.change_file_download_status(file_id)
                self.orm.change_file_update_status(file_id)
        else:
            result = (False, 'File is currently unupdatable')
        ThreadManager.LOCK.release()

        try:
            if result[0]:
                recv_len = fields['extended_length']
                FileManager.clear_file(file_id)
                while recv_len > 0:
                    data = socket.receive(1024)
                    recv_len -= len(data)
                    data = self.security.decrypt(data)
                    FileManager.append_data(f'{file_id}_update', data)
                else:
                    FileManager.append_data(f'{file_id}_update', b'')
            else:
                recv_len = fields['extended_length']
                while recv_len > 0:
                    recv_len -= len(socket.receive(recv_len))
            
            command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
            command['template']['description'] = result[1]

            data = Assembler.build_message(command, self.security)
            socket.send(data)

            if result[0]:
                FileManager.delete_file(file_id)
                FileManager.rename_file(f'{file_id}_update', file_id)

                ThreadManager.LOCK.acquire()
                self.orm.change_file_download_status(file_id)
                self.orm.change_file_update_status(file_id)
                ThreadManager.LOCK.release()
            
            return command['opcode'], None
        except Exception as e:
            FileManager.delete_file(f'{file_id}_update')

            if DEBUG:
                print(f'Exception at file update: {e}.')
            
            return ServerCommands.get_opcode("Query_Failed"), None
    
    def __remove_shared_file(self, socket, fields):
        file_id = fields['file']

        result = self.orm.remove_shared_file(self.__user_id, file_id)

        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]

        data = Assembler.build_message(command, self.security)
        socket.send(data)

        return command['opcode'], None
