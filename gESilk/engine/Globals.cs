using Assimp;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.misc;
using gESilk.engine.render;
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
    

    static Globals()
    {
        Assimp = new AssimpContext();
        Camera = new BasicCamera(Vector3.Zero, (float)1280 / 720);
        ShadowCamera = new BasicCamera(new Vector3(10, 10, 10), 1f);
        ShadowCamera.DepthFar = 50f;
        ShaderProgram depthProgram = new ShaderProgram("../../../shader/depth.shader");
        DepthMaterial = new Material(depthProgram);
    }

    public static void UpdateRender(bool isShadow = false)
    {
        if (isShadow)
        {
            var currentCameraPos = CameraSystem.CurrentCamera!.GetComponent<Transform>()!.Location;
            ShadowCamera.Position = new Vector3(10, 10, 10) + currentCameraPos;
            View = ShadowCamera.GetViewMatrix(currentCameraPos);
            Projection = ShadowCamera.GetOrthoProjectionMatrix();
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