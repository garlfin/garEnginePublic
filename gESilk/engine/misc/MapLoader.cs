﻿using System.Text;
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

    public static void LoadMap(string path, Application application)
    {
        _materials = new List<MatHolder>();
        if (!File.Exists(path)) throw new FileNotFoundException();

        var program = new ShaderProgram("../../../resources/shader/default.glsl");

        var reader = new BinaryReader(File.Open(path, FileMode.Open), Encoding.UTF8, false);

        reader.ReadChars(4);
        var matCount = reader.ReadInt32();
        for (int i = 0; i < matCount; i++)
        {
            reader.ReadInt32();
            Material tempMat = new Material(program, application);
            _materials.Add(new MatHolder(reader.ReadString(), tempMat));
            var uniformCount = reader.ReadInt16();
            for (int j = 0; j < uniformCount; j++)
            {
                reader.ReadInt32();
                var uniformName = reader.ReadString();
                var uniformType = (UniformTypeEnum)reader.ReadInt32();
                switch (uniformType)
                {
                    case UniformTypeEnum.Texture2D:
                        tempMat.AddSetting(new TextureSetting(uniformName,
                            new ImageTexture(reader.ReadString(), application)));
                        break;
                    case UniformTypeEnum.Float:
                        tempMat.AddSetting(new FloatSetting(uniformName, reader.ReadSingle()));
                        break;
                    case UniformTypeEnum.Int:
                        break;
                    case UniformTypeEnum.Vector2:
                        break;
                    case UniformTypeEnum.Vector3:
                        break;
                    case UniformTypeEnum.Texture3D:
                        break;
                    case UniformTypeEnum.String:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        var meshCount = reader.ReadUInt32();
        Mesh[] meshes = new Mesh[meshCount];
        
        for (int i = 0; i < meshCount; i++)
        {
            Mesh finalMesh = new Mesh();
            
            if (reader.ReadInt16() != 2) throw new InvalidDataException("Build version mismatch"); 
            int subMeshCount = reader.ReadInt32();
            
            for (int meshID = 0; meshID < subMeshCount; meshID++)
            {
                MeshData subMesh = new MeshData();

                subMesh.Vert = new List<Vector3>();
                int vertLength = reader.ReadInt32();
                for (int vert = 0; vert < vertLength; vert++)
                {
                    subMesh.Vert.Add(reader.ReadVec3());
                }

                subMesh.TexCoord = new List<Vector2>();
                vertLength = reader.ReadInt32();
                for (int vert = 0; vert < vertLength; vert++)
                {
                    subMesh.TexCoord.Add(reader.ReadVec2());
                }

                subMesh.Normal = new List<Vector3>();
                vertLength = reader.ReadInt32();
                for (int vert = 0; vert < vertLength; vert++)
                {
                    subMesh.Normal.Add(reader.ReadVec3());
                }

                subMesh.Tangent = new List<Vector3>();
                vertLength = reader.ReadInt32();
                for (int vert = 0; vert < vertLength; vert++)
                {
                    subMesh.Tangent.Add(reader.ReadVec3());
                }

                subMesh.Faces = new List<IntVec3>();
                vertLength = reader.ReadInt32();
                for (int vert = 0; vert < vertLength; vert++)
                {
                    subMesh.Faces.Add(reader.ReadIntVec3());
                }

                reader.ReadString();

                subMesh.MaterialId = meshID;
                subMesh.Data = new VertexArray(subMesh);

                finalMesh.AddMesh(subMesh);
            }

            meshes[i] = finalMesh;
        }

        var objectCount = reader.ReadInt32();

        for (int i = 0; i < objectCount; i++)
        {
            Entity entity = new Entity(application, reader.ReadString());
            entity.AddComponent(new Transform()
            {
                Location = reader.ReadVec3(),
                Rotation = reader.ReadVec3(),
                Scale = reader.ReadVec3()
            });

            ushort scriptCount = reader.ReadUInt16();
            Type[] scriptTypes = new Type[scriptCount];

            for (int j = 0; j < scriptCount; j++)
            {
                scriptTypes[j] = Type.GetType(reader.ReadString());
                entity.AddComponent((Component) Activator.CreateInstance(scriptTypes[j]));
            }

            scriptCount = reader.ReadUInt16();
            for (int j = 0; j < scriptCount; j++)
            {
                uint scriptIndex = reader.ReadUInt32(); // I need to be a lot more consistent LOL
                var scriptField = scriptTypes[scriptIndex].GetField(reader.ReadString());
                dynamic value = 0;
                UniformTypeEnum fieldType = (UniformTypeEnum) reader.ReadInt32();
                switch (fieldType)
                {
                    case UniformTypeEnum.Float:
                        value = reader.ReadSingle();
                        break;
                    case UniformTypeEnum.Int:
                        value = reader.ReadInt32();
                        break;
                    case UniformTypeEnum.Vector3:
                        value = reader.ReadVec3();
                        break;
                    case UniformTypeEnum.String:
                        value = reader.ReadString();
                        break;
                }
                scriptField.SetValue(entity.GetComponent(scriptTypes[j]), value);
            }
        }

        char[] end = reader.ReadChars(4);
        if (end != new char[] {'G', 'M', 'A', 'P'}) throw new Exception("No EOF found.");
        reader.Close();
        
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

    private static Vector3 ReadVec3(this BinaryReader reader)
    {
        return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    private static IntVec3 ReadIntVec3(this BinaryReader reader)
    {
        return new IntVec3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
    }

    private static Vector2 ReadVec2(this BinaryReader reader)
    {
        return new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }
}

public enum UniformTypeEnum
{
    Int = 0,
    Float = 1,
    Vector2 = 2,
    Vector3 = 3,
    Texture2D = 4,
    Texture3D = 5,
    String = 6
}