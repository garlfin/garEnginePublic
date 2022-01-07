using garEngine.ecs_sys.entity;

namespace garEngine.ecs_sys.component;

public class Component
{

        public Entity entity { get; set; }

        public virtual void Update(float gameTime) { }

        public virtual void UpdateMouse() {}

        public virtual void Close() {}

        public virtual void UpdateDepth(bool isShadow) { }

}