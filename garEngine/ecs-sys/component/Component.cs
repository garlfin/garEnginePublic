using garEngine.ecs_sys.entity;
using OpenTK.Mathematics;

namespace garEngine.ecs_sys.component;

public class Component
{

        public Entity entity { get; set; }

        public virtual void Update(float gameTime) { }

        public virtual void UpdateMouse() {}

        public virtual void Close() {}

}