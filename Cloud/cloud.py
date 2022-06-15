from Client import Client
from Network.Network import ServerSocket, ClientSocket
from DataBase.ORM import ORM
from FileManagement.FileManager import FileManager
from ThreadManagement.ThreadManager import ThreadManager

from Protocol.MessageAssembler.Assembler import Assembler
from Constants.ProtocolConstants import DATA_SIZE

from Protocol.Types.Types import ServerCommands, ClientShortCommands, ClientLongCommands, ClientOtherCommands
from Constants.NetworkConstants import IP, PORT

from Crypto.PublicKey import RSA
from Crypto.Cipher import PKCS1_OAEP
from base64 import b64decode

from Constants.DebugConstants import DEBUG


def open_new_port():
    port = 1
    sock = ServerSocket()
    while port < 65536:
        try:
            sock.initiate(IP, port)
            break
        except:
            pass
        port += 1
    return sock, port


def key_exchange(client):
    data_length = client.socket.receive(DATA_SIZE)
    if not data_length:
        raise Exception("Did not receive public key")
    data_length = Assembler.bytes_to_int(data_length)

    public_key = client.socket.receive(data_length)
    public_key = b64decode(public_key)
    rsa = RSA.importKey(public_key)
    cipher = PKCS1_OAEP.new(rsa)

    symmetric_key = client.security.get_key().encode()
    encrypted = cipher.encrypt(symmetric_key)
    length = Assembler.int_to_bytes(len(encrypted), DATA_SIZE)
    client.socket.send(length + encrypted)


def client_thread(client, addr):
    try:
        key_exchange(client)
        while True:
            data_length = client.socket.receive(DATA_SIZE)
            if not data_length:
                break
            data_length = Assembler.bytes_to_int(data_length)
            
            data = client.socket.receive(data_length)
            data = Assembler.load_message(data, client.security)
            
            if data['opcode'] == ClientOtherCommands.get_opcode('Quit'):
                break
            elif data['opcode'] == ClientOtherCommands.get_opcode('Disconnect'):
                client.handler.check_protocol_change(data['opcode'], None, None)
            elif data['opcode'] in ClientShortCommands.get_all_opcodes():
                client.handler.handle_request(client.socket, data['opcode'], data['template'])
            elif data['opcode'] in ClientLongCommands.get_all_opcodes():
                long_server_socket, port_num = open_new_port()

                client.socket.send(client.security.encrypt(Assembler.int_to_bytes(port_num, 5)))
                long_client_socket = ClientSocket(long_server_socket.accept()[0])

                ThreadManager.create_new_thread(client.handler.handle_request, (long_client_socket, data['opcode'], data['template']))
                long_server_socket.close()
            else:
                command = ServerCommands.get_command("Query_Failed")
                command["template"]["description"] = "Unknown command."

                data = Assembler.build_message(command)
                client.socket.send(data)
    except Exception as e:
        if DEBUG:
            print(f'Exception at client thread: {e}.')
    client.socket.disconnect()

    if DEBUG:
        print(f'Client disconnected: IP-{addr[0]}, Port-{addr[1]}.')


def main():
    server = ServerSocket()
    orm = ORM()

    try:
        orm.create_tables()
        FileManager.create_save_folder()
        server.initiate(IP, PORT)

        if DEBUG:
            print('Server initiated')

        while True:
            sock, addr = server.accept()
            client_sock = ClientSocket(sock)

            if DEBUG:
                print(f'Client connected: IP-{addr[0]}, Port-{addr[1]}.')

            client = Client(client_sock)
            ThreadManager.create_new_thread(client_thread, (client, addr))
    except Exception as e:
        if DEBUG:
            print(f'Exception at main loop: {e}.')
    server.close()


if __name__ == '__main__':
    main()