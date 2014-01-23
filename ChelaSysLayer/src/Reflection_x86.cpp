#include <algorithm>
#include "SysLayer.hpp"
#include "Reflection_Common.hpp"

// The trap written in assembly that performs the actual parameter passing.
extern "C" void _Chela_Reflection_InvokeTrap(void *function, int numparameters, void *signature, void **parameters,
                    void *returnValue);

SYSAPI void _Chela_Reflection_Invoke(void *function, const char *signature, void **parameters, void *returnValue)
{
    // Build a clean signature.
    std::vector<uint8_t> cleanSignature;
    
    const char *pos = signature;
    int numparameters = 0;
    int vectorComponents = 0;
    bool addVector = false;
    for(; *pos; ++pos)
    {
        bool retParam = cleanSignature.empty();
        int type = *pos;
        switch(type)
        {
        case 'v':
            cleanSignature.push_back(PT_Void);
            break;
        case 'x':
            cleanSignature.push_back(PT_Bool);
            break;
        case 'b':
        case 'B':
            cleanSignature.push_back(PT_Byte);
            break;
        case 's':
        case 'S':
            cleanSignature.push_back(PT_Short);
            break;
        case 'i':
        case 'I':
            cleanSignature.push_back(PT_Int);
            break;
        case 'l':
        case 'L':
            cleanSignature.push_back(PT_Long);
            break;
        case 'p':
        case 'P':
            cleanSignature.push_back(PT_Pointer);
            break;
        case 'R':
            cleanSignature.push_back(PT_Reference);
            break;
        case 'f':
            cleanSignature.push_back(PT_Float);
            break;
        case 'F':
            cleanSignature.push_back(PT_Double);
            break;
        case 'D':
            cleanSignature.push_back(PT_LongDouble);
            break;
        case 'V':
            ++pos;
            vectorComponents = *pos;
            break;
        case 'M':
            {
                ++pos;
                int rows = *pos++;
                int columns = *pos++;
                vectorComponents = columns*rows;
            }
            break;
        case 'd':
            {
                // Ignore the 'd';
                ++signature;

                // Compute the structure size.
                size_t structureAlignment = 0;
                size_t structureSize = ComputeStructureSize(signature, signature, structureAlignment);

                // Check if its return pointer.
                if(!retParam)
                {
                    cleanSignature.push_back((structureSize >> 24) & 0xFF);
                    cleanSignature.push_back((structureSize >> 16) & 0xFF);
                    cleanSignature.push_back((structureSize >> 8) & 0xFF);
                    cleanSignature.push_back(structureSize & 0xFF);
                    cleanSignature.push_back(PT_ValueStruct);                    
                }
                else
                {
                    cleanSignature.push_back(PT_MemoryStruct);
                }
            }
            break;
        }

        // Store the vector components.
        if(addVector)
        {
            cleanSignature.push_back((vectorComponents >> 8) & 0xFF);
            cleanSignature.push_back(vectorComponents & 0xFF);
            cleanSignature.push_back(PT_Vector);
            vectorComponents = 0;
            addVector = false;
        }
        else if(vectorComponents != 0)
            addVector = true;

        if(!retParam)
            ++numparameters;
    }

    // Reverse the arguments order.
    std::reverse(cleanSignature.begin(), cleanSignature.end());

    // Perform the call.
    _Chela_Reflection_InvokeTrap(function, numparameters, &cleanSignature[0], parameters, returnValue);
}

