using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using gESilk.engine.misc;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;


namespace gESilk.engine.render.assets;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class Texture : Asset
{
    private readonly int _id;
    private readonly int _slot;

    public Texture(string path, int slot,
        System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format32bppArgb)
    {
        _id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _id);
        
        _slot = slot;
        
        AssetManager.Register(this);
        
        var bmp = new Bitmap(path);
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, format);
        
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra,
            PixelType.UnsignedByte, bmpData.Scan0);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        bmp.UnlockBits(bmpData);
        bmp.Dispose();
    }

    public override void Delete()
    {
        GL.DeleteTexture(_id);
    }

    public int Use()
    {
        GL.ActiveTexture(TextureUnit.Texture0+_slot);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        return _slot;
    }
}
