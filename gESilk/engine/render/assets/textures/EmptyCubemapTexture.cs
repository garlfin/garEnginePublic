using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace gESilk.engine.render.assets.textures;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class EmptyCubemapTexture : Texture
{
    public EmptyCubemapTexture(int size)
    {
        Format = PixelInternalFormat.Rgba16f;
        AssetManager.Register(this);
        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        Width = size;
        Height = size;
        for (var i = 0; i < 6; i++)
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX + i, 0, PixelInternalFormat.Rgba16f, size, size, 0,
                PixelFormat.Bgr,
                PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
            (int)TextureWrapMode.ClampToEdge);
    }

    public override int Use(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + slot);
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        return slot;
    }

    public override void BindToBuffer(RenderBuffer buffer, FramebufferAttachment attachmentLevel)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Get());
        GL.BindTexture(TextureTarget.Texture2D, Id);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, TextureTarget.Texture2D, Id, 0);
    }
}