#ifndef CHELAG_WIN32_SCREEN_HPP
#define CHELAG_WIN32_SCREEN_HPP

namespace Win32Gui
{
    // The screen representation.
    class Display;
    class Screen: public GuiScreen
    {
    public:
        Screen(Display *display);
        ~Screen();

        virtual int GetWidth() const;
        virtual int GetHeight() const;

        virtual GuiWindow *GetRootWindow();

    private:
        Display *display;
        GuiWindow *rootWindow;
    };
}

#endif //CHELAG_WIN32_SCREEN_HPP

