#include "SysLayer.hpp"

struct FunctionData
{
	const void *function;
	const char *name;
};

#define API(function) {(const void*)&function, #function }
// This is to prevent the linker from removing functions.
static int LayerFunctionCount = -1;
static FunctionData LayerFunctions[] = {
	// Layer api.
	API(_Chela_SysLayer_Version),
	API(_Chela_SysLayer_GetFunction),
    API(_Chela_SysLayer_GetFunctionName),
	
	// Memory management
	API(_Chela_Malloc),
	API(_Chela_Memalign),
	API(_Chela_Free),
    API(_Chela_Memset),
    API(_Chela_Memcpy),

    // Input/Output
    API(_Chela_IO_GetStdin),
    API(_Chela_IO_GetStdout),
    API(_Chela_IO_GetStderr),
    API(_Chela_IO_Read),
    API(_Chela_IO_Write),
    API(_Chela_IO_Puts),
    API(_Chela_IO_PutInt),
    API(_Chela_IO_PutSize),
    API(_Chela_IO_PutPointer),
    API(_Chela_IO_Open),
    API(_Chela_IO_Close),
    API(_Chela_IO_Flush),
    API(_Chela_IO_Tell),
    API(_Chela_IO_Seek),
    API(_Chela_IO_SetLength),
    API(_Chela_IO_InvalidFile),
    API(_Chela_IO_ErrorNo),
    API(_Chela_IO_ErrorStr),
    API(_Chela_IO_TransError),

    // Reflection
    API(_Chela_Reflection_Invoke),

    // Data conversion.
    API(_Chela_DC_DToA),

    // Date time
    API(_Chela_Date_Now),
    API(_Chela_Date_UtcNow),

    // Maths
    API(_Chela_Math_Cos),
    API(_Chela_Math_Sin),
    API(_Chela_Math_Tan),

    API(_Chela_Math_ACos),
    API(_Chela_Math_ASin),
    API(_Chela_Math_ATan),
    API(_Chela_Math_ATan2),

    API(_Chela_Math_Exp),
    API(_Chela_Math_Log),
    API(_Chela_Math_Log10),
    API(_Chela_Math_Sqrt),
    API(_Chela_Math_Pow),

    API(_Chela_Math_Cosh),
    API(_Chela_Math_Sinh),
    API(_Chela_Math_Tanh),

    // Threading
    API(_Chela_Sleep),
    
    API(_Chela_CreateThread),
    API(_Chela_DestroyThread),
    API(_Chela_StartThread),
    API(_Chela_JoinThread),
    API(_Chela_AbortThread),

    API(_Chela_CreateMutex),
    API(_Chela_DestroyMutex),
    API(_Chela_EnterMutex),
    API(_Chela_TryEnterMutex),
    API(_Chela_LeaveMutex),

    API(_Chela_CreateCondition),
    API(_Chela_DestroyCondition),
    API(_Chela_TimedWaitCondition),
    API(_Chela_WaitCondition),
    API(_Chela_NotifyCondition),
    API(_Chela_NotifyAllCondition),

    // Atomic operations.
    API(_Chela_Atomic_CompareAndSwapInt),

    // Native modules.
    API(_Chela_Module_Load),
    API(_Chela_Module_Unload),
    API(_Chela_Module_GetSymbol),
    API(_Chela_Module_Error),

    // OpenGL
    API(_Chela_OpenGL_GetProcedure),

    // System
    API(_Chela_Sys_Exit),
    API(_Chela_Sys_Abort),

    // ChelaGui ------------------------------------------

    // Gui driver
    API(_ChGui_GetDriverCount),
    API(_ChGui_GetDriver),

    API(_ChGui_Driver_GetDisplay),

    // Display
    API(_ChGui_Display_GetScreenCount),
    API(_ChGui_Display_GetDefaultScreenIndex),
    API(_ChGui_Display_GetScreen),
    API(_ChGui_Display_PollEvent),
    API(_ChGui_Display_CreateSimpleWindow),
    API(_ChGui_Display_CreateRenderWindow),
    API(_ChGui_Display_CreateRenderContext),

    // Screen
    API(_ChGui_Screen_GetRootWindow),
    API(_ChGui_Screen_GetWidth),
    API(_ChGui_Screen_GetHeight),

    // Drawable
    API(_ChGui_Drawable_GetWidth),
    API(_ChGui_Drawable_GetHeight),

    // Window
    API(_ChGui_Window_Show),
    API(_ChGui_Window_Hide),
    API(_ChGui_Window_CreateGraphicContext),

    API(_ChGui_Window_GetStringAttribute),
    API(_ChGui_Window_SetStringAttribute),

    // GraphicContext
    API(_ChGui_GC_BeginDrawing),
    API(_ChGui_GC_EndDrawing),

    API(_ChGui_GC_SetBackground),
    API(_ChGui_GC_GetBackground),
    API(_ChGui_GC_SetForeground),
    API(_ChGui_GC_GetForeground),

    API(_ChGui_GC_Clear),
    API(_ChGui_GC_ClearRect),

    API(_ChGui_GC_DrawPoint),
    API(_ChGui_GC_DrawLine),
    API(_ChGui_GC_DrawRectangle),
    API(_ChGui_GC_DrawFillRectangle),
    API(_ChGui_GC_DrawTriangle),
    API(_ChGui_GC_DrawFillTriangle),
    API(_ChGui_GC_DrawHorizGradient),
    API(_ChGui_GC_DrawVertGradient),

    // RenderContext
    API(_ChGui_Render_MakeCurrent),
    API(_ChGui_Render_SwapBuffers),

	{NULL, NULL},
};

SYSAPI void _Chela_SysLayer_Version(int *major, int *minor, int *numfunctions)
{
    if(major && minor)
    {
        *major = SysLayer_VersionMajor;
        *minor = SysLayer_VersionMinor;
    }

    if(numfunctions)
    {
	    // Count the functions
	    if(LayerFunctionCount < 0)
	    {
		    LayerFunctionCount = 0;
		    while(LayerFunctions[LayerFunctionCount].function != NULL)
		    	LayerFunctionCount++;
	    }

		// Return the number of functions.
		*numfunctions = LayerFunctionCount;		
    }
}

SYSAPI const void *_Chela_SysLayer_GetFunction(size_t index)
{
    int numfunctions;
    _Chela_SysLayer_Version(NULL, NULL, &numfunctions);

    if(index >= (size_t)numfunctions)
        return NULL;
	return LayerFunctions[index].function;
}

SYSAPI const char *_Chela_SysLayer_GetFunctionName(size_t index)
{
    int numfunctions;
    _Chela_SysLayer_Version(NULL, NULL, &numfunctions);

    if(index >= (size_t)numfunctions)
        return NULL;
    return LayerFunctions[index].name;
}

