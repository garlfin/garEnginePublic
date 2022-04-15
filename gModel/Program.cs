using System.Text;
using gESilk.engine.misc;
using gModel.res;
using OpenTK.Mathematics;
using static gModel.res.MathHelper;

namespace gModel;

internal static class Program
{
    static void Main(string[] args)
    {
        // I don't see the need to use classes here.
        var file = args[0];
        var fileNoExtension = Path.GetFileNameWithoutExtension(file);
        var fileName = Path.GetFileName(file);

        if (!File.Exists(file)) throw new ArgumentException($"File {fileName} doesn't exist!");
        if (!Directory.Exists("output")) Directory.CreateDirectory("output");

        var reader = new BinaryReader(File.Open(file, FileMode.Open), Encoding.UTF8);

        var itemCount = reader.ReadInt32();

        var materials = new Material[itemCount];

        for (var i = 0; i < itemCount; i++)
        {
            ReadString(reader);

            var material = new Material
            {
                Name = ReadString(reader),
                MaterialID = (ushort)i,
                UniformCount = 4,
                Uniforms = new IScriptValue[4]
            };

            for (var x = 0; x < 3; x++)
            {
                material.Uniforms[x] = new ScriptValue<string>()
                {
                    ValueType = UniformTypeEnum.Texture2D,
                    Name = ReadString(reader),
                    Value = ReadString(reader)
                };
            }

            material.Uniforms[3] = new ScriptValue<float>()
            {
                Name = "normalStrength",
                ValueType = UniformTypeEnum.Float,
                Value = reader.ReadSingle()
            };

            materials[i] = material;
        }

        var meshes = new List<Mesh>();
        var meshIndex = 0;
        var entities = new List<Entity>();

        itemCount = reader.ReadInt32();

        for (var i = 0; i < itemCount; i++)
        {
            var itemType = ReadString(reader);

            var itemName = ReadString(reader);

            var location = ReadVec3(reader);
            var rotation = ReadVec3(reader);
            var scale = ReadVec3(reader);

            var entity = new Entity()
            {
                Name = itemName,
                Location = location,
                Rotation = rotation,
                Scale = scale
            };

            switch (itemType)
            {
                case "MESH":
                {
                    var meshPath = ReadString(reader);
                    var matCount = reader.ReadInt32();
                    for (var x = 0; x < matCount; x++) ReadString(reader);

                    var loadedMesh = AssimpLoader.GetMeshFromFile(meshPath);
                    meshes.Add(loadedMesh);
                    entity.Scripts = new string[]
                        { "gESilk.engine.components.ModelRenderer", "gESilk.engine.components.MaterialComponent" };
                    entity.ScriptValues = new IScriptValue[3];
                    entity.ScriptValues[0] = new ScriptValue<int>()
                    {
                        ScriptIndex = 0,
                        Name = "Mesh",
                        ValueType = UniformTypeEnum.Mesh,
                        Value = meshIndex
                    };
                    entity.ScriptValues[1] = new ScriptValue<int>()
                    {
                        ScriptIndex = 1,
                        Name = "DefaultMaterial",
                        ValueType = UniformTypeEnum.Material,
                        Value = GetMatID(loadedMesh.Meshes[0].MaterialId, materials)
                    };
                    entity.ScriptValues[2] = new ScriptValue<int>()
                    {
                        ScriptIndex = 1,
                        Name = "Mesh",
                        ValueType = UniformTypeEnum.Mesh,
                        Value = meshIndex
                    };
                    meshIndex++;
                    break;
                }
                case "CAMERA":
                    entity.Scripts = new[]
                        { "gESilk.engine.components.Camera", "gESilk.resources.Scripts.MovementBehavior" };
                    entity.ScriptValues = new IScriptValue[4];
                    entity.ScriptValues[0] = new ScriptValue<float>()
                    {
                        ScriptIndex = 0,
                        Name = "Fov",
                        ValueType = UniformTypeEnum.Float,
                        Value = 50f
                    };
                    entity.ScriptValues[1] = new ScriptValue<float>()
                    {
                        ScriptIndex = 0,
                        Name = "ClipStart",
                        ValueType = UniformTypeEnum.Float,
                        Value = 0.1f
                    };
                    entity.ScriptValues[2] = new ScriptValue<float>()
                    {
                        ScriptIndex = 0,
                        Name = "ClipEnd",
                        ValueType = UniformTypeEnum.Float,
                        Value = 1000f
                    };
                    entity.ScriptValues[3] = new ScriptValue<float>()
                    {
                        ScriptIndex = 1,
                        Name = "Sensitivity",
                        ValueType = UniformTypeEnum.Float,
                        Value = 0.03f
                    };
                    break;
                case "LIGHT_PROBE":
                    entity.Scripts = new[] { "gESilk.engine.components.CubemapCapture" };
                    entity.ScriptValues = new IScriptValue[1];
                    entity.ScriptValues[0] = new ScriptValue<int>()
                    {
                        ScriptIndex = 0,
                        Name = "Size",
                        ValueType = UniformTypeEnum.Int,
                        Value = 512
                    };
                    break;
                case "LIGHT":
                {
                    var lightType = ReadString(reader);
                    switch (lightType)
                    {
                        case "SUN":
                            entity.Scripts = new[] { "gESilk.engine.components.Light" };
                            break;
                        case "POINT":
                        {
                            var lightPower = reader.ReadSingle();
                            var lightSize = reader.ReadSingle();
                            var lightColor = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                            entity.Scripts = new[] { "gESilk.engine.components.PointLight" };
                            entity.ScriptValues = new IScriptValue[4];
                            entity.ScriptValues[0] = new ScriptValue<int>()
                            {
                                ScriptIndex = 0,
                                Name = "Size",
                                ValueType = UniformTypeEnum.Int,
                                Value = 1024
                            };
                            entity.ScriptValues[1] = new ScriptValue<float>()
                            {
                                ScriptIndex = 0,
                                Name = "Power",
                                ValueType = UniformTypeEnum.Float,
                                Value = lightPower
                            };
                            entity.ScriptValues[2] = new ScriptValue<Vector3>()
                            {
                                ScriptIndex = 0,
                                Name = "Color",
                                ValueType = UniformTypeEnum.Vector3,
                                Value = lightColor
                            };
                            entity.ScriptValues[3] = new ScriptValue<float>()
                            {
                                ScriptIndex = 0,
                                Name = "Radius",
                                ValueType = UniformTypeEnum.Float,
                                Value = lightSize
                            };
                            break;
                        }
                    }

                    break;
                }
            }

            entities.Add(entity);
        }

        reader.Close();

        var outFile = File.Open($"{fileNoExtension}.gmap", FileMode.Create);
        var writer = new BinaryWriter(outFile);

        writer.Write(new char[] { 'G', 'M', 'A', 'P' });
        writer.Write(materials.Length);
        foreach (var material in materials)
        {
            material.Write(writer);
        }

        Console.WriteLine($"There are {materials.Length} materials");

        writer.Write(meshes.Count);
        foreach (var mesh in meshes)
        {
            mesh.Write(writer);
        }

        Console.WriteLine($"There are {meshes.Count} meshes");

        writer.Write(entities.Count);
        foreach (var entity in entities)
        {
            entity.Write(writer);
        }

        Console.WriteLine($"There are {entities.Count} entities");

        writer.Write(new char[] { 'G', 'M', 'A', 'P' });
        writer.Close();
    }

    public static int GetMatID(string matName, Material[] materials)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].Name == matName)
            {
                return i;
            }
        }

        return 0;
    }
}