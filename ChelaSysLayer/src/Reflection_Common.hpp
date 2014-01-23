#ifndef _REFLECTION_COMMON_HPP
#define _REFLECTION_COMMON_HPP

#include <string.h>
#include <inttypes.h>
#include <vector>

enum ParameterType
{
    PT_Void=0,
    PT_Bool,
    PT_Byte,
    PT_Short,
    PT_Int,
    PT_Long,
    PT_Pointer,
    PT_Reference,
    PT_Float,
    PT_Double,
    PT_LongDouble,
    PT_Vector,
    PT_MemoryStruct,
    PT_ValueStruct,
};

inline size_t max(size_t a, size_t b)
{
    return a > b ? a : b;
}

inline void AlignSize(size_t &size, size_t alignment)
{
    size_t diff = size%alignment;
    if(diff)
        size += alignment - diff;
}

static size_t ComputeStructureSize(const char *signature, const char *&endpos, size_t &alignment)
{
    size_t size = 0;
    alignment = 1; // Start with one byte alignment.
    int numelements = *signature++;
    numelements |= (*signature++) << 8;
    for(int i = 0; i < numelements; ++i)
    {
        int type = *signature;
        switch(type)
        {
        case 'v':
            break;
        case 'x':
            ++size;
            break;
        case 'b':
        case 'B':
            ++size;
            break;
        case 's':
        case 'S':
            AlignSize(size, 2);
            size += 2;
            alignment = max(alignment, 2);
            break;
        case 'i':
        case 'I':
        case 'p':
        case 'P':
        case 'f':
            AlignSize(size, 4);
            size += 4;
            alignment = max(alignment, 4);
            break;
        case 'l':
        case 'L':
        case 'F':
            AlignSize(size, 8);
            size += 8;
            alignment = max(alignment, 8);
            break;
        case 'D':
            AlignSize(size, 16);
            size += 16;
            alignment = max(alignment, 16);
            break;
        case 'V':
            abort();
            break;
        case 'd':
            size_t subAlignment;
            size_t subSize = ComputeStructureSize(signature, signature, subAlignment);
            AlignSize(size, subAlignment);
            size += subSize;
            alignment = max(alignment, subAlignment);
            break;            
        }
    }

    endpos = signature - 1;
    AlignSize(size, alignment);
    return size;
}

#endif //_REFLECTION_COMMON_HPP

