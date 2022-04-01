using gESilk.engine.misc;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ClearBufferMask = OpenTK.Graphics.OpenGL.ClearBufferMask;
using DrawBufferMode = OpenTK.Graphics.OpenGL.DrawBufferMode;
using GL = OpenTK.Graphics.OpenGL.GL;
using ReadBufferMode = OpenTK.Graphics.OpenGL.ReadBufferMode;

namespace gESilk.engine.components;

public class PointLight : Light
{
    public float Power;
    public float Radius = 1f;
    public Vector3 Color = Vector3.One;

    private FrameBuffer _buffer;
    private EmptyCubemapTexture _texture;
    private int _size;

    public PointLight(float power, int width = 1024)
    {
        Power = power;
        LightSystem.Register(this);
        _size = width;
        _texture = new EmptyCubemapTexture(width, false, PixelInternalFormat.DepthComponent24,
            PixelFormat.DepthComponent);
    }

    public EmptyCubemapTexture GetShadowMap()
    {
        return _texture;
    }

    public override void UpdateShadowMatrices()
    {
        GL.Viewport(0, 0, _size, _size);
        _buffer = new FrameBuffer(_size, _size);
        Set();

        for (int i = 0; i < 6; i++)
        {
            LightSystem.ShadowView = MiscMath.GetLookAt(Owner.GetComponent<Transform>().Location, i);
            LightSystem.ShadowProjection =
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 100f);

            _texture.BindToBuffer(_buffer, FramebufferAttachment.DepthAttachment,
                TextureTarget.TextureCubeMapPositiveX + i, 0);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            ModelRendererSystem.Update(0f);
        }

        _buffer.Delete();
        AssetManager.Register(_buffer);
    }
}