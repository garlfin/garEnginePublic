using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpEXR;
using PixelType = OpenTK.Graphics.OpenGL4.PixelType;

namespace gESilk.engine.render.assets.textures;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class CubemapTexture : Texture
{
    public CubemapTexture(string path)
    {
        Format = PixelInternalFormat.Rgba16f;


        int originalID = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, originalID);
        var exrFile = EXRFile.FromFile(path);
        var part = exrFile.Parts[0];
        part.OpenParallel(path);

        float[] bytes = part.GetFloats(ChannelConfiguration.RGB, true, GammaEncoding.Linear, false);

        GCHandle pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        IntPtr pointer = pinnedArray.AddrOfPinnedObject();

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, 2048, 1024, 0,
            OpenTK.Graphics.OpenGL4.PixelFormat.Rgb,
            PixelType.Float, pointer);

        pinnedArray.Free();
        part.Close();

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        Width = 512;
        Height = 512;
        for (var i = 0; i < 6; i++)
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgb32f, 512, 512, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
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

        var program = new ShaderProgram("../../../resources/shader/preSkybox.glsl");
        program.Use();

        int _fbo, _rbo;

        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

        _rbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width,
            Height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, _rbo);

        GL.Viewport(0, 0, Width, Height);

        for (var i = 0; i < 6; i++)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.TextureCubeMapPositiveX + i, Id, 0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, originalID);
            program.SetUniform("equirectangularMap", 0);
            program.SetUniform("model", Matrix4.Identity);
            program.SetUniform("view", Matrix4.LookAt(Vector3.Zero, Vector3.Zero + GetAngle(i),
                i is 2 or 3 ? i is 2 ? Vector3.UnitZ : -Vector3.UnitZ : -Vector3.UnitY));
            program.SetUniform("projection", Matrix4.CreatePerspectiveFieldOfView(1.5708f, 1, 0.1f, 100f));
            Globals.cubeMesh.Render();
        }

        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        GL.DeleteFramebuffer(_fbo);
        GL.DeleteRenderbuffer(_rbo);

        GL.DeleteTexture(originalID);
        program.Delete();
        AssetManager.Remove(program);
    }

    public override int Use(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + slot);
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        return slot;
    }

    public override void BindToBuffer(RenderBuffer buffer, FramebufferAttachment attachmentLevel)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Get());
        GL.BindTexture(TextureTarget.Texture2D, Id);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, TextureTarget.Texture2D, Id, 0);
    }

    private Vector3 GetAngle(int index)
    {
        return index switch
        {
            0 => new Vector3(1, 0, 0), // posx
            1 => new Vector3(-1, 0, 0), // negx
            2 => new Vector3(0, 1, 0), // posy
            3 => new Vector3(0, -1, 0), // negy
            4 => new Vector3(0, 0, 1), // posz
            5 => new Vector3(0, 0, -1), //negz
            _ => Vector3.Zero
        };
    }
}