using gESilk.engine.components;
using gESilk.engine.window;

namespace gESilk.engine;

public class Entity
{
    private readonly List<Component> _components = new();
    public readonly bool IsStatic;
    public readonly string Name;
    public readonly Application Application;

    public Entity(Application application, string name = "Entity", bool isStatic = true)
    {
        EntityManager.AddEntity(this);
        Name = name;
        Application = application;
        IsStatic = isStatic;
    }

    public void AddComponent(Component component)
    {
        component.Owner = this;
        _components.Add(component);
    }

    public T? GetComponent<T>() where T : Component
    {
        foreach (var component in _components)
        {
            if (typeof(T) == component.GetType()) return (component as T)!;
        }
        return null;
    }

    public Component GetComponent(Type T)
    {
        foreach (var component in _components)
        {
            if (T == component.GetType()) return component;
        }

        return null;
    }
}

public static class EntityManager
{
    public static readonly List<Entity> Entities = new();

    public static void AddEntity(Entity entity)
    {
        if (!Entities.Contains(entity)) Entities.Add(entity);
    }

    public static void RemoveEntity(Entity entity)
    {
        if (Entities.Contains(entity)) Entities.Remove(entity);
    }
}