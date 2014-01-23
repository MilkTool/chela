set(SystemDriver_SRC
    System/SystemApi.chela
    System/SystemDisplay.chela
    System/SystemDriver.chela
    System/SystemFont.chela
    System/SystemFontManager.chela
    System/SystemGraphicContext.chela
    System/SystemRenderContext.chela
    System/SystemScreen.chela
    System/SystemWindowContext.chela
)

set(GUI_SRC
    ${SystemDriver_SRC}
    ActivateEvent.chela
    BMPFormatProvider.chela
    ButtonEvent.chela
    Button.chela
    Canvas.chela
    Color.chela
    Display.chela
    Drawable.chela
    ExposeEvent.chela
    Font.chela
    FontManager.chela
    Frame.chela
    GraphicContext.chela
    GuiDriver.chela
    GuiEvent.chela
    GuiEventType.chela
    IdleEvent.chela
    Image.chela
    ImageFormatProvider.chela
    ImageUtil.chela
    KeyCode.chela
    KeyEvent.chela
    MotionEvent.chela
    MouseButton.chela
    Palette.chela
    PixelFormat.chela
    Rect.chela
    RenderAttribute.chela
    RenderContext.chela
    Screen.chela
    Sizer.chela
    Theme.chela
    UnsupportedImageFormatException.chela
    Window.chela
    WindowContext.chela
    Widget.chela
)

# Build the gui module.
chela_module(ChelaGui LIBRARY LIBS ChelaSysLayer SRC ${GUI_SRC})


