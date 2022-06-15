from Constants.SecurityConstants import AES_KEY_SIZE, SALT_SIZE

from os import urandom
from binascii import hexlify
from base64 import b64encode
from json import dumps

# requires installation of pycryptodome
from Crypto.Hash import SHA256
from Crypto.Cipher import AES


class Security:
    def __init__(self):
        self.__key, self.__iv  = self.__generate_key()
    
    def __generate_key(self):
        key = urandom(AES_KEY_SIZE)
        aes = AES.new(key, AES.MODE_CFB)
        return key, aes.iv
    
    def encrypt(self, data):
        aes = AES.new(self.__key, AES.MODE_CFB, iv=self.__iv)
        return aes.encrypt(data)
    
    def decrypt(self, data):
        aes = AES.new(self.__key, AES.MODE_CFB, iv=self.__iv)
        return aes.decrypt(data)
    
    def get_key(self):
        return dumps({'key': b64encode(self.__key).decode(),
        'iv': b64encode(self.__iv).decode()})

    @staticmethod
    def create_salt():
        return hexlify(urandom(SALT_SIZE))
    
    @staticmethod
    def hash_data(data, salt):
        sha256 = SHA256.new()
        sha256.update(data + salt)
        return sha256.hexdigest()