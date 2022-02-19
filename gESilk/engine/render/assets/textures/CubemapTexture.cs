using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace gESilk.engine.render.assets.textures;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class CubemapTexture : ITexture
{
    public CubemapTexture(IReadOnlyList<string> path)
    {
        _format = PixelInternalFormat.Rgba16f;
        AssetManager.Register(this);
        List<TextureTarget> targets = new List<TextureTarget>()
        {
            TextureTarget.TextureCubeMapNegativeX,
            TextureTarget.TextureCubeMapNegativeY,
            TextureTarget.TextureCubeMapNegativeZ,
            TextureTarget.TextureCubeMapPositiveX,
            TextureTarget.TextureCubeMapPositiveY,
            TextureTarget.TextureCubeMapPositiveZ
        };
        _id = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, _id);
        for (int i = 0; i < 6; i++)
        {
            Bitmap bmp = new Bitmap(path[i]);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.TexImage2D(targets[i], 0, PixelInternalFormat.Rgba16f, bmp.Width, bmp.Height, 0, PixelFormat.Bgr,
                PixelType.UnsignedByte, bmpData.Scan0);

            bmp.UnlockBits(bmpData);
            bmp.Dispose();
        }

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
            (int)TextureWrapMode.ClampToEdge);
        GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
    }

    public override int Use(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0+slot);
        GL.BindTexture(TextureTarget.TextureCubeMap, _id);
        return slot;
    }
    
}

