#include "SysLayer.hpp"
#include "Driver.hpp"
#include "Window.hpp"
#include "GraphicContext.hpp"

namespace Win32Gui
{
    inline COLORREF ConvertColor(int color)
    {
        return ((color >> 16) & 0xFF) |
                (color & 0xFF00) |
               ((color & 0xFF) << 16);
    }

    inline void DecodeColor(int color, int *red, int *blue, int *green)
    {
        *red = (color >> 16) & 0xFF;
        *green = (color >> 8) & 0xFF;
        *blue = color & 0xFF;
    }

    GraphicContext::GraphicContext(Drawable *drawable, bool painting)
        : drawable(drawable), painting(painting)
    {
        if(painting)
        {
            // Make sure the drawable is a window.
            if(!drawable->IsWindow())
                _Chela_Sys_Error(SLE_INVALID_OPERATION, "Cannot create painting GC for now window drawable.");

            // The device context is obtained and relesedin the BeginDrawing/EndDrawing pair.
            dc = NULL;
        }
        else
        {
            // Get the window DC.
            if(drawable->IsWindow())
            {
                Window *window = static_cast<Window*> (drawable);
                dc = GetDC(window->GetHandle());
            }
        }

        // Use a default background and foregound.
        foregroundColor = 0x00000000;
        backgroundColor = 0x00FFFFFF;

        // Create the pen and the brush.
        pen = CreatePen(PS_SOLID, 1, 0);
        brush = CreateSolidBrush(RGB(255,255,255));

        // Use the pen and brush.
        if(dc)
        {
            SelectObject(dc, pen);
            SelectObject(dc, brush);
        }
    }

    GraphicContext::~GraphicContext()
    {
        // Release window dc.
        if(drawable->IsWindow())
        {
            Window *window = static_cast<Window*> (drawable);
            ReleaseDC(window->GetHandle(), dc);
        }

        // Delete memory dc.
        if(drawable->IsBitmap())
            DeleteDC(dc);

        // Delete the pend and brush.
        DeleteObject(pen);
        DeleteObject(brush);
    }

    void GraphicContext::BeginDrawing()
    {
        // Only paint dc needs initialization.
        if(!painting)
            return;

        // Sanity checks.
        if(dc)
            _Chela_Sys_Error(SLE_INVALID_OPERATION, "Cannot use BeginDrawing multiple times");

        // Get the window handle.
        Window *window = static_cast<Window*> (drawable);
        HWND hwnd = window->GetHandle();

        // Begin painting.
        BeginPaint(hwnd, &paintStruct);

        // Use the paint dc.
        dc = paintStruct.hdc;

        // Set the stock pen and brush
        SelectObject(dc, pen);
        SelectObject(dc, brush);

        // Set the pen and brush colors.
        SetDCPenColor(dc, ConvertColor(foregroundColor));
        SetDCBrushColor(dc, ConvertColor(backgroundColor));
    }

    void GraphicContext::EndDrawing()
    {
        // This is only used by painting.
        if(!painting)
            return;

        // Sanity checks.
        if(dc == NULL)
            _Chela_Sys_Error(SLE_INVALID_OPERATION, "EndDrawing must be called after BeginDrawing");

        // Get the window handle.
        Window *window = static_cast<Window*> (drawable);
        HWND hwnd = window->GetHandle();

        // End painting.
        EndPaint(hwnd, &paintStruct);

        // Set the device context to null.
        dc = NULL;
    }

    void GraphicContext::SetBackground(int color)
    {
        backgroundColor = color;
        if(dc)
            SetDCBrushColor(dc, ConvertColor(backgroundColor));
    }

    int GraphicContext::GetBackground()
    {
        return backgroundColor;
    }

    void GraphicContext::SetForeground(int color)
    {
        foregroundColor = color;
        if(dc)
            SetDCPenColor(dc, ConvertColor(foregroundColor));
    }

    int GraphicContext::GetForeground()
    {
        return foregroundColor;
    }

    void GraphicContext::Clear()
    {
        ClearRect(0, 0, drawable->GetWidth()+1, drawable->GetHeight()+1);
    }

    void GraphicContext::ClearRect(int x0, int y0, int x1, int y1)
    {
        // Get the drawable background color.
        int dBg = backgroundColor;
        if(drawable->IsWindow())
        {
            Window *window = static_cast<Window*> (drawable);
            dBg = window->GetBackground();
        }

        SetDCBrushColor(dc, ConvertColor(dBg));
        DrawFillRectangle(x0, y0, x1, y1);
        SetDCBrushColor(dc, ConvertColor(backgroundColor));
    }

    void GraphicContext::DrawPoint(int x, int y)
    {
        SetPixel(dc, x, y, ConvertColor(foregroundColor));
    }

    void GraphicContext::DrawLine(int x0, int y0, int x1, int y1)
    {
        MoveToEx(dc, x0, y0, NULL);
        LineTo(dc, x1, y1);
    }

    void GraphicContext::DrawRectangle(int x0, int y0, int x1, int y1)
    {
        MoveToEx(dc, x0, y0, NULL);
        LineTo(dc, x1, y0);
        LineTo(dc, x1, y1);
        LineTo(dc, x0, y1);
        LineTo(dc, x0, y0);
    }

    void GraphicContext::DrawFillRectangle(int x0, int y0, int x1, int y1)
    {
        RECT rect;
        rect.left = x0;
        rect.top = y0;
        rect.right = x1;
        rect.bottom = y1;
        FillRect(dc, &rect, brush);
    }

    void GraphicContext::DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2)
    {
        MoveToEx(dc, x0, y0, NULL);
        LineTo(dc, x1, y1);
        LineTo(dc, x2, y2);
        LineTo(dc, x0, y0);
    }

    void GraphicContext::DrawFillTriangle(int x0, int y0, int x1, int y1, int x2, int y2)
    {
        POINT points[] = {
            {x0, y0},
            {x1, y1},
            {x2, y2},
        };

        SelectObject(dc, GetStockObject(NULL_PEN));
        Polygon(dc, points, 3);
        SelectObject(dc, pen);
    }

    void GraphicContext::DrawHorizGradient(int x0, int y0, int x1, int y1, int start, int end)
    {
        // Decode colors.
        int sr, sg, sb;
        int er, eg, eb;
        DecodeColor(start, &sr, &sg, &sb);
        DecodeColor(start, &er, &eg, &eb);

        // Create the vertices.
        TRIVERTEX vertices[] = {
            {x0, y0, sr<<8, sg<<8, sb<<8, 0xFF00},
            {x1, y1, er<<8, eg<<8, eb<<8, 0xFF00},
        };

        // Create the rectangle indices
        GRADIENT_RECT rect = {0, 1};

        // Draw the gradient.
        GradientFill(dc, vertices, 2, &rect, 1, GRADIENT_FILL_RECT_H);
    }

    void GraphicContext::DrawVertGradient(int x0, int y0, int x1, int y1, int start, int end)
    {
        // Decode colors.
        int sr, sg, sb;
        int er, eg, eb;
        DecodeColor(start, &sr, &sg, &sb);
        DecodeColor(end, &er, &eg, &eb);

        // Create the vertices.
        TRIVERTEX vertices[] = {
            {x0, y0, sr<<8, sg<<8, sb<<8, 0xFF00},
            {x1, y1, er<<8, eg<<8, eb<<8, 0xFF00},
        };

        // Create the rectangle indices
        GRADIENT_RECT rect = {0, 1};

        // Draw the gradient.
        GradientFill(dc, vertices, 2, &rect, 1, GRADIENT_FILL_RECT_V);
    }
}

