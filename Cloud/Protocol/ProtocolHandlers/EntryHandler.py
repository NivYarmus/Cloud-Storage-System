from Protocol.ProtocolHandlers.Handler import Handler
from Protocol.Types.Types import ServerCommands, ClientShortCommands
from Protocol.MessageAssembler.Assembler import Assembler


class EntryHandler(Handler):
    def __init__(self, security):
        super().__init__(security, {
            ClientShortCommands.get_opcode("Sign_Up"): self.__sign_up,
            ClientShortCommands.get_opcode("Log_In"): self.__log_in
        })
    
    def __sign_up(self, socket, fields):
        username = fields['username'] if type(fields['username']) is str else str(fields['username'])
        password = fields['password'] if type(fields['password']) is str else str(fields['password'])

        salt = self.security.create_salt()
        result = self.orm.sign_up(username, self.security.hash_data(password.encode(), salt), salt.decode())

        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]

        data = Assembler.build_message(command, self.security)
        socket.send(data)

        return command['opcode'], None

    def __log_in(self, socket, fields):
        username = fields['username'] if type(fields['username']) is str else str(fields['username'])
        password = fields['password'] if type(fields['password']) is str else str(fields['password'])
        
        salt = self.orm.get_salt(username)
        result = self.orm.log_in(username, self.security.hash_data(password.encode(), salt.encode()))
        command = ServerCommands.get_command("Query_Success" if result[0] else "Query_Failed")
        command['template']['description'] = result[1]
        if result[0]:
            command['template']['extended_length'] = 8

        data = Assembler.build_message(command, self.security)
        socket.send(data)

        if result[0]:
            data = result[3].to_bytes(8, 'big')
            data = self.security.encrypt(data)
            socket.send(data)
            return command['opcode'], result[2]
        return command['opcode'], None