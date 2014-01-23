#ifndef CHELAG_WIN32_GRAPHICCONTEXT_HPP
#define CHELAG_WIN32_GRAPHICCONTEXT_HPP

namespace Win32Gui
{
    class GraphicContext: public GuiGraphicContext
    {
    public:
        GraphicContext(Drawable *drawable, bool painting);
        ~GraphicContext();

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
        Drawable *drawable;
        bool painting;
        HDC dc;
        PAINTSTRUCT paintStruct;
        int backgroundColor;
        int foregroundColor;
        HPEN pen;
        HBRUSH brush;
    };
}

#endif //CHELAG_WIN32_GRAPHICCONTEXT_HPP

