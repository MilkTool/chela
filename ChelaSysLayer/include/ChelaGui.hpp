#ifndef CHELA_GUI_HPP
#define CHELA_GUI_HPP

#include "SysLayer.hpp"

namespace ChelaGui
{
    class GuiDisplay;
    class GuiScreen;
    class GuiRenderContext;
    class GuiWindow;
    union Event;

    // Make sure the virtual destructors are available.
    class GuiObject
    {
    public:
        virtual ~GuiObject() {};
    };

    // The gui driver interface.
    class GuiDriver: public GuiObject
    {
    public:
        virtual GuiDisplay *GetDisplay() = 0;
    };

    // The gui display interface.
    class GuiDisplay: public GuiObject
    {
    public:
        virtual int GetScreenCount() = 0;
        virtual int GetDefaultScreenIndex() = 0;
        virtual GuiScreen *GetScreen(int index) = 0;
        virtual GuiWindow *CreateSimpleWindow(GuiWindow *parent, int x, int y,
                int w, int h, int borderWidth, int border, int bg) = 0;
        virtual GuiWindow *CreateRenderWindow(GuiRenderContext *context, GuiWindow *parent, int x, int y,
                int w, int h, int borderWidth, int border, int bg) = 0;
        virtual GuiRenderContext *CreateRenderContext(const char *systemName, GuiScreen *screen, int numattributes, int *attributes) = 0;

        virtual bool PollEvent(Event *ev) = 0;
    };

    // The gui screen interface
    class GuiScreen: public GuiObject
    {
    public:
        virtual int GetWidth() const = 0;
        virtual int GetHeight() const = 0;

        virtual GuiWindow *GetRootWindow() = 0;
    };

    // The drawable interface.
    class GuiDrawable: public GuiObject
    {
    public:
        virtual int GetHeight() const = 0;
        virtual int GetWidth() const = 0;
    };

    // The graphic context interface
    class GuiGraphicContext: public GuiObject
    {
    public:
        virtual void BeginDrawing() = 0;
        virtual void EndDrawing() = 0;

        virtual void SetBackground(int color) = 0;
        virtual int GetBackground() = 0;

        virtual void SetForeground(int color) = 0;
        virtual int GetForeground() = 0;

        virtual void Clear() = 0;
        virtual void ClearRect(int x0, int y0, int x1, int y1) = 0;

        virtual void DrawPoint(int x, int y) = 0;
        virtual void DrawLine(int x0, int y0, int x1, int y1) = 0;
        virtual void DrawRectangle(int x0, int y0, int x1, int y1) = 0;
        virtual void DrawFillRectangle(int x0, int y0, int x1, int y1) = 0;
        virtual void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2) = 0;
        virtual void DrawFillTriangle(int x0, int y0, int x1, int y1, int x2, int y2) = 0;
        virtual void DrawHorizGradient(int x0, int y0, int x1, int y1, int start, int end) = 0;
        virtual void DrawVertGradient(int x0, int y0, int x1, int y1, int start, int end) = 0;
    };

    // The gui window interface
    class GuiWindow: public virtual GuiDrawable
    {
    public:
        virtual void Show() = 0;
        virtual void Hide() = 0;
        
        virtual void CaptureMouse() = 0;
        virtual void ReleaseMouse() = 0;
        
        virtual bool HasFocus() = 0;
        virtual void SetFocus() = 0;
        
        virtual bool IsInputEnabled() = 0;
        virtual void EnableInput() = 0;
        virtual void DisableInput() = 0;

        virtual void Refresh() = 0;

        virtual const char16_t *GetStringAttribute(const char *name) = 0;
        virtual void SetStringAttribute(const char *name, int attrSize, const char16_t *attr) = 0;

        virtual GuiGraphicContext *CreateGraphicContext(bool painting) = 0;
    };

    // The gui render context.
    class GuiRenderContext
    {
    public:
        virtual bool MakeCurrent(GuiWindow *window) = 0;
        virtual void SwapBuffers() = 0;
    };

    // Event structure
    struct AnyEvent
    {
        GuiWindow *window;
        int type;
    };

    struct KeyEvent
    {
        // Common data.
        GuiWindow *window;
        int type;
        int modifiers;
        int keySym;
        int unicode;
    };

    struct ButtonEvent
    {
        // Common data.
        GuiWindow *window;
        int type;
        int x, y;
        int modifiers;
        int button;
    };

    struct MotionEvent
    {
        // Common data.
        GuiWindow *window;
        int type;
        int x, y;
        int modifiers;
    };

    struct ExposeEvent
    {
        // Common data.
        GuiWindow *window;
        int type;

        // Expose data
        int x, y;
        int width, height;
        int count;
    };

    namespace RenderAttr
    {
        enum Attr
        {
            RAttr_DoubleBuffer = 1,
            RAttr_Indexed = 2,
            RAttr_RGB = 3,
            RAttr_RGBA = 4,
            RAttr_RedSize = 5,
            RAttr_GreenSize = 6,
            RAttr_BlueSize = 7,
            RAttr_AlphaSize = 8,
            RAttr_DepthSize = 9,
            RAttr_StencilSize = 10,
            RAttr_Window = 11,
            RAttr_Pixmap = 12,
        };
    }

    namespace KeyCode
    {
        enum KC
        {
            Escape = 0x01,
            K1 = 0x02,
            K2 = 0x03,
            K3 = 0x04,
            K4 = 0x05,
            K5 = 0x06,
            K6 = 0x07,
            K7 = 0x08,
            K8 = 0x09,
            K9 = 0x0A,
            K0 = 0x0B,
            Minus = 0x0C,
            Equals = 0x0D,
            Back = 0x0E,
            Tab = 0x0F,
            Q = 0x10,
            W = 0x11,
            E = 0x12,
            R = 0x13,
            T = 0x14,
            Y = 0x15,
            U = 0x16,
            I = 0x17,
            O = 0x18,
            P = 0x19,
            LBracket = 0x1A,
            RBracket = 0x1B,
            Return = 0x1C,
            LControl = 0x1D,
            A = 0x1E,
            S = 0x1F,
            D = 0x20,
            F = 0x21,
            G = 0x22,
            H = 0x23,
            J = 0x24,
            K = 0x25,
            L = 0x26,
            Semicolon = 0x27,
            Apostrophe = 0x28,
            Grave = 0x29,
            LShift = 0x2A,
            Backslash = 0x2B,
            Z = 0x2C,
            X = 0x2D,
            C = 0x2E,
            V = 0x2F,
            B = 0x30,
            N = 0x31,
            M = 0x32,
            Comma = 0x33,
            Period = 0x34,
            Slash = 0x35,
            RShift = 0x36,
            Multiply = 0x37,
            LMenu = 0x38,
            Space = 0x39,
            Capital = 0x3A,
            F1 = 0x3B,
            F2 = 0x3C,
            F3 = 0x3D,
            F4 = 0x3E,
            F5 = 0x3F,
            F6 = 0x40,
            F7 = 0x41,
            F8 = 0x42,
            F9 = 0x43,
            F10 = 0x44,
            Numlock = 0x45,
            Scroll = 0x46,
            Numpad7 = 0x47,
            Numpad8 = 0x48,
            Numpad9 = 0x49,
            Subtract = 0x4A,
            Numpad4 = 0x4B,
            Numpad5 = 0x4C,
            Numpad6 = 0x4D,
            Add = 0x4E,
            Numpad1 = 0x4F,
            Numpad2 = 0x50,
            Numpad3 = 0x51,
            Numpad0 = 0x52,
            Decimal = 0x53,
            Oem102 = 0x56,
            F11 = 0x57,
            F12 = 0x58,
            F13 = 0x64,
            F14 = 0x65,
            F15 = 0x66,
            Kana = 0x70,
            AbntC1 = 0x73,
            Convert = 0x79,
            Noconvert = 0x7B,
            Yen = 0x7D,
            AbntC2 = 0x7E,
            NumpadEquals = 0x8D,
            Circumflex = 0x90,
            At = 0x91,
            Colon = 0x92,
            Underline = 0x93,
            Kanji = 0x94,
            Stop = 0x95,
            Ax = 0x96,
            Unlabeled = 0x97,
            Nexttrack = 0x99,
            NumpadEnter = 0x9C,
            RControl = 0x9D,
            Mute = 0xA0,
            Calculator = 0xA1,
            PlayPause = 0xA2,
            MediaStop = 0xA4,
            VolumeDown = 0xAE,
            VolumeUp = 0xB0,
            WebHome = 0xB2,
            NumpadComma = 0xB3,
            Divide = 0xB5,
            SysRq = 0xB7,
            RMenu = 0xB8,
            Pause = 0xC5,
            Home = 0xC7,
            Up = 0xC8,
            Prior = 0xC9,
            Left = 0xCB,
            Right = 0xCD,
            End = 0xCF,
            Down = 0xD0,
            Next = 0xD1,
            Insert = 0xD2,
            Delete = 0xD3,
            LWin = 0xDB,
            RWin = 0xDC,
            Apps = 0xDD,
            Power = 0xDE,
            Sleep = 0xDF,
            Wake = 0xE3,
            WebSearch = 0xE5,
            WebFavorites = 0xE6,
            WebRefresh = 0xE7,
            WebStop = 0xE8,
            WebForward = 0xE9,
            WebBack = 0xEA,
            MyComputer = 0xEB,
            Mail = 0xEC,
            Mediaselect = 0xED,
        };
    }

    namespace MouseButtonID
    {
        enum ID
        {
            Any = 0,
            Left,
            Right,
            Middle,
            Aux1,
            Aux2
        };
    }
    
    union Event
    {
        enum EventType
        {
            EVT_Unknown=0,
            EVT_Show,
            EVT_Hide,
            EVT_Size,
            EVT_Expose,
            EVT_GotFocus,
            EVT_LostFocus,
            EVT_MouseEnter,
            EVT_MouseLeave,
            EVT_KeyPressed,
            EVT_KeyReleased,
            EVT_ButtonPressed,
            EVT_ButtonReleased,
            EVT_MouseMotion,
            EVT_Destroy,
            EVT_Close,
            EVT_Idle,
        };

        // Any event elements.
        struct {
            GuiWindow *window;
            int type;
        };

        AnyEvent any;
        KeyEvent key;
        ButtonEvent button;
        MotionEvent motion;
        ExposeEvent expose;
    };

#ifdef __unix__
    GuiDriver *CreateX11Driver();
#endif
#ifdef _WIN32
    GuiDriver *CreateWin32GuiDriver();
#endif

};

#endif //CHELA_GUI_HPP
