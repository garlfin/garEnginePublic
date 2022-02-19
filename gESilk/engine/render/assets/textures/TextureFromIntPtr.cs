using OpenTK.Graphics.OpenGL4;

namespace gESilk.engine.render.assets.textures;

public class TextureFromIntPtr : Texture
{
    public TextureFromIntPtr(int width, int height, IntPtr pixels)
    {
        AssetManager.Register(this);

        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Id);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb,
            PixelType.Float, pixels);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }
}