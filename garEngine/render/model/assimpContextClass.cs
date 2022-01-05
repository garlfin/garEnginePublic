using Assimp;

namespace garEngine.render.model;

public static class assimpContextClass
{
    private static AssimpContext Context = new AssimpContext();

    public static AssimpContext get()
    {
        return Context;
    }
}