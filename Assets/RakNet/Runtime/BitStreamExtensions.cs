#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;

public static class BitStreamExtensions
{
    public static void Write(this BitStream bitStream, Vector2 value)
    {
        bitStream.Write(value.x);
        bitStream.Write(value.y);
    }

    public static void Write(this BitStream bitStream, Vector3 value, bool compressed = false)
    {
        if (!compressed)
        {
            bitStream.Write(value.x);
            bitStream.Write(value.y);
            bitStream.Write(value.z);
        }
        else
        {
            float m = value.magnitude;
            bitStream.Write(m);
            bitStream.WriteCompressed(value.x / m);
            bitStream.WriteCompressed(value.y / m);
            bitStream.WriteCompressed(value.z / m);
        }
    }

    public static void Write(this BitStream bitStream, Vector4 value)
    {
        bitStream.Write(value.x);
        bitStream.Write(value.y);
        bitStream.Write(value.z);
        bitStream.Write(value.w);
    }

    public static void Write(this BitStream bitStream, Quaternion value, bool compressed = false)
    {
        if (!compressed)
        {
            bitStream.Write(value.eulerAngles);
        }
        else
        {
            bitStream.Write(value.x, -1f, 1f);
            bitStream.Write(value.y, -1f, 1f);
            bitStream.Write(value.z, -1f, 1f);
            bitStream.Write(value.w, -1f, 1f);
        }
    }

    public static Vector2 ReadVector2(this BitStream bitStream)
    {
        float x = bitStream.ReadFloat();
        float y = bitStream.ReadFloat();
        return new Vector2(x, y);
    }

    public static Vector3 ReadVector3(this BitStream bitStream, bool compressed = false)
    {
        if (!compressed)
        {
            float x = bitStream.ReadFloat();
            float y = bitStream.ReadFloat();
            float z = bitStream.ReadFloat();

            return new Vector3(x, y, z);
        }
        else
        {
            float m = bitStream.ReadFloat();
            float x = bitStream.ReadFloatCompressed() * m;
            float y = bitStream.ReadFloatCompressed() * m;
            float z = bitStream.ReadFloatCompressed() * m;
            return new Vector3(x, y, z);
        }
    }

    public static Vector4 ReadVector4(this BitStream bitStream)
    {
        float x = bitStream.ReadFloat();
        float y = bitStream.ReadFloat();
        float z = bitStream.ReadFloat();
        float w = bitStream.ReadFloat();
        return new Vector4(x, y, z, w);
    }

    public static Quaternion ReadQuaternion(this BitStream bitStream, bool compressed = false)
    {
        if (!compressed)
        {
            return Quaternion.Euler(bitStream.ReadVector3());
        }
        else
        {
            return new Quaternion(bitStream.ReadFloat16(-1f, 1f), bitStream.ReadFloat16(-1f, 1f), bitStream.ReadFloat16(-1f, 1f), bitStream.ReadFloat16(-1f, 1f));
        }
    }
}
#endif