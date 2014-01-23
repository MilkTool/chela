#ifndef CHELAG_WGL_CONTEXT_HPP
#define CHELAG_WGL_CONTEXT_HPP

namespace Win32Gui
{
    class WglContext: public GuiRenderContext
    {
    public:
        WglContext(Display *display, int pixelFormat, const PIXELFORMATDESCRIPTOR &pfd, HGLRC context);
        ~WglContext();

        int GetPixelFormat() const;
        const PIXELFORMATDESCRIPTOR *GetPixelFormatDesc() const;

        virtual bool MakeCurrent(GuiWindow *window);
        virtual void SwapBuffers();

        static WglContext *Create(Display *display, int numattributes, int *attributes);

    private:
        Display *display;
        int pixelFormat;
        PIXELFORMATDESCRIPTOR pixelFormatDesc;
        HGLRC context;
        HWND oldWindow;
        HDC oldDC;
    };
}

#endif //CHELAG_WGL_CONTEXT_HPP
