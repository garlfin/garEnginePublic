namespace garEngine.render.model;

public class MeshObject
{
    private AssimpLoaderTest _loaderTest;
    private List<VertexArray> _arrays = new List<VertexArray>();
    public MeshObject(AssimpLoaderTest loader)
    {
        _loaderTest = loader;
        foreach (var mesh in _loaderTest.getAllMeshes())
        {
            _arrays.Add(new VertexArray(mesh));
        }
    }

    public void RenderAll()
    {
        foreach (var mesh in _arrays)
        {
            mesh.Render();
        }
    }

    public void Render(int index)
    {
        _arrays[index].Render();
    }
    
    public int Length()
    {
        return _arrays.Count;
    }
}