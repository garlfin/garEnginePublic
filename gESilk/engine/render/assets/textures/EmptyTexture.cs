using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets.textures;

public class EmptyTexture : ITexture
{
    public EmptyTexture(int slot, int width, int height, PixelInternalFormat format, PixelFormat format2, int mipLevels)
    {
        _slot = slot;
        AssetManager.Register(this);
        _id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _id);
        _format = format;
        _width = width;
        _height = height;

        //GL.TexImage2D(TextureTarget.Texture2D, 0, format, width, height, 0, format2,
        //    PixelType.Float, IntPtr.Zero);
        GL.TexStorage2D(TextureTarget2d.Texture2D, mipLevels, (SizedInternalFormat) format, width, height);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
        
    }

    public override int Use()
    {
        GL.ActiveTexture(TextureUnit.Texture0+_slot);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        return _slot;
    }

    public override int Use(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0+slot);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        return slot;
    }

    public override void Bind(int slot, TextureAccess access, int level = 0)
    {
        GL.BindImageTexture(slot, _id, level, false, 0, access, (SizedInternalFormat) _format);
    }

    public override void Delete()
    {
        GL.DeleteTexture(_id);
    }
}