#include "Driver.hpp"
#include "WglContext.hpp"

namespace Win32Gui
{
    static const char *RenderWindowClassName = "ChelaGuiRenderWindow";

    WglContext::WglContext(Display *display, int pixelFormat, const PIXELFORMATDESCRIPTOR &pfd, HGLRC context)
        : display(display), pixelFormat(pixelFormat), pixelFormatDesc(pfd), context(context)
    {
        oldDC = NULL;
        oldWindow = NULL;
    }

    WglContext::~WglContext()
    {
        MakeCurrent(NULL);
        wglDeleteContext(context);
    }

    int WglContext::GetPixelFormat() const
    {
        return pixelFormat;
    }

    const PIXELFORMATDESCRIPTOR *WglContext::GetPixelFormatDesc() const
    {
        return &pixelFormatDesc;
    }

    bool WglContext::MakeCurrent(GuiWindow *window)
    {
        // Get the window device context.
        HDC newDC = NULL;
        HWND newWindow = NULL;
        if(window)
        {
            Window *impl = static_cast<Window*> (window);
            newWindow = impl->GetHandle();
            newDC = GetDC(newWindow);
        }

        // Make current.
        bool res = wglMakeCurrent(newDC, context);

        // Release the old device context.
        if(oldDC)
            ReleaseDC(oldWindow, oldDC);
        oldWindow = newWindow;
        oldDC = newDC;

        return res;
    }

    void WglContext::SwapBuffers()
    {
        if(oldDC)
            wglSwapLayerBuffers(oldDC, WGL_SWAP_MAIN_PLANE);
    }

    WglContext *WglContext::Create(Display *display, int numattributes, int *attributes)
    {
        // Create a dummy window.
        HWND dummy = CreateWindowEx(0,
                     RenderWindowClassName,
                    "Dummy",
                    0,   // style
                    0,   // x
                    0,   // y
                    2,   // width
                    2,   // height
                    NULL, // Parent
                    NULL, // Menu
                    NULL, // hInstance
                    NULL  // lpParam
                    );
        if(!dummy)
            return NULL;

        // Select the pixel format.
        PIXELFORMATDESCRIPTOR pfd;
        memset(&pfd, 0, sizeof(PIXELFORMATDESCRIPTOR));

        // Set default parameters.
        pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR);
        pfd.nVersion = 1;
        pfd.dwFlags = PFD_SUPPORT_OPENGL;
        pfd.iLayerType = PFD_MAIN_PLANE;
        
        // Parse the render attributes.
        for(int i = 0; i < numattributes; ++i)
        {
            switch(attributes[i])
            {
            case RenderAttr::RAttr_DoubleBuffer:
                pfd.dwFlags |= PFD_DOUBLEBUFFER;
                break;
            case RenderAttr::RAttr_Indexed:
                pfd.iPixelType = PFD_TYPE_COLORINDEX;
                break;
            case RenderAttr::RAttr_RGB: // Only rgb is not supported.
            case RenderAttr::RAttr_RGBA:
                pfd.iPixelType = PFD_TYPE_RGBA;
                break;
            case RenderAttr::RAttr_RedSize:
                pfd.cRedBits = attributes[++i];
                break;
            case RenderAttr::RAttr_GreenSize:
                pfd.cGreenBits = attributes[++i];
                break;
            case RenderAttr::RAttr_BlueSize:
                pfd.cBlueBits = attributes[++i];
                break;
            case RenderAttr::RAttr_AlphaSize:
                pfd.cAlphaBits = attributes[++i];
                break;
            case RenderAttr::RAttr_DepthSize:
                pfd.cDepthBits = attributes[++i];
                break;
            case RenderAttr::RAttr_StencilSize:
                pfd.cStencilBits = attributes[++i];
                break;
            case RenderAttr::RAttr_Window:
                pfd.dwFlags |= PFD_DRAW_TO_WINDOW;
                break;
            case RenderAttr::RAttr_Pixmap:
                pfd.dwFlags |= PFD_DRAW_TO_BITMAP;
                break;
            default:
                // Ignore the attribute.
                break;
            }
        }

        // Get the dummy DC and set the pixel format.
        HDC dc = GetDC(dummy);
        int pixelFormat = ChoosePixelFormat(dc, &pfd);
        if(pixelFormat == 0)
        {
            ReleaseDC(dummy, dc);
            DestroyWindow(dummy);
            return NULL;
        }

        // Set the pixel format.
        if(!SetPixelFormat(dc, pixelFormat, &pfd))
        {
            ReleaseDC(dummy, dc);
            DestroyWindow(dummy);
            return NULL;
        }

        // Create the context.
        HGLRC context = wglCreateContext(dc);
        
        // Release the device context and destroy the window, the fullfiled their purpose.
        ReleaseDC(dummy, dc);
        DestroyWindow(dummy);

        // Return the create context.
        return new WglContext(display, pixelFormat, pfd, context);
    }
}

