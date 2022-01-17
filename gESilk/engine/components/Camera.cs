using Silk.NET.Maths;

namespace gESilk.engine.components;

public class Camera : Component
{
    public float fov { get; set; } = 4; // Radians (Vertical not horizontal)
    public float clipStart { get; set; } = 0.1f;
    public float clipEnd { get; set; } = 1000f;
    
    private Vector2D<float> _lastPos = Vector2D<float>.Zero;
    private bool _firstMove = true;
    

    private float _cameraSpeed = 25;

    private Transform _entityTransform;
    
    private float _sensitivity = 1f;
    
    public Camera(float fov_, float clipStart_, float clipEnd_, float sensitivity)
    {
        _sensitivity = sensitivity * 0.1f;
        fov = fov_;
        clipStart = clipStart_;
        clipEnd = clipEnd_;
        CameraSystem.Register(this);
    }

    public void Set()
    {
        CameraSystem.currentCamera = Entity;
    }
    public override void Update(float gameTime)
    {
    }

    public override void UpdateMouse()
    {
    }
}

class CameraSystem : BaseSystem<Camera>
{
    public static Entity currentCamera;
}