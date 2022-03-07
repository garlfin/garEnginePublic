using System.Runtime.CompilerServices;
using gESilk.engine.render.assets.textures;
using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;

namespace gESilk.engine.render.assets;

public class ImGuiController
{
    private readonly Vector2 _scaleFactor = Vector2.One;
    private readonly int _windowHeight;

    private readonly int _windowWidth;
    private Texture _fontTexture;
    private bool _frameBegun;
    private int _indexBuffer;
    private int _indexBufferSize;
    private ShaderProgram _shader;

    private int _vertexArray;
    private int _vertexBuffer;
    private int _vertexBufferSize;

    public ImGuiController(int width, int height)
    {
        _windowWidth = width;
        _windowHeight = height;

        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        CreateDeviceResources();

        ImGui.NewFrame();
        _frameBegun = true;
    }

    public void CreateDeviceResources()
    {
        _vertexArray = GL.GenVertexArray();

        _vertexBufferSize = 10000;
        _indexBufferSize = 2000;

        _vertexBuffer = GL.GenBuffer();
        _indexBuffer = GL.GenBuffer();

        GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        RecreateFontDeviceTexture();

        _shader = new ShaderProgram("../../../resources/shader/imgui.glsl");

        GL.VertexArrayVertexBuffer(_vertexArray, 0, _vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
        GL.VertexArrayElementBuffer(_vertexArray, _indexBuffer);

        GL.EnableVertexArrayAttrib(_vertexArray, 0);
        GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
        GL.VertexArrayAttribFormat(_vertexArray, 0, 2, VertexAttribType.Float, false, 0);

        GL.EnableVertexArrayAttrib(_vertexArray, 1);
        GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
        GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 8);

        GL.EnableVertexArrayAttrib(_vertexArray, 2);
        GL.VertexArrayAttribBinding(_vertexArray, 2, 0);
        GL.VertexArrayAttribFormat(_vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);
    }

    public void Render()
    {
        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData());
        }
    }

    public void RecreateFontDeviceTexture()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height, out var bytesPerPixel);

        _fontTexture = new TextureFromIntPtr(width, height, pixels);


        io.Fonts.SetTexID((IntPtr) _fontTexture.Get());

        io.Fonts.ClearTexData();
    }

    private void RenderImDrawData(ImDrawDataPtr drawData)
    {
        if (drawData.CmdListsCount == 0) return;

        for (var i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmd_list = drawData.CmdListsRange[i];

            var vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
            if (vertexSize > _vertexBufferSize)
            {
                var newSize = (int) Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                GL.NamedBufferData(_vertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _vertexBufferSize = newSize;

                Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
            }

            var indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
            if (indexSize > _indexBufferSize)
            {
                var newSize = (int) Math.Max(_indexBufferSize * 1.5f, indexSize);
                GL.NamedBufferData(_indexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                _indexBufferSize = newSize;

                Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
            }
        }

        // Setup orthographic projection matrix into our constant buffer
        var io = ImGui.GetIO();
        var mvp = Matrix4.CreateOrthographicOffCenter(
            0.0f,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0.0f,
            -1.0f,
            1.0f);

        _shader.Use();
        GL.UniformMatrix4(_shader.GetUniform("projection_matrix"), false, ref mvp);
        GL.Uniform1(_shader.GetUniform("in_fontTexture"), 0);


        GL.BindVertexArray(_vertexArray);


        drawData.ScaleClipRects(io.DisplayFramebufferScale);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.ScissorTest);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        // Render command lists
        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmd_list = drawData.CmdListsRange[n];

            GL.NamedBufferSubData(_vertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(),
                cmd_list.VtxBuffer.Data);


            GL.NamedBufferSubData(_indexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort),
                cmd_list.IdxBuffer.Data);


            for (var cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                var pcmd = cmd_list.CmdBuffer[cmd_i];
                if (pcmd.UserCallback != IntPtr.Zero) throw new NotImplementedException();

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, (int) pcmd.TextureId);


                // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                var clip = pcmd.ClipRect;
                GL.Scissor((int) clip.X, _windowHeight - (int) clip.W, (int) (clip.Z - clip.X),
                    (int) (clip.W - clip.Y));


                GL.DrawElements(BeginMode.Triangles, (int) pcmd.ElemCount, DrawElementsType.UnsignedShort,
                    (int) pcmd.IdxOffset * sizeof(ushort));
            }
        }

        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.ScissorTest);
    }

    public void Update(float deltaSeconds)
    {
        if (_frameBegun) ImGui.Render();

        SetPerFrameImGuiData(deltaSeconds);


        _frameBegun = true;
        ImGui.NewFrame();
    }

    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(
            _windowWidth / _scaleFactor.X,
            _windowHeight / _scaleFactor.Y);
        io.DisplayFramebufferScale = _scaleFactor;
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }
}