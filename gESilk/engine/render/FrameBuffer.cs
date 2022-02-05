using gESilk.engine.misc;
using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL;

namespace gESilk.engine.render;

public class FrameBuffer : Asset
{
    public int _fbo { get; private set; }
    private int _width, _height;
    public FrameBuffer(int width, int height)
    {
        FrameBufferManager.Register(this);
        _width = width;
        _height = height;
        _fbo = GL.GenFramebuffer();
    }

    public void Bind(ClearBufferMask mask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)
    {
        GL.Viewport(0,0,_width, _height);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,_fbo);
        GL.Clear(mask);
    }

    public override void Delete()
    {
        GL.DeleteFramebuffer(_fbo);
    }
}

class FrameBufferManager : AssetManager<FrameBuffer>
{
    
}