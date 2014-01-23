#include "Driver.hpp"

namespace Win32Gui
{
    Screen::Screen(Display *display)
        : display(display)
    {
        rootWindow = NULL;
    }

    Screen::~Screen()
    {
    }

    int Screen::GetWidth() const
    {
        return GetSystemMetrics(SM_CXSCREEN);
    }

    int Screen::GetHeight() const
    {
        return GetSystemMetrics(SM_CYSCREEN);
    }

    GuiWindow *Screen::GetRootWindow()
    {
        if(!rootWindow)
            rootWindow = new Window(display, GetDesktopWindow(), 0, 0, GetWidth(), GetHeight(), 0);
        return rootWindow;
    }
}
