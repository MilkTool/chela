#ifndef CHELAG_X11_DRIVER_HPP
#define CHELAG_X11_DRIVER_HPP

#include <stdlib.h>
#include <stdio.h>
#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <X11/Xresource.h>
#include "ChelaGui.hpp"

using namespace ChelaGui;

#include "X11Display.hpp"
#include "X11Screen.hpp"
#include "X11Window.hpp"

namespace X11Driver
{
    // The base X11 driver.
    class X11GuiDriver: public GuiDriver
    {
    public:
        X11GuiDriver();
        ~X11GuiDriver();

        virtual GuiDisplay *GetDisplay();

    private:
        X11Display display;
    };
}

#endif //CHELAG_X11_DRIVER_HPP
