using System.Drawing;
using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render;

public class RenderBuffer : Asset
{
    private readonly int _fbo, _width, _height, _rbo;
    private readonly bool _shadow;

    public RenderBuffer(int width, int height)
    {
        _width = width;
        _height = height;
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
            Console.WriteLine($"Error in RenderBuffer: {fboStatus}");
        }
    }

    public int Get()
    {
        return _fbo;
    }
    
    public void Bind(ClearBufferMask mask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)
    {
        GL.Viewport(0, 0, _width, _height);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.Clear(mask);
    }

    public void SetShadowBuffer()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
    }

    public override void Delete()
    {
        GL.DeleteFramebuffer(_fbo);
        GL.DeleteRenderbuffer(_rbo);
    }
}