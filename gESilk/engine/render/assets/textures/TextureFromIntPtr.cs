using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets.textures;

public class TextureFromIntPtr : ITexture
{
    public TextureFromIntPtr(int width, int height, IntPtr pixels)
    {
              
        AssetManager.Register(this);
        
        _id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _id);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width , height, 0, PixelFormat.Rgb,
            PixelType.Float, pixels);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }
}