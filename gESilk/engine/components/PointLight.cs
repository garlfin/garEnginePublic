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

    private static Matrix4 GetLookAt(Vector3 location, int i)
    {
        return i switch
        {
            0 => Matrix4.LookAt(location, location + Vector3.UnitX, -Vector3.UnitY), // posx
            1 => Matrix4.LookAt(location, location - Vector3.UnitX, -Vector3.UnitY), // negx
            2 => Matrix4.LookAt(location, location + Vector3.UnitY, Vector3.UnitZ), // posy
            3 => Matrix4.LookAt(location, location - Vector3.UnitY, -Vector3.UnitZ), // negy
            4 => Matrix4.LookAt(location, location + Vector3.UnitZ, -Vector3.UnitY), // posz
            5 => Matrix4.LookAt(location, location - Vector3.UnitZ, -Vector3.UnitY), //negz
        };
    }

    public override void UpdateShadowMatrices()
    {
        _buffer = new FrameBuffer(_size, _size);
        Set();

        for (int i = 0; i < 6; i++)
        {
            LightSystem.ShadowView = GetLookAt(Owner.GetComponent<Transform>().Location, i);
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