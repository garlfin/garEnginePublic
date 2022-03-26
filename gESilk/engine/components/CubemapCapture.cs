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


    public CubemapCapture(int size)
    {
        CubemapCaptureManager.Register(this);
        _texture = new EmptyCubemapTexture(size);
        _texturePong = new EmptyCubemapTexture(size);
        _irradiance = new EmptyCubemapTexture(32, false);
        _irradiancePong = new EmptyCubemapTexture(32, false);
        _camera = new BasicCamera(new Vector3(0), 1f);
    }

    public Texture Get()
    {
        return Owner.Application.State() is EngineState.GenerateCubemapState or EngineState.IterationCubemapState
            ? _texture
            : _texturePong; // texturePong is the 2nd iteration
    }


    public Texture GetIrradiance()
    {
        return Owner.Application.State() is EngineState.GenerateCubemapState or EngineState.IterationCubemapState
            ? _irradiance
            : _irradiancePong; // texturePong is the 2nd iteration
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
        TransformSystem.Update(0f);
        Owner.Application.State(EngineState.RenderShadowState);
        Owner.Application.ShadowMap.Bind(ClearBufferMask.DepthBufferBit);
        ModelRendererSystem.Update(0f);
        Owner.Application.State(previousState);

        int _rbo;
        var fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

        _rbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, _texture.Width,
            _texture.Height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, _rbo);

        GL.Viewport(0, 0, _texture.Width, _texture.Height);


        var entityTransform = Owner.GetComponent<Transform>();

        for (var i = 0; i < 6; i++)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.TextureCubeMapPositiveX + i,
                Owner.Application.State() is EngineState.GenerateCubemapState ? _texture.Get() : _texturePong.Get(),
                0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            View = Matrix4.LookAt(entityTransform.Location, entityTransform.Location + GetAngle(i),
                i is 2 or 3 ? i is 2 ? Vector3.UnitZ : -Vector3.UnitZ : -Vector3.UnitY);
            Projection = _camera.GetProjectionMatrix();

            ModelRendererSystem.Update(0f);
            CubemapMManager.Update(0f);
        }


        GL.BindTexture(TextureTarget.TextureCubeMap,
            Owner.Application.State() is EngineState.GenerateCubemapState ? _texture.Get() : _texturePong.Get());
        GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        GL.DeleteFramebuffer(fbo);
        GL.DeleteRenderbuffer(_rbo);

        camera.Set();

        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Always);

        var renderBuffer = new RenderBuffer(32, 32);
        renderBuffer.Bind();

        var program = new ShaderProgram("../../../resources/shader/irradiance.glsl");

        program.Use();
        program.SetUniform("environmentMap", Get().Use(0));
        program.SetUniform("model", Matrix4.Identity);
        program.SetUniform("projection",
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 100f));

        for (var i = 0; i < 6; i++)
        {
            (Owner.Application.State() is EngineState.GenerateCubemapState ? _irradiance : _irradiancePong)
                .BindToBuffer(renderBuffer, FramebufferAttachment.ColorAttachment0,
                    TextureTarget.TextureCubeMapPositiveX + i);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            program.SetUniform("view", Matrix4.LookAt(Vector3.Zero, Vector3.Zero + GetAngle(i),
                i is 2 or 3 ? i is 2 ? Vector3.UnitZ : -Vector3.UnitZ : -Vector3.UnitY));

            Globals.cubeMesh.Render();
        }

        renderBuffer.Delete();
        AssetManager.Remove(renderBuffer);
        program.Delete();
        AssetManager.Remove(program);

        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Less);
    }

    public void DeleteOldAssets()
    {
        AssetManager.Remove(_texture);
        _texture.Delete();
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

        //Console.WriteLine("Not in any bounds, falling back to nearest.");

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