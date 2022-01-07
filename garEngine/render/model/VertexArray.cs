using OpenTK.Graphics.OpenGL;

namespace garEngine.render.model;

public class VertexArray
{
    private AssimpLoaderTest.MeshStruct _parser;
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly int _vtvbo;
    private readonly int _nmvbo;
    private readonly int _tanvbo;

    public VertexArray(AssimpLoaderTest.MeshStruct parser)
    {
        _parser = parser;
        _vbo = GL.GenBuffer();
        _vtvbo = GL.GenBuffer();
        _nmvbo = GL.GenBuffer();
        _tanvbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * _parser.points.Count, _parser.points.ToArray(),
            BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _nmvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * _parser.normal.Count, _parser.normal.ToArray(),
            BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vtvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 2 * _parser.uvs.Count, _parser.uvs.ToArray(),
            BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _tanvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * _parser.tangents.Count, _parser.tangents.ToArray(),
            BufferUsageHint.StaticCopy);

        GL.EnableVertexAttribArray(3);
        GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _parser.faces.Count * 3 * sizeof(uint), _parser.faces.ToArray(),
            BufferUsageHint.StaticCopy);
    }

    public void Render()
    {
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _parser.faces.Count * 3, DrawElementsType.UnsignedInt, 0);
    }

    public void Delete()
    {
        GL.DeleteVertexArray( _vao);
        GL.DeleteBuffer( _vbo);
        GL.DeleteBuffer( _ebo);
        GL.DeleteBuffer( _vtvbo);
        GL.DeleteBuffer( _nmvbo);
    }
}