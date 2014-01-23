#include <string.h>
#include "Driver.hpp"
#include "Window.hpp"
#include "GraphicContext.hpp"

namespace Win32Gui
{
    Window::Window(Display *display, HWND window, int x, int y, int w, int h, int bg)
        : display(display), window(window), x(x), y(y), width(w), height(h), bg(bg)
    {
        isMouseOver = false;
        display->MapWindow(window, this);
    }

    Window::~Window()
    {
        display->UnmapWindow(window);
    }

    bool Window::IsWindow() const
    {
        return true;
    }

    bool Window::IsBitmap() const
    {
        return false;
    }

    int Window::GetHeight() const
    {
        return height;
    }

    int Window::GetWidth() const
    {
        return width;
    }

    int Window::GetBackground() const
    {
        return bg;
    }

    void Window::SetBackground(int color)
    {
        bg = color;
    }

    void Window::Show()
    {
        ShowWindow(window, SW_SHOW);
    }

    void Window::Hide()
    {
        ShowWindow(window, SW_HIDE);
    }

    void Window::CaptureMouse()
    {
        SetCapture(window);
    }
    
    void Window::ReleaseMouse()
    {
        ReleaseCapture();
    }
        
    bool Window::HasFocus()
    {
        return GetFocus() == window;
    }
    
    void Window::SetFocus()
    {
        ::SetFocus(window);
    }
    
    bool Window::IsInputEnabled()
    {
        return IsWindowEnabled(window);
    }
    
    void Window::EnableInput()
    {
        EnableWindow(window, true);
    }
    
    void Window::DisableInput()
    {
        EnableWindow(window, false);
    }

    void Window::Refresh()
    {
        InvalidateRect(window, NULL, TRUE);
    }

    const char16_t *Window::GetStringAttribute(const char *name)
    {
        if(!name)
            return NULL;

        // Find the attribute.
        AttributeMap::iterator it = attributeMap.find(name);
        if(it != attributeMap.end())
            return it->second.c_str();

        return NULL;
    }

    void Window::SetStringAttribute(const char *name, int attrSize, const char16_t *attr)
    {
        if(!name)
            return;

        // Store the attribute.
        attributeMap[name] = attr;

        // Set the attribute.
        if(!strcmp(name, "Title"))
            SetWindowTextW(window, (LPCWSTR)attr);
    }

    GuiGraphicContext *Window::CreateGraphicContext(bool painting)
    {
        return new GraphicContext(this, painting);
    }

    void Window::UpdateSize(int w, int h)
    {
        this->width = w;
        this->height = h;
    }

    void Window::UpdatePosition(int x, int y)
    {
        this->x = x;
        this->y = y;
    }
}

