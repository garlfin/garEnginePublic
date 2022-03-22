using Assimp;
using gESilk.engine.assimp;
using gESilk.engine.render.assets;
using Material = gESilk.engine.render.materialSystem.Material;
using Mesh = gESilk.engine.render.assets.Mesh;

namespace gESilk.engine;

public static class Globals
{
    public static readonly AssimpContext Assimp;
    public static readonly Material DepthMaterial;
    public static Mesh cubeMesh;

    static Globals()
    {
        Assimp = new AssimpContext();
        var shader = new ShaderProgram("../../../resources/shader/depth.glsl");
        DepthMaterial = new Material(shader, Program.MainWindow ?? throw new InvalidOperationException());

        cubeMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/cube.obj");
        cubeMesh.IsSkybox(true);
    }
}