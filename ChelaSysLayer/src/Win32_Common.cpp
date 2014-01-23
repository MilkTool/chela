#ifdef _WIN32
#include "SysLayer.hpp"
#include <windows.h>

__thread
LPTSTR lastMessage = NULL;

const char *_Chela_Win32_GetErrorMessage(int errorCode)
{
    return NULL;
}

const char *_Chela_Win32_GetLastError()
{
    return _Chela_Win32_GetErrorMessage(GetLastError());
}

#endif

