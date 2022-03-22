using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class PointLight : Light
{
    public float Power;
    public float Radius = 1f;
    public Vector3 Color = Vector3.One;

    public PointLight(float power)
    {
        Power = power;
        LightSystem.Register(this);
    }
}