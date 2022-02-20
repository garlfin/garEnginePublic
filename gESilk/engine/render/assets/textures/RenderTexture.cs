using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets.textures;

public class RenderTexture : Texture
{
    public RenderTexture(int width, int height, PixelInternalFormat type = PixelInternalFormat.Rgba16f,
        PixelFormat format = PixelFormat.Rgba, PixelType byteType = PixelType.Float, bool shadow = false,
        TextureWrapMode mode = TextureWrapMode.ClampToBorder, TextureMinFilter minFilter = TextureMinFilter.Linear,
        TextureMagFilter magFilter = TextureMagFilter.Linear)
    {
        Format = type;
        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Id);
        GL.TexImage2D(TextureTarget.Texture2D, 0, type, width, height, 0, format, byteType, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)mode);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)mode);
        if (!shadow) return;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode,
            (int)TextureCompareMode.CompareRefToTexture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Less);
        float[] borderColor = { 1f, 1f, 1f, 1f };
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
    }

    public override void Use(int slot, TextureAccess access, int level = 0)
    {
        GL.BindImageTexture(slot, Id, 0, false, 0, access, (SizedInternalFormat)Format);
    }

    public override void BindToBuffer(RenderBuffer buffer, FramebufferAttachment attachmentLevel)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Get());
        GL.BindTexture(TextureTarget.Texture2D, Id);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, TextureTarget.Texture2D, Id, 0);
    }

    public override void BindToBuffer(FrameBuffer buffer, FramebufferAttachment attachmentLevel, bool isShadow = false)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Fbo);
        GL.BindTexture(TextureTarget.Texture2D, Id);
        if (isShadow)
        {
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
        }

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, TextureTarget.Texture2D, Id, 0);
    }
}