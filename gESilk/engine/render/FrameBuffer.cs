using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render;

public class FrameBuffer : Asset
{
    private readonly int _rbo;
    private readonly int _fbo;

    public FrameBuffer(int width, int height)
    {
        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.DrawBuffers(1,
            new[]
            {
                DrawBuffersEnum.ColorAttachment0
            });

        _rbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer, _rbo);

        var fboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (fboStatus != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine($"Error in FrameBuffer: {fboStatus}");
        }
    }

    public int Get()
    {
        return _fbo;
    }
    
    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public override void Delete()
    {
        GL.DeleteFramebuffer(_fbo);
        GL.DeleteRenderbuffer(_rbo);
    }
}