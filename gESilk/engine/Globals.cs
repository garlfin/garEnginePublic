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
    public static Matrix4 View, Projection;
    public static GameWindow Window;
    public static readonly Material DepthMaterial;

    static Globals()
    {
        Assimp = new AssimpContext();
        Camera = new BasicCamera(Vector3.Zero, (float)1280 / 720);
        ShaderProgram depthProgram = new ShaderProgram("../../../shader/depth.shader");
        DepthMaterial = new Material(depthProgram, DepthFunction.Less);
    }

    public static void UpdateRender()
    {
        View = Camera.GetViewMatrix();
        Projection = Camera.GetProjectionMatrix();
    }
}