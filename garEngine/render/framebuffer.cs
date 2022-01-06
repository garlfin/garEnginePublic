using OpenTK.Graphics.OpenGL;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace garEngine.render;

public class Framebuffer
{
    private readonly int _fbo, _fboTex, _rbo, _rectVao, _rectVbo, _rectVboUv;
    
    
    private float[] rectangleVertices =
    {
        // Coords    
        1.0f, -1.0f, 
        -1.0f, -1.0f,
        -1.0f,  1.0f,

        1.0f,  1.0f, 
        1.0f, -1.0f, 
        -1.0f,  1.0f
    };
    private float[] rectangleVerticesUV =
    {
        // Coords    
        1.0f, 0.0f,
        0.0f, 0.0f,
        0.0f, 1.0f,

        1.0f, 1.0f,
        1.0f, 0.0f,
        -0.0f, 1.0f
    };


    private Material _material;
    

    public Framebuffer(Material material)
    {
        _material = material;
        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _fboTex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _fboTex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 1280, 720, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _fbo, 0);

        _rbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, 1280, 720);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);

        _rectVao = GL.GenVertexArray();
        _rectVbo = GL.GenBuffer();
        _rectVboUv = GL.GenBuffer();
        
        GL.BindVertexArray(_rectVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _rectVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, rectangleVertices.Length * sizeof(float), rectangleVertices, BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0,2, VertexAttribPointerType.Float, false, 0, 0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _rectVboUv);
        GL.BufferData(BufferTarget.ArrayBuffer, rectangleVerticesUV.Length * sizeof(float), rectangleVerticesUV, BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
    }

    public void Render()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _material.Use();
        GL.Disable(EnableCap.CullFace | EnableCap.DepthTest);
        GL.BindVertexArray(_rectVao);
        GL.BindTexture(TextureTarget.Texture2D, _fboTex);
        _material.SetUniform("screenTexture", 0);
        GL.DrawArrays(BeginMode.Triangles, 0, 6);
        GL.Enable(EnableCap.CullFace | EnableCap.DepthTest);

    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.ClearColor(0,0f, 1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);
    }
}