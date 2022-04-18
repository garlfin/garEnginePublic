using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using Image = PVRTC.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace gESilk.engine.render.assets.textures;

public class ImageTexture : Texture
{
    public ImageTexture(Image image, Application application)
    {
        Format = PixelInternalFormat.Rgba;
        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Id);
        
        GCHandle pinnedArray = GCHandle.Alloc(image.ImageData, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();
        GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.CompressedRgbaS3tcDxt3Ext, (int) image.Width,
            (int) image.Height, 0, image.ImageData.Length, pointer);
        pinnedArray.Free();
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.LinearMipmapLinear);
        if (application.HasExtension("GL_EXT_texture_filter_anisotropic"))
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                (TextureParameterName)ArbTextureFilterAnisotropic.TextureMaxAnisotropy, 4f);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, 0f);
        }
    }
}