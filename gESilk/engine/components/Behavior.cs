namespace gESilk.engine.components;

public class Behavior : Component
{
    protected Behavior()
    {
        BehaviorSystem.Register(this);
    }
}

internal class BehaviorSystem : BaseSystem<Behavior>
{
}