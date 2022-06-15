from Protocol.ProtocolHandlers.EntryHandler import EntryHandler
from Protocol.ProtocolHandlers.UserHandler import UserHandler
from Protocol.Types.Types import ServerCommands, ClientShortCommands, ClientOtherCommands


class EventHandler:
    def __init__(self, security):
        self.__protocol = EntryHandler(security)
    
    def handle_request(self, socket, opcode, fields):
        response_opcode, protocol_change_arguments = self.__protocol.handle_request(socket, opcode, fields)
        self.check_protocol_change(opcode, response_opcode, protocol_change_arguments)

    def check_protocol_change(self, request_opcode, response_opcode, protocol_change_arguments):
        if request_opcode == ClientShortCommands.get_opcode("Log_In"):
            if response_opcode == ServerCommands.get_opcode("Query_Success"):
                self.__protocol = UserHandler(protocol_change_arguments, self.__protocol.security)
        elif request_opcode == ClientOtherCommands.get_opcode("Disconnect"):
            self.__protocol = EntryHandler(self.__protocol.security)