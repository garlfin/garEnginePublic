﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpEXR;
using PixelType = OpenTK.Graphics.OpenGL4.PixelType;

namespace gESilk.engine.render.assets.textures;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class CubemapTexture : Texture
{
    public readonly EmptyCubemapTexture Irradiance;

    public CubemapTexture(string path, Application application)
    {
        Format = PixelInternalFormat.Rgba16f;


        var originalId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, originalId);
        var exrFile = EXRFile.FromFile(path);
        var part = exrFile.Parts[0];
        part.OpenParallel(path);

        var bytes = part.GetFloats(ChannelConfiguration.RGB, true, GammaEncoding.Linear, false);

        var pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        var pointer = pinnedArray.AddrOfPinnedObject();

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, 2048, 1024, 0,
            PixelFormat.Rgb,
            PixelType.Float, pointer);

        pinnedArray.Free();
        part.Close();

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);

        Width = Height = 512;

        for (var i = 0; i < 6; i++)
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba32f, 512, 512, 0,
                PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

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

        var renderBuffer = new RenderBuffer(512, 512);
        renderBuffer.Bind();
        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Always);

        program.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, originalId);
        program.SetUniform("equirectangularMap", 0);
        program.SetUniform("model", Matrix4.Identity);
        program.SetUniform("projection",
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 100f));

        for (var i = 0; i < 6; i++)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.TextureCubeMapPositiveX + i, Id, 0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.SetUniform("view", Matrix4.LookAt(Vector3.Zero, Vector3.Zero + GetAngle(i),
                i is 2 or 3 ? i is 2 ? Vector3.UnitZ : -Vector3.UnitZ : -Vector3.UnitY));

            Globals.cubeMesh.Render();
        }


        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        GenerateMipsSpecular(application.GetSpecularProgram());

        GL.DeleteTexture(originalId);

        renderBuffer.Delete();
        AssetManager.Remove(renderBuffer);

        program.Delete();
        AssetManager.Remove(program);

        renderBuffer = new RenderBuffer(32, 32);
        renderBuffer.Bind(default, false);

        program = application.GetIrradianceProgram();

        Irradiance = new EmptyCubemapTexture(32, false);

        program.Use();
        program.SetUniform("environmentMap", Use(0));
        program.SetUniform("model", Matrix4.Identity);
        program.SetUniform("projection",
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 100f));

        for (var i = 0; i < 6; i++)
        {
            Irradiance.BindToBuffer(renderBuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.TextureCubeMapPositiveX + i);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.SetUniform("view", Matrix4.LookAt(Vector3.Zero, Vector3.Zero + GetAngle(i),
                i is 2 or 3 ? i is 2 ? Vector3.UnitZ : -Vector3.UnitZ : -Vector3.UnitY));

            Globals.cubeMesh.Render();
        }


        renderBuffer.Delete();
        AssetManager.Remove(renderBuffer);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Less);
    }


    public override int Use(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + slot);
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        return slot;
    }

    public override void BindToBuffer(RenderBuffer buffer, FramebufferAttachment attachmentLevel,
        TextureTarget target = TextureTarget.Texture2D, int level = 0)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Get());
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, target, Id, level);
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

    public void GenerateMipsSpecular(ShaderProgram program)
    {
        int mips = GetMipsCount();

        RenderBuffer buffer = new RenderBuffer(512, 512);
        buffer.Bind();

        program.Use();

        program.SetUniform("environmentMap", Use(0));
        program.SetUniform("projection",
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 100f));

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        for (int mip = 1; mip < mips; mip++)
        {
            var mipSize = GetMipSize(mip);
            GL.Viewport(0, 0, (int)mipSize.X, (int)mipSize.Y);
            program.SetUniform("roughness", (float)mip / (mips - 1));

            for (int i = 0; i < 6; i++)
            {
                BindToBuffer(buffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i,
                    mip);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                program.SetUniform("view", Matrix4.LookAt(Vector3.Zero, Vector3.Zero + GetAngle(i),
                    i is 2 or 3 ? i is 2 ? Vector3.UnitZ : -Vector3.UnitZ : -Vector3.UnitY));

                Globals.cubeMesh.Render();
            }
        }

        buffer.Delete();
        AssetManager.Remove(buffer);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
    }
}