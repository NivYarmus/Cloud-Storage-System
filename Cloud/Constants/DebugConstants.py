from distutils.debug import DEBUG
from os import getcwd
import configparser


config = configparser.ConfigParser()
config.read(getcwd() + '\\Cloud.ini')


DEBUG = bool(int(config['Debug']['State']))