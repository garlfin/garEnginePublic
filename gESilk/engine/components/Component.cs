namespace gESilk.engine.components;

public class Component
{
        public Entity Entity { get; set; } = null!;
        public virtual void Update(float gameTime) { }
        
        public virtual void Update(bool isShadow) { }
        public virtual void UpdateMouse() {}

}

class BaseSystem<T> where T : Component
{
        public static readonly List<T> Components = new List<T>();

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
}
