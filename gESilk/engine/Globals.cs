using Assimp;
using gESilk.engine.assimp;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using Material = gESilk.engine.render.materialSystem.Material;
using Mesh = gESilk.engine.render.assets.Mesh;

namespace gESilk.engine;

public static class Globals
{
    public static readonly AssimpContext Assimp;
    public static readonly Material DepthMaterial, LinearDepthMaterial;
    public static Mesh CubeMesh;

    static Globals()
    {
        Assimp = new AssimpContext();
        DepthMaterial = new Material(new ShaderProgram("../../../resources/shader/depth.glsl"),
            Program.MainWindow ?? throw new InvalidOperationException());

        LinearDepthMaterial = new Material(new ShaderProgram("../../../resources/shader/lineardepth.glsl"),
            Program.MainWindow ?? throw new InvalidOperationException());
        LinearDepthMaterial.AddSetting(new FloatSetting("far", 100f));

        CubeMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/cube.obj");
        CubeMesh.IsSkybox(true);
    }
}