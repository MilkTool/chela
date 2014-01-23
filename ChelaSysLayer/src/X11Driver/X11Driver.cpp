#include <stdlib.h>
#include <stdio.h>
#include <X11/Xlib.h>
#include "X11Driver.hpp"

using namespace ChelaGui;

namespace X11Driver
{
    // X11 driver singleton.
    static X11GuiDriver *x11Driver = NULL;

    X11GuiDriver::X11GuiDriver()
    {
    }

    X11GuiDriver::~X11GuiDriver()
    {
    }

    GuiDisplay *X11GuiDriver::GetDisplay()
    {
        // Make sure the display is opened.
        display.Open();

        // Return the display.
        return &display;
    }
};

// Driver interfacing.
namespace ChelaGui
{
    using namespace X11Driver;

    GuiDriver *CreateX11Driver()
    {
        if(x11Driver == NULL)
            x11Driver = new X11GuiDriver();
        return x11Driver;
    }
}

