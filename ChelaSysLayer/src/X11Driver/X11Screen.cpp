#include "X11Driver.hpp"

namespace X11Driver
{
    // X11Screen
    X11Screen::X11Screen(X11Display &context, int screenNum)
        : context(context), display(context.get()), screenNum(screenNum)
    {
        rootWindow = NULL;
    }

    X11Screen::~X11Screen()
    {
        delete rootWindow;
    }

    int X11Screen::GetWidth() const
    {
        return DisplayWidth(display, screenNum);
    }

    int X11Screen::GetHeight() const
    {
        return DisplayHeight(display, screenNum);
    }

    GuiWindow *X11Screen::GetRootWindow()
    {
        if(!rootWindow)
        {
            rootWindow = new X11Window(context, RootWindow(display, screenNum), screenNum, 0, 0, GetWidth(), GetHeight(), 0);
            rootWindow->SetRootWindow(true);
        }
        return rootWindow;
    }

    Display *X11Screen::GetDisplay()
    {
        return display;
    }

    int X11Screen::GetScreenNumber()
    {
        return screenNum;
    }
}
