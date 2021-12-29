using garEngine.ecs_sys.component;
using garEngine.ecs_sys.entity;
using garEngine.ecs_sys.system;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace garEngine.render;

public static class RenderView
{
    public static BasicCamera _camera;
    public static MyWindow _Window;

    static RenderView()
    {
        _camera = new BasicCamera(Vector3.Zero, (float)1280/720);
    }
    
    public static void Update()
    {
        _camera.Position = CameraSystem.currentCamera.GetComponent<Transform>().Location;
        _camera.Fov = CameraSystem.currentCamera.GetComponent<Camera>().fov;
        _camera.AspectRatio = (float) _Window.Size.X / _Window.Size.Y;
        _camera.depthFar = CameraSystem.currentCamera.GetComponent<Camera>().clipEnd;
        _camera.depthNear = CameraSystem.currentCamera.GetComponent<Camera>().clipStart;
    }
    
}