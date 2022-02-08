using gESilk.engine.render.assets.textures;
using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets;

public class ComputeTexture : ITexture
{
    public ComputeTexture(int slot, int width, int height, PixelInternalFormat format)
    {
        _slot = slot;
        AssetManager.Register(this);
        _id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _id);


        GL.TexImage2D(TextureTarget.Texture2D, 0, format, width, height, 0, PixelFormat.Rgba,
            PixelType.Float, IntPtr.Zero);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }

    public override int Use()
    {
        GL.ActiveTexture(TextureUnit.Texture0+_slot);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        return _slot;
    }

    public void Bind(int slot, TextureAccess access, SizedInternalFormat format )
    {
        GL.BindImageTexture(slot, _id, 0, false, 0, access, format);
    }

    public override void Delete()
    {
        GL.DeleteTexture(_id);
    }
}