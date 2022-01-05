using garEngine.ecs_sys.component;
using garEngine.ecs_sys.entity;

namespace garEngine.ecs_sys.system;

class CameraSystem : BaseSystem<Camera>
{
    public static Entity currentCamera { get; set; }
}