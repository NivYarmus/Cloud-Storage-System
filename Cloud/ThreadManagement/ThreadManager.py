from threading import Lock, Thread


class ThreadManager:
    LOCK = Lock()

    @staticmethod
    def lock_thread(funcion):
        def inner(*args):
            ThreadManager.LOCK.acquire()
            res = funcion(*args)
            ThreadManager.LOCK.release()
            return res
        return inner
    
    @staticmethod
    def create_new_thread(funcion, params):
        Thread(target=funcion, args=params).start()