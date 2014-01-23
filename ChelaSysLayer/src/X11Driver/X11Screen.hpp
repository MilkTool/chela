#ifndef X11_SCREEN_HPP
#define X11_SCREEN_HPP

namespace X11Driver
{
    class X11Window;

    // The screen.
    class X11Screen: public GuiScreen
    {
    public:
        X11Screen(X11Display &context, int screenNum);
        ~X11Screen();

        virtual int GetWidth() const;
        virtual int GetHeight() const;

        virtual GuiWindow *GetRootWindow();
        Display *GetDisplay();
        int GetScreenNumber();

    private:
        X11Display &context;
        Display *display;
        int screenNum;
        X11Window *rootWindow;
    };
}

#endif //X11_SCREEN_HPP
