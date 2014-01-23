#include <string.h>
#include "ChelaUtf8.hpp"
#include "X11Driver.hpp"
#include "X11GraphicContext.hpp"

namespace X11Driver
{
    X11Window::X11Window(X11Display &context, Window window, int screenNum, int x, int y, int w, int h, int bg)
        : context(context), display(context.get()), window(window), screenNum(screenNum),
          width(w), height(h), x(x), y(y), bg(bg)
    {
        rootWindow = false;
        context.MapWindow(window, this);
    }

    X11Window::~X11Window()
    {
        context.UnmapWindow(window);
    }

    Drawable X11Window::GetDrawable() const
    {
        return window;
    }

    bool X11Window::IsWindow() const
    {
        return true;
    }

    bool X11Window::IsBitmap() const
    {
        return false;
    }

    void X11Window::Show()
    {
        // Map the window.
        XMapWindow(display, window);
    }

    void X11Window::Hide()
    {
        // Unmap the window.
        XUnmapWindow(display, window);
    }

    int X11Window::GetBackground() const
    {
        return bg;
    }

    void X11Window::SetBackground(int color)
    {
        bg = color;
    }

    void X11Window::CaptureMouse()
    {
        XGrabPointer(display, window, False,
            ButtonPressMask | ButtonReleaseMask |
            EnterWindowMask | LeaveWindowMask |
            PointerMotionMask,
            GrabModeAsync, GrabModeAsync,
            None, None, CurrentTime);
    }
    
    void X11Window::ReleaseMouse()
    {
        XUngrabPointer(display, CurrentTime);
    }
        
    bool X11Window::HasFocus()
    {
        Window currentFocus;
        int focusState;
        XGetInputFocus(display, &currentFocus, &focusState);
        return currentFocus == window;
    }
    
    void X11Window::SetFocus()
    {
        XSetInputFocus(display, window, RevertToNone, CurrentTime);
    }
        
    bool X11Window::IsInputEnabled()
    {
        return true;
    }
    
    void X11Window::EnableInput()
    {
    }
    
    void X11Window::DisableInput()
    {
    }

    void X11Window::Refresh()
    {
        XClearArea(display, window, 0, 0, 0, 0, True);
    }
        
    void X11Window::OnConfigureNotify(XConfigureEvent &ev)
    {
        width = ev.width;
        height = ev.height;
        x = ev.x;
        y = ev.y;
    }

    void X11Window::OnDestroyNotify()
    {
    }

    int X11Window::GetHeight() const
    {
        return height;
    }

    int X11Window::GetWidth() const
    {
        return width;
    }

    const char16_t *X11Window::GetStringAttribute(const char *name)
    {
        if(!name)
            return NULL;

        AttributeMap::iterator it = attributeMap.find(name);
        if(it != attributeMap.end())
            return it->second.c_str();
        return NULL;
    }

    void X11Window::SetStringAttribute(const char *name, int attrSize, const char16_t *attr)
    {
        if(!name)
            return;

        // Store the attribute.
        attributeMap[name] = attr;

        // Handle some attributes.
        if(!strcmp(name, "Title"))
        {
            // Change the wm_name property.
            ChelaUtf8String value(attrSize, attr);
            XChangeProperty(display, window, context.GetNetWmNameAtom(),
                    context.GetUtf8StringAtom(),
                    8, PropModeReplace,
                    (const unsigned char*)value.c_str(), value.size());
        }
    }

    GuiGraphicContext *X11Window::CreateGraphicContext(bool painting)
    {
        return new X11GraphicContext(context, this);
    }
}
