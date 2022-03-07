using OpenTK.Windowing.Common;

namespace gESilk.engine.components;

public class Component
{
    public Entity Owner = null!;

    public virtual void Update(float gameTime)
    {
    }

    public virtual void UpdateMouse(MouseMoveEventArgs args)
    {
    }
}

internal class BaseSystem<T> where T : Component
{
    public static readonly List<T> Components = new();

    public static void Register(T component)
    {
        if (!Components.Contains(component)) Components.Add(component);
        else Console.WriteLine("Cannot add same component");
    }

    public static void Update(float gameTime)
    {
        foreach (var component in Components) component.Update(gameTime);
    }

    public static void UpdateMouse(MouseMoveEventArgs args)
    {
        foreach (var component in Components) component.UpdateMouse(args);
    }
}