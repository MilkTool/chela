#ifndef CHELAG_WIN32_DRAWABLE_HPP
#define CHELAG_WIN32_DRAWABLE_HPP

namespace Win32Gui
{
    class Drawable: public GuiDrawable
    {
    public:
        virtual bool IsWindow() const = 0;
        virtual bool IsBitmap() const = 0;
    };
}

#endif //CHELAG_WIN32_DRAWABLE_HPP

