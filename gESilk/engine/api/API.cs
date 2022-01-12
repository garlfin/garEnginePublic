using System.Drawing;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace gESilk.engine.api;

public class API
{
    public virtual void ClearColor(Color color)
    {
    }

    public virtual bool GetApi(IWindow context)
    {
        return false;
    }

    public virtual bool GetApi(IView context)
    {
        return false;
    }
}
