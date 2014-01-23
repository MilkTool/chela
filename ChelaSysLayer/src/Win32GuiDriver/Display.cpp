#include "SysLayer.hpp"
#include "Driver.hpp"
#include "Window.hpp"
#include "WglContext.hpp"
#include <windowsx.h>

namespace Win32Gui
{
    static const char *SimpleWindowClassName = "ChelaGuiWindow";
    static const char *RenderWindowClassName = "ChelaGuiRenderWindow";
    static Display *CurrentDisplay = NULL;

    // Window message processor.
    LRESULT CALLBACK SimpleWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
    {
        // Handle the message in the display the data into 
        return CurrentDisplay->ReceivedMessage(hwnd, msg, wParam, lParam);
    }

    Display::Display()
        : screen(this)
    {
        CurrentDisplay = this;
        currentEvent = NULL;

        // Get the current instance,
        HINSTANCE hInstance = GetModuleHandle(NULL);

        // Register the window class.
        WNDCLASSEX wc;
        wc.cbSize        = sizeof(WNDCLASSEX);
        wc.style         = 0;
        wc.lpfnWndProc   = SimpleWndProc;
        wc.cbClsExtra    = 0;
        wc.cbWndExtra    = 0;
        wc.hInstance     = hInstance;
        wc.hIcon         = LoadIcon(NULL, IDI_APPLICATION);
        wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
        wc.hbrBackground = NULL;
        wc.lpszMenuName  = NULL;
        wc.lpszClassName = SimpleWindowClassName;
        wc.hIconSm       = NULL;
        if(!RegisterClassEx(&wc))
            _Chela_Sys_Fatal("Failed to register window class");

        // Register the render window clss.
        wc.style |= CS_OWNDC;
        wc.lpszClassName = RenderWindowClassName;
        if(!RegisterClassEx(&wc))
            _Chela_Sys_Fatal("Failed to register render window class");

    }

    Display::~Display()
    {
    }

    int Display::GetScreenCount()
    {
        return 1;
    }

    int Display::GetDefaultScreenIndex()
    {
        return 0;
    }

    GuiScreen *Display::GetScreen(int index)
    {
        if(index == 0)
            return &screen;
        return NULL;
    }

    GuiWindow *Display::CreateSimpleWindow(GuiWindow *parent, int x, int y,
                int w, int h, int borderWidth, int border, int bg)
    {
        return CreateRenderWindow(NULL, parent, x, y, w, h, borderWidth, border, bg);
    }

    GuiWindow *Display::CreateRenderWindow(GuiRenderContext *context, GuiWindow *parent, int x, int y,
                int w, int h, int borderWidth, int border, int bg)
    {
        // Get the current instance,
        HINSTANCE hInstance = GetModuleHandle(NULL);

        // Window style depends if this is a top-level window or not.
        DWORD exStyle = 0;
        DWORD style = 0;
        if(border != 0)
            style = WS_BORDER;

        // Get the parent window.
        Window *parentWindow = static_cast<Window*> (parent);
        HWND parentHWnd = parentWindow->GetHandle();
        if(parentHWnd == GetDesktopWindow())
        {
            parentHWnd = NULL;
            style = WS_OVERLAPPEDWINDOW;
        }
        else
        {
            style = WS_CHILD | WS_VISIBLE;
        }

        // Set some styles required by render windows.
        if(context)
            style |= WS_CLIPCHILDREN | WS_CLIPSIBLINGS;


        // Select the window class.
        const char *windowClass = SimpleWindowClassName;
        if(context != NULL)
            windowClass = RenderWindowClassName;

        // Create the window.
        HWND hwnd = CreateWindowEx(
            exStyle,
            windowClass,
            "",
            style,
            x, y, w, h,
            parentHWnd,
            NULL,
            hInstance,
            NULL);
        if(!hwnd)
            return NULL;

        // Set the window pixel format.
        if(context)
        {
            // Cast the context.
            WglContext *wglContext = static_cast<WglContext*> (context);

            // Set the pixel format.
            HDC dc = GetDC(hwnd);
            SetPixelFormat(dc, wglContext->GetPixelFormat(), wglContext->GetPixelFormatDesc());
            ReleaseDC(hwnd, dc);
        }

        // Return the window object.
        return new Window(this, hwnd, x, y, w, h, bg);
    }

    GuiRenderContext *Display::CreateRenderContext(const char *systemName, GuiScreen *screen, int numattributes, int *attributes)
    {
        if(!strcmp(systemName, "OpenGL"))
            return WglContext::Create(this, numattributes, attributes);;
        return NULL;
    }

    Window *Display::GetWindow(HWND hwnd) const
    {
        return (Window*)GetWindowLongPtr(hwnd, GWLP_USERDATA);
    }

    void Display::MapWindow(HWND hwnd, Window *window)
    {
        SetWindowLongPtr(hwnd, GWLP_USERDATA, (LONG_PTR)window);
    }

    void Display::UnmapWindow(HWND hwnd)
    {
        SetWindowLongPtr(hwnd, GWLP_USERDATA, 0);
    }

    bool Display::PollEvent(Event *ev)
    {
        MSG msg;
        // Store the current event.
        currentEvent = ev;
        processedMessage = false;
        while(!processedMessage)
        {
            // Get the message.
            if(!PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
            {
                // Send an idle message.
                ev->window = NULL;
                ev->type = Event::EVT_Idle;
                currentEvent = NULL;
                return true;
            }

            // Handle quit message.
            if(msg.message == WM_QUIT)
            {
                currentEvent = NULL;
                return false;
            }

            // Translate and dispatch the message.
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
        currentEvent = NULL;

        return true;
    }

    inline int ReadMouseButton(UINT msg, WPARAM wParam)
    {
        int xbutton;

        switch(msg)
        {
        case WM_LBUTTONDOWN:
        case WM_LBUTTONUP:
            return MouseButtonID::Left;
        case WM_RBUTTONDOWN:
        case WM_RBUTTONUP:
            return MouseButtonID::Right;
        case WM_MBUTTONDOWN:
        case WM_MBUTTONUP:
            return MouseButtonID::Middle;
        case WM_XBUTTONDOWN:
        case WM_XBUTTONUP:
            xbutton = wParam >> 16;
            if(xbutton == XBUTTON1)
                return MouseButtonID::Aux1;
            else if(xbutton == XBUTTON2)
                return MouseButtonID::Aux2;
            else
                return MouseButtonID::Any;
        default:
            return MouseButtonID::Any;
        }
    }

    inline int ReadModifiers(int state)
    {
        return 0;
    }

    LRESULT Display::ReceivedMessage(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
    {
        // Get the window.
        Window *myWindow = GetWindow(hwnd);

        // If not polling, send to the default window proc.
        if(!currentEvent || !myWindow)
            return DefWindowProc(hwnd, msg, wParam, lParam);
        
        // Read the common data.
        Event *ev = currentEvent;
        bool &done = processedMessage;
        ev->window = myWindow;
        ev->type = Event::EVT_Unknown;

        // Handle the messages according to their type.
        switch(msg)
        {
        // Window destruction.
        case WM_DESTROY:
            {
                Window *window = CurrentDisplay->GetWindow(hwnd);
                if(window)
                    delete window;
                else
                    return DefWindowProc(hwnd, msg, wParam, lParam);
            }
            break;
        case WM_CLOSE:
            ev->type = Event::EVT_Close;
            done = true;
            break;
        case WM_ERASEBKGND:
            // Always return 1 for erase background, we want to manage
            // everything in the paint event
            return 1;
            break;
        case WM_PAINT:
            {
                // Get the update rect.
                RECT rect;
                GetUpdateRect(hwnd, &rect, false);
                
                // Set the event parameters.
                ev->type = Event::EVT_Expose;
                ev->expose.x = rect.left;
                ev->expose.y = rect.top;
                ev->expose.width = rect.right - rect.left;
                ev->expose.height = rect.bottom - rect.top;
                ev->expose.count = 0;
                done = true;
            }
            break;
        case WM_SIZE:
            ev->type = Event::EVT_Size;
            myWindow->UpdateSize(LOWORD(lParam), HIWORD(lParam));
            done = true;
            break;
        case WM_MOVE:
            myWindow->UpdatePosition(LOWORD(lParam), HIWORD(lParam));
            break;
        case WM_SETFOCUS:
            ev->type = Event::EVT_GotFocus;
            done = true;
            break;
        case WM_KILLFOCUS:
            ev->type = Event::EVT_LostFocus;
            done = true;
            break;
        case WM_MOUSEMOVE:
            {
                if(!myWindow->IsMouseOver())
                {
                    ev->type = Event::EVT_MouseEnter;
                    myWindow->SetMouseOver(true);

                    // Request mouse leave tracking.
                    TRACKMOUSEEVENT tracking;
                    memset(&tracking, 0, sizeof(tracking));
                    tracking.cbSize = sizeof(tracking);
                    tracking.dwFlags = TME_LEAVE;
                    tracking.hwndTrack = hwnd;
                    tracking.dwHoverTime = HOVER_DEFAULT;
                    TrackMouseEvent(&tracking);
                }
                else
                {
                    ev->type = Event::EVT_MouseMotion;
                    ev->motion.x = GET_X_LPARAM(lParam);
                    ev->motion.y = GET_Y_LPARAM(lParam);
                    ev->motion.modifiers = ReadModifiers(wParam);
                }
                done = true;
            }
            break;
        case WM_MOUSELEAVE:
            ev->type = Event::EVT_MouseLeave;
            myWindow->SetMouseOver(false);
            done = true;
            break;
        case WM_LBUTTONDOWN:
        case WM_RBUTTONDOWN:
        case WM_MBUTTONDOWN:
        case WM_XBUTTONDOWN:
            ev->type = Event::EVT_ButtonPressed;
            ev->button.x = GET_X_LPARAM(lParam);
            ev->button.y = GET_Y_LPARAM(lParam);
            ev->button.modifiers = ReadModifiers(wParam);
            ev->button.button = ReadMouseButton(msg, wParam);
            done = true;
            return TRUE;    
        case WM_LBUTTONUP:
        case WM_RBUTTONUP:
        case WM_MBUTTONUP:
        case WM_XBUTTONUP:
            ev->type = Event::EVT_ButtonReleased;
            ev->button.x = GET_X_LPARAM(lParam);
            ev->button.y = GET_Y_LPARAM(lParam);
            ev->button.modifiers = ReadModifiers(wParam);
            ev->button.button = ReadMouseButton(msg, wParam);
            done = true;
            break;
        // Some messages are required to be handled by the default window processor.
        default:
            return DefWindowProc(hwnd, msg, wParam, lParam);
        }

        return 0;
    }
}

