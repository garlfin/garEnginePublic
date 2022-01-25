using System.Runtime.InteropServices;
using gESilk.engine.assimp;
using OpenTK.Graphics.OpenGL4;
using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class VertexArray : Asset
{
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly int _vtvbo;
    private readonly int _nmvbo;
    private readonly int _tanvbo;
    private readonly int _elementCount;
    
    private static int CreateBufferAttribute(int size, List<float> data, BufferUsageHint usageArb,
        VertexAttribPointerType type, uint index, int sizePerItem = 1)
    {
        int buffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Count, data.ToArray(), usageArb);
        GL.VertexAttribPointer(index, size, type, false, 0, 0);
        GL.EnableVertexAttribArray(index);
        return buffer;
    }

    private static int CreateBuffer(List<float> data, BufferUsageHint usageArb)
    {
        int buffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        GL.BufferData(BufferTarget.ArrayBuffer,sizeof(float) * data.Count, data.ToArray(), usageArb);
        return buffer;
    }
    private static int CreateBuffer(List<int> data, BufferUsageHint usageArb)
    {
        int buffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        GL.BufferData(BufferTarget.ArrayBuffer,sizeof(int) * data.Count, data.ToArray(), usageArb);
        return buffer;
    }

    public VertexArray(MeshData mesh)
    {
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = CreateBufferAttribute(3, mesh.Vert, BufferUsageHint.StaticCopy, VertexAttribPointerType.Float,
            0);
        _nmvbo = CreateBufferAttribute(3, mesh.Normal, BufferUsageHint.StaticCopy, VertexAttribPointerType.Float,
            1);
        _vtvbo = CreateBufferAttribute(2, mesh.TexCoord, BufferUsageHint.StaticCopy, VertexAttribPointerType.Float,
            2);
        _tanvbo = CreateBufferAttribute(3, mesh.Tangent, BufferUsageHint.StaticCopy, VertexAttribPointerType.Float,
            3);
        _ebo = CreateBuffer(mesh.Faces, BufferUsageHint.StaticCopy);
        _elementCount = mesh.Faces.Count;
    }

    public void Render()
    {
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _elementCount, DrawElementsType.UnsignedInt, 0);
    }
    
    public override void Delete()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteBuffer(_nmvbo);
        GL.DeleteBuffer(_tanvbo);
        GL.DeleteBuffer(_vtvbo);
    }
}