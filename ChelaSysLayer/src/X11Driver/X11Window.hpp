#ifndef CHELAG_X11_WINDOW_HPP
#define CHELAG_X11_WINDOW_HPP

#include <string>
#include <map>
#include "X11Drawable.hpp"

namespace X11Driver
{
    typedef std::basic_string<char16_t> utf16_string;

    class X11Display;
    class X11Window: public GuiWindow, public X11Drawable
    {
    public:
        X11Window(X11Display &context, Window window, int screenNum, int x, int y, int w, int h, int bg);
        ~X11Window();

        virtual Drawable GetDrawable() const;

        virtual bool IsWindow() const;
        virtual bool IsBitmap() const;

        virtual void Show();
        virtual void Hide();

        virtual int GetBackground() const;
        virtual void SetBackground(int color);
        
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

        virtual int GetHeight() const;
        virtual int GetWidth() const;

        virtual GuiGraphicContext *CreateGraphicContext(bool painting);

        Window GetWindow() const { return window; }

        void SetRootWindow(bool root)
        {
            rootWindow = root;
        }

        bool IsRootWindow() const
        {
            return rootWindow;
        }

        int GetScreenNum() const
        {
            return screenNum;
        }

        void OnConfigureNotify(XConfigureEvent &ev);
        void OnDestroyNotify();

    private:
        void CacheGeometry();

        X11Display &context;
        Display *display;
        Window window;
        int screenNum;
        int width, height;
        int x, y;
        int bg;
        bool rootWindow;

        typedef std::map<std::string, utf16_string> AttributeMap;
        AttributeMap attributeMap;
    };
}

#endif //CHELAG_X11_WINDOW_HPP
