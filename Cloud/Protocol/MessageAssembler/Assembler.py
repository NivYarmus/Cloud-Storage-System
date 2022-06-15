from Constants.ProtocolConstants import DATA_SIZE, NUMBER_FORMATTING
import json

from Constants.DebugConstants import DEBUG


class Assembler:
    @staticmethod
    def build_message(data, security):
        json_data = json.dumps(data)

        if DEBUG:
            print(f'Sent >>> {json_data}')

        cipher = security.encrypt(json_data.encode())
        return Assembler.int_to_bytes(len(cipher), DATA_SIZE) + cipher
    
    @staticmethod
    def load_message(data, security):
        json_data = security.decrypt(data).decode()

        if DEBUG:
            print(f'Received >>> {json_data}')

        return json.loads(json_data)
    
    @staticmethod
    def int_to_bytes(num, length):
        return num.to_bytes(length, NUMBER_FORMATTING)
    
    @staticmethod
    def bytes_to_int(data):
        return int.from_bytes(data, NUMBER_FORMATTING)
