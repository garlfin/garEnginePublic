using gESilk.engine.components;

namespace gESilk.engine;

public class Entity
{
    private readonly List<Component> _components = new();

    public void AddComponent(Component component)
    {
        component.Entity = this;
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