using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets;

public class FrameBuffer : Asset
{
    public readonly int Fbo;
    private readonly int _width;
    private readonly int _height;

    public FrameBuffer(int width, int height)
    {
        AssetManager.Register(this);
        _width = width;
        _height = height;
        Fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        GL.DrawBuffers(3,
            new[]
            {
                DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2
            });
    }

    public void Bind(ClearBufferMask? mask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit)
    {
        GL.DepthMask(true);
        GL.Viewport(0, 0, _width, _height);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        if (mask != null) GL.Clear((ClearBufferMask)mask);
    }

    public override void Delete()
    {
        GL.DeleteFramebuffer(Fbo);
    }
}