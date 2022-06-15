import json
import copy

from Constants.ProtocolConstants import SERVERTYPES_PATH, CLIENTTYPES_PATH


with open(SERVERTYPES_PATH, 'r') as file:
    server_json = json.loads(file.read())
with open(CLIENTTYPES_PATH, 'r') as file:
    client_json = json.loads(file.read())

class Commands:
    def __init__(self, commands = {}):
        self.__commands = commands
    
    def get_command(self, command):
        return copy.deepcopy(self.__commands[command])
    
    def get_opcode(self, command):
        return self.__commands[command]["opcode"]
    
    def get_template(self, command):
        return copy.deepcopy(self.__commands[command]["template"])
    
    def get_all_opcodes(self):
        return [self.get_opcode(command) for command in self.__commands]


ServerCommands = Commands(server_json['Command'])
ClientShortCommands = Commands(client_json['ShortCommand'])
ClientLongCommands = Commands(client_json['LongCommand'])
ClientOtherCommands = Commands(client_json['OtherCommands'])