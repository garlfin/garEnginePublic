using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace gESilk.engine.render.assets;

public class NoiseTexture : Asset
{
 
    private int _id;
    private readonly int _slot;


    public NoiseTexture(int slot)
    {
        var rand = new Random();
        Vector3[] _data = {new((float)(rand.NextDouble()*2.0-1.0),(float)(rand.NextDouble()*2.0-1.0), 0f)};
        for (int i = 0; i < 15; i++)
        {
            Vector3 data = new((float)(rand.NextDouble() * 2.0 - 1.0), (float)(rand.NextDouble() * 2.0 - 1.0), 0f);
            Array.Resize(ref _data, _data.Length + 1);
            _data[_data.GetUpperBound(0)] = data;
        }
        
        _id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _id);
            
        _slot = slot;
            
        AssetManager.Register(this);


        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 4 , 4, 0, PixelFormat.Rgb,
        PixelType.Float, _data);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

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

