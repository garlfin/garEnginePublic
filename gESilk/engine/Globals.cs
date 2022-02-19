using Assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render.assets;
using OpenTK.Mathematics;

namespace gESilk.engine;

using render.materialSystem;

public static class Globals
{
    public static AssimpContext Assimp;
    public static BasicCamera ShadowCamera;
    public static Matrix4 ShadowView, ShadowProjetion;
    public static readonly Material DepthMaterial;
    public static Vector3 SunPos;

    static Globals()
    {
        Assimp = new AssimpContext();
        ShadowCamera = new BasicCamera(new Vector3(10, 10, 10), 1f)
        {
            DepthFar = 50f
        };
        var depthProgram = new ShaderProgram("../../../resources/shader/depth.shader");
        DepthMaterial = new Material(depthProgram);
    }

    public static void UpdateShadow()
    {
        var currentCameraPos = CameraSystem.CurrentCamera.Entity.GetComponent<Transform>().Location;
        ShadowCamera.Position = SunPos.Normalized() + currentCameraPos + new Vector3(0, 20, 0);
        ShadowView = ShadowCamera.GetViewMatrix(currentCameraPos + new Vector3(0, 20, 0));
        ShadowProjetion = ShadowCamera.GetOrthoProjectionMatrix(20f);
    }
}