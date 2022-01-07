using garEngine.render.model;

namespace garEngine.render.utility;

public class AssetManager<T> where T : Asset
{
    private static List<T> components = new List<T>();
    public static void Register(T asset)
    {
        components.Add(asset);
    }
    
    public static void Delete()
    {
        foreach (T component in components)
        {
            component.Delete();
        }   
    }
}

class TextureManager : AssetManager<Texture> { }
class MaterialManager : AssetManager<Material> { }
class RenderTexManager : AssetManager<RenderTexture> { }
class VertexArrayManager : AssetManager<VertexArray> { }
class FBManager : AssetManager<Framebuffer> { }