from abc import ABC

from Protocol.Types.Types import ServerCommands
from Protocol.MessageAssembler.Assembler import Assembler
from DataBase.ORM import ORM


class Handler(ABC):
    def __init__(self, security, functions = {}):
        self.orm = ORM()
        self.security = security
        self.__functions = functions
    
    def handle_request(self, socket, opcode, fields):
        if sum(1 for x in fields.values() if x is None or '') > 0:
            command = ServerCommands.get_command("Query_Failed")
            command["template"]["description"] = "Invalid aruments."

            data = Assembler.build_message(command, self.security)
            socket.send(data)
        elif opcode in self.__functions.keys():
            return self.__functions[opcode](socket, fields)
        else:
            command = ServerCommands.get_command("Query_Failed")
            command["template"]["description"] = "Unknown command."

            data = Assembler.build_message(command, self.security)
            socket.send(data)
        return ServerCommands.get_opcode("Query_Failed"), None
        