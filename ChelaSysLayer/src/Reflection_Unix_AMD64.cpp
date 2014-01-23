#include "SysLayer.hpp"
#include "Reflection_Common.hpp"

// The trap written in assembly that performs the actual parameter passing.
extern "C" void _Chela_Reflection_InvokeTrap(void *function, int numparameters, void *signature, void **parameters);
                    
static void ClassifyStructure(const char *&signature, void *structure, bool retValue, 
    std::vector<uint8_t> &cleanSignature, std::vector<void*> &cleanParams)
{
    // Ignore the 'd';
    ++signature;

    // First compute the structure size.
    const char *endPos;
    size_t structureAlignment = 0;
    size_t structureSize = ComputeStructureSize(signature, endPos, structureAlignment);

    // If the structure size is greater than 4 eightbytes, pass-through memory.
    if(structureSize > 32)
    {
        if(!retValue)
        {
            cleanSignature.push_back(PT_ValueStruct);
            cleanSignature.push_back(structureSize & 0xFF);
            cleanSignature.push_back((structureSize >> 8) & 0xFF);
            cleanSignature.push_back((structureSize >> 16) & 0xFF);
            cleanSignature.push_back((structureSize >> 24) & 0xFF);
        }
        else
        {
            cleanSignature.push_back(PT_MemoryStruct);
        }

        cleanParams.push_back(structure);

        // Set the final position.
        signature = endPos;
        return;
    }
}

SYSAPI void _Chela_Reflection_Invoke(void *function, const char *signature, void **parameters, void *returnValue)
{
    // Build a clean signature.
    std::vector<uint8_t> cleanSignature;
    std::vector<void*> cleanParams;
    
    const char *pos = signature;
    for(int i=-1; *pos; ++pos, ++i)
    {
        bool retParam = i < 0;
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
            cleanSignature.push_back(PT_Vector);
            ++pos;
            cleanSignature.push_back(*pos);
            cleanSignature.push_back(PT_Vector);
            break;
        case 'd':
            void *structParam = retParam ? returnValue : parameters[i];
            ClassifyStructure(pos, structParam, retParam, cleanSignature, cleanParams);
            break;            
        }

        // Always pass the return pointer.
        if(retParam)
        {
            cleanParams.push_back(returnValue);
            continue;
        }

        // Pass pointers for non-structure parameters.
        if(type != 'd')
            cleanParams.push_back(parameters[i]);
    }

    // Perform the call.
    _Chela_Reflection_InvokeTrap(function, cleanParams.size()-1, &cleanSignature[0], &cleanParams[0]);
}

