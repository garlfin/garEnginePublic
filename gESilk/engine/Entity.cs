using gESilk.engine.components;
using gESilk.engine.render.assets;
using gESilk.engine.window;

namespace gESilk.engine;

public class Entity
{
    private readonly List<Component> _components = new();
    public readonly string Name;
    public Application Application;

    public Entity(Application application, string name = "Entity")
    {
        Name = name;
        Application = application;
    }

    public void AddComponent(Component component)
    {
        component.Owner = this;
        _components.Add(component);
    }

    public T? GetComponent<T>() where T : Component
    {
        foreach (var component in _components)
            if (component.GetType() == typeof(T))
                return (T) component;
        return null;
    }
}