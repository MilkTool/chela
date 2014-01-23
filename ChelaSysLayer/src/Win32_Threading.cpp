#ifdef _WIN32
#include "SysLayer.hpp"

#include <windows.h>

// Thread abstraction.
typedef void *(*StartRoutine)(void*);

class Win32Thread
{
public:
    Win32Thread();
    ~Win32Thread();

    void Start(void *entry, void *arg);
    void Join();
    void Abort();

    void RealStart()
    {
        start(threadArg);
    }
    
private:
    HANDLE thread;
    StartRoutine start;
    void *threadArg;
};

static WINAPI DWORD BasicThreadStart(void *self)
{
    ((Win32Thread*)self)->RealStart();
    return 0;
}


Win32Thread::Win32Thread()
{
}

Win32Thread::~Win32Thread()
{
}

void Win32Thread::Start(void *entry, void *arg)
{
    // Store the entry point and the argument.
    start = StartRoutine(entry);
    threadArg = arg;
    
    // Create the thread.
    thread = CreateThread(NULL, 0, BasicThreadStart, this, 0, NULL);
}

void Win32Thread::Join()
{
    WaitForSingleObject(thread, INFINITE);
}

void Win32Thread::Abort()
{
    // Unimplemented for now.
}

// Condition variable implementation.
class ConditionVariable
{
public:
    ConditionVariable()
    {
    }
    
    ~ConditionVariable()
    {
    }
    
    void Wait(CRITICAL_SECTION *mutex)
    {
    }
    
    void TimedWait(CRITICAL_SECTION *mutex, int milliseconds)
    {
    }
    
    void Notify()
    {
    }
    
    void NotifyAll()
    {
    }
    
private:
};

// Cpu time releasing api.
SYSAPI void _Chela_Sleep(int milliseconds)
{
    if(milliseconds > 0)
        Sleep(milliseconds);
}

// Thread C api
SYSAPI void* _Chela_CreateThread()
{
    return new Win32Thread();
}

SYSAPI void _Chela_DestroyThread(void* thread)
{
    Win32Thread *handle = (Win32Thread*)thread;
    delete handle;
}

SYSAPI void _Chela_StartThread(void* thread, void *entry, void *arg)
{
    ((Win32Thread*)thread)->Start(entry, arg);
}

SYSAPI void _Chela_JoinThread(void* thread)
{
    ((Win32Thread*)thread)->Join();
}

SYSAPI void _Chela_AbortThread(void* thread)
{
    ((Win32Thread*)thread)->Abort();
}

// Mutex C api
SYSAPI void* _Chela_CreateMutex()
{
    CRITICAL_SECTION *cs = new CRITICAL_SECTION;
    InitializeCriticalSection(cs);
    return cs;
}

SYSAPI void _Chela_DestroyMutex(void* handle)
{
    CRITICAL_SECTION *cs = (CRITICAL_SECTION*)handle;
    DeleteCriticalSection(cs);
    delete cs;
}

SYSAPI void _Chela_EnterMutex(void* handle)
{
    EnterCriticalSection((CRITICAL_SECTION*)handle);
}

SYSAPI bool _Chela_TryEnterMutex(void* handle)
{
    return TryEnterCriticalSection((CRITICAL_SECTION*)handle);
}

SYSAPI void _Chela_LeaveMutex(void* handle)
{
    LeaveCriticalSection((CRITICAL_SECTION*)handle);
}

// Condition variable C api
SYSAPI void *_Chela_CreateCondition()
{
    return new ConditionVariable();
}

SYSAPI void _Chela_DestroyCondition(void *condition)
{
    delete (ConditionVariable*)condition;
}

SYSAPI void _Chela_TimedWaitCondition(void *condition, void *mutex, int milliseconds)
{
    ((ConditionVariable*)condition)->TimedWait((CRITICAL_SECTION*)mutex, milliseconds);
}

SYSAPI void _Chela_WaitCondition(void *condition, void *mutex)
{
    ((ConditionVariable*)condition)->Wait((CRITICAL_SECTION*)mutex);
}

SYSAPI void _Chela_NotifyCondition(void *condition)
{
    ((ConditionVariable*)condition)->Notify();
}

SYSAPI void _Chela_NotifyAllCondition(void *condition)
{
    ((ConditionVariable*)condition)->NotifyAll();
}

#endif
