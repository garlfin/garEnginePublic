using gESilk.engine.render.assets;
using OpenTK.Graphics.OpenGL4;


namespace gESilk.engine.render;

public class RenderTexture : Asset
{
    private int _id;
    private readonly int _slot;

    public RenderTexture(int width, int height, int slot, PixelInternalFormat type = PixelInternalFormat.Rgb, PixelFormat format = PixelFormat.Rgb, PixelType byteType = PixelType.UnsignedByte)
    {
        _slot = slot;
        //RenderTexManager.Register(this);
        _id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _id);
        GL.TexImage2D(TextureTarget.Texture2D, 0, type, width, height, 0, format, byteType, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        float[] borderColor = {1.0f, 1.0f, 1.0f, 1.0f};
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
    }

    public override void Delete()
    {
        GL.DeleteTexture(_id);
        _id = -1;
    }

    public int Use()
    {
        GL.ActiveTexture(TextureUnit.Texture0+_slot);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        return _slot;
    }

    public void BindToBuffer(RenderBuffer buffer, FramebufferAttachment attachmentLevel)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Get());
        GL.BindTexture(TextureTarget.Texture2D, _id);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer , attachmentLevel, TextureTarget.Texture2D, _id, 0);
    }
    public void BindToBuffer(FrameBuffer buffer, FramebufferAttachment attachmentLevel)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer._fbo);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, TextureTarget.Texture2D, _id, 0);
    }
}