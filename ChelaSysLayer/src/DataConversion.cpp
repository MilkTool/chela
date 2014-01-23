#include <stdio.h>
#include "SysLayer.hpp"

SYSAPI int _Chela_DC_DToA(char *buffer, double value)
{
    return sprintf(buffer, "%f", value);
}

SYSAPI int _Chela_DC_Utf16ToUtf8(char *dst, const char16_t *src)
{
    char *oldDest = dst;
    while(*src)
    {
        // Read the first part.
        int p1 = *src++;

        // TODO: Handle invalid surrogate pairs.

        // Read the complete surrogate pair if needed.
        int character;
        if(p1 >= 0xD800 && p1 <= 0xDBFF)
        {
            int p2 = *src++;
            character = ((p1 - 0xD800) << 10) | (p2 - 0xDC00);
            character += 0x10000;
        }
        else
        {
            character = p1;
        }

        // Encode the character using utf-8.
        if(character <= 0x007F)
        {
            *dst++ = character;
        }
        else if(character <= 0x07FF)
        {
            *dst++ = 0xC0 | (character >> 6);
            *dst++ = 0x80 | (character & 0x3F);
        }
        else if(character <= 0xFFFF)
        {
            *dst++ = 0xE0 | (character >> 12);
            *dst++ = 0x80 | ((character >> 6) & 0x3F);
            *dst++ = 0x80 | (character & 0x3F);
        }
        else if(character <= 0x1FFFFF)
        {
            *dst++ = 0xF0 | (character >> 18);
            *dst++ = 0x80 | ((character >> 12) & 0x3F);
            *dst++ = 0x80 | ((character >> 6) & 0x3F);
            *dst++ = 0x80 | (character & 0x3F);
        }
        // Ignore invalid characters.
    }

    // Append a null terminator.
    *dst = 0;
    return int(dst - oldDest);
}

SYSAPI int _Chela_DC_Utf8ToUtf16(char16_t *dst, const char *src)
{
    char16_t *oldDest = dst;
    while(*src)
    {
        // Get the first byte.
        int c = *src;
        
        // Count the number of characters.
        int numbytes = 1;
        if((c & (1<<7)) != 0)
        {
            numbytes = 0;
            for(int bc = 7; bc > 0; --bc)
            {
                if(c & (1<<bc))
                    ++numbytes;
                else
                    break;
            }
        }

        // Read the first byte.
        uint32_t character = c & ((1 << (8 - numbytes)) - 1);

        // Decode the remaining bytes.
        if(numbytes > 1)
        {
            character <<= numbytes + 1;
    
            // Read the next bytes.
            for(int j = 1; j < numbytes; ++j)
            {
                character |= src[j] & 0x3F;
                if(j + 1 < numbytes)
                    character <<= 6;
            }
        }

        // Encode the character.
        if(character <= 0xFFFF)
        {
            *dst++ = character;
        }
        else if (character <= 0x10FFFF)
        {
            character -= 0x10000;
            *dst++ = (character >> 10) + 0xD800;
            *dst++ = (character & 0x3FF) + 0xDC00;
        }
        else
        {
            // Ignore invalid characters.
        }

        // Increase the pointers.
        src += numbytes;
    }

    // Add the null terminator.
    *dst = 0;
    return int(dst - oldDest);
}

SYSAPI int _Chela_DC_Utf8ToUtf32(char32_t *dst, const char *src)
{
    char32_t *oldDest = dst;
    while(*src)
    {
        // Get the character.
        int c = *src;
        
        // Count the number of characters.
        int numbytes = 1;
        if((c & (1<<7)) != 0)
        {
            numbytes = 0;
            for(int bc = 7; bc > 0; --bc)
            {
                if(c & (1<<bc))
                    ++numbytes;
                else
                    break;
            }
        }
        
        // Read the first byte.
        uint32_t character = c & ((1 << (8 - numbytes)) - 1);

        // Decode the remaining bytes.
        if(numbytes > 1)
        {
            character <<= numbytes + 1;
    
            // Read the next bytes.
            for(int j = 1; j < numbytes; ++j)
            {
                character |= src[j] & 0x3F;
                if(j + 1 < numbytes)
                    character <<= 6;
            }
        }
        
        // Store the character.
        *dst++ = character;
        
        // Increase the pointers.
        src += numbytes;
    }

    // Add the null terminator.
    *dst = 0;
    return int(dst - oldDest);
}

