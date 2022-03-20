using System.Text;
using gESilk.engine.assimp;
using gESilk.engine.components;
using gESilk.engine.render.assets;
using gESilk.engine.render.assets.textures;
using gESilk.engine.render.materialSystem;
using gESilk.engine.render.materialSystem.settings;
using gESilk.engine.window;
using OpenTK.Mathematics;

namespace gESilk.engine.misc;

public struct MatHolder
{
    public string Name;
    public Material Mat;

    public MatHolder(string name, Material mat)
    {
        Name = name;
        Mat = mat;
    }

    public override string ToString()
    {
        return Name;
    }
}

public static class MapLoader
{
    private static List<MatHolder> _materials;

    private static string ReadString(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        var finalString = "";
        for (var i = 0; i < length; i++) finalString += reader.ReadChar();

        return finalString;
    }

    private static Vector3 ReadVec3(BinaryReader reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    public static void LoadMap(string path, Application application)
    {
        _materials = new List<MatHolder>();
        if (!File.Exists(path)) return;
        using var reader = new BinaryReader(File.Open(path, FileMode.Open), Encoding.UTF8, false);

        var itemCount = reader.ReadInt32();

        var program = new ShaderProgram("../../../resources/shader/default.glsl");

        for (var i = 0; i < itemCount; i++)
        {
            ReadString(reader);
            var matName = ReadString(reader);

            var temp = new Material(program, application);
            ReadString(reader);
            temp.AddSetting(new TextureSetting("albedo", new ImageTexture(ReadString(reader), application)));
            ReadString(reader);
            temp.AddSetting(new TextureSetting("specularTex", new ImageTexture(ReadString(reader), application)));
            ReadString(reader);
            temp.AddSetting(new TextureSetting("normalMap", new ImageTexture(ReadString(reader), application)));
            temp.AddSetting(new FloatSetting("normalStrength", reader.ReadSingle()));


            _materials.Add(new MatHolder(matName, temp));
        }

        itemCount = reader.ReadInt32();


        for (var i = 0; i < itemCount; i++)
        {
            var itemType = ReadString(reader);

            var itemName = ReadString(reader);

            var location = ReadVec3(reader);
            var rotation = ReadVec3(reader);
            var scale = ReadVec3(reader);

            var transform = new Transform();
            transform.Location = location;
            transform.Rotation = rotation;
            transform.Scale = scale;

            if (itemType == "MESH")
            {
                var meshPath = ReadString(reader);
                var matCount = reader.ReadInt32();
                var matName = "nothing";
                for (var x = 0; x < matCount; x++) matName = ReadString(reader);

                var loadedMesh = AssimpLoader.GetMeshFromFile(meshPath);

                var mesh = new Entity(application, isStatic: !itemName.StartsWith("D_"));
                mesh.AddComponent(transform);
                mesh.AddComponent(new MaterialComponent(loadedMesh, FindMat(matName)));
                mesh.AddComponent(new ModelRenderer(loadedMesh));
            }
            else if (itemType == "CAMERA")
            {
                var cam = new Entity(application);
                cam.AddComponent(transform);
                cam.AddComponent(new Camera(43f, 0.1f, 1000f));
                cam.AddComponent(new MovementBehavior(0.3f));
                cam.GetComponent<Camera>().Set();
            }
            else if (itemType == "LIGHT_PROBE")
            {
                var probe = new Entity(application);
                probe.AddComponent(transform);
                probe.AddComponent(new CubemapCapture(new EmptyCubemapTexture(1024)));
            }
            else if (itemType == "LIGHT")
            {
                Globals.SunPos = location;
            }
            else
            {
                Console.WriteLine($"UNKNOWN: {itemType}");
            }
        }

        foreach (var entity in EntityManager.Entities.Where(entity => entity.GetComponent<MaterialComponent>() != null))
            entity.GetComponent<MaterialComponent>().GetNearestCubemap();
    }

    private static Material FindMat(string name)
    {
        foreach (var material in _materials)
            if (material.ToString() == name)
                return material.Mat;

        return null;
    }
}