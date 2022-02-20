using gESilk.engine.misc;
using gESilk.engine.render.assets.textures;
using OpenTK.Mathematics;
using ClearBufferMask = OpenTK.Graphics.OpenGL.ClearBufferMask;
using DrawBufferMode = OpenTK.Graphics.OpenGL.DrawBufferMode;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.FramebufferAttachment;
using FramebufferTarget = OpenTK.Graphics.OpenGL.FramebufferTarget;
using GenerateMipmapTarget = OpenTK.Graphics.OpenGL.GenerateMipmapTarget;
using GL = OpenTK.Graphics.OpenGL.GL;
using RenderbufferStorage = OpenTK.Graphics.OpenGL.RenderbufferStorage;
using RenderbufferTarget = OpenTK.Graphics.OpenGL.RenderbufferTarget;
using TextureTarget = OpenTK.Graphics.OpenGL.TextureTarget;

namespace gESilk.engine.components;

public class CubemapCapture : BaseCamera
{
    private EmptyCubemapTexture _texture;
    

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
            0 => new Vector3(0,0,0), // posx
            1 => new Vector3(0,180,0), // negx
            2 => new Vector3(0,90,0), // posy
            3 => new Vector3(0,-90,0), // negy
            4 => new Vector3(90,0,0), // posz
            5 => new Vector3(-90,0,0), //negz
            _ => Vector3.Zero
        };
    }

    public override void Update(float gameTime)
    {
        int _fbo, _rbo;
        _fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

        _rbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, _texture.Width, _texture.Height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _rbo);
        
        GL.Viewport(0,0, _texture.Width, _texture.Height);
        
        _camera.Position = Entity.GetComponent<Transform>().Location;
        _camera.Fov = 90;

        BaseCamera camera = CameraSystem.CurrentCamera;
        Set();

        Transform entityTransform = Entity.GetComponent<Transform>();
        
        for (int i = 0; i < 6; i++)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX+i, _texture.Get(), 0);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            entityTransform.Rotation = GetAngle(i);
            _camera.Pitch = entityTransform.Rotation.X;
            _camera.Yaw = entityTransform.Rotation.Y;
            View = _camera.GetViewMatrix();
            Projection = _camera.GetProjectionMatrix();
            ModelRendererSystem.Update(0f);
            CubemapMManager.Update(0f);
            
            
        }

        GL.BindTexture(TextureTarget.TextureCubeMap ,_texture.Get());
        GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        
        GL.DeleteFramebuffer(_fbo);
        GL.DeleteRenderbuffer(_rbo);
        
        camera.Set();
    }

    public Entity GetOwner()
    {
        return Entity;
    }
}

class CubemapCaptureManager : BaseSystem<CubemapCapture>
{
    public static CubemapCapture GetNearest(Vector3 currentLocation)
    {
        CubemapCapture nearest = Components[0];
        float minDistance = Vector3.Distance(Components[0].Entity.GetComponent<Transform>().Location, currentLocation);
        foreach (var item in Components)
        {
            float distance = Vector3.Distance(item.Entity.GetComponent<Transform>().Location, currentLocation);
            if (distance < minDistance)
            {
                nearest = item;
                minDistance = distance;
            }
        }
        return nearest!;
    }
}