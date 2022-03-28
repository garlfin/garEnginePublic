using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Light : Component
{
    public void Set()
    {
        LightSystem.CurrentLight = this;
    }

    public virtual void UpdateShadowMatrices()
    {
        var SunPos = Owner.GetComponent<Transform>().Location.Normalized();
        var currentCameraPos = CameraSystem.CurrentCamera.Owner.GetComponent<Transform>().Location;
        LightSystem.ShadowView = Matrix4.LookAt(currentCameraPos + new Vector3(0, 25, 0),
            currentCameraPos - SunPos + new Vector3(0, 25, 0), Vector3.UnitZ);
        LightSystem.ShadowProjection = Matrix4.CreateOrthographic(40f, 40f, 0.1f, 100f);
    }

    public virtual void UpdateShadowMatrices(int index)
    {
    }
}

class LightSystem : BaseSystem<PointLight>
{
    public static Light CurrentLight;

    public static Matrix4 ShadowView;
    public static Matrix4 ShadowProjection;

    static LightSystem()
    {
    }

    public static void UpdateShadow()
    {
        CurrentLight.UpdateShadowMatrices();
    }
}