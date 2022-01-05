using garEngine.ecs_sys.entity;
using garEngine.ecs_sys.system;
using garEngine.render;
using garEngine.render.model;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace garEngine.ecs_sys.component;

public class ModelRenderer : Component
{
    private Material _material;
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly int _vtvbo;
    private Transform _modelTransform;
    private readonly int _nmvbo;
    private readonly AssimpLoaderTest.MeshStruct _parser;
    private readonly int _tanvbo;
    private Matrix4 model;
    private Matrix4 mvp;




    public ModelRenderer(AssimpLoaderTest.MeshStruct loaderTest, Entity entityref, Material material)
    {
        _parser = loaderTest;
        ModelRendererSystem.Register(this);
        _material = material;
        _vbo = GL.GenBuffer();
        _vtvbo = GL.GenBuffer();
        _nmvbo = GL.GenBuffer();
        _tanvbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();
        entity = entityref;
        
        _modelTransform = entity.GetComponent<Transform>();
       
      
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float)* 3 * _parser.points.Count, _parser.points.ToArray(), BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _nmvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * _parser.normal.Count, _parser.normal.ToArray(), BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vtvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 2 * _parser.uvs.Count, _parser.uvs.ToArray(), BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _tanvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * _parser.tangents.Count, _parser.tangents.ToArray(), BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _parser.faces.Count * 3 * sizeof(uint), _parser.faces.ToArray(), BufferUsageHint.StaticCopy);
       
    }

    public void ChangeShader(Material material)
    {
        _material = material;
    }

    public override void Update(float gameTime)
    {
        _material.Use();
        //_modelTransform = entity.GetComponent<Transform>();
       // model = CreateModelMatrix();
        //mvp =  model * RenderView._camera.GetViewMatrix() * RenderView._camera.GetProjectionMatrix();
        _material.SetUniform("model", ref model);
        _material.SetUniform("lightSpaceMatrix", ref WorldSettings.lightSpaceMatrix);
        _material.SetUniform("mvp", ref mvp);
        _material.SetUniform("lightPos", WorldSettings.LightPos);
        _material.SetUniform("viewVec", RenderView._camera.Position);
        _material.SetUniform("cubemap", 0);
        GL.ActiveTexture(TextureUnit.Texture6);
        GL.BindTexture(TextureTarget.Texture2D, WorldSettings._texture);
        _material.SetUniform("shadowMap", 6);
        
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _parser.faces.Count * 3, DrawElementsType.UnsignedInt, 0);
    }

    private Matrix4 CreateModelMatrix()
    {
        return Matrix4.CreateRotationX(_modelTransform.Rotation.X) * Matrix4.CreateRotationY(_modelTransform.Rotation.Y) *
                Matrix4.CreateRotationZ(_modelTransform.Rotation.Z) * Matrix4.CreateScale(_modelTransform.Scale) *
                Matrix4.CreateTranslation(_modelTransform.Location);
    }

    public override void Close()
    {
        Console.WriteLine($"Deleted Vertex Array: {_vao}");
        GL.DeleteVertexArray( _vao);
        GL.DeleteBuffer( _vbo);
        GL.DeleteBuffer( _ebo);
        GL.DeleteBuffer( _vtvbo);
        GL.DeleteBuffer( _nmvbo);
    }


    public override void UpdateDepth(bool isShadow)
    {
        _modelTransform = entity.GetComponent<Transform>();
        model = CreateModelMatrix();
        if (isShadow)
        {
            WorldSettings.ShadowDepthMaterial.Use();
            WorldSettings.ShadowDepthMaterial.SetUniform("model", ref model);
            WorldSettings.ShadowDepthMaterial.SetUniform("lightSpaceMatrix", ref WorldSettings.lightSpaceMatrix);
        }
        else
        {
            WorldSettings.DepthMaterial.Use();
            mvp =  model * RenderView._camera.GetViewMatrix() * RenderView._camera.GetProjectionMatrix();
            WorldSettings.DepthMaterial.SetUniform("model", ref model);
            WorldSettings.DepthMaterial.SetUniform("mvp", ref mvp);
        }
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _parser.faces.Count * 3, DrawElementsType.UnsignedInt, 0);
    }
  
}