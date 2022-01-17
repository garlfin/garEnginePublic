using Silk.NET.OpenGL;

namespace gESilk.engine.render;

using System.Drawing;
using System.Drawing.Imaging;
using static Globals;

public class Texture : Asset
{
    private uint _id;
    private uint slot;
    private TextureUnit _unit; 
    
    private BitmapData _bmpData;
    public Texture(string path, uint slot, PixelFormat format = PixelFormat.Format32bppArgb)
    {
        this.slot = slot;
        switch (slot)
        {
            case 0:
                _unit = TextureUnit.Texture0;
                break;
            case 1:
                _unit = TextureUnit.Texture1;
                break;
                
        }
        
        //TextureManager.Register(this);
        Bitmap bmp = new Bitmap(path);
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        _id = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, _id);
        _bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, format);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)bmp.Width, (uint)bmp.Height, 0,
            Silk.NET.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, _bmpData.Scan0);
        gl.GenerateMipmap(TextureTarget.Texture2D);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        bmp.UnlockBits(_bmpData);
        bmp.Dispose();

    }

    public override void Delete()
    {
        gl.DeleteTexture(_id);
        Console.WriteLine($"Deleted texture: {_id}");
        _id = 0;
    }

    public uint Use()
    {
        
        gl.ActiveTexture(_unit);
        gl.BindTexture(TextureTarget.Texture2D, _id);
        return slot;
    }

}