using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets.textures;

public abstract class ITexture : Asset
{
    protected int _id;
    protected PixelInternalFormat _format;
    protected int _width, _height;
    
    public ITexture()
    {
    }

    public virtual int Use(int slot)
    {
        GL.ActiveTexture(TextureUnit.Texture0+slot);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        return slot;
    }
  
    public virtual int Get()
    {
        return _id;
    }

    public override void Delete()
    {
        GL.DeleteTexture(_id);
    }
    
    
    public virtual Vector2 GetMipSize(int level)
    {
        int width = _width;
        int height = _height;
        while (level != 0)
        {
            width /= 2;
            height /= 2;
            level--;
        }

        return new Vector2(width, height);
    }

    public static int GetMipLevelCount(int width, int height)
    {
        return (int) Math.Floor(Math.Log2(Math.Min(width, height)));
    }
    public virtual void Use(int slot, TextureAccess access, int level = 0)
    {
        GL.BindImageTexture(slot, _id, 0, false, 0, access, (SizedInternalFormat) _format);
    }
    public virtual void BindToBuffer(RenderBuffer buffer, FramebufferAttachment attachmentLevel)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer.Get());
        GL.BindTexture(TextureTarget.Texture2D, _id);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer , attachmentLevel, TextureTarget.Texture2D, _id, 0);
    }
    public virtual void BindToBuffer(FrameBuffer buffer, FramebufferAttachment attachmentLevel, bool isShadow = false)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer._fbo);
        GL.BindTexture(TextureTarget.Texture2D, _id);
        if (isShadow)
        {
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
        }

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachmentLevel, TextureTarget.Texture2D, _id, 0);
    }
}