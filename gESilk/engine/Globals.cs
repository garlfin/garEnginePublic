using Assimp;
using gESilk.engine.misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace gESilk.engine;

public static class Globals
{
    public static AssimpContext Assimp;
    public static BasicCamera Camera;
    public static Matrix4 View, Projection;
    public static GameWindow Window;

    static Globals()
    {
        Assimp = new AssimpContext();
        Camera = new BasicCamera(Vector3.Zero, (float)1280 / 720);
    }

    public static void UpdateRender()
    {
        View = Camera.GetViewMatrix();
        Projection = Camera.GetProjectionMatrix();
    }
}