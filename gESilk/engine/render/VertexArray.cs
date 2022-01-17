using System.Runtime.InteropServices;
using gESilk.engine.assimp;
using Silk.NET.OpenGL;
using static gESilk.engine.Globals;

namespace gESilk.engine.render;

public class VertexArray : Asset
{
    private readonly MeshData _mesh;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private readonly uint _vtvbo;
    private readonly uint _nmvbo;
    private readonly uint _tanvbo;

    private static unsafe uint CreateBufferAttribute<T>(int size, List<T> data, BufferUsageARB usageArb,
        VertexAttribPointerType type, uint index, int sizePerItem = 1) where T : unmanaged
    {
        uint buffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);
        gl.BufferData(BufferTargetARB.ArrayBuffer,
            (nuint)(Marshal.SizeOf(data.GetType().GetGenericArguments()[0]) * sizePerItem * data.Count),
            (ReadOnlySpan<T>)CollectionsMarshal.AsSpan(data), usageArb);
        gl.VertexAttribPointer(index, size, type, false, 0, null);
        gl.EnableVertexAttribArray(index);
        return buffer;
    }

    private static unsafe uint CreateBuffer<T>(List<T> data, BufferUsageARB usageArb) where T : unmanaged
    {
        uint buffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);
        gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(T) * data.Count), 
            (ReadOnlySpan<T>)CollectionsMarshal.AsSpan(data), usageArb);
        return buffer;
    }

    public VertexArray(MeshData mesh)
    {
        _mesh = mesh;
        
        _vao = gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = CreateBufferAttribute(3, mesh.Vert, BufferUsageARB.StaticCopy, VertexAttribPointerType.Float,
            0);
        _nmvbo = CreateBufferAttribute(3, mesh.Normal, BufferUsageARB.StaticCopy, VertexAttribPointerType.Float,
            1);
        _vtvbo = CreateBufferAttribute(2, mesh.TexCoord, BufferUsageARB.StaticCopy, VertexAttribPointerType.Float,
            2);
        _tanvbo = CreateBufferAttribute(3, mesh.Tangent, BufferUsageARB.StaticCopy, VertexAttribPointerType.Float,
            3);
        _ebo = CreateBuffer(mesh.Faces, BufferUsageARB.StaticCopy);
    }

    public void Render()
    {
        gl.BindVertexArray(_vao);
        gl.DrawElements(PrimitiveType.Triangles, (uint) _mesh.Faces.Count, DrawElementsType.UnsignedInt, 0);
    }
    
    public override void Delete()
    {
        gl.DeleteVertexArray(_vao);
        gl.DeleteBuffer(_vbo);
        gl.DeleteBuffer(_ebo);
        gl.DeleteBuffer(_nmvbo);
        gl.DeleteBuffer(_tanvbo);
        gl.DeleteBuffer(_vtvbo);
    }
}