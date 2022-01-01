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
    private ShaderProgram _shader;
    private readonly int _mvpUniform;
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly int _vtvbo;
    private Transform _modelTransform;
    private readonly int _nmvbo;
    private readonly AssimpLoaderTest.MeshStruct _parser;
    private readonly int _viewUniform;
    private readonly int _lightUniform;
    private readonly int _cubemapUniform;
    private readonly int _tanvbo;




    public ModelRenderer(AssimpLoaderTest.MeshStruct loaderTest, Entity entityref, ShaderProgram shader)
    {
        _parser = loaderTest;
        ModelRendererSystem.Register(this);
        _shader = shader;
        _mvpUniform = GL.GetUniformLocation(_shader.Id, "mvp");
        _viewUniform = GL.GetUniformLocation(_shader.Id, "viewVec");
        _lightUniform = GL.GetUniformLocation(_shader.Id, "lightPos");
        _cubemapUniform = GL.GetUniformLocation(_shader.Id, "cubemap");
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

    public void ChangeShader(ShaderProgram shader)
    {
        _shader = shader;
    }

    public override void Update(float gameTime)
    {
        GL.UseProgram(_shader.Id);
        _modelTransform = entity.GetComponent<Transform>();
        Matrix4 model = Matrix4.CreateScale(_modelTransform.Scale) *
                        Matrix4.CreateTranslation(_modelTransform.Location);
        Matrix4 mvp = model * RenderView._camera.GetViewMatrix() * RenderView._camera.GetProjectionMatrix();
        GL.UniformMatrix4(GL.GetUniformLocation(_shader.Id, "model"), false, ref model);
        GL.UniformMatrix4(GL.GetUniformLocation(_shader.Id,"lightSpaceMatrix"), false, ref WorldSettings.lightSpaceMatrix);
        GL.UniformMatrix4(_mvpUniform, false, ref mvp);
        GL.Uniform3(_lightUniform, WorldSettings.LightPos);
        GL.Uniform3(_viewUniform, RenderView._camera.Position);
        GL.Uniform1(_cubemapUniform, 0);
        GL.BindVertexArray(_vao);
        int i = 0;
        GL.ActiveTexture(TextureUnit.Texture6);
        GL.BindTexture(TextureTarget.Texture2D, WorldSettings._texture);
        GL.Uniform1(GL.GetUniformLocation(_shader.Id, "shadowMap"), 6);
        List<TextureUnit> availableTargets =
            new List<TextureUnit>
            {
                TextureUnit.Texture1, TextureUnit.Texture2, TextureUnit.Texture3
            };
        foreach (ShaderSettingTex settingTex in _shader.ShaderSettingTexes)
        {
            GL.ActiveTexture(availableTargets[i]);
            GL.BindTexture(TextureTarget.Texture2D, settingTex.value.id);
            GL.Uniform1(GL.GetUniformLocation(_shader.Id, settingTex.uniformName), i+1);
            i++;
        }
        GL.DrawElements(PrimitiveType.Triangles, _parser.faces.Count * 3, DrawElementsType.UnsignedInt, 0);
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


    public override void UpdateShadow()
    {
        _modelTransform = entity.GetComponent<Transform>();
        Matrix4 model = Matrix4.CreateScale(_modelTransform.Scale) *
                        Matrix4.CreateTranslation(_modelTransform.Location);
        GL.UseProgram(WorldSettings.depthShader.Id);
        GL.UniformMatrix4(GL.GetUniformLocation(WorldSettings.depthShader.Id, "model"), false, ref model );
        GL.UniformMatrix4(GL.GetUniformLocation(WorldSettings.depthShader.Id, "lightSpaceMatrix"), false, ref WorldSettings.lightSpaceMatrix );
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _parser.faces.Count * 3, DrawElementsType.UnsignedInt, 0);
    }
  
}