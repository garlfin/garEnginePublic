using Assimp;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets;
using OpenTK.Mathematics;
using Material = gESilk.engine.render.materialSystem.Material;
using Mesh = gESilk.engine.render.assets.Mesh;

namespace gESilk.engine;

public static class Globals
{
    public static readonly AssimpContext Assimp;
    private static readonly BasicCamera _shadowCamera;
    public static Matrix4 ShadowView, ShadowProjection;
    public static readonly Material DepthMaterial;
    public static Mesh cubeMesh;
    public static Vector3 SunPos;

    static Globals()
    {
        Assimp = new AssimpContext();
        _shadowCamera = new BasicCamera(new Vector3(10, 10, 10), 1f)
        {
            DepthFar = 50f
        };
        var shader = new ShaderProgram("../../../resources/shader/depth.glsl");
        DepthMaterial = new Material(shader, Program.MainWindow);

        cubeMesh = AssimpLoader.GetMeshFromFile("../../../resources/models/cube.obj");
        cubeMesh.IsSkybox(true);
    }

    public static void UpdateShadow()
    {
        SunPos.Normalize();
        var currentCameraPos = CameraSystem.CurrentCamera.Owner.GetComponent<Transform>().Location;
        _shadowCamera.Position = currentCameraPos + new Vector3(10);
        ShadowView = _shadowCamera.GetViewMatrix(currentCameraPos - SunPos + new Vector3(10));
        ShadowProjection = _shadowCamera.GetOrthoProjectionMatrix(20f);
    }
}