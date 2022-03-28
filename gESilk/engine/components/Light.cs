using gESilk.engine.misc;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Light : Component
{
    public virtual void UpdateMatrices(int iteration)
    {
        Vector3 sunPos = Owner.GetComponent<Transform>().Location.Normalized();
        var currentCameraPos = CameraSystem.CurrentCamera.Owner.GetComponent<Transform>().Location;
        LightSystem.ShadowView = Matrix4.LookAt(currentCameraPos + new Vector3(0, 25, 0), currentCameraPos - sunPos + new Vector3(0, 25, 0), Vector3.UnitZ);
        LightSystem.ShadowProjection = Matrix4.CreateOrthographicOffCenter(-20, 20, -20, 20, 0.1f, 100f);
    }
    
    public virtual void Set()
    {
        LightSystem.emitterLight = this;
    }
}

class LightSystem : BaseSystem<PointLight>
{
    public static Light emitterLight;
    
    public static Matrix4 ShadowView;
    public static Matrix4 ShadowProjection;

    public static void UpdateShadow(int iteration)
    {
        emitterLight.UpdateMatrices(iteration);
    }
}