#include "SysLayer.hpp"


SYSAPI int _Chela_IO_Puts(const char *str)
{
    return puts(str);
}

SYSAPI int _Chela_IO_PutInt(const char *msg, int value)
{
    printf("%s: %d\n", msg, value);
    return 0;
}

SYSAPI int _Chela_IO_PutSize(const char *msg, size_t size)
{
    printf("%s: %lu\n", msg, (unsigned long)size);
    return 0;
}

SYSAPI int _Chela_IO_PutPointer(const char *msg, const void *ptr)
{
    printf("%s: %p\n", msg, ptr);
    return 0;
}
