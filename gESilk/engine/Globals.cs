using Assimp;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render;
using gESilk.engine.render.assets;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Material = Assimp.Material;

namespace gESilk.engine;

using render.materialSystem;

public static class Globals
{
    public static AssimpContext Assimp;
    public static BasicCamera Camera;
    public static BasicCamera ShadowCamera;
    public static Matrix4 View, Projection, ShadowView, ShadowProjection;
    public static GameWindow Window;
    public static readonly Material DepthMaterial;
    public static Vector3 SunPos;
    

    static Globals()
    {
        Assimp = new AssimpContext();
        Camera = new BasicCamera(Vector3.Zero, (float)1280 / 720);
        ShadowCamera = new BasicCamera(new Vector3(10, 10, 10), 1f)
        {
            DepthFar = 50f
        };
        ShaderProgram depthProgram = new ShaderProgram("../../../resources/shader/depth.shader");
        DepthMaterial = new Material(depthProgram);
    }

    public static void UpdateRender(bool isShadow = false)
    {
        if (isShadow)
        {
            var currentCameraPos = CameraSystem.CurrentCamera!.GetComponent<Transform>()!.Location;
            ShadowCamera.Position = SunPos.Normalized() + currentCameraPos + new Vector3(0,20,0);
            View = ShadowCamera.GetViewMatrix(currentCameraPos + new Vector3(0,20,0));
            Projection = ShadowCamera.GetOrthoProjectionMatrix(20f);
            ShadowProjection = Projection;
            ShadowView = View;
        }
        else
        {
            View = Camera.GetViewMatrix();
            Projection = Camera.GetProjectionMatrix();
        }
    }
}