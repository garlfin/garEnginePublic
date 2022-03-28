using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class PointLight : Light
{
    public float Power;
    public float Radius = 1f;
    public Vector3 Color = Vector3.One;
    private int _shadowSize;
    private EmptyCubemapTexture _shadowMap;
    private RenderBuffer _buffer;

    public PointLight(float power, int shadowSize = 1024)
    {
        Power = power;
        _shadowSize = shadowSize;
        LightSystem.Register(this);
        _shadowMap = new EmptyCubemapTexture(_shadowSize, false, PixelInternalFormat.DepthComponent, PixelFormat.DepthComponent);
        _buffer = new RenderBuffer(_shadowSize, _shadowSize);
    }
    
    private Vector3 GetAngle(int index)
    {
        return index switch
        {
            0 => new Vector3(1, 0, 0), // pos x
            1 => new Vector3(-1, 0, 0), // neg x
            2 => new Vector3(0, 1, 0), // pos y
            3 => new Vector3(0, -1, 0), // neg y
            4 => new Vector3(0, 0, 1), // pos z
            5 => new Vector3(0, 0, -1), // neg z
            _ => Vector3.Zero
        };
    }

    public override void UpdateMatrices(int i)
    {
        Set();
        LightSystem.ShadowView = Matrix4.LookAt(Owner.GetComponent<Transform>().Location, Owner.GetComponent<Transform>().Location + GetAngle(i),
            i is 2 or 3 ? i is 2 ? Vector3.UnitZ : -Vector3.UnitZ : -Vector3.UnitY); // this is a mess holy moley im too lazy to change it 
        LightSystem.ShadowProjection =
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 50f);
    }

    public void RenderShadow()
    {
        TransformSystem.Update(0f);
        _buffer.Bind();
        GL.ColorMask(false, false, false, false);
        for (int i = 0; i < 6; i++)
        {
            _shadowMap.BindToBuffer(_buffer, FramebufferAttachment.DepthAttachment, TextureTarget.TextureCubeMapPositiveX + i);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            UpdateMatrices(i);
            ModelRendererSystem.Update(0f);
        }
        
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        _buffer.Delete();
        AssetManager.Remove(_buffer);
        GL.ColorMask(true, true, true, true);
    }

    public Texture getShadowMap()
    {
        return _shadowMap;
    }
    
}