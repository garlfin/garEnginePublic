using garEngine.ecs_sys.component;
using garEngine.ecs_sys.entity;
using OpenTK.Mathematics;

namespace garEngine.ecs_sys.system;

class BaseSystem<T> where T : Component
{
    public static List<T> Components = new List<T>();

    public static void Register(T component)
    {
        Components.Add(component);
    }

    public static void Update(float gameTime)
    {
        foreach (T component in Components)
        {
            component.Update(gameTime);
        }
    }

    public static void UpdateMouse()
    {
        foreach (T component in Components)
        {
            component.UpdateMouse();
        }

    }
    
    public static void UpdateShadow()
    {
        foreach (T component in Components)
        {
            component.UpdateShadow();
        }

    }
    

    public static void Close()
    {
        foreach (T component in Components)
        {
            component.Close();
        }
    }
}

class TransformSystem : BaseSystem<Transform> { }

class ModelRendererSystem : BaseSystem<ModelRenderer>
{
}

class MovementSystem : BaseSystem<Movement> {}
class CameraSystem : BaseSystem<Camera>
{
    public static Entity currentCamera { get; set; }
}
