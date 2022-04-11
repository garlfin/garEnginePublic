using System.Drawing;
using Assimp;
using gESilk.engine.assimp;
using gESilk.engine.misc;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Font = gESilk.engine.misc.Font;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace gESilk.engine.components;

public class FontRenderer : Component
{
    private Font _font;

    public string Text;

    private readonly int _vbo, _vtvbo, _ebo, _vao, _elementLength;

    public FontRenderer(Font font, string text)
    {
        _font = font;
        Text = text;
        _vao = GL.GenVertexArray();

        GL.BindVertexArray(_vao);

        Vector3[] data = new Vector3[4 * Text.Length];
        Vector3[] vtdata = new Vector3[4 * Text.Length];
        _elementLength = 6 * Text.Length;
        int[] ebodata = new int[_elementLength];
        Vector3 cursorPos = Vector3.Zero;

        int vertexOffset = 0;

        for (int i = 0; i < Text.Length; i++, vertexOffset += 4)
        {
            Character character = font.Characters[0];
            
            foreach (var currentCharacter in font.Characters)
            {
                if (currentCharacter.isCharacter(Text[i]))
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

            ebodata[vertexOffset] = vertexOffset;
            ebodata[vertexOffset + 1] = vertexOffset + 1;
            ebodata[vertexOffset + 2] = vertexOffset + 2;
            ebodata[vertexOffset + 3] = vertexOffset + 2;
            ebodata[vertexOffset + 4] = vertexOffset + 1;
            ebodata[vertexOffset + 5] = vertexOffset + 3;

            cursorPos += new Vector3(character.Advance, 0, 0);
        }

        _vbo = CreateBufferAttribute(data, BufferUsageHint.StaticDraw, VertexAttribPointerType.Float, 0);
        _vtvbo = CreateBufferAttribute(vtdata, BufferUsageHint.StaticDraw, VertexAttribPointerType.Float, 1);
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, 6 * sizeof(int) * Text.Length, ebodata, BufferUsageHint.StaticDraw);
    }
    
    private static int CreateBufferAttribute(Vector3[] data, BufferUsageHint usageArb,
        VertexAttribPointerType type, uint index, BufferTarget target = BufferTarget.ArrayBuffer)
    {
        var buffer = GL.GenBuffer();
        GL.BindBuffer(target, buffer);
        GL.BufferData(target, sizeof(float) * data.Length * 3, data, usageArb);
        GL.VertexAttribPointer(index, 3, type, false, 0, 0);
        GL.EnableVertexAttribArray(index);
        return buffer;
    }

    public override void Update(float gameTime)
    {
        GL.Disable(EnableCap.CullFace);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.DrawElements(PrimitiveType.Triangles, _elementLength, DrawElementsType.UnsignedInt, 0);
        GL.Enable(EnableCap.CullFace);
    }
}