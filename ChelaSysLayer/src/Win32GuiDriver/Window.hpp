#ifndef CHELAG_WIN32_WINDOW_HPP
#define CHELAG_WIN32_WINDOW_HPP

#include <map>
#include <string>
#include "Drawable.hpp"

namespace Win32Gui
{
    typedef std::basic_string<char16_t> utf16_string;

    class Window: public GuiWindow, public Drawable
    {
    public:
        Window(Display *display, HWND window, int x, int y, int w, int h, int bg);
        ~Window();
    
        virtual bool IsWindow() const;
        virtual bool IsBitmap() const;

        virtual int GetHeight() const;
        virtual int GetWidth() const;

        virtual int GetBackground() const;
        virtual void SetBackground(int color);

        virtual void Show();
        virtual void Hide();
        
        virtual void CaptureMouse();
        virtual void ReleaseMouse();
        
        virtual bool HasFocus();
        virtual void SetFocus();
        
        virtual bool IsInputEnabled();
        virtual void EnableInput();
        virtual void DisableInput();

        virtual void Refresh();
        
        virtual const char16_t *GetStringAttribute(const char *name);
        virtual void SetStringAttribute(const char *name, int attrSize, const char16_t *attr);

        virtual GuiGraphicContext *CreateGraphicContext(bool painting);

        void UpdateSize(int w, int h);
        void UpdatePosition(int x, int y);

        HWND GetHandle()
        {
            return window;
        }

        bool IsMouseOver() const
        {
            return isMouseOver;
        }

        void SetMouseOver(bool value)
        {
            isMouseOver = value;
        }

    private:
        Display *display;
        HWND window;
        int x, y;
        int width, height;
        int bg;
        bool isMouseOver;
        typedef std::map<std::string, utf16_string> AttributeMap;
        AttributeMap attributeMap;
    };
}

#endif //CHELAG_WIN32_WINDOW_HPP

