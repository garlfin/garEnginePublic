using System.Drawing.Text;
using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets;

public class FrameBuffer : Asset
{
    public int _fbo { get; private set; }
    public int _width {
        get; private set;
    }
    public int _height {
        get; private set;
    }
    public FrameBuffer(int width, int height)
    {
        AssetManager.Register(this);
        _width = width;
        _height = height;
        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,_fbo);
        GL.DrawBuffers(3,
            new[]
            {
                DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2
            });


    }

    public void Bind(ClearBufferMask? mask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)
    {
        GL.DepthMask(true);
        GL.Viewport(0,0,_width, _height);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,_fbo);
        if (mask != null) GL.Clear((ClearBufferMask)mask);
    }

    public override void Delete()
    {
        GL.DeleteFramebuffer(_fbo);
    }
}
