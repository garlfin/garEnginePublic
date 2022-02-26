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
        int length = reader.ReadInt32();
        string finalString = "";
        for (int i = 0; i < length; i++)
        {
            finalString += reader.ReadChar();
        }

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
        using BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open), Encoding.UTF8, false);

        int itemCount = reader.ReadInt32();

        ShaderProgram program = new ShaderProgram("../../../resources/shader/default.glsl");

        for (int i = 0; i < itemCount; i++)
        {
            ReadString(reader);
            string matName = ReadString(reader);

            Material temp = new Material(program, application);
            ReadString(reader);
            temp.AddSetting(new TextureSetting("albedo", new ImageTexture(ReadString(reader), application), 1));
            ReadString(reader);
            temp.AddSetting(new TextureSetting("roughnessTex", new ImageTexture(ReadString(reader), application), 2));
            ReadString(reader);
            temp.AddSetting(new TextureSetting("normalMap", new ImageTexture(ReadString(reader), application), 3));
            temp.AddSetting(new FloatSetting("normalStrength", reader.ReadSingle()));
            temp.AddSetting(new FloatSetting("metallic", reader.ReadSingle()));

            _materials.Add(new MatHolder(matName, temp));
        }

        itemCount = reader.ReadInt32();


        for (int i = 0; i < itemCount; i++)
        {
            string itemType = ReadString(reader);

            string itemName = ReadString(reader);

            Vector3 location = ReadVec3(reader);
            Vector3 rotation = ReadVec3(reader);
            Vector3 scale = ReadVec3(reader);

            Transform transform = new Transform();
            transform.Location = location;
            transform.Rotation = rotation;
            transform.Scale = scale;

            if (itemType == "MESH")
            {
                string meshPath = ReadString(reader);
                int matCount = reader.ReadInt32();
                string matName = "nothing";
                for (int x = 0; x < matCount; x++)
                {
                    matName = ReadString(reader);
                }

                Mesh loadedMesh = AssimpLoader.GetMeshFromFile(meshPath);

                Entity mesh = new Entity();
                mesh.AddComponent(transform);
                mesh.AddComponent(new MaterialComponent(loadedMesh, FindMat(matName)));
                Console.WriteLine(itemName);
                mesh.AddComponent(new ModelRenderer(loadedMesh, application, !itemName.StartsWith("D_")));
            }
            else if (itemType == "CAMERA")
            {
                Entity cam = new Entity();
                cam.AddComponent(transform);
                cam.AddComponent(new Camera(43f, 0.1f, 1000f));
                cam.AddComponent(new MovementBehavior(application, 0.3f));
                cam.GetComponent<Camera>().Set();
            }
            else if (itemType == "LIGHT_PROBE")
            {
                Entity probe = new Entity();
                probe.AddComponent(transform);
                probe.AddComponent(new CubemapCapture(new EmptyCubemapTexture(512)));
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
    }

    private static Material FindMat(string name)
    {
        foreach (var material in _materials)
        {
            if (material.ToString() == name) return material.Mat;
        }

        return null;
    }
}