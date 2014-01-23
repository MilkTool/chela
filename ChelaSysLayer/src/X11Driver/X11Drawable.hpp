#ifndef CHELAG_X11_DRAWABLE_HPP
#define CHELAG_X11_DRAWABLE_HPP

namespace X11Driver
{
    class X11Drawable: public virtual GuiDrawable
    {
    public:
        virtual Drawable GetDrawable() const = 0;
        virtual bool IsWindow() const = 0;
        virtual bool IsBitmap() const = 0;
    };
}

#endif //CHELAG_X11_DRAWABLE_HPP
