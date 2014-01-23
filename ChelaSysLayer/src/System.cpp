#include "SysLayer.hpp"

// System
static ChelaErrorLauncher ErrorLauncher = NULL;

SYSAPI void _Chela_Sys_Exit(int code)
{
    exit(code);
}

SYSAPI void _Chela_Sys_SetErrorLauncher(ChelaErrorLauncher launcher)
{
    ErrorLauncher = launcher;
}

SYSAPI void _Chela_Sys_Error(int code, const char *error)
{
    // Use first the error launcher.
    if(ErrorLauncher)
        ErrorLauncher(code, error);

    // Now abort if an exception wasn't thrown.
    if(code != SLE_NONE)
        _Chela_Sys_Fatal(error);
}


SYSAPI void _Chela_Sys_Fatal(const char *error)
{
    fprintf(stderr, "FATAL Runtime Error: %s\n", error);
    abort();
}

SYSAPI void _Chela_Sys_Abort()
{
    abort();
}
