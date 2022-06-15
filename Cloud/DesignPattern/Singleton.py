from ThreadManagement.ThreadManager import ThreadManager


class SingletonMeta(type):
    _instance = None

    @ThreadManager.lock_thread
    def __call__(cls, *args, **kwargs):
        if cls._instance is None:
            cls._instance = type.__call__(cls, *args, **kwargs)
        return cls._instance