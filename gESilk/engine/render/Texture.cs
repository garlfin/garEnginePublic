using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;


namespace gESilk.engine.render;

using static Globals;

public class Texture : Asset
{
    private int _id;
    private uint slot;
    private TextureUnit _unit; 
    private BitmapData _bmpData;
    
    public Texture(string path, uint slot, System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format32bppArgb)
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
        _bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, format);
        
        Load(bmp.Width, bmp.Height, _bmpData.Scan0);
        
        bmp.UnlockBits(_bmpData);
        bmp.Dispose();
        
        
        
        
    }

    private void Load(int width, int height, IntPtr data)
    {
        GL.BindTexture(TextureTarget.Texture2D, _id);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }
    public override void Delete()
    {
        GL.DeleteTexture(_id);
        Console.WriteLine($"Deleted texture: {_id}");
        _id = 0;
    }

    public uint Use()
    {
        
        GL.ActiveTexture(_unit);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        return slot;
    }

}