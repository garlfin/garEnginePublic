using garEngine.ecs_sys.entity;
using garEngine.ecs_sys.system;
using garEngine.render;
using garEngine.render.model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace garEngine.ecs_sys.component;

public class ModelRenderer : Component
{
    private ShaderProgram _shader;
    private int _mvpUniform;
    private int _vao;
    private int _Vbo;
    private int _ebo;
    private int _vtvbo;
    private Transform _modelTransform;
    private int _nmvbo;
    private Texture _texture;
    private int _texUniform;
    private ColladaParser _parser;


    public ModelRenderer(ColladaParser parser, Entity entityref, Texture tex, ShaderProgram shader)
    {
        this._texture = tex;
        _parser = parser;
        ModelRendererSystem.Register(this);
        _shader = shader;
        _mvpUniform = GL.GetUniformLocation(_shader.Id, "mvp");
        _texUniform = GL.GetUniformLocation(_shader.Id, "albedo");
        _Vbo = GL.GenBuffer();
        _vtvbo = GL.GenBuffer();
        _nmvbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();
        entity = entityref;
        
        _modelTransform = entity.GetComponent<Transform>();
       
      
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _parser.vert.Count, _parser.vert.ToArray(), BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _nmvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _parser.normal.Count, _parser.normal.ToArray(), BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vtvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * _parser.uv.Count, _parser.uv.ToArray(), BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _parser.triangle.Count * sizeof(uint), _parser.triangle.ToArray(), BufferUsageHint.StaticCopy);
       
    }

    public void ChangeShader(ShaderProgram shader)
    {
        _shader = shader;
        _mvpUniform = GL.GetUniformLocation(_shader.Id, "mvp");
        _texUniform = GL.GetUniformLocation(_shader.Id, "albedo");

    }

    public void ChangeImage(Texture tex)
    {
        this._texture = tex;
    }

    public override void Update(float gameTime)
    {
        _modelTransform = entity.GetComponent<Transform>();
        Matrix4 model = Matrix4.Identity * Matrix4.CreateScale(_modelTransform.Scale) *
                        Matrix4.CreateTranslation(_modelTransform.Location);
        Matrix4 mvp = model * RenderView._camera.GetViewMatrix() * RenderView._camera.GetProjectionMatrix();
        GL.UniformMatrix4(_mvpUniform, false, ref mvp);
        GL.UseProgram(_shader.Id);
        GL.BindVertexArray(_vao);
        GL.BindTexture(TextureTarget.Texture2D, _texture.id);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.Uniform1(_texUniform, 0);
        GL.DrawElements(PrimitiveType.Triangles, _parser.triangle.Count, DrawElementsType.UnsignedInt, 0);
    }

    public override void Close()
    {
        Console.WriteLine($"Deleted Vertex Array: {_vao}");
        GL.DeleteVertexArray( _vao);
        GL.DeleteBuffer( _Vbo);
        GL.DeleteBuffer( _ebo);
        GL.DeleteBuffer( _vtvbo);
        GL.DeleteBuffer( _nmvbo);
    }

  
}