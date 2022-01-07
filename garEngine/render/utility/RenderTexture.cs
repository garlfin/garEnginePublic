using OpenTK.Graphics.OpenGL;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace garEngine.render.utility;

public class RenderTexture : Asset
{
    private int id;

    public RenderTexture(int width, int height)
    {
        RenderTexManager.Register(this);
        id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, id);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    }

    public override void Delete()
    {
        GL.DeleteTexture(id);
        id = -1;
    }

    public int GetTex()
    {
        return id;
    }

    public void BindToFramebuffer(int framebufferObject ,FramebufferAttachment attachmentLevel)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferObject);
        GL.BindTexture(TextureTarget.Texture2D, id);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer , attachmentLevel, TextureTarget.Texture2D, id, 0);
    }
}