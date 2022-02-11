using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets.textures;

public abstract class ITexture : Asset
{
    protected int _id;
    protected PixelInternalFormat _format;
    protected int _width, _height;
    
    public ITexture()
    {
    }

    public virtual int Use(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0+slot);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        return slot;
    }
  
    public virtual int Get()
    {
        return _id;
    }

    public override void Delete()
    {
    }
    
    public virtual void Use(int slot, TextureAccess access, int level = 0)
    {
    }
    public virtual Vector2 GetMipSize(int level)
    {
        int width = _width;
        int height = _height;
        while (level != 0)
        {
            width /= 2;
            height /= 2;
            level--;
        }

        return new Vector2(width, height);
    }

    public static int GetMipLevelCount(int width, int height)
    {
        return (int) Math.Floor(Math.Log2(Math.Min(width, height)));
    }
}