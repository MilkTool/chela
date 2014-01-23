#ifndef CHELA_UTF8_STRING_HPP
#define CHELA_UTF8_STRING_HPP

#include <string.h>
#include "SysLayer.hpp"

template<size_t INTERNAL_CAPACITY = 255>
class ChelaSmallUtf8String
{
public:
    ChelaSmallUtf8String(size_t stringSize, const char *string)
    {
        currentCapacity = stringSize;
        if(currentCapacity <= INTERNAL_CAPACITY)
        {
            data = internalStorage;
            strcpy(data, string);
            currentSize = stringSize;
        }
        else
        {
            data = new char[currentCapacity + 1];
            strcpy(data, string);
            currentSize = stringSize;            
        }
    }
    
    ChelaSmallUtf8String(size_t stringSize, const char16_t *string)
    {
        currentCapacity = MAX_UTF8_BUFFER(stringSize);
        if(currentCapacity <= INTERNAL_CAPACITY)
        {
            data = internalStorage;
            currentSize = _Chela_DC_Utf16ToUtf8(data, string);
        }
        else
        {
            data = new char[currentCapacity + 1];
            currentSize = _Chela_DC_Utf16ToUtf8(data, string);
        }
    }
    
    ~ChelaSmallUtf8String()
    {
        if(currentCapacity > INTERNAL_CAPACITY)
            delete [] data;
    }

    const char *c_str()
    {
        return currentSize == 0 ? NULL : data;
    }
    
    size_t size()
    {
        return currentSize;
    }
    
private:
    ChelaSmallUtf8String() {}
    ChelaSmallUtf8String(const ChelaSmallUtf8String &o) {}
    const ChelaSmallUtf8String &operator=(const ChelaSmallUtf8String &o)
    {
        return *this;
    }
    
    size_t currentSize;
    size_t currentCapacity;
    char *data;
    char internalStorage[INTERNAL_CAPACITY+1];
};

typedef ChelaSmallUtf8String<> ChelaUtf8String;

#endif //CHELA_UTF8_STRING_HPP

