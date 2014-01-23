#ifndef CHELAG_GLX_CONTEXT_HPP
#define CHELAG_GLX_CONTEXT_HPP

#include <GL/glx.h>

namespace X11Driver
{
    class GlxContext: public GuiRenderContext
    {
    public:
        GlxContext(Display *display, XVisualInfo *visual, GLXContext context);
        ~GlxContext();

        XVisualInfo *GetVisualInfo() const;

        virtual bool MakeCurrent(GuiWindow *window);
        virtual void SwapBuffers();

        static GlxContext *Create(X11Screen *screen, int numattributes, int *attributes);

    private:
        Display *display;
        XVisualInfo *visualInfo;
        GLXContext context;
        X11Window *currentWindow;
    };
}

#endif //CHELAG_GLX_CONTEXT_HPP
