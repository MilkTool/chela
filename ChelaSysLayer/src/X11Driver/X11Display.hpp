#ifndef CHELAG_X11_DISPLAY_HPP
#define CHELAG_X11_DISPLAY_HPP

#include <vector>
#include <map>

namespace X11Driver
{
    // The X11 display
    class X11Screen;
    class X11Window;
    class GlxContext;
    class X11Display: public GuiDisplay
    {
    public:
        X11Display();
        ~X11Display();

        void Open();

        virtual int GetScreenCount();
        virtual int GetDefaultScreenIndex();
        virtual GuiScreen *GetScreen(int index);
        virtual GuiWindow *CreateSimpleWindow(GuiWindow *parent, int x, int y,
                int w, int h, int borderWidth, int border, int bg);
        virtual GuiWindow *CreateRenderWindow(GuiRenderContext *context, GuiWindow *parent, int x, int y,
                int w, int h, int borderWidth, int border, int bg);
        virtual GuiRenderContext *CreateRenderContext(const char *systemName, GuiScreen *screen, int numattributes, int *attributes);

        virtual bool PollEvent(Event *ev);

        X11Window *GetWindow(Window id) const;
        void MapWindow(Window id, X11Window *window);
        void UnmapWindow(Window id);

        Display *get()
        {
            if(!display)
                Open();
            return display;
        }

        operator Display*()
        {
            return get();
        }
        
        Atom GetNetWmNameAtom()
        {
            return netWmName;
        }
        
        Atom GetUtf8StringAtom()
        {
            return utf8StringAtom;
        }

    private:
        typedef std::map<int, int> KeyMap;
        void ReadKeyCode(XKeyEvent &xkey, KeyEvent &key, bool press);
        
        Display *display;
        std::vector<X11Screen*> screens;
        KeyMap keyMap;

        XContext windowContext;
        Atom wmProtocols;
        Atom wmDelete;
        Atom netWmName;
        Atom utf8StringAtom;
    };
};

#endif //CHELAG_X11_DISPLAY_HPP

