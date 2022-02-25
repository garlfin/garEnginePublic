using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets.textures;

public class EmptyTexture : Texture
{
    public EmptyTexture(int width, int height, PixelInternalFormat format, int mipLevels)
    {
        AssetManager.Register(this);
        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Id);
        Format = format;
        Width = width;
        Height = height;

        //GL.TexImage2D(TextureTarget.Texture2D, 0, format, width, height, 0, format2,
        //    PixelType.Float, IntPtr.Zero);
        GL.TexStorage2D(TextureTarget2d.Texture2D, mipLevels, (SizedInternalFormat) format, width, height);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int) TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
    }

    public override void Use(int slot, TextureAccess access, int level = 0)
    {
        GL.BindImageTexture(slot, Id, level, false, 0, access, (SizedInternalFormat) Format);
    }
}