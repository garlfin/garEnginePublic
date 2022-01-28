namespace gESilk.engine.components;

public class Component
{
    public Entity? Entity { get; set; } = null!;

    public virtual void Update(float gameTime)
    {
    }

    public virtual void Update(bool isShadow)
    {
    }

    public virtual void UpdateMouse()
    {
    }
}

internal class BaseSystem<T> where T : Component
{
    public static readonly List<T> Components = new();

    public static void Register(T component)
    {
        Components.Add(component);
    }

    public static void Update(bool isShadow)
    {
        foreach (var component in Components) component.Update(isShadow);
    }

    public static void Update(float gameTime)
    {
        foreach (var component in Components) component.Update(gameTime);
    }

    public static void UpdateMouse()
    {
        foreach (var component in Components) component.UpdateMouse();
    }
}