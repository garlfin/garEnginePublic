using System.Windows.Forms;
using OpenTK.Windowing.Common;

namespace gESilk.engine.components;

public class Component
{
    public Entity? Entity { get; set; } = null!;

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
        Components.Add(component);
    }
    public static void Update(float gameTime)
    {
        foreach (T component in Components) component.Update(gameTime);
    }

    public static void UpdateMouse(MouseMoveEventArgs args)
    {
        foreach (T component in Components) component.UpdateMouse(args);
    }
}