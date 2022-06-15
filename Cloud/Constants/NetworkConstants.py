from os import getcwd
import configparser


config = configparser.ConfigParser()
config.read(getcwd() + '\\Cloud.ini')


IP = config['Network']['IP']
PORT = int(config['Network']['Port'])