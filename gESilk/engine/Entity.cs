using gESilk.engine.components;

namespace gESilk.engine;

public class Entity
{
        readonly List<Component> _components = new List<Component>();
        
        public void AddComponent(Component component)
        {
                component.Entity = this;
                _components.Add(component);
                
        }
        
        public T? GetComponent<T>() where T : Component
        {
                foreach (Component component in _components)
                {
                        if (component.GetType() == typeof(T))
                        {
                                return (T)component;
                        }
                }
                return null;
        }
    
}