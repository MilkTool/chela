#include "SysLayer.hpp"
#include "ChelaUtf8.hpp"

#if defined(__unix__)

// Posix shared library loading.
#include <dlfcn.h>

SYSAPI void *_Chela_Module_Load(int nameSize, const char16_t *utf16Name)
{
    ChelaUtf8String path(nameSize, utf16Name);
    return dlopen(path.c_str(), RTLD_GLOBAL | RTLD_LAZY);
}

SYSAPI void _Chela_Module_Unload(void *module)
{
    dlclose(module);
}

SYSAPI void *_Chela_Module_GetSymbol(void *module, const char *name)
{
    return dlsym(module, name);
}

SYSAPI const char *_Chela_Module_Error(void *module)
{
    return dlerror();
}

#elif defined(__WIN32)

// Window DLL loading.
#include <windows.h>

SYSAPI void *_Chela_Module_Load(int nameSize, const char16_t *utf16Name)
{
    return LoadLibraryW((LPCWSTR)utf16Name);
}

SYSAPI void _Chela_Module_Unload(void *module)
{
    FreeLibrary((HMODULE)module);
}

SYSAPI void *_Chela_Module_GetSymbol(void *module, const char *name)
{
    return (void*)GetProcAddress((HMODULE)module, name);
}

const char *_Chela_Win32_GetLastError();
SYSAPI const char *_Chela_Module_Error(void *module)
{
    return _Chela_Win32_GetLastError();
}

#endif
