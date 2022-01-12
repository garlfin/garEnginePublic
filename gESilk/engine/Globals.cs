using Assimp;
using Silk.NET.OpenGL;


namespace gESilk.engine;

public static class Globals
{
    public static GL gl;
    public static AssimpContext Assimp;

    static Globals()
    {
        Assimp = new AssimpContext();
    }
}