using System.Drawing;
using gESilk.engine.api;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public class GLAPI : API
{
    private GL gl;

    public override bool GetApi(IWindow context)
    {
        gl = GL.GetApi(context);
        return true;
    }

    public override bool GetApi(IView context)
    {
        gl = GL.GetApi(context);
        return true;
    }

    public override void ClearColor(Color color)
    {
        gl.ClearColor(color);
    }
}