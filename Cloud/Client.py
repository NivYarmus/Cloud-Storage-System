from Security.Security import Security
from Protocol.ProtocolHandlers.EventHandler import EventHandler


class Client:
    def __init__(self, client):
        self.socket = client
        self.security = Security()
        self.handler = EventHandler(self.security)