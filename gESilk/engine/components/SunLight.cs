namespace gESilk.engine.components;

public class SunLight : Light
{
    public void Set()
    {
        LightSystem.Sun = this;
    }
}