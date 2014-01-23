#ifndef CHELA_SYS_LAYER_HPP
#define CHELA_SYS_LAYER_HPP

#ifndef __STDC_CONSTANT_MACROS
#define __STDC_CONSTANT_MACROS
#endif

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>

#if defined(_WIN32)
#   ifdef BUILD_SYSLAYER
#       define SYSAPI extern "C" __declspec(dllexport)
#   else
#       define SYSAPI extern "C" __declspec(dllimport)
#   endif
#elif defined(__unix__)
#   define SYSAPI extern "C"
#else
#   define SYSAPI extern "C"
#endif

const int SysLayer_VersionMajor = 0;
const int SysLayer_VersionMinor = 1;

// Max buffer size to hold an utf-8 encoded string.
#define MAX_UTF8_BUFFER(utf16Size) ((utf16Size*4) + 1)

#if __cplusplus < 201103L
// UTF16 code unit. This defined in the Core C++11 standard.
typedef uint16_t char16_t;

// UTF32 code unit.
typedef uint32_t char32_t;
#endif

// Some types used.
typedef void *FILE_HANDLE;

// Propagated errors.
typedef void (*ChelaErrorLauncher) (int errorCode, const char *errorMessage);

enum SysLayerErrors
{
    SLE_NONE = 0,
    SLE_GENERIC,
    SLE_ARGUMENT,
    SLE_OUT_OF_RANGE,
    SLE_INVALID_OPERATION,
};

// FileMode enumeration.
enum FileMode
{
    FM_CreateNew = 0,
    FM_Create,
    FM_Open,
    FM_OpenOrCreate,
    FM_Truncate,
    FM_Append
};

// FileAccess enumeration.
enum FileAccess
{
    FA_Read = 1,
    FA_Write = 2,
    FA_ReadWrite = FA_Read | FA_Write
};

// FileShare enumeration.
enum FileShare
{
    FS_None = 0,
    FS_Read = 1,
    FS_Write = 2,
    FS_ReadWrite = FA_Read | FA_Write
};

// Some I/O errors.
enum IOError
{
    IOE_NameTooLong = 0,
    IOE_NotFound,
    IOE_NotDir,
    IOE_Access,
};

// SeekOrigin enumeration values.
enum SeekOrigin
{
    SO_Begin = 0,
    SO_Current,
    SO_End,
};

//-------------------------------------------------------------------
// System layer api version.
SYSAPI void _Chela_SysLayer_Version(int *major, int *minor, int *numfunctions);
SYSAPI const void *_Chela_SysLayer_GetFunction(size_t index);
SYSAPI const char *_Chela_SysLayer_GetFunctionName(size_t index);

//-------------------------------------------------------------------
// Memory management
SYSAPI void *_Chela_Malloc(size_t amount);
SYSAPI void *_Chela_Memalign(size_t boundary, size_t size);
SYSAPI void _Chela_Free(void *block);
SYSAPI void _Chela_Memset(void *ptr, int value, size_t num);
SYSAPI void _Chela_Memcpy(void *dest, const void *source, size_t num);

//-------------------------------------------------------------------
// Input/Output
SYSAPI FILE_HANDLE _Chela_IO_GetStdin();
SYSAPI FILE_HANDLE _Chela_IO_GetStdout();
SYSAPI FILE_HANDLE _Chela_IO_GetStderr();
SYSAPI size_t _Chela_IO_Read(void *ptr, size_t size, size_t count, FILE_HANDLE stream);
SYSAPI size_t _Chela_IO_Write(const void *ptr, size_t size, size_t count, FILE_HANDLE stream);
SYSAPI int _Chela_IO_Puts(const char *str);
SYSAPI int _Chela_IO_PutInt(const char *msg, int value);
SYSAPI int _Chela_IO_PutSize(const char *msg, size_t size);
SYSAPI int _Chela_IO_PutPointer(const char *msg, const void *ptr);

SYSAPI FILE_HANDLE _Chela_IO_Open(int pathSize, const char16_t *utf16Path, int mode, int access, int share, int async);
SYSAPI int _Chela_IO_Close(FILE_HANDLE file);
SYSAPI int _Chela_IO_Flush(FILE_HANDLE stream);
SYSAPI size_t _Chela_IO_Tell(FILE_HANDLE stream);
SYSAPI int _Chela_IO_Seek(FILE_HANDLE stream, size_t offset, int origin);
SYSAPI int _Chela_IO_SetLength(FILE_HANDLE stream, size_t length);
SYSAPI FILE_HANDLE _Chela_IO_InvalidFile();
SYSAPI int _Chela_IO_ErrorNo();
SYSAPI const char *_Chela_IO_ErrorStr(int error);
SYSAPI int _Chela_IO_TransError(int id);

//-------------------------------------------------------------------
// Reflection support
SYSAPI void _Chela_Reflection_Invoke(void *function, const char *signature, void **parameters, void *returnValue);

//-------------------------------------------------------------------
// Data conversion
SYSAPI int _Chela_DC_DToA(char *buffer, double value);
SYSAPI int _Chela_DC_Utf16ToUtf8(char *dst, const char16_t *src);
SYSAPI int _Chela_DC_Utf8ToUtf16(char16_t *dst, const char *src);
SYSAPI int _Chela_DC_Utf8ToUtf32(char32_t *dst, const char *src);

//-------------------------------------------------------------------
// Date time
SYSAPI uint64_t _Chela_Date_Now();
SYSAPI uint64_t _Chela_Date_UtcNow();

//-------------------------------------------------------------------
// Maths

// Trigonometric functions.
SYSAPI double _Chela_Math_Cos(double angle);
SYSAPI double _Chela_Math_Sin(double angle);
SYSAPI double _Chela_Math_Tan(double angle);

// Inverse trigonometric functions.
SYSAPI double _Chela_Math_ACos(double value);
SYSAPI double _Chela_Math_ASin(double value);
SYSAPI double _Chela_Math_ATan(double value);
SYSAPI double _Chela_Math_ATan2(double y, double x);

// Exponential, logarithm.
SYSAPI double _Chela_Math_Exp(double x);
SYSAPI double _Chela_Math_Log(double x);
SYSAPI double _Chela_Math_Log10(double x);
SYSAPI double _Chela_Math_Sqrt(double x);
SYSAPI double _Chela_Math_Pow(double base, double exp);

// Hiperbolic functions
SYSAPI double _Chela_Math_Cosh(double angle);
SYSAPI double _Chela_Math_Sinh(double angle);
SYSAPI double _Chela_Math_Tanh(double angle);

//-------------------------------------------------------------------
// Threading

// Cpu time releasing.
SYSAPI void _Chela_Sleep(int ms);

// Thread.
SYSAPI void* _Chela_CreateThread();
SYSAPI void _Chela_DestroyThread(void* thread);
SYSAPI void _Chela_StartThread(void* thread, void *entry, void *arg);
SYSAPI void _Chela_JoinThread(void* thread);
SYSAPI void _Chela_AbortThread(void* thread);

// Mutex.
SYSAPI void* _Chela_CreateMutex();
SYSAPI void _Chela_DestroyMutex(void* mutex);
SYSAPI void _Chela_EnterMutex(void* mutex);
SYSAPI bool _Chela_TryEnterMutex(void* mutex);
SYSAPI void _Chela_LeaveMutex(void* mutex);

// Condition variable.
SYSAPI void *_Chela_CreateCondition();
SYSAPI void _Chela_DestroyCondition(void *condition);
SYSAPI void _Chela_TimedWaitCondition(void *condition, void *mutex, int milliseconds);
SYSAPI void _Chela_WaitCondition(void *condition, void *mutex);
SYSAPI void _Chela_NotifyCondition(void *condition);
SYSAPI void _Chela_NotifyAllCondition(void *condition);

//-------------------------------------------------------------------
// Atomic operations.
SYSAPI int _Chela_Atomic_CompareAndSwapInt(int *ptr, int oldValue, int newValue);

//-------------------------------------------------------------------
// Native module.
SYSAPI void *_Chela_Module_Load(int nameSize, const char16_t *utf16Name);
SYSAPI void _Chela_Module_Unload(void *module);
SYSAPI void *_Chela_Module_GetSymbol(void *module, const char *name);
SYSAPI const char *_Chela_Module_Error(void *module);

//-------------------------------------------------------------------
// OpenGL
SYSAPI void *_Chela_OpenGL_GetProcedure(const char *name);

// System
SYSAPI void _Chela_Sys_Exit(int code);
SYSAPI void _Chela_Sys_SetErrorLauncher(ChelaErrorLauncher launcher);
SYSAPI void _Chela_Sys_Error(int code, const char *message);
SYSAPI void _Chela_Sys_Fatal(const char *error);
SYSAPI void _Chela_Sys_Abort();

//----------------------------------------------------------------
// Gui/Drawing subsystem

// Driver retrieving.
SYSAPI int _ChGui_GetDriverCount();
SYSAPI void *_ChGui_GetDriver(int index);

// Driver functions.
SYSAPI void *_ChGui_Driver_GetDisplay(void *driver);
SYSAPI void _ChGui_Driver_MainLoop(void *driver);

// Display
SYSAPI int _ChGui_Display_GetScreenCount(void *disp);
SYSAPI int _ChGui_Display_GetDefaultScreenIndex(void *disp);
SYSAPI void *_ChGui_Display_GetScreen(void *disp, int index);
SYSAPI bool _ChGui_Display_PollEvent(void *disp, void *dest);
SYSAPI void *_ChGui_Display_CreateSimpleWindow(void *disp, void *parent, int x, int y, int w,
                                int h, int borderWidth, int border, int bg);
SYSAPI void *_ChGui_Display_CreateRenderWindow(void *disp, void *context, void *parent, int x, int y, int w,
                                int h, int borderWidth, int border, int bg);
SYSAPI void *_ChGui_Display_CreateRenderContext(void *disp, const char *systemName,
                                void *screen, int numattributes, int *attributes);

// Screen
SYSAPI void *_ChGui_Screen_GetRootWindow(void *screen);
SYSAPI int _ChGui_Screen_GetWidth(void *screen);
SYSAPI int _ChGui_Screen_GetHeight(void *screen);

// Drawable
SYSAPI int _ChGui_Drawable_GetWidth(void *drawable);
SYSAPI int _ChGui_Drawable_GetHeight(void *drawable);

// Window.
SYSAPI void _ChGui_Window_Show(void *window);
SYSAPI void _ChGui_Window_Hide(void *window);

SYSAPI void _ChGui_Window_CaptureMouse(void *window);
SYSAPI void _ChGui_Window_ReleaseMouse(void *window);

SYSAPI bool _ChGui_Window_HasFocus(void *window);
SYSAPI void _ChGui_Window_SetFocus(void *window);

SYSAPI bool _ChGui_Window_IsInputEnabled(void *window);
SYSAPI void _ChGui_Window_EnableInput(void *window);
SYSAPI void _ChGui_Window_DisableInput(void *window);

SYSAPI void* _ChGui_Window_CreateGraphicContext(void *window, bool painting);
SYSAPI void _ChGui_Window_Refresh(void *window);

SYSAPI const char16_t *_ChGui_Window_GetStringAttribute(void *window, const char *name);
SYSAPI void _ChGui_Window_SetStringAttribute(void *window, const char *name, int attrName, const char16_t *attr);

// Graphic context
SYSAPI void _ChGui_GC_BeginDrawing(void *gc);
SYSAPI void _ChGui_GC_EndDrawing(void *gc);

SYSAPI void _ChGui_GC_SetBackground(void *gc, int color);
SYSAPI int _ChGui_GC_GetBackground(void *gc);
SYSAPI void _ChGui_GC_SetForeground(void *gc, int color);
SYSAPI int _ChGui_GC_GetForeground(void *gc);

SYSAPI void _ChGui_GC_Clear(void *gc);
SYSAPI void _ChGui_GC_ClearRect(void *gc, int x0, int y0, int x1, int y1);

SYSAPI void _ChGui_GC_DrawPoint(void *gc, int x0, int y0);
SYSAPI void _ChGui_GC_DrawLine(void *gc, int x0, int y0, int x1, int y1);
SYSAPI void _ChGui_GC_DrawRectangle(void *gc, int x0, int y0, int x1, int y1);
SYSAPI void _ChGui_GC_DrawFillRectangle(void *gc, int x0, int y0, int x1, int y1);
SYSAPI void _ChGui_GC_DrawTriangle(void *gc, int x0, int y0, int x1, int y1, int x2, int y2);
SYSAPI void _ChGui_GC_DrawFillTriangle(void *gc, int x0, int y0, int x1, int y1, int x2, int y2);
SYSAPI void _ChGui_GC_DrawHorizGradient(void *gc, int x0, int y0, int x1, int y1, int start, int end);
SYSAPI void _ChGui_GC_DrawVertGradient(void *gc, int x0, int y0, int x1, int y1, int start, int end);

// Render context
SYSAPI bool _ChGui_Render_MakeCurrent(void *context, void *window);
SYSAPI void _ChGui_Render_SwapBuffers(void *context);

#endif //CHELA_SYS_LAYER_HPP
