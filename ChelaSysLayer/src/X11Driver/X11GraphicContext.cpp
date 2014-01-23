#include "X11Driver.hpp"
#include "X11Drawable.hpp"
#include "X11GraphicContext.hpp"

namespace X11Driver
{
    X11GraphicContext::X11GraphicContext(X11Display &context, X11Drawable *drawableWrapper)
        : context(context), drawableWrapper(drawableWrapper),
          display(context.get()), drawable(drawableWrapper->GetDrawable())
    {
        graphicContext = XCreateGC(display, drawable, 0, 0);
        colormap = DefaultColormap(display, 0);
        backgroundColor = 0xFFFFFF;
        foregroundColor = 0x000000;
    }

    X11GraphicContext::~X11GraphicContext()
    {
    }

    void X11GraphicContext::BeginDrawing()
    {
    }

    void X11GraphicContext::EndDrawing()
    {
        // Send the message.
        //XFlush(display);
    }

    inline void DecodeColor(XColor *out, int color)
    {
        out->red = ((color >> 16) & 0xFF) << 8;
        out->green = ((color >> 8) & 0xFF) << 8;
        out->blue = (color & 0xFF) << 8;
        out->flags = DoRed | DoGreen | DoBlue;
    }

    void X11GraphicContext::SetBackground(int color)
    {
        // Parse the color.
        XColor col;
        DecodeColor(&col, color);
        XAllocColor(display, colormap, &col);

        // Set the background.
        XSetBackground(display, graphicContext, col.pixel);

        // Store the color.
        backgroundColor = color;
    }

    int X11GraphicContext::GetBackground()
    {
        return backgroundColor;
    }

    void X11GraphicContext::SetForeground(int color)
    {
        // Parse the color.
        XColor col;
        DecodeColor(&col, color);
        XAllocColor(display, colormap, &col);

        // Set the foreground.
        XSetForeground(display, graphicContext, col.pixel);

        // Store the color.
        foregroundColor = color;
    }

    int X11GraphicContext::GetForeground()
    {
        return foregroundColor;
    }

    void X11GraphicContext::Clear()
    {
        ClearRect(0, 0, drawableWrapper->GetWidth()+1, drawableWrapper->GetHeight()+1);
    }

    void X11GraphicContext::ClearRect(int x0, int y0, int x1, int y1)
    {
        // Get the clear background.
        int bg = backgroundColor;
        if(drawableWrapper->IsWindow())
        {
            X11Window *window = static_cast<X11Window*> (drawableWrapper);
            bg = window->GetBackground();
        }

        // Store the old foreground color.
        int oldFg = foregroundColor;
        SetForeground(bg);

        // Draw a rectangle in the clearing area.
        XFillRectangle(display, drawable, graphicContext,
                        x0, y0, x1-x0, y1-y0);

        // Restore the background color.
        SetForeground(oldFg);
    }

    void X11GraphicContext::DrawPoint(int x, int y)
    {
        XDrawPoint(display, drawable, graphicContext, x, y);
    }

    void X11GraphicContext::DrawLine(int x0, int y0, int x1, int y1)
    {
        XDrawLine(display, drawable, graphicContext, x0, y0, x1, y1);
    }

    void X11GraphicContext::DrawRectangle(int x0, int y0, int x1, int y1)
    {
        XDrawRectangle(display, drawable, graphicContext,
                        x0, y0, x1-x0, y1-y0);
    }

    void X11GraphicContext::DrawFillRectangle(int x0, int y0, int x1, int y1)
    {
        XFillRectangle(display, drawable, graphicContext,
                        x0, y0, x1-x0, y1-y0);
    }

    void X11GraphicContext::DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2)
    {
        // Draw the three triangle sides.
        DrawLine(x0, y0, x1, y1);
        DrawLine(x0, y0, x2, y2);
        DrawLine(x1, y1, x2, y2);
    }

    void X11GraphicContext::DrawFillTriangle(int x0, int y0, int x1, int y1, int x2, int y2)
    {
        // Store the points in an array.
        XPoint points[3];
        points[0].x = x0;
        points[0].y = y0;
        points[1].x = x1;
        points[1].y = y1;
        points[2].x = x2;
        points[2].y = y2;

        // Draw the triangle.
        XFillPolygon(display, drawable, graphicContext, points, 3, Convex, CoordModeOrigin);
    }

    void X11GraphicContext::DrawHorizGradient(int x0, int y0, int x1, int y1, int start, int end)
    {
        // Decode the colors.
        XColor s, e;
        XColor cur;
        DecodeColor(&s, start);
        DecodeColor(&e, end);
        cur.flags = DoRed | DoGreen | DoBlue;
        
        // Write each line.
        int lines = x1 - x0;
        for(int i = 0; i <= lines; ++i)
        {
            // Compute the line color.
            cur.red = (lines*e.red + i*(s.red - e.red))/lines;
            cur.green = (lines*e.green + i*(s.green - e.green))/lines;
            cur.blue = (lines*e.blue + i*(s.blue - e.blue))/lines;
            
            // Set the foreground.
            XAllocColor(display, colormap, &cur);
            XSetForeground(display, graphicContext, cur.pixel);
            
            // Draw the line.
            int x = x0 + i;
            XDrawLine(display, drawable, graphicContext, x, y0, x, y1);
        }
        
        // Restore the foreground.
        SetForeground(foregroundColor);
    }

    void X11GraphicContext::DrawVertGradient(int x0, int y0, int x1, int y1, int start, int end)
    {
        // Decode the colors.
        XColor s, e;
        XColor cur;
        DecodeColor(&s, start);
        DecodeColor(&e, end);
        cur.flags = DoRed | DoGreen | DoBlue;
        
        // Write each line.
        int lines = y1 - y0;
        for(int i = 0; i <= lines; ++i)
        {
            // Compute the line color.
            cur.red = (lines*s.red + i*(e.red - s.red))/lines;
            cur.green = (lines*s.green + i*(e.green - s.green))/lines;
            cur.blue = (lines*s.blue + i*(e.blue - s.blue))/lines;
            
            // Set the foreground.
            XAllocColor(display, colormap, &cur);
            XSetForeground(display, graphicContext, cur.pixel);
            
            // Draw the line.
            int y = y0 + i;
            XDrawLine(display, drawable, graphicContext, x0, y, x1, y);
        }
        
        // Restore the foreground.
        SetForeground(foregroundColor);
    }
}
