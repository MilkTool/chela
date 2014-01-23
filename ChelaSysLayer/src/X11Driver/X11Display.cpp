#include <string.h>
#include "X11Driver.hpp"
#include "GlxContext.hpp"

namespace X11Driver
{
    // The X11 display
    X11Display::X11Display()
    {
        display = NULL;
        
        // Initialize the key map.
#define MAP_KEY(from,to) keyMap.insert(std::make_pair(from, to))
        MAP_KEY(XK_Escape, KeyCode::Escape);
        MAP_KEY(XK_1, KeyCode::K1);
        MAP_KEY(XK_2, KeyCode::K2);
        MAP_KEY(XK_3, KeyCode::K3);
        MAP_KEY(XK_4, KeyCode::K4);
        MAP_KEY(XK_5, KeyCode::K5);
        MAP_KEY(XK_6, KeyCode::K6);
        MAP_KEY(XK_7, KeyCode::K7);
        MAP_KEY(XK_8, KeyCode::K8);
        MAP_KEY(XK_9, KeyCode::K9);
        
        MAP_KEY(XK_minus, KeyCode::Minus);
        MAP_KEY(XK_equal, KeyCode::Equals);
        MAP_KEY(XK_BackSpace, KeyCode::Back);
        MAP_KEY(XK_Tab, KeyCode::Tab);
        
        MAP_KEY(XK_q, KeyCode::Q);
        MAP_KEY(XK_w, KeyCode::W);
        MAP_KEY(XK_e, KeyCode::E);
        MAP_KEY(XK_r, KeyCode::R);
        MAP_KEY(XK_t, KeyCode::T);
        MAP_KEY(XK_y, KeyCode::Y);
        MAP_KEY(XK_u, KeyCode::U);
        MAP_KEY(XK_i, KeyCode::I);
        MAP_KEY(XK_o, KeyCode::O);
        MAP_KEY(XK_p, KeyCode::P);
        MAP_KEY(XK_a, KeyCode::A);
        MAP_KEY(XK_s, KeyCode::S);
        MAP_KEY(XK_d, KeyCode::D);
        MAP_KEY(XK_f, KeyCode::F);
        MAP_KEY(XK_g, KeyCode::G);
        MAP_KEY(XK_h, KeyCode::H);
        MAP_KEY(XK_j, KeyCode::J);
        MAP_KEY(XK_k, KeyCode::K);
        MAP_KEY(XK_l, KeyCode::L);
        MAP_KEY(XK_z, KeyCode::Z);
        MAP_KEY(XK_x, KeyCode::X);
        MAP_KEY(XK_c, KeyCode::C);
        MAP_KEY(XK_v, KeyCode::V);
        MAP_KEY(XK_b, KeyCode::B);
        MAP_KEY(XK_n, KeyCode::N);
        MAP_KEY(XK_m, KeyCode::M);
                
        MAP_KEY(XK_bracketleft, KeyCode::LBracket);
        MAP_KEY(XK_bracketright, KeyCode::RBracket);
        MAP_KEY(XK_Return, KeyCode::Return);

        MAP_KEY(XK_semicolon, KeyCode::Semicolon);
        MAP_KEY(XK_apostrophe, KeyCode::Apostrophe);
        MAP_KEY(XK_backslash, KeyCode::Backslash);
        MAP_KEY(XK_grave, KeyCode::Grave);
        MAP_KEY(XK_colon, KeyCode::Colon);
        
        MAP_KEY(XK_comma, KeyCode::Comma);
        MAP_KEY(XK_period, KeyCode::Period);
        MAP_KEY(XK_slash, KeyCode::Slash);
        MAP_KEY(XK_multiply, KeyCode::Multiply);
        MAP_KEY(XK_space, KeyCode::Space);
        
        MAP_KEY(XK_F1, KeyCode::F1);
        MAP_KEY(XK_F2, KeyCode::F2);
        MAP_KEY(XK_F3, KeyCode::F3);
        MAP_KEY(XK_F4, KeyCode::F4);
        MAP_KEY(XK_F5, KeyCode::F5);
        MAP_KEY(XK_F6, KeyCode::F6);
        MAP_KEY(XK_F7, KeyCode::F7);
        MAP_KEY(XK_F8, KeyCode::F8);
        MAP_KEY(XK_F9, KeyCode::F9);
        MAP_KEY(XK_F10, KeyCode::F10);
        MAP_KEY(XK_F11, KeyCode::F11);
        MAP_KEY(XK_F12, KeyCode::F12);
        MAP_KEY(XK_F13, KeyCode::F13);
        MAP_KEY(XK_F14, KeyCode::F14);
        MAP_KEY(XK_F15, KeyCode::F15);        

        MAP_KEY(XK_Shift_L, KeyCode::LShift);
        MAP_KEY(XK_Shift_R, KeyCode::RShift);
        MAP_KEY(XK_Control_L, KeyCode::LControl);
        MAP_KEY(XK_Control_R, KeyCode::RControl);
        MAP_KEY(XK_Alt_L, KeyCode::LMenu);
        MAP_KEY(XK_Alt_R, KeyCode::RMenu);
        
        MAP_KEY(XK_Caps_Lock, KeyCode::Capital);                
        MAP_KEY(XK_Num_Lock, KeyCode::Numlock);
        MAP_KEY(XK_Scroll_Lock, KeyCode::Scroll);
        
        // Keypad
        MAP_KEY(XK_KP_0, KeyCode::Numpad0);
        MAP_KEY(XK_KP_1, KeyCode::Numpad1);
        MAP_KEY(XK_KP_2, KeyCode::Numpad2);
        MAP_KEY(XK_KP_3, KeyCode::Numpad3);
        MAP_KEY(XK_KP_4, KeyCode::Numpad4);
        MAP_KEY(XK_KP_5, KeyCode::Numpad5);
        MAP_KEY(XK_KP_6, KeyCode::Numpad6);
        MAP_KEY(XK_KP_7, KeyCode::Numpad7);
        MAP_KEY(XK_KP_8, KeyCode::Numpad8);
        MAP_KEY(XK_KP_9, KeyCode::Numpad9);
        MAP_KEY(XK_KP_Subtract, KeyCode::Subtract);
        MAP_KEY(XK_KP_Add, KeyCode::Add);
        MAP_KEY(XK_KP_Decimal, KeyCode::Decimal);
        MAP_KEY(XK_KP_Multiply, KeyCode::Multiply);
        MAP_KEY(XK_KP_Divide, KeyCode::Divide);
        MAP_KEY(XK_KP_Enter, KeyCode::NumpadEnter);
        
        // Keypad without numlock
        MAP_KEY(XK_KP_Home, KeyCode::Numpad7);
        MAP_KEY(XK_KP_Up, KeyCode::Numpad8);
        MAP_KEY(XK_KP_Page_Up, KeyCode::Numpad9);
        MAP_KEY(XK_KP_Left, KeyCode::Numpad4);
        MAP_KEY(XK_KP_Begin, KeyCode::Numpad5);
        MAP_KEY(XK_KP_Right, KeyCode::Numpad6);
        MAP_KEY(XK_KP_End, KeyCode::Numpad1);
        MAP_KEY(XK_KP_Down, KeyCode::Numpad2);
        MAP_KEY(XK_KP_Page_Down, KeyCode::Numpad3);
        MAP_KEY(XK_KP_Insert, KeyCode::Numpad0);
        MAP_KEY(XK_KP_Delete, KeyCode::Decimal);
        
        MAP_KEY(XK_Up, KeyCode::Up);
        MAP_KEY(XK_Left, KeyCode::Left);
        MAP_KEY(XK_Right, KeyCode::Right);
        MAP_KEY(XK_Down, KeyCode::Down);
        
        MAP_KEY(XK_Print, KeyCode::SysRq);
        MAP_KEY(XK_Pause, KeyCode::Pause);
        MAP_KEY(XK_Home, KeyCode::Home);
        MAP_KEY(XK_Prior, KeyCode::Prior);
        MAP_KEY(XK_End, KeyCode::End);
        MAP_KEY(XK_Next, KeyCode::Next);
        MAP_KEY(XK_Insert, KeyCode::Insert);
        MAP_KEY(XK_Delete, KeyCode::Delete);
        
        MAP_KEY(XK_Super_L, KeyCode::LWin);
        MAP_KEY(XK_Super_R, KeyCode::RWin);
        MAP_KEY(XK_Menu, KeyCode::Apps);
#undef MAP_KEY
    }

    X11Display::~X11Display()
    {
        // Delete the screens.
        for(size_t i = 0; i < screens.size(); ++i)
            delete screens[i];

        // Close the display.
        if(display)
            XCloseDisplay(display);
    }

    void X11Display::Open()
    {
        // Only open the display if necessary.
        if(display)
            return;

        display = XOpenDisplay(NULL);
        
        // Add the default screen.
        screens.push_back(new X11Screen(*this, DefaultScreen(display)));
        
        // Create a context to associate window data.
        windowContext = XUniqueContext();

        // Get some common atoms.
        wmDelete = XInternAtom(display, "WM_DELETE_WINDOW", True);
        wmProtocols = XInternAtom(display, "WM_PROTOCOLS", True);
        netWmName = XInternAtom(display, "_NET_WM_NAME", True);
        utf8StringAtom = XInternAtom(display, "UTF8_STRING", True);
    }

    int X11Display::GetScreenCount()
    {
        if(!display) Open();
        return screens.size();
    }

    int X11Display::GetDefaultScreenIndex()
    {
        return 0;
    }

    GuiScreen *X11Display::GetScreen(int index)
    {
        if(!display) Open();
        if(index >= 0 && size_t(index) < screens.size())
            return screens[index];
        return NULL;
    }

    GuiWindow *X11Display::CreateSimpleWindow(GuiWindow *parent, int x, int y,
                                    int w, int h, int borderWidth, int border, int bg)
    {
        return CreateRenderWindow(NULL, parent, x, y, w, h, borderWidth, border, bg);
    }

    GuiWindow *X11Display::CreateRenderWindow(GuiRenderContext *context, GuiWindow *parent, int x, int y,
                    int w, int h, int borderWidth, int border, int bg)
    {
        // Cast the parent window.
        X11Window *p = static_cast<X11Window*> (parent);

        // Select the visual.
        int depth = CopyFromParent;
        int screenNum = p->GetScreenNum();
        Visual *visual = DefaultVisual(display, p->GetScreenNum());
        if(context != NULL)
        {
            GlxContext *glCtx = static_cast<GlxContext*> (context);
            XVisualInfo *visualInfo = glCtx->GetVisualInfo();
            visual = visualInfo->visual;
            depth = visualInfo->depth;
            screenNum = visualInfo->screen;
        }

        // Create the window colormap.
        Colormap cmap = XCreateColormap(display, RootWindow(display, screenNum), visual, AllocNone);

        // Window attributes
        XSetWindowAttributes wattr;
        wattr.colormap = cmap;
        wattr.event_mask = StructureNotifyMask | ExposureMask |
            KeyPressMask | KeyReleaseMask |
            ButtonPressMask | ButtonReleaseMask |
            EnterWindowMask | LeaveWindowMask |
            FocusChangeMask | PointerMotionMask;
        wattr.border_pixel = BlackPixel(display, DefaultScreen(display));
        wattr.background_pixel = 0;
        wattr.background_pixmap = None;

        // Create the X11 window.
        Window window = XCreateWindow(display, p->GetWindow(),
                                      x, y, w, h, borderWidth,
                                      depth, InputOutput, visual,
                                      CWColormap | CWEventMask|
                                      CWBorderPixel | CWBackPixel,
                                      &wattr);

        // Remove the window background.
        XSetWindowBackgroundPixmap(display, window, None);

        // Receive delete requests.
        if(p->IsRootWindow())
            XSetWMProtocols(display, window, &wmDelete, 1);

        // Wrap the window.
        X11Window *ret = new X11Window(*this, window, p->GetScreenNum(), x, y, w, h, bg);

        // No top-level windows are visible by default.
        if(!p->IsRootWindow())
            ret->Show();

        return ret;
    }

    GuiRenderContext *X11Display::CreateRenderContext(const char *systemName, GuiScreen *screen, int numattributes, int *attributes)
    {
        if(!strcmp(systemName, "OpenGL"))
            return GlxContext::Create(static_cast<X11Screen*> (screen), numattributes, attributes);
        return NULL;
    }

    X11Window *X11Display::GetWindow(Window id) const
    {
        XPointer res;
        if(XFindContext(display, id, windowContext, &res) != 0)
            return NULL;
        return (X11Window*)res;
    }

    void X11Display::MapWindow(Window id, X11Window *window)
    {
        XSaveContext(display, id, windowContext, (XPointer)window);
    }

    void X11Display::UnmapWindow(Window id)
    {
        XDeleteContext(display, id, windowContext);
    }

    void X11Display::ReadKeyCode(XKeyEvent &xkey, KeyEvent &key, bool press)
    {
        KeySym sym;
        
        // Translate the character.
        if(press)
        {
            char string[16];
            char32_t string32[2];
            // TODO: Use the right call.
            XLookupString(&xkey, string, 16, &sym, NULL);
            
            // Convert the UTF-8 into UTF-32
            _Chela_DC_Utf8ToUtf32(string32, string);
            key.unicode = string32[0];
            
        }
        else
        {
            key.unicode = 0;
        }
        
        // Remove modifiers to get a clean key symbol.
		xkey.state &= ~(ShiftMask | LockMask);
		XLookupString(&xkey, NULL, 0, &sym, NULL);
        key.keySym = keyMap[sym];
    }

    inline int ReadMouseButton(int button)
    {
        return button;
    }

    inline int ReadModifiers(int state)
    {
        return 0;
    }

    bool X11Display::PollEvent(Event *ev)
    {
        // If there is not a pending event, just idle.
        if(!XPending(display))
        {
            ev->window = NULL;
            ev->type = Event::EVT_Idle;
            return true;
        }

        // Get the next X event
        XEvent e;
        XNextEvent(display, &e);

        // Read the common data.
        X11Window *myWindow = GetWindow(e.xany.window);
        ev->window = myWindow;
        ev->type = Event::EVT_Unknown;

        // Read the event
        switch(e.type)
        {
        case ConfigureNotify:
            ev->type = Event::EVT_Size;
            myWindow->OnConfigureNotify(e.xconfigure);
            break;
        case DestroyNotify:
            ev->type = Event::EVT_Destroy;
            myWindow->OnDestroyNotify();
            break;
        case ClientMessage:
            if(e.xclient.message_type == wmProtocols)
            {
                ev->type = Event::EVT_Close;
            }
            else
            {
                // Ignore the event, poll the next one.
                return PollEvent(ev);
            }
            break;
        case Expose:
            ev->type = Event::EVT_Expose;
            ev->expose.x = e.xexpose.x;
            ev->expose.y = e.xexpose.y;
            ev->expose.width = e.xexpose.width;
            ev->expose.height = e.xexpose.height;
            ev->expose.count = e.xexpose.count;
            break;
        case MapNotify:
            ev->type = Event::EVT_Show;
            break;
        case UnmapNotify:
            ev->type = Event::EVT_Hide;
            break;
        case FocusIn:
            ev->type = Event::EVT_GotFocus;
            break;
        case FocusOut:
            ev->type = Event::EVT_LostFocus;
            break;
        case EnterNotify:
            ev->type = Event::EVT_MouseEnter;
            break;
        case LeaveNotify:
            ev->type = Event::EVT_MouseLeave;
            break;
        case KeyPress:
            ev->type = Event::EVT_KeyPressed;
            ev->key.modifiers = ReadModifiers(e.xkey.state);
            ReadKeyCode(e.xkey, ev->key, true);
            break;
        case KeyRelease:
            ev->type = Event::EVT_KeyReleased;
            ev->key.modifiers = ReadModifiers(e.xkey.state);
            ReadKeyCode(e.xkey, ev->key, false);
            break;
        case MotionNotify:
            ev->type = Event::EVT_MouseMotion;
            ev->motion.x = e.xmotion.x;
            ev->motion.y = e.xmotion.y;
            ev->motion.modifiers = ReadModifiers(e.xmotion.state);
            break;
        case ButtonPress:
            ev->type = Event::EVT_ButtonPressed;
            ev->button.x = e.xbutton.x;
            ev->button.y = e.xbutton.y;
            ev->button.button = ReadMouseButton(e.xbutton.button);
            ev->button.modifiers = ReadModifiers(e.xbutton.state);
            break;
        case ButtonRelease:
            ev->type = Event::EVT_ButtonReleased;
            ev->button.x = e.xbutton.x;
            ev->button.y = e.xbutton.y;
            ev->button.button = ReadMouseButton(e.xbutton.button);
            ev->button.modifiers = ReadModifiers(e.xbutton.state);
            break;
        default:
            // Ignore the event, poll the next one.
            return PollEvent(ev);
        }

        return true;
    }
}
