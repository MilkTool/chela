#include "SysLayer.hpp"

SYSAPI int _Chela_Atomic_CompareAndSwapInt(int *ptr, int oldValue, int newValue)
{
#if __GNUC__ >= 4 && !defined(_WIN32)
    return __sync_val_compare_and_swap(ptr, oldValue, newValue);
#elif defined(__x86_64__)
#   error unimplemented amd64 compare and swap
#elif defined(__i386__)
#   if defined(__GNUC__)
    int res;
    __asm__ __volatile__ (
        "   lock\n"
        "   cmpxchgl %2, %1\n"
        : "=a" (res), "=m" (*ptr)
        : "r"(newValue), "m" (*ptr), "a" (oldValue)
        : "memory");
    return res;
#   else
#       error unsupported compiler for x86_32 compare and swap
#   endif
#else
#   error unsupported platform
#endif
}

