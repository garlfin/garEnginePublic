using Assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets;
using OpenTK.Mathematics;
using Material = gESilk.engine.render.materialSystem.Material;

namespace gESilk.engine;

public static class Globals
{
    public static readonly AssimpContext Assimp;
    private static readonly BasicCamera _shadowCamera;
    public static Matrix4 ShadowView, ShadowProjection;
    public static readonly Material DepthMaterial;
    public static Vector3 SunPos;

    static Globals()
    {
        Assimp = new AssimpContext();
        _shadowCamera = new BasicCamera(new Vector3(10, 10, 10), 1f)
        {
            DepthFar = 50f
        };
        var depthProgram = new ShaderProgram("../../../resources/shader/depth.glsl");
        DepthMaterial = new Material(depthProgram, Program.MainWindow);
    }

    public static void UpdateShadow()
    {
        SunPos.Normalize();
        var currentCameraPos = CameraSystem.CurrentCamera.Entity.GetComponent<Transform>().Location;
        _shadowCamera.Position = currentCameraPos + new Vector3(10);
        ShadowView = _shadowCamera.GetViewMatrix(currentCameraPos - SunPos + new Vector3(10));
        ShadowProjection = _shadowCamera.GetOrthoProjectionMatrix(20f);
        
    }
}