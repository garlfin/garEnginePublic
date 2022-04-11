using gESilk.engine.misc;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.components;

public class FontRenderer : Component
{
    public Font Font;

    public string Text;

    private readonly int _vbo, _vtvbo, _ebo, _vao, _elementLength;

    public FontRenderer(Font font, string text)
    {
        Font = font;
        Text = text;
        _vao = GL.GenVertexArray();

        GL.BindVertexArray(_vao);

        _elementLength = 6 * Text.Length;

        GetData(out var data, out var vtdata, out var ebodata);

        _vbo = CreateBufferAttribute(GL.GenBuffer(), data, BufferUsageHint.StaticDraw, VertexAttribPointerType.Float,
            0);
        _vtvbo = CreateBufferAttribute(GL.GenBuffer(), vtdata, BufferUsageHint.StaticDraw,
            VertexAttribPointerType.Float, 1);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, 6 * sizeof(int) * Text.Length, ebodata,
            BufferUsageHint.StaticDraw);
    }

    private static int CreateBufferAttribute(int buffer, Vector3[] data, BufferUsageHint usageArb,
        VertexAttribPointerType type, uint index, BufferTarget target = BufferTarget.ArrayBuffer)
    {
        GL.BindBuffer(target, buffer);
        GL.BufferData(target, sizeof(float) * data.Length * 3, data, usageArb);
        GL.VertexAttribPointer(index, 3, type, false, 0, 0);
        GL.EnableVertexAttribArray(index);
        return buffer;
    }

    public override void Update(float gameTime)
    {
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.DrawElements(PrimitiveType.Triangles, _elementLength, DrawElementsType.UnsignedInt, 0);
    }

    public void UpdateText(string text)
    {
        Text = text;

        GetData(out var data, out var vtdata, out var ebodata);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        //GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vtvbo);
        //GL.BufferSubData(BufferTarget.ArrayBuffer, sizeof(float) * vtdata.Length * 3, data, BufferUsageHint.DynamicDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        //GL.BufferSubData(BufferTarget.ElementArrayBuffer, 6 * sizeof(int) * Text.Length, ebodata, BufferUsageHint.DynamicDraw);
    }

    private void GetData(out Vector3[] data, out Vector3[] vtdata, out int[] ebodata)
    {
        data = new Vector3[4 * Text.Length];
        vtdata = new Vector3[4 * Text.Length];
        ebodata = new int[6 * Text.Length];

        int vertexOffset = 0;
        int triOffset = 0;
        Vector3 cursorPos = Vector3.Zero;

        foreach (var letter in Text)
        {
            Character character = Font.Characters[0];

            foreach (var currentCharacter in Font.Characters)
            {
                if (currentCharacter.isCharacter(letter))
                {
                    character = currentCharacter;
                }
            }

            data[vertexOffset] = cursorPos + character.PlaneBounds[0];
            data[vertexOffset + 1] = cursorPos + character.PlaneBounds[1];
            data[vertexOffset + 2] = cursorPos + character.PlaneBounds[2];
            data[vertexOffset + 3] = cursorPos + character.PlaneBounds[3];

            vtdata[vertexOffset] = character.AtlasBounds[0];
            vtdata[vertexOffset + 1] = character.AtlasBounds[1];
            vtdata[vertexOffset + 2] = character.AtlasBounds[2];
            vtdata[vertexOffset + 3] = character.AtlasBounds[3];

            ebodata[triOffset] = vertexOffset;
            ebodata[triOffset + 1] = vertexOffset + 1;
            ebodata[triOffset + 2] = vertexOffset + 2;
            ebodata[triOffset + 3] = vertexOffset;
            ebodata[triOffset + 4] = vertexOffset + 3;
            ebodata[triOffset + 5] = vertexOffset + 2;

            cursorPos += new Vector3(character.Advance, 0, 0);
            vertexOffset += 4;
            triOffset += 6;
        }
    }
}