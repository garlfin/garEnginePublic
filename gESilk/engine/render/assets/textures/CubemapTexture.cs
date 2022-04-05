using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using gESilk.engine.misc;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
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
    private Application _application;

    public CubemapTexture(string path, Application application)
    {
        _application = application;
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

            program.SetUniform("view", MiscMath.GetLookAt(Vector3.Zero, i));

            Globals.CubeMesh.Render();
        }


        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
        GenerateMipsSpecular();

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        GL.DeleteTexture(originalId);

        renderBuffer.Delete();
        AssetManager.Remove(renderBuffer);

        program.Delete();
        AssetManager.Remove(program);

        renderBuffer = new RenderBuffer(32, 32);
        renderBuffer.Bind();

        program = application.IrradianceProgram;

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

            program.SetUniform("view", MiscMath.GetLookAt(Vector3.Zero, i));

            Globals.CubeMesh.Render();
        }


        renderBuffer.Delete();
        AssetManager.Remove(renderBuffer);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Less);
    }


    public override int Use(int slot)
    {
        if (TextureSlotManager.IsSlotSame(slot, Id)) return slot;
        TextureSlotManager.SetSlot(slot, Id);
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

    private void GenerateMipsSpecular()
    {
        int mips = GetMipsCount();

        RenderTexture pongTexture = new RenderTexture(512, 512, PixelInternalFormat.Rgba32f,
            minFilter: TextureMinFilter.LinearMipmapLinear, computeMips: true);

        RenderBuffer buffer = new RenderBuffer(512, 512);
        buffer.Bind();

        var program = _application.SpecularProgram;
        var pongProgram = _application.PongProgram;


        program.SetUniform("environmentMap", Use(0));
        program.SetUniform("projection",
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 100f));

        pongProgram.SetUniform("colorTex", pongTexture.Use(1));

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);

        for (int mip = 0; mip < mips; mip++)
        {
            var mipSize = GetMipSize(mip);
            GL.Viewport(0, 0, (int)mipSize.X, (int)mipSize.Y);
            program.SetUniform("roughness", (float)mip / (mips + 1));
            pongProgram.SetUniform("lod", mip);

            for (int i = 0; i < 6; i++)
            {
                program.Use();

                pongTexture.BindToBuffer(buffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, mip);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                program.SetUniform("view", MiscMath.GetLookAt(Vector3.Zero, i));

                Globals.CubeMesh.Render();

                pongProgram.Use();

                BindToBuffer(buffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i,
                    mip);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                _application.RenderPlaneMesh.Render();
            }
        }

        buffer.Delete();
        AssetManager.Remove(buffer);

        pongTexture.Delete();
        AssetManager.Remove(pongTexture);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
    }
}