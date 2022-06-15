import socket


class ServerSocket:
    def __init__(self):
        self.__socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    def initiate(self, ip, port):
        self.__socket.bind((ip, port))
        self.__socket.listen()
    
    def accept(self):
        return self.__socket.accept()
    
    def close(self):
        self.__socket.close()


class ClientSocket:
    def __init__(self, client):
        self.__socket = client
    
    def disconnect(self):
        self.__socket.close()
    
    def send(self, data):
        self.__send_on_loop(data)

    def receive(self, length):
        return self.__receive_on_loop(length)

    def __send_on_loop(self, data):
        sent_len = 0
        while len(data) != sent_len:
            sent_len += self.__socket.send(data[sent_len:])

    def __receive_on_loop(self, length):
        receive_data = b''
        while len(receive_data) != length:
            receive_data += self.__socket.recv(length - len(receive_data))
        return receive_data