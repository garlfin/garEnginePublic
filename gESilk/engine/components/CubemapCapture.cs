using gESilk.engine.misc;
using gESilk.engine.render.assets.textures;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class CubemapCapture : BaseCamera
{
    private readonly EmptyCubemapTexture _texture;


    public CubemapCapture(EmptyCubemapTexture texture)
    {
        CubemapCaptureManager.Register(this);
        _texture = texture;
        _camera = new BasicCamera(new Vector3(0), 1f);
    }

    public Texture Get()
    {
        return _texture;
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
        Set();

        Globals.UpdateShadow();
        TransformSystem.Update(0f);
        Owner!.Application.State(EngineState.RenderShadowState);
        Owner!.Application.ShadowMap.Bind(ClearBufferMask.DepthBufferBit);
        ModelRendererSystem.Update(0f);
        Owner.Application.State(EngineState.GenerateCubemapState);

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

        _camera.Position = Owner.GetComponent<Transform>().Location;
        _camera.Fov = 90;

        var entityTransform = Owner.GetComponent<Transform>();

        for (var i = 0; i < 6; i++)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.TextureCubeMapPositiveX + i, _texture.Get(), 0);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            View = Matrix4.LookAt(entityTransform.Location, entityTransform.Location + GetAngle(i),
                i is 2 or 3 ? i is 2 ? Vector3.UnitZ : -Vector3.UnitZ : -Vector3.UnitY);
            Projection = _camera.GetProjectionMatrix();

            ModelRendererSystem.Update(0f);
            CubemapMManager.Update(0f);
        }

        GL.BindTexture(TextureTarget.TextureCubeMap, _texture.Get());
        GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        GL.DeleteFramebuffer(fbo);
        GL.DeleteRenderbuffer(_rbo);

        camera.Set();
    }

    public Entity GetOwner()
    {
        return Owner;
    }
}



internal class CubemapCaptureManager : BaseSystem<CubemapCapture>
{
    private static bool IsInBounds(Vector3 box, Vector3 position)
    {
        return box.X < position.X || box.Y < position.Y || box.Z < position.Z;
    }
    public static CubemapCapture GetNearest(Vector3 currentLocation)
    {
        foreach (var item in Components)
        {
            var itemTransform = item.Owner.GetComponent<Transform>();
            if (IsInBounds(itemTransform.Location - itemTransform.Scale, currentLocation) && !IsInBounds(itemTransform.Location + itemTransform.Scale, currentLocation) ) return item;
        }
        
        Console.WriteLine("Not in any bounds, falling back to nearest.");
        
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