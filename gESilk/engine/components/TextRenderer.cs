using System.Runtime.InteropServices;
using gESilk.engine.misc;
using gESilk.engine.render.materialSystem.settings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gESilk.engine.components;
public class TextRenderer : Component
{
    public Font Font;

    private string _realText;

    public string Text
    {
        get => _realText;
        set
        {
            GetData(value, out var data, out var vtData, out var eboData);

            GCHandle dataPinned = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr dataPtr = dataPinned.AddrOfPinnedObject();
            GCHandle vtPinned = GCHandle.Alloc(vtData, GCHandleType.Pinned);
            IntPtr vtPtr = vtPinned.AddrOfPinnedObject();
            GCHandle eboPinned = GCHandle.Alloc(eboData, GCHandleType.Pinned);
            IntPtr eboPtr = eboPinned.AddrOfPinnedObject();

            _elementLength = value.Length;
            UpdateData(data.Length * sizeof(float) * 3, dataPtr, vtPtr, _elementLength * 6 * sizeof(int), eboPtr);

            eboPinned.Free();
            vtPinned.Free();
            dataPinned.Free();

            _realText = value;
            
            
        }
    }

    private readonly int _vbo, _vtvbo, _ebo, _vao;
    private int _elementLength;

    public TextRenderer(Font font, string text)
    {
        TextRenderingSystem.Register(this);
        Font = font;

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        
        _elementLength = MaxChar;
        var vertexCount = 4 * MaxChar;
        
        _vbo = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexCount * 3 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(0);

        _vtvbo = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vtvbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexCount * 3 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(1);

        _ebo = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ArrayBuffer, _elementLength * 6* sizeof(int), IntPtr.Zero,
            BufferUsageHint.DynamicDraw);

        Text = text;
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
        if (!SlotManager.IsSlotSame(Font.Program))
        {
            SlotManager.SetSlot(Font.Program);
            Font.Program.Use();
        }
        
        Font.Program.SetUniform("model", Matrix4.CreateScale(Owner.Application.InverseScreen) * (Owner.GetComponent<Transform>()?.Model ?? Matrix4.Identity));
        Font.TexAtlas.TexAtlas.Use(0);
        
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.DrawElements(PrimitiveType.Triangles, _elementLength * 6, DrawElementsType.UnsignedInt, 0);
    }

    private void UpdateData(int dataLength, IntPtr data, IntPtr vtData, int eboDataLength, IntPtr eboData)
    {
        GL.BindVertexArray(_vao);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, dataLength, data);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vtvbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, dataLength, vtData);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, eboDataLength, eboData);
    }

    private void GetData(string text, out Vector3[] data, out Vector3[] vtdata, out int[] ebodata)
    {
        data = new Vector3[4 * text.Length];
        vtdata = new Vector3[4 * text.Length];
        ebodata = new int[6 * text.Length];

        int vertexOffset = 0;
        int triOffset = 0;
        Vector3 cursorPos = Vector3.Zero;

        foreach (var letter in text)
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

    ~TextRenderer()
    {
        GL.DeleteBuffers(3, new[] { _vbo, _vtvbo, _ebo });
        GL.DeleteVertexArray(_vao);
    }

    private const int MaxChar = 100;
}

class TextRenderingSystem : BaseSystem<TextRenderer>
{
    
}