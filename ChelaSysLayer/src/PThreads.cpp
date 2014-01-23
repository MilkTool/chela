#if defined(__unix__)

#include <vector>
#include <time.h>
#include <pthread.h>
#include <unistd.h>
#include "SysLayer.hpp"

// Thread abstraction.
class PThread
{
public:
    PThread();
    ~PThread();

    void Start(void *entry, void *arg);
    void Join();
    void Abort();

private:
    pthread_t thread;
};

typedef void *(*StartRoutine)(void*);

PThread::PThread()
{
}

PThread::~PThread()
{
}

void PThread::Start(void *entry, void *arg)
{
    pthread_create(&thread, NULL, StartRoutine(entry), arg);
}

void PThread::Join()
{
    pthread_join(thread, NULL);
}

void PThread::Abort()
{
    pthread_cancel(thread);
}

// Mutex abstraction.
class PThreadMutex
{
public:
    PThreadMutex();
    ~PThreadMutex();

    void Enter();
    bool TryEnter();
    void Leave();

    pthread_mutex_t mutex;
};

PThreadMutex::PThreadMutex()
{
    // Initialize the mutex.
    pthread_mutex_init(&mutex, NULL);
}

PThreadMutex::~PThreadMutex()
{
    // Destroy the mutex.
    pthread_mutex_destroy(&mutex);
}

void PThreadMutex::Enter()
{
    pthread_mutex_lock(&mutex);
}

bool PThreadMutex::TryEnter()
{
    return pthread_mutex_trylock(&mutex) == 0;
}

void PThreadMutex::Leave()
{
    pthread_mutex_unlock(&mutex);
}

// Condition variable
class PThreadCondition
{
public:
    PThreadCondition();
    ~PThreadCondition();

    void Wait(PThreadMutex *mutex);
    void TimedWait(PThreadMutex *mutex, int milliseconds);
    void Notify();
    void NotifyAll();

private:
    pthread_cond_t condition;
};

PThreadCondition::PThreadCondition()
{
    pthread_cond_init(&condition, NULL);
}

PThreadCondition::~PThreadCondition()
{
    pthread_cond_destroy(&condition);
}

void PThreadCondition::Wait(PThreadMutex *mutex)
{
    pthread_cond_wait(&condition, &mutex->mutex);
}

void PThreadCondition::TimedWait(PThreadMutex *mutex, int milliseconds)
{
    timespec endtime;
    clock_gettime(CLOCK_REALTIME, &endtime);
    endtime.tv_sec += milliseconds/1000;
    endtime.tv_nsec += long(milliseconds % 1000) * 1000;
    pthread_cond_timedwait(&condition, &mutex->mutex, &endtime);
}

void PThreadCondition::Notify()
{
    pthread_cond_signal(&condition);
}

void PThreadCondition::NotifyAll()
{
    pthread_cond_broadcast(&condition);
}

// Cpu time releasing api.
SYSAPI void _Chela_Sleep(int milliseconds)
{
    usleep(useconds_t(milliseconds)*1000);
}

// Thread C api
SYSAPI void* _Chela_CreateThread()
{
    return new PThread();
}

SYSAPI void _Chela_DestroyThread(void* thread)
{
    PThread *handle = (PThread*)thread;
    delete handle;
}

SYSAPI void _Chela_StartThread(void* thread, void *entry, void *arg)
{
    ((PThread*)thread)->Start(entry, arg);
}

SYSAPI void _Chela_JoinThread(void* thread)
{
    ((PThread*)thread)->Join();
}

SYSAPI void _Chela_AbortThread(void* thread)
{
    ((PThread*)thread)->Abort();
}

// Mutex C api
SYSAPI void* _Chela_CreateMutex()
{
    return new PThreadMutex();
}

SYSAPI void _Chela_DestroyMutex(void* handle)
{
    PThreadMutex *mutex = (PThreadMutex*)handle;
    delete mutex;
}

SYSAPI void _Chela_EnterMutex(void* handle)
{
    ((PThreadMutex*)handle)->Enter();
}

SYSAPI bool _Chela_TryEnterMutex(void* handle)
{
    return ((PThreadMutex*)handle)->TryEnter();
}

SYSAPI void _Chela_LeaveMutex(void* handle)
{
    ((PThreadMutex*)handle)->Leave();
}

// Condition variable C api
SYSAPI void *_Chela_CreateCondition()
{
    return new PThreadCondition();
}

SYSAPI void _Chela_DestroyCondition(void *condition)
{
    PThreadCondition *handle = (PThreadCondition*)condition;
    delete handle;
}

SYSAPI void _Chela_TimedWaitCondition(void *condition, void *mutex, int milliseconds)
{
    ((PThreadCondition*)condition)->TimedWait((PThreadMutex*)mutex, milliseconds);
}

SYSAPI void _Chela_WaitCondition(void *condition, void *mutex)
{
    ((PThreadCondition*)condition)->Wait((PThreadMutex*)mutex);
}

SYSAPI void _Chela_NotifyCondition(void *condition)
{
    ((PThreadCondition*)condition)->Notify();
}

SYSAPI void _Chela_NotifyAllCondition(void *condition)
{
    ((PThreadCondition*)condition)->NotifyAll();
}
#endif
