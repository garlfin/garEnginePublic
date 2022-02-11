﻿using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace gESilk.engine.render.assets.textures;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class CubemapTexture : ITexture
{
    public CubemapTexture(IReadOnlyList<string> path, int slot)
    {
        _format = PixelInternalFormat.Rgba16f;
        AssetManager.Register(this);
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

    public override int Use()
    {
        GL.ActiveTexture(TextureUnit.Texture0+_slot);
        GL.BindTexture(TextureTarget.TextureCubeMap, _id);
        return _slot;
    }
    public override void Bind(int slot, TextureAccess access, int level = 0)
    {
        GL.BindImageTexture(slot, _id, 0, false, 0, access, (SizedInternalFormat) _format);
    }

    public override void Delete()
    {
        GL.DeleteTexture(_id);
    }
}

