using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using gESilk.engine.misc;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace gESilk.engine.render.assets;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class CubemapTexture : Asset
{
    private readonly int _id;
    private readonly int _slot;

    public CubemapTexture(IReadOnlyList<string> path, int slot)
    {
        CubemapManager.Register(this);
        _slot = slot;
        var targets = new List<TextureTarget>()
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
        for (var i = 0; i < 6; i++)
        {
            var bmp = new Bitmap(path[i]);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.TexImage2D(targets[i], 0, PixelInternalFormat.Rgb8, bmp.Width, bmp.Height, 0, PixelFormat.Bgr,
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

    public int Use()
    {
        GL.ActiveTexture(TextureUnit.Texture0+_slot);
        GL.BindTexture(TextureTarget.TextureCubeMap, _id);
        return _slot;
    }

    public override void Delete()
    {
        GL.DeleteTexture(_id);
    }
}

internal class CubemapManager : AssetManager<CubemapTexture>
{
}