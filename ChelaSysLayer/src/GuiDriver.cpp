#include "SysLayer.hpp"
#include "ChelaGui.hpp"

using namespace ChelaGui;

typedef GuiDriver *(*CreateDriverFn)();

static CreateDriverFn guiDrivers [] = {
// Use the win32 driver in windows
#ifdef _WIN32
    CreateWin32GuiDriver,
#endif
// Use X11 when supported.
#ifdef USE_X11
    CreateX11Driver,
#endif
    NULL,
};
static int numdrivers = -1;

SYSAPI int _ChGui_GetDriverCount()
{
    if(numdrivers < 0)
    {
        numdrivers = 0;

        CreateDriverFn *pos = guiDrivers;
        while(*pos++)
            numdrivers++;
    }

    return numdrivers;
}

SYSAPI void *_ChGui_GetDriver(int index)
{
    if(index < 0 || index >= _ChGui_GetDriverCount())
        return NULL;
    return guiDrivers[index]();
}

// Driver
SYSAPI void *_ChGui_Driver_GetDisplay(void *driver)
{
    GuiDriver *guiDriver = (GuiDriver*)driver;
    return guiDriver->GetDisplay();
}

// Display
SYSAPI int _ChGui_Display_GetScreenCount(void *disp)
{
    return ((GuiDisplay*)disp)->GetScreenCount();
}

SYSAPI int _ChGui_Display_GetDefaultScreenIndex(void *disp)
{
    return ((GuiDisplay*)disp)->GetDefaultScreenIndex();
}

SYSAPI void *_ChGui_Display_GetScreen(void *disp, int index)
{
    return ((GuiDisplay*)disp)->GetScreen(index);
}

SYSAPI bool _ChGui_Display_PollEvent(void *disp, void *dest)
{
    return ((GuiDisplay*)disp)->PollEvent((Event*)dest);
}

SYSAPI void *_ChGui_Display_CreateSimpleWindow(void *disp, void *parent, int x, int y, int w,
                                              int h, int borderWidth, int border, int bg)
{
    return ((GuiDisplay*)disp)->CreateSimpleWindow((GuiWindow*)parent, x, y, w, h, borderWidth, border, bg);
}

SYSAPI void *_ChGui_Display_CreateRenderWindow(void *disp, void *context, void *parent, int x, int y, int w,
                                int h, int borderWidth, int border, int bg)
{
    return ((GuiDisplay*)disp)->CreateRenderWindow((GuiRenderContext*)context, (GuiWindow*)parent, x, y, w, h, borderWidth, border, bg);
}

SYSAPI void *_ChGui_Display_CreateRenderContext(void *disp, const char *systemName, void *screen, int numattributes, int *attributes)
{
    return ((GuiDisplay*)disp)->CreateRenderContext(systemName, (GuiScreen*)screen, numattributes, attributes);
}

// Screen
SYSAPI void *_ChGui_Screen_GetRootWindow(void *screen)
{
    return ((GuiScreen*)screen)->GetRootWindow();
}

SYSAPI int _ChGui_Screen_GetWidth(void *screen)
{
    return ((GuiScreen*)screen)->GetWidth();
}

SYSAPI int _ChGui_Screen_GetHeight(void *screen)
{
    return ((GuiScreen*)screen)->GetHeight();
}

// Drawable
SYSAPI int _ChGui_Drawable_GetWidth(void *drawable)
{
    return ((GuiWindow*)drawable)->GetWidth();
}

SYSAPI int _ChGui_Drawable_GetHeight(void *drawable)
{
    return ((GuiWindow*)drawable)->GetHeight();
}

// Window.
SYSAPI void _ChGui_Window_Show(void *window)
{
    ((GuiWindow*)window)->Show();
}

SYSAPI void _ChGui_Window_Hide(void *window)
{
    ((GuiWindow*)window)->Hide();
}

SYSAPI void _ChGui_Window_CaptureMouse(void *window)
{
    ((GuiWindow*)window)->CaptureMouse();
}

SYSAPI void _ChGui_Window_ReleaseMouse(void *window)
{
    ((GuiWindow*)window)->ReleaseMouse();
}

SYSAPI bool _ChGui_Window_HasFocus(void *window)
{
    return ((GuiWindow*)window)->HasFocus();
}

SYSAPI void _ChGui_Window_SetFocus(void *window)
{
    ((GuiWindow*)window)->SetFocus();
}

SYSAPI bool _ChGui_Window_IsInputEnabled(void *window)
{
    return ((GuiWindow*)window)->IsInputEnabled();
}

SYSAPI void _ChGui_Window_EnableInput(void *window)
{
    ((GuiWindow*)window)->EnableInput();
}

SYSAPI void _ChGui_Window_DisableInput(void *window)
{
    ((GuiWindow*)window)->DisableInput();
}

SYSAPI void* _ChGui_Window_CreateGraphicContext(void *window, bool painting)
{
    return ((GuiWindow*)window)->CreateGraphicContext(painting);
}

SYSAPI void _ChGui_Window_Refresh(void *window)
{
    ((GuiWindow*)window)->Refresh();
}

SYSAPI const char16_t *_ChGui_Window_GetStringAttribute(void *window, const char *name)
{
    return ((GuiWindow*)window)->GetStringAttribute(name);
}

SYSAPI void _ChGui_Window_SetStringAttribute(void *window, const char *name, int attrSize, const char16_t *attr)
{
    ((GuiWindow*)window)->SetStringAttribute(name, attrSize, attr);
}

// Graphic context
SYSAPI void _ChGui_GC_BeginDrawing(void *gc)
{
    ((GuiGraphicContext*)gc)->BeginDrawing();
}

SYSAPI void _ChGui_GC_EndDrawing(void *gc)
{
    ((GuiGraphicContext*)gc)->EndDrawing();
}

SYSAPI void _ChGui_GC_SetBackground(void *gc, int color)
{
    ((GuiGraphicContext*)gc)->SetBackground(color);
}

SYSAPI int _ChGui_GC_GetBackground(void *gc)
{
    return ((GuiGraphicContext*)gc)->GetBackground();
}

SYSAPI void _ChGui_GC_SetForeground(void *gc, int color)
{
    ((GuiGraphicContext*)gc)->SetForeground(color);
}

SYSAPI int _ChGui_GC_GetForeground(void *gc)
{
    return ((GuiGraphicContext*)gc)->GetForeground();
}

SYSAPI void _ChGui_GC_Clear(void *gc)
{
    ((GuiGraphicContext*)gc)->Clear();
}

SYSAPI void _ChGui_GC_ClearRect(void *gc, int x0, int y0, int x1, int y1)
{
    ((GuiGraphicContext*)gc)->ClearRect(x0, y0, x1, y1);
}

SYSAPI void _ChGui_GC_DrawPoint(void *gc, int x0, int y0)
{
    ((GuiGraphicContext*)gc)->DrawPoint(x0, y0);
}

SYSAPI void _ChGui_GC_DrawLine(void *gc, int x0, int y0, int x1, int y1)
{
    ((GuiGraphicContext*)gc)->DrawLine(x0, y0, x1, y1);
}

SYSAPI void _ChGui_GC_DrawRectangle(void *gc, int x0, int y0, int x1, int y1)
{
    ((GuiGraphicContext*)gc)->DrawRectangle(x0, y0, x1, y1);
}

SYSAPI void _ChGui_GC_DrawFillRectangle(void *gc, int x0, int y0, int x1, int y1)
{
    ((GuiGraphicContext*)gc)->DrawFillRectangle(x0, y0, x1, y1);
}

SYSAPI void _ChGui_GC_DrawTriangle(void *gc, int x0, int y0, int x1, int y1, int x2, int y2)
{
    ((GuiGraphicContext*)gc)->DrawTriangle(x0, y0, x1, y1, x2, y2);
}

SYSAPI void _ChGui_GC_DrawFillTriangle(void *gc, int x0, int y0, int x1, int y1, int x2, int y2)
{
    ((GuiGraphicContext*)gc)->DrawFillTriangle(x0, y0, x1, y1, x2, y2);
}

SYSAPI void _ChGui_GC_DrawHorizGradient(void *gc, int x0, int y0, int x1, int y1, int start, int end)
{
    ((GuiGraphicContext*)gc)->DrawHorizGradient(x0, y0, x1, y1, start, end);
}

SYSAPI void _ChGui_GC_DrawVertGradient(void *gc, int x0, int y0, int x1, int y1, int start, int end)
{
    ((GuiGraphicContext*)gc)->DrawVertGradient(x0, y0, x1, y1, start, end);
}

// Render context
SYSAPI bool _ChGui_Render_MakeCurrent(void *context, void *window)
{
    return ((GuiRenderContext*)context)->MakeCurrent((GuiWindow*)window);
}

SYSAPI void _ChGui_Render_SwapBuffers(void *context)
{
    ((GuiRenderContext*)context)->SwapBuffers();
}
