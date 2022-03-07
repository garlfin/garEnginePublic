using gESilk.engine.components;
using gESilk.engine.window;

namespace gESilk.engine;

public class Entity
{
    private readonly List<Component> _components = new();
    public readonly bool IsStatic;
    public readonly string Name;
    public Application Application;

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

    public T GetComponent<T>() where T : Component
    {
        return _components.Where(component => component.GetType() == typeof(T)).Cast<T>().FirstOrDefault();
    }
}

public static class EntityManager
{
    public static List<Entity> Entities = new();

    public static void AddEntity(Entity entity)
    {
        if (!Entities.Contains(entity)) Entities.Add(entity);
    }

    public static void RemoveEntity(Entity entity)
    {
        if (Entities.Contains(entity)) Entities.Remove(entity);
    }
}