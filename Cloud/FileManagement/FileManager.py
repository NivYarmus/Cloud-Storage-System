from Constants.FileManagerConstants import FILES_PATH
from ThreadManagement.ThreadManager import ThreadManager

import os


class FileManager:
    @staticmethod
    @ThreadManager.lock_thread
    def create_save_folder():
        if not os.path.isdir(FILES_PATH):
            os.mkdir(FILES_PATH)
            return True
        return False

    @staticmethod
    def delete_file(file_id):
        os.remove(f'{FILES_PATH}\\{file_id}')
    
    @staticmethod
    def append_data(file_id, appended_data):
        with open(f'{FILES_PATH}\\{file_id}', 'ab') as file:
            file.write(appended_data)
    
    @staticmethod
    def get_file_length(file_id):
        return os.stat(f'{FILES_PATH}\\{file_id}').st_size
    
    @staticmethod
    def read_file_data(file_id):
        with open(f'{FILES_PATH}\\{file_id}', 'rb') as file:
            while True:
                data = file.read(1024)
                if not data:
                    break
                yield data
    
    @staticmethod
    def clear_file(file_id):
        with open(f'{FILES_PATH}\\{file_id}', 'rb+') as file:
            file.truncate(0)
    
    @staticmethod
    def rename_file(prev_name, new_name):
        os.rename(f'{FILES_PATH}\\{prev_name}', f'{FILES_PATH}\\{new_name}')