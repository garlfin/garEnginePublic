using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets.textures;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class EmptyCubemapTexture : Texture
{
    public EmptyCubemapTexture(int size, bool genMips = true)
    {
        Format = PixelInternalFormat.Rgba16f;
        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        Width = size;
        Height = size;
        for (var i = 0; i < 6; i++)
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba16f, size, size, 0,
                PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
            (int)(genMips ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
            (int)TextureWrapMode.ClampToEdge);

        if (genMips) GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
    }

    public override int Use(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + slot);
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        return slot;
    }

    public override void BindToBuffer(RenderBuffer buffer, FramebufferAttachment attachmentLevel,
        TextureTarget target = TextureTarget.Texture2D, int level = 0)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Get());
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, target, Id, level);
    }
}