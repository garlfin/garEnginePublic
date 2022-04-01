using gESilk.engine.misc;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class CubemapCapture : BaseCamera
{
    private readonly EmptyCubemapTexture _texture;
    private readonly EmptyCubemapTexture _texturePong;
    private readonly EmptyCubemapTexture _irradiance;
    private readonly EmptyCubemapTexture _irradiancePong;
    private readonly RenderBuffer _mainRenderBuffer, _renderBuffer;


    public CubemapCapture(int size)
    {
        CubemapCaptureManager.Register(this);
        _texture = new EmptyCubemapTexture(size);
        _texturePong = new EmptyCubemapTexture(size);
        _irradiance = new EmptyCubemapTexture(32, false);
        _irradiancePong = new EmptyCubemapTexture(32, false);
        _camera = new BasicCamera(new Vector3(0), 1f);
        _mainRenderBuffer = new RenderBuffer(size, size);
        _renderBuffer = new RenderBuffer(32, 32);
    }

    public Texture Get()
    {
        return Owner.Application.State() is EngineState.IterationCubemapState
            ? _texture
            : _texturePong; // texturePong is the 2nd iteration
    }


    public Texture GetIrradiance()
    {
        return Owner.Application.State() is EngineState.IterationCubemapState
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
        var camera = CameraSystem.CurrentCamera;

        EngineState previousState = Owner.Application.State();

        _camera.Position = Owner.GetComponent<Transform>().Location;
        _camera.Fov = 90;
        Set();

        LightSystem.UpdateShadow();
        Owner.Application.State(EngineState.RenderShadowState);
        Owner.Application.ShadowMap.Bind(ClearBufferMask.DepthBufferBit);
        ModelRendererSystem.Update(0f);

        Owner.Application.State(previousState);

        _mainRenderBuffer.Bind(null);

        var entityTransform = Owner.GetComponent<Transform>();

        var currentTex = Owner.Application.State() is EngineState.GenerateCubemapState
            ? _texture
            : _texturePong;

        for (var i = 0; i < 6; i++)
        {
            currentTex.BindToBuffer(_mainRenderBuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.TextureCubeMapPositiveX + i);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            View = MiscMath.GetLookAt(entityTransform.Location, i);
            Projection = _camera.GetProjectionMatrix();

            ModelRendererSystem.Update(0f);
            CubemapMManager.Update(0f);
        }

        GL.BindTexture(TextureTarget.TextureCubeMap, currentTex.Get());
        GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
        currentTex.GenerateMipsSpecular(Owner.Application);

        camera.Set();

        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Always);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        _renderBuffer.Bind(null);

        var program = Owner.Application.GetIrradianceProgram();

        program.Use();
        program.SetUniform("environmentMap",
            (Owner.Application.State() is EngineState.GenerateCubemapState ? _texture : _texturePong).Use(0));
        program.SetUniform("model", Matrix4.Identity);
        program.SetUniform("projection",
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 100f));


        currentTex = Owner.Application.State() is EngineState.GenerateCubemapState ? _irradiance : _irradiancePong;

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
        foreach (var item in Components)
        {
            var itemTransform = item.Owner.GetComponent<Transform>();
            if (IsInBounds(itemTransform.Location - itemTransform.Scale, itemTransform.Location + itemTransform.Scale,
                    currentLocation)) return item;
        }

        var nearest = Components[0];
        var minDistance = Vector3.Distance(Components[0].Owner.GetComponent<Transform>().Location, currentLocation);

        foreach (var item in Components)
        {
            var distance = Vector3.Distance(item.Owner.GetComponent<Transform>().Location, currentLocation);
            if (!(distance <= minDistance)) continue;
            nearest = item;
            minDistance = distance;
        }

        return nearest;
    }
}