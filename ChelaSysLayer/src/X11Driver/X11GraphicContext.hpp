#ifndef X11_GRAPHIC_CONTEXT_HPP
#define X11_GRAPHIC_CONTEXT_HPP

namespace X11Driver
{
    class X11GraphicContext: public GuiGraphicContext
    {
    public:
        X11GraphicContext(X11Display &context, X11Drawable *drawableWrapper);
        ~X11GraphicContext();

        virtual void BeginDrawing();
        virtual void EndDrawing();

        virtual void SetBackground(int color);
        virtual int GetBackground();

        virtual void SetForeground(int color);
        virtual int GetForeground();

        virtual void Clear();
        virtual void ClearRect(int x0, int y0, int x1, int y1);

        virtual void DrawPoint(int x, int y);
        virtual void DrawLine(int x0, int y0, int x1, int y1);
        virtual void DrawRectangle(int x0, int y0, int x1, int y1);
        virtual void DrawFillRectangle(int x0, int y0, int x1, int y1);
        virtual void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2);
        virtual void DrawFillTriangle(int x0, int y0, int x1, int y1, int x2, int y2);
        virtual void DrawHorizGradient(int x0, int y0, int x1, int y1, int start, int end);
        virtual void DrawVertGradient(int x0, int y0, int x1, int y1, int start, int end);

    private:
        X11Display &context;
        X11Drawable *drawableWrapper;
        Display *display;
        Drawable drawable;
        GC graphicContext;
        Colormap colormap;
        int backgroundColor;
        int foregroundColor;
    };
}

#endif //X11_GRAPHIC_CONTEXT_HPP
