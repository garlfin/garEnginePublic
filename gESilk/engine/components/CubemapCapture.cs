using gESilk.engine.misc;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class CubemapCapture : BaseCamera
{
    private EmptyCubemapTexture _texture;
    private EmptyCubemapTexture _texturePong;
    private EmptyCubemapTexture _irradiance;
    private EmptyCubemapTexture _irradiancePong;
    private RenderBuffer _mainRenderBuffer, _renderBuffer;
    public Matrix4[] CubemapView = new Matrix4[6];

    public int Size;

    public CubemapCapture()
    {
        CubemapCaptureManager.Register(this);
    }

    public override void Activate()
    {
        _texture = new EmptyCubemapTexture(Size);
        _texturePong = new EmptyCubemapTexture(Size);
        _irradiance = new EmptyCubemapTexture(32, false);
        _irradiancePong = new EmptyCubemapTexture(32, false);
        _camera = new BasicCamera(new Vector3(0), 1f);
        _mainRenderBuffer = new RenderBuffer(Size, Size);
        _renderBuffer = new RenderBuffer(32, 32);
    }

    public Texture Get()
    {
        return Owner.Application.AppState is EngineState.IterationCubemapState
            ? _texture
            : _texturePong; // texturePong is the 2nd iteration
    }


    public Texture GetIrradiance()
    {
        return Owner.Application.AppState is EngineState.IterationCubemapState
            ? _irradiance
            : _irradiancePong; // irradiancePong is the 2nd iteration
    }

    private Vector3 GetAngle(int index)
    {
        return index switch
        {
            0 => new Vector3(1, 0, 0), // posx
            1 => new Vector3(-1, 0, 0), // negx
            2 => new Vector3(0, 1, 0), // posy
            3 => new Vector3(0, -1, 0), // negy
            4 => new Vector3(0, 0, 1), // posz
            5 => new Vector3(0, 0, -1), //negz
            _ => Vector3.Zero
        };
    }

    public override void Update(float gameTime)
    {
        var previousCamera = CameraSystem.CurrentCamera;

        EngineState previousState = Owner.Application.AppState;

        _camera.Position = Owner.GetComponent<Transform>().Location;
        _camera.Fov = 90;
        Set();

        LightSystem.UpdateShadow();
        Owner.Application.AppState = EngineState.RenderShadowState;
        Owner.Application.ShadowMap.Bind(ClearBufferMask.DepthBufferBit);
        ModelRendererSystem.Update(0f);

        Owner.Application.AppState = previousState;

        _mainRenderBuffer.Bind(null);

        var entityTransform = Owner.GetComponent<Transform>();

        var currentTex = Owner.Application.AppState is EngineState.GenerateCubemapState
            ? _texture
            : _texturePong;


        currentTex.BindToBuffer3D(_mainRenderBuffer, FramebufferAttachment.ColorAttachment0);

        for (int i = 0; i < 6; i++)
        {
            CubemapView[i] = MiscMath.GetLookAt(entityTransform.Location, i);
        }

        Projection = _camera.GetProjectionMatrix();

        ModelRendererSystem.Update(0f);
        CubemapMManager.Update(0f);

        GL.BindTexture(TextureTarget.TextureCubeMap, currentTex.Get());
        GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
        currentTex.GenerateMipsSpecular(Owner.Application);

        previousCamera.Set();

        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Always);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        _renderBuffer.Bind(null);

        var program = Owner.Application.IrradianceProgram;

        program.Use();
        program.SetUniform("environmentMap",
            (Owner.Application.AppState is EngineState.GenerateCubemapState ? _texture : _texturePong).Use(0));
        program.SetUniform("model", Matrix4.Identity);
        program.SetUniform("projection",
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 100f));


        currentTex = Owner.Application.AppState is EngineState.GenerateCubemapState ? _irradiance : _irradiancePong;

        for (var i = 0; i < 6; i++)
        {
            currentTex.BindToBuffer(_renderBuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.TextureCubeMapPositiveX + i);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.SetUniform("view", MiscMath.GetLookAt(Vector3.Zero, i));

            Globals.CubeMesh.Render();
        }

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Less);
    }

    public void DeleteOldAssets()
    {
        AssetManager.Remove(_texture);
        _texture.Delete();
        AssetManager.Remove(_irradiance);
        _irradiance.Delete();
        AssetManager.Remove(_renderBuffer);
        _renderBuffer.Delete();
        AssetManager.Remove(_mainRenderBuffer);
        _mainRenderBuffer.Delete();
    }

    public Entity GetOwner()
    {
        return Owner;
    }
}

internal class CubemapCaptureManager : BaseSystem<CubemapCapture>
{
    private static bool IsInBounds(Vector3 box1, Vector3 box2, Vector3 position)
    {
        return position.X < box1.X && position.X > box2.X && position.Y < box1.Y &&
               position.Y > box2.Y && position.Z < box1.Z && position.Z > box2.Z;
    }

    public static CubemapCapture GetNearest(Vector3 currentLocation)
    {
        for (var i = 0; i < Components.Count; i++)
        {
            var item = Components[i];
            var itemTransform = item.Owner.GetComponent<Transform>();
            if (IsInBounds(itemTransform.Location - itemTransform.Scale, itemTransform.Location + itemTransform.Scale,
                    currentLocation)) return item;
        }

        var nearest = Components[0];
        var minDistance = Vector3.Distance(Components[0].Owner.GetComponent<Transform>().Location, currentLocation);

        for (var i = 0; i < Components.Count; i++)
        {
            var item = Components[i];
            var distance = Vector3.Distance(item.Owner.GetComponent<Transform>().Location, currentLocation);
            if (!(distance <= minDistance)) continue;
            nearest = item;
            minDistance = distance;
        }

        return nearest;
    }

    public static List<Tuple<float, CubemapCapture>> ReturnClosestInOrder(Vector3 position)
    {
        List<Tuple<float, CubemapCapture>> list = new List<Tuple<float, CubemapCapture>>();
        foreach (var component in Components)
        {
            list.Add(new Tuple<float, CubemapCapture>(
                Vector3.Distance(component.Owner.GetComponent<Transform>().Location, position), component));
        }

        return list.OrderBy(KeySelector).ToList();

        // I wonder how slow this is...
    }

    private static float KeySelector(Tuple<float, CubemapCapture> i)
    {
        return i.Item1;
    }
}