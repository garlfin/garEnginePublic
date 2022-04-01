using OpenTK.Mathematics;

namespace gESilk.engine.misc;

public static class MiscMath
{
    public static Matrix4 GetLookAt(Vector3 location, int i)
    {
        return i switch
        {
            0 => Matrix4.LookAt(location, location + Vector3.UnitX, -Vector3.UnitY), // posx
            1 => Matrix4.LookAt(location, location - Vector3.UnitX, -Vector3.UnitY), // negx
            2 => Matrix4.LookAt(location, location + Vector3.UnitY, Vector3.UnitZ), // posy
            3 => Matrix4.LookAt(location, location - Vector3.UnitY, -Vector3.UnitZ), // negy
            4 => Matrix4.LookAt(location, location + Vector3.UnitZ, -Vector3.UnitY), // posz
            5 => Matrix4.LookAt(location, location - Vector3.UnitZ, -Vector3.UnitY), //negz
            _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
        };
    }
}