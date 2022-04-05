using System.Diagnostics.CodeAnalysis;
using gESilk.engine.misc;
using gESilk.engine.render.materialSystem.settings;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets.textures;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class EmptyCubemapTexture : Texture
{
    public EmptyCubemapTexture(int size, bool genMips = true, PixelInternalFormat format = PixelInternalFormat.Rgba16f,
        PixelFormat byteFormat = PixelFormat.Rgba, bool isShadow = false)
    {
        Format = format;
        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        Width = Height = size;
        for (var i = 0; i < 6; i++)
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, format, size, size, 0,
                byteFormat, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
            (int)(genMips ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
            (int)TextureWrapMode.ClampToEdge);

        if (genMips) GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
        if (!isShadow) return;
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureCompareMode,
            (int)TextureCompareMode.CompareRefToTexture);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureCompareFunc, (int)All.Less);
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
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, target, Id, level);
    }

    public void BindToBuffer(FrameBuffer buffer, FramebufferAttachment attachment, TextureTarget target, int level)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Fbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, target, Id, level);
    }


    public void GenerateMipsSpecular(Application application)
    {
        int mips = GetMipsCount();

        RenderTexture pongTexture = new RenderTexture(Width, Height, PixelInternalFormat.Rgba16f,
            minFilter: TextureMinFilter.LinearMipmapLinear, computeMips: true);

        RenderBuffer buffer = new RenderBuffer(Width, Height);
        buffer.Bind();

        var program = application.SpecularProgram;
        var pongProgram = application.PongProgram;


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

                application.RenderPlaneMesh.Render();
            }
        }

        buffer.Delete();
        AssetManager.Remove(buffer);

        pongTexture.Delete();
        AssetManager.Remove(pongTexture);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
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