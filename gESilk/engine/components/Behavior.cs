namespace gESilk.engine.components;

public class Behavior : Component
{
    protected Behavior()
    {
        BehaviorSystem.Register(this);
    }
}

class BehaviorSystem : BaseSystem<Behavior>
{
    
}