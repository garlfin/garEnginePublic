using garEngine.ecs_sys.component;

namespace garEngine.ecs_sys.entity;

public class Entity
{ 
        public int Id { get; set; }

        List<Component> _components = new List<Component>();

        public Entity()
        {
                this.Id = 0;
        }

        public void AddComponent(Component component)
        {
                component.entity = this;
                _components.Add(component);
        }
        public T GetComponent<T>() where T : Component
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