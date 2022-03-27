﻿using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets.textures;

[SuppressMessage("Interoperability", "CA1416", MessageId = "Validate platform compatibility")]
public class EmptyCubemapTexture : Texture
{
    public EmptyCubemapTexture(int size, bool genMips = true)
    {
        Format = PixelInternalFormat.Rgba16f;
        Id = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        Width = Height = size;
        for (var i = 0; i < 6; i++)
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba16f, size, size, 0,
                PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
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
        GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Get());
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, target, Id, level);
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