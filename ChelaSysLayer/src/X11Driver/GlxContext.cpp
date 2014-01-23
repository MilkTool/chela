#include "X11Driver.hpp"
#include "GlxContext.hpp"

namespace X11Driver
{
    GlxContext::GlxContext(Display *display, XVisualInfo *visual, GLXContext context)
        : display(display), visualInfo(visual), context(context)
    {
    }

    GlxContext::~GlxContext()
    {
        // Free the visual.
        XFree(visualInfo);

        // Release the current context.
        glXMakeCurrent(display, 0, NULL);

        // Destroy the context.
        glXDestroyContext(display, context);
    }

    XVisualInfo *GlxContext::GetVisualInfo() const
    {
        return visualInfo;
    }

    bool GlxContext::MakeCurrent(GuiWindow *window)
    {
        //printf("current window %p\n", window);
        currentWindow = static_cast<X11Window*> (window);
        if(currentWindow == NULL)
            return glXMakeCurrent(display, 0, NULL) == True;

        // Cast the window.
        return glXMakeCurrent(display, currentWindow->GetWindow(), context) == True;
    }

    void GlxContext::SwapBuffers()
    {
        if(currentWindow)
            glXSwapBuffers(display, currentWindow->GetWindow());
    }

    GlxContext *GlxContext::Create(X11Screen *screen, int numattributes, int *attributes)
    {
        std::vector<int> glattrs;
        int renderType = 0;
        int drawableType = 0;
        int contextRenderType = GLX_RGBA_TYPE;

        // Add basic attributes
        glattrs.push_back(GLX_X_RENDERABLE);
        glattrs.push_back(True);
        glattrs.push_back(GLX_X_VISUAL_TYPE);
        glattrs.push_back(GLX_TRUE_COLOR);

        // Parse the render attributes.
        for(int i = 0; i < numattributes; ++i)
        {
            switch(attributes[i])
            {
            case RenderAttr::RAttr_DoubleBuffer:
                glattrs.push_back(GLX_DOUBLEBUFFER);
                glattrs.push_back(True);
                break;
            case RenderAttr::RAttr_Indexed:
                renderType |= GLX_COLOR_INDEX_BIT;
                contextRenderType = GLX_COLOR_INDEX_TYPE;
                break;
            case RenderAttr::RAttr_RGB: // Only rgb is not supported.
            case RenderAttr::RAttr_RGBA:
                renderType |= GLX_RGBA_BIT;
                contextRenderType = GLX_RGBA_TYPE;
                break;
            case RenderAttr::RAttr_RedSize:
                glattrs.push_back(GLX_RED_SIZE);
                glattrs.push_back(attributes[++i]);
                break;
            case RenderAttr::RAttr_GreenSize:
                glattrs.push_back(GLX_GREEN_SIZE);
                glattrs.push_back(attributes[++i]);
                break;
            case RenderAttr::RAttr_BlueSize:
                glattrs.push_back(GLX_BLUE_SIZE);
                glattrs.push_back(attributes[++i]);
                break;
            case RenderAttr::RAttr_AlphaSize:
                glattrs.push_back(GLX_ALPHA_SIZE);
                glattrs.push_back(attributes[++i]);
                break;
            case RenderAttr::RAttr_DepthSize:
                glattrs.push_back(GLX_DEPTH_SIZE);
                glattrs.push_back(attributes[++i]);
                break;
            case RenderAttr::RAttr_StencilSize:
                glattrs.push_back(GLX_STENCIL_SIZE);
                glattrs.push_back(attributes[++i]);
                break;
            case RenderAttr::RAttr_Window:
                drawableType |= GLX_WINDOW_BIT;
                break;
            case RenderAttr::RAttr_Pixmap:
                drawableType |= GLX_PIXMAP_BIT;
                break;
            default:
                // Ignore the attribute.
                break;
            }
        }

        // Append the drawable type.
        if(drawableType != 0)
        {
            glattrs.push_back(GLX_DRAWABLE_TYPE);
            glattrs.push_back(drawableType);
        }

        // Append the render type.
        if(renderType != 0)
        {
            glattrs.push_back(GLX_RENDER_TYPE);
            glattrs.push_back(renderType);
        }

        // Finish the attribute list.
        glattrs.push_back(None);

        // Get the display and screen
        Display *display = screen->GetDisplay();
        int screenNum = screen->GetScreenNumber();

        // Select the frame buffer config.
        int numReturned = 0;
        GLXFBConfig *fbConfigs = glXChooseFBConfig(display, screenNum, &glattrs[0], &numReturned);
        if(!fbConfigs)
            return NULL; // No matching configuration available.

        // Pick the first member.
        GLXFBConfig pickedFbc = fbConfigs[0];

        // Free the list returned by glXChooseFBConfig.
        XFree(fbConfigs);

        // Get a visual.
        XVisualInfo *vi = glXGetVisualFromFBConfig(display, pickedFbc);

        // Create the opengl context.
        GLXContext context = glXCreateNewContext(display, pickedFbc, contextRenderType, NULL, True);
        if(context == NULL)
        {
            // Failed to create the context.
            XFree(vi);
            return NULL;
        }

        // Create the context wrapper.
        GlxContext *wrapper = new GlxContext(display, vi, context);
        return wrapper;
    }
}

