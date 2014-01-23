#ifndef CHELAG_WIN32_DISPLAY_HPP
#define CHELAG_WIN32_DISPLAY_HPP

#include <map>

namespace Win32Gui
{
    // The display representation.
    class Window;
    class Display: public GuiDisplay
    {
    public:
        Display();
        ~Display();

        virtual int GetScreenCount();
        virtual int GetDefaultScreenIndex();
        virtual GuiScreen *GetScreen(int index);
        virtual GuiWindow *CreateSimpleWindow(GuiWindow *parent, int x, int y,
                int w, int h, int borderWidth, int border, int bg);
        virtual GuiWindow *CreateRenderWindow(GuiRenderContext *context, GuiWindow *parent, int x, int y,
                int w, int h, int borderWidth, int border, int bg);
        virtual GuiRenderContext *CreateRenderContext(const char *systemName, GuiScreen *screen, int numattributes, int *attributes);

        virtual bool PollEvent(Event *ev);
        LRESULT ReceivedMessage(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
        
        Window *GetWindow(HWND hwnd) const;
        void MapWindow(HWND hwnd, Window *window);
        void UnmapWindow(HWND hwnd);

    private:
        Screen screen;
        Event *currentEvent;
        bool processedMessage;
    };
}

#endif //CHELAG_WIN32_DISPLAY_HPP
