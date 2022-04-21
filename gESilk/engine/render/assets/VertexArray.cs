using gESilk.engine.assimp;
using gESilk.engine.window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.render.assets;

public class VertexArray : Asset
{
    private readonly int _ebo;
    private readonly int _elementCount;
    private readonly int _nmvbo;
    private readonly int _tanvbo;
    private int _vao;
    private readonly int _vbo;
    private readonly int _vtvbo;
    private Application _application;

    public VertexArray(MeshData mesh, Application application)
    {
        _application = application;
        AssetManager.Register(this);

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = CreateBufferAttribute(mesh.Vert, BufferUsageHint.StaticCopy, VertexAttribPointerType.Float,
            0);
        _nmvbo = CreateBufferAttribute(mesh.Normal, BufferUsageHint.StaticCopy, VertexAttribPointerType.Float,
            1);
        _vtvbo = CreateBufferAttribute(mesh.TexCoord, BufferUsageHint.StaticCopy, VertexAttribPointerType.Float,
            2);
        _tanvbo = CreateBufferAttribute(mesh.Tangent, BufferUsageHint.StaticCopy, VertexAttribPointerType.Float,
            3);
        _ebo = CreateBuffer(mesh.Faces, BufferUsageHint.StaticCopy, BufferTarget.ElementArrayBuffer);
        _elementCount = mesh.Faces.Count;
    }

    private static int CreateBufferAttribute(List<Vector3> data, BufferUsageHint usageArb,
        VertexAttribPointerType type, uint index, BufferTarget target = BufferTarget.ArrayBuffer)
    {
        var buffer = GL.GenBuffer();
        GL.BindBuffer(target, buffer);
        GL.BufferData(target, sizeof(float) * data.Count * 3, data.ToArray(), usageArb);
        GL.VertexAttribPointer(index, 3, type, false, 0, 0);
        GL.EnableVertexAttribArray(index);
        return buffer;
    }

    private static int CreateBufferAttribute(List<Vector2> data, BufferUsageHint usageArb,
        VertexAttribPointerType type, uint index, BufferTarget target = BufferTarget.ArrayBuffer)
    {
        var buffer = GL.GenBuffer();
        GL.BindBuffer(target, buffer);
        GL.BufferData(target, sizeof(float) * data.Count * 2, data.ToArray(), usageArb);
        GL.VertexAttribPointer(index, 2, type, false, 0, 0);
        GL.EnableVertexAttribArray(index);
        return buffer;
    }

    private static int CreateBuffer(List<IntVec3> data, BufferUsageHint usageArb,
        BufferTarget target = BufferTarget.ArrayBuffer)
    {
        var buffer = GL.GenBuffer();
        GL.BindBuffer(target, buffer);
        GL.BufferData(target, sizeof(int) * data.Count * 3, data.ToArray(), usageArb);
        return buffer;
    }

    public void Render()
    {
        GL.BindVertexArray(_vao);
        if (_application.AppState is EngineState.RenderLinearShadowState)
            GL.DrawElementsInstanced(PrimitiveType.Triangles, _elementCount * 3, DrawElementsType.UnsignedInt,
                IntPtr.Zero, 6);
        else
            GL.DrawElements(PrimitiveType.Triangles, _elementCount * 3, DrawElementsType.UnsignedInt, 0);
    }

    public override void Delete()
    {
        if (_vao == -1) return;
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteBuffer(_nmvbo);
        GL.DeleteBuffer(_tanvbo);
        GL.DeleteBuffer(_vtvbo);
        _vao = -1;
    }
}