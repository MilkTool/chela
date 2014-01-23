#ifndef CHELAG_WIN32_DRIVER_HPP
#define CHELAG_WIN32_DRIVER_HPP

#define WINVER       0x0500
#define _WIN32_WINNT 0x0500
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include "ChelaGui.hpp"

using namespace ChelaGui;

#include "Screen.hpp"
#include "Display.hpp"
#include "Window.hpp"

namespace Win32Gui
{
    // The base class for the Win32 GUI driver.
    class Win32GuiDriver: public GuiDriver
    {
    public:
        Win32GuiDriver();
        ~Win32GuiDriver();

        virtual GuiDisplay *GetDisplay();

    private:
        Display display;
    };
}

#endif //CHELAG_WIN32_DRIVER_HPP

