using gESilk.engine.misc;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class Light : Component
{
}

class LightSystem : BaseSystem<PointLight>
{
    public static SunLight Sun;

    public static readonly BasicCamera ShadowCamera;
    public static Matrix4 ShadowView;
    public static Matrix4 ShadowProjection;

    public static Vector3 SunPos;

    static LightSystem()
    {
        ShadowCamera = new BasicCamera(new Vector3(10, 10, 10), 1f)
        {
            DepthFar = 50f
        };
    }

    public static void UpdateShadow()
    {
        SunPos = Sun.Owner.GetComponent<Transform>().Location;

        SunPos.Normalize();
        var currentCameraPos = CameraSystem.CurrentCamera.Owner.GetComponent<Transform>().Location;
        ShadowCamera.Position = currentCameraPos + new Vector3(0, 25, 0);
        ShadowView = ShadowCamera.GetViewMatrix(currentCameraPos - SunPos + new Vector3(0, 25, 0));
        ShadowProjection = ShadowCamera.GetOrthoProjectionMatrix(20f);
    }
}