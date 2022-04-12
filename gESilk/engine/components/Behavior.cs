namespace gESilk.engine.components;

public class Behavior : Component
{
    protected Behavior()
    {
        BehaviorSystem.Register(this);
    }

    public virtual void UpdateRender(float gameTime)
    {
        
    }
}
class BehaviorSystem : BaseSystem<Behavior>
{
    public static void UpdateRender(float gameTime)
    {
        foreach (var component in Components)
        {
            component.UpdateRender(gameTime);
        }
    }
}