using System.Data;
using Assimp;
using garEngine.render.utility;
using gESilk.engine.components;
using OpenTK.Mathematics;
using Camera = gESilk.engine.components.Camera;

namespace gESilk.engine;


public static class Globals
{
    public static AssimpContext Assimp;
    public static BasicCamera _camera;
    public static Matrix4 view, projection;

    static Globals()
    {
        Assimp = new AssimpContext();
        _camera = new BasicCamera(Vector3.Zero, (float)1280/720);
    }

    public static void Update()
    {
        _camera.Position = CameraSystem.currentCamera.GetComponent<Transform>()!.Location;
        _camera.Fov = CameraSystem.currentCamera.GetComponent<Camera>()!.fov;
        _camera.depthFar = CameraSystem.currentCamera.GetComponent<Camera>()!.clipEnd;
        _camera.depthNear = CameraSystem.currentCamera.GetComponent<Camera>()!.clipStart;
    }

    public static void UpdateRender()
    {
        view = _camera.GetViewMatrix();
        projection = _camera.GetProjectionMatrix();
    }
}