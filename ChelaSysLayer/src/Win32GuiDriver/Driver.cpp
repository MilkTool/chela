#include "Driver.hpp"

namespace Win32Gui
{
    static Win32GuiDriver *win32GuiDriver = NULL;

    Win32GuiDriver::Win32GuiDriver()
    {
    }

    Win32GuiDriver::~Win32GuiDriver()
    {
    }

    GuiDisplay *Win32GuiDriver::GetDisplay()
    {
        return &display;
    }
}

// Driver interfacing.
namespace ChelaGui
{
    using namespace Win32Gui;

    GuiDriver *CreateWin32GuiDriver()
    {
        if(win32GuiDriver == NULL)
            win32GuiDriver = new Win32GuiDriver();
        return win32GuiDriver;
    }
}

