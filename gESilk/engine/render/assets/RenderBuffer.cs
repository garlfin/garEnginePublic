using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets;

public class RenderBuffer : Asset
{
    private int _fbo;
    private readonly int _width, _height;
    private int _rbo;

    public RenderBuffer(int width, int height)
    {
        AssetManager.Register(this);
        _width = width;
        _height = height;
        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);


        _rbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer, _rbo);
        GL.DrawBuffers(3,
            new[]
            {
                DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2
            });
        var fboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (fboStatus != FramebufferErrorCode.FramebufferComplete)
            Console.WriteLine($"Error in RenderBuffer: {fboStatus}");
    }

    public int Get()
    {
        return _fbo;
    }

    public void Bind(ClearBufferMask? mask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit,
        bool setVP = true)
    {
        GL.DepthMask(true);
        if (setVP) GL.Viewport(0, 0, _width, _height);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        if (mask is not null) GL.Clear((ClearBufferMask)mask);
    }

    public void SetShadowBuffer()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
    }

    public override void Delete()
    {
        if (_fbo == -1) return;
        GL.DeleteFramebuffer(_fbo);
        GL.DeleteRenderbuffer(_rbo);
        _rbo = _fbo = -1;
    }
}