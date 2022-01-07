using garEngine.ecs_sys.system;
using garEngine.render;
using garEngine.render.model;
using garEngine.render.utility;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace garEngine.ecs_sys.component;

public class ModelRenderer : Component
{
    private Transform _modelTransform;
    private readonly MeshObject _parser;
    private Matrix4 model;
    private Matrix4 mvp;
    private Matrix4 view;




    public ModelRenderer(MeshObject loaderTest)
    {
        _parser = loaderTest;
        ModelRendererSystem.Register(this);
    }

    public override void Update(float gameTime)
    {
        
        for (int i = 0; i < _parser.Length(); i++)
        {
            Material currentMaterial = entity.GetComponent<MaterialComponent>().GetMaterial(_parser.GetMeshMatIndex(i));
            currentMaterial.Use();
            currentMaterial.SetUniform("model", ref model);
            currentMaterial.SetUniform("lightSpaceMatrix", ref WorldSettings.lightSpaceMatrix);
            currentMaterial.SetUniform("mvp", ref mvp);
            currentMaterial.SetUniform("view", ref view);
            currentMaterial.SetUniform("lightPos", WorldSettings.LightPos);
            currentMaterial.SetUniform("viewVec", RenderView._camera.Position);
            currentMaterial.SetUniform("cubemap", 0);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, WorldSettings._texture);
            currentMaterial.SetUniform("shadowMap", 6);
            _parser.Render(i);
        }
        
        
    }

    private Matrix4 CreateModelMatrix()
    {
        return Matrix4.CreateRotationX(_modelTransform.Rotation.X) * Matrix4.CreateRotationY(_modelTransform.Rotation.Y) *
                Matrix4.CreateRotationZ(_modelTransform.Rotation.Z) * Matrix4.CreateScale(_modelTransform.Scale) *
                Matrix4.CreateTranslation(_modelTransform.Location);
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
            view = RenderView._camera.GetViewMatrix();
            WorldSettings.DepthMaterial.SetUniform("model", ref model);
            WorldSettings.DepthMaterial.SetUniform("mvp", ref mvp);
        }
        _parser.RenderAll();
    }
  
}