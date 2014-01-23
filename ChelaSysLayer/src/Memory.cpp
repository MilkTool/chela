#include "SysLayer.hpp"
#include <stdlib.h>
#include <string.h>

#if defined(_WIN32)
#include <windows.h>
#define SysMalloc(x) HeapAlloc(GetProcessHeap(), 0, x)
#define SysFree(x) HeapFree(GetProcessHeap(), 0, x)
#else
#define SysMalloc malloc
#define SysFree free
#endif

#if defined(_WIN32)
SYSAPI void *_Chela_Malloc(size_t size)
{
    return _Chela_Memalign(sizeof(void*), size);
}

SYSAPI void *_Chela_Memalign(size_t boundary, size_t size)
{
    // Use a minimal boundary.
    size_t extraSize = boundary + sizeof(void*);    
    
    // Allocate the memory
    uintptr_t memory = (uintptr_t)SysMalloc(size + extraSize);
    uintptr_t memoryWithPointer = memory + sizeof(void*);
    
    // Compute the offset.
    size_t offset = boundary - memoryWithPointer % boundary;
    
    // Compute the block pointer.
    uintptr_t *blockPointer = (uintptr_t*) (memoryWithPointer + offset);
    
    // Store the pointer and return the block.
    blockPointer[-1] = memory;
    return &blockPointer[0]; 
}

SYSAPI void _Chela_Free(void *block)
{
    void **blockPointer = (void**)block;
    SysFree(blockPointer[-1]);
}

#else
SYSAPI void *_Chela_Malloc(size_t size)
{
    return malloc(size);
}

SYSAPI void *_Chela_Memalign(size_t boundary, size_t size)
{
    void *ret;
    return posix_memalign(&ret, boundary, size) ? NULL : ret;
}

SYSAPI void _Chela_Free(void *block)
{
    free(block);
}
#endif

// Memory manipulation routines.
SYSAPI void _Chela_Memset(void *ptr, int value, size_t num)
{
    memset(ptr, value, num);
}

SYSAPI void _Chela_Memcpy(void *dest, const void *source, size_t num)
{
    memcpy(dest, source, num);
}

