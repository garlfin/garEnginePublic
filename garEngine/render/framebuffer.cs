using OpenTK.Graphics.OpenGL;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace garEngine.render;

public class Framebuffer
{
    private readonly int _fbo, _rbo, _rectVao, _rectVbo, _rectVboUv;
    
    
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
    private RenderTexture fragTex, normalTex, positionTex;

    

    public Framebuffer(Material material)
    {
        _material = material;
        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

        fragTex = new(1280, 720);
        normalTex = new(1280, 720);
        positionTex = new(1280, 720);
        
        GL.DrawBuffers(3, new [] { DrawBuffersEnum.ColorAttachment0 , DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2});
        
       fragTex.BindToFramebuffer(_fbo, FramebufferAttachment.ColorAttachment0);
       normalTex.BindToFramebuffer(_fbo, FramebufferAttachment.ColorAttachment1);
       positionTex.BindToFramebuffer(_fbo, FramebufferAttachment.ColorAttachment2);
       
        _rbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, 1280, 720);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);

        var fboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (fboStatus != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine($"Error in FrameBuffer: {fboStatus}");
        }
        
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
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Render()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _material.Use();
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        GL.BindVertexArray(_rectVao);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, fragTex.GetTex());
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, normalTex.GetTex());
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, positionTex.GetTex());
        _material.SetUniform("screenTexture", 0);
        _material.SetUniform("normalTexture", 1);
        _material.SetUniform("positionTexture", 2);
        GL.DrawArrays(BeginMode.Triangles, 0, 6);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);

    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);
    }

    public void Delete()
    {
        GL.DeleteBuffer(_rectVbo);
        GL.DeleteVertexArray(_rectVao);
        GL.DeleteBuffer(_rectVboUv);
        GL.DeleteRenderbuffer(_rbo);
        GL.DeleteFramebuffer(_fbo);
        fragTex.Delete();
        normalTex.Delete();
        positionTex.Delete();

    }
}