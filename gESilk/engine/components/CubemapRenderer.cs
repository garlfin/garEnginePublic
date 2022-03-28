using gESilk.engine.render.assets;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class CubemapRenderer : Component
{
    private readonly Application _application;
    private readonly Mesh _mesh;

    public CubemapRenderer(Application application)
    {
        _application = application;
        CubemapMManager.Register(this);
        _mesh = Globals.CubeMesh;
    }

    public override void Update(float gameTime)
    {
        _mesh.Render(_application.SkyboxMaterial, Matrix4.Identity, DepthFunction.Lequal, false);
    }
}

internal class CubemapMManager : BaseSystem<CubemapRenderer>
{
}