#include "SysLayer.hpp"

#ifndef USE_OPENGL
// OpenGL is disabled.
SYSAPI void *_Chela_OpenGL_GetProcedure(const char *name)
{
    return NULL;
}
#else

#if defined(__unix__)
// GLX support.
#include <GL/glx.h>

SYSAPI void *_Chela_OpenGL_GetProcedure(const char *name)
{
    return (void*)glXGetProcAddress((const GLubyte*)name);
}

#elif defined(_WIN32)
// wgl lookup.
#include <windows.h>

static HINSTANCE _OpenGL_Module = NULL;
SYSAPI void *_Chela_OpenGL_GetProcedure(const char *name)
{
    // HACK: Load OpenGL dll.
    if(!_OpenGL_Module)
    {
        _OpenGL_Module = (HINSTANCE)LoadLibrary("OpenGL32.dll");
    }

    // Check first built-in functions.
    void *res = (void*) GetProcAddress(_OpenGL_Module, name);
    if(res)
        return res;

    // Check for extension function.
    res = (void*) wglGetProcAddress((LPCSTR)name);
    return res;
}
#endif
#endif //USE_OPENGL

