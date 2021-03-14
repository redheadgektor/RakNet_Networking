using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using UnityEngine;

/// <summary>
/// Pooled bitstream
/// </summary>
public sealed class PooledBitStream : BitStream, IDisposable
{
    public override void Dispose()
    {
        Recycle(this);
    }

    static readonly Stack<PooledBitStream> stack_list = new Stack<PooledBitStream>();
    static PooledBitStream Take() => stack_list.Count > 0 ? stack_list.Pop() : new PooledBitStream();
    static void Return(PooledBitStream item) => stack_list.Push(item);
    public static int Count => stack_list.Count;
    public static PooledBitStream GetBitStream()
    {
        PooledBitStream bitStream = Take();
        bitStream.Reset();
        return bitStream;
    }
    public static void Recycle(PooledBitStream writer)
    {
        Return(writer);
    }
}

/// <summary>
/// Bitstream - for reading data native network packets, it converts data arrays into data types that are convenient for you.
/// </summary>
[SuppressUnmanagedCodeSecurity] /* Increase performance by suppressing unmanaged code security checks */
public class BitStream : IDisposable
{
    public IntPtr Pointer { get; private set; } = IntPtr.Zero;

    public BitStream()
    {
        Pointer = Imports.BitStream_Create1();
        Reset();
    }

    public bool Exist()
    {
        return Pointer != IntPtr.Zero;
    }

    internal BitStream(IntPtr packet_ptr)
    {
        Pointer = Imports.BitStream_Create2(packet_ptr);
    }

    internal void ReadPacket(IntPtr packet_ptr)
    {
        if (Pointer == IntPtr.Zero || packet_ptr == IntPtr.Zero)
        {
            return;
        }
        Imports.BitStream_ReadPacket(Pointer, packet_ptr);
    }

    #region Disposing
    public virtual void Dispose()
    {
        lock (this)
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }

    ~BitStream()
    {
        Dispose();
    }
    #endregion

    internal void Close()
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_Close(Pointer);
    }

    /// <summary>
    /// Reset Write Pointer (set to 0)
    /// </summary>
    public void ResetWritePointer()
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_ResetWritePointer(Pointer);
    }

    /// <summary>
    /// Set Write Pointer (in bits)
    /// </summary>
    public void SetWritePointer(uint offset)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_SetWriteOffset(Pointer, offset);
    }

    /// <summary>
    /// Get Write Pointer (in bits)
    /// </summary>
    public uint GetWritePointer()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_GetWriteOffset(Pointer);
    }

    /// <summary>
    /// Reset Read Pointer (set to 0)
    /// </summary>
    public void ResetReadPointer()
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_ResetReadPointer(Pointer);
    }

    /// <summary>
    /// Set Read Pointer (in bits)
    /// </summary>
    public void SetReadPointer(uint offset)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_SetReadOffset(Pointer, offset);
    }

    /// <summary>
    /// Get Read Pointer (in bits)
    /// </summary>
    public uint GetReadPointer()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_GetReadOffset(Pointer);
    }

    /// <summary>
    /// Ignore bytes (GetReadPointer + IgnoreBytes)
    /// </summary>
    public void IgnoreBytes(int numberOfBytes)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_IgnoreBytes(Pointer, numberOfBytes);
    }

    /// <summary>
    /// Reset Read/Write Pointers
    /// </summary>
    public void Reset()
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_Reset(Pointer);
    }

    private byte[] GetData_Buffer = new byte[0];
    public byte[] GetData()
    {
        Imports.BitStream_GetData(Pointer, out IntPtr data, out int data_size);

        if (data_size > GetData_Buffer.Length)
        {
            Array.Resize(ref GetData_Buffer, data_size);
        }

        unsafe
        {
            byte* ptr = (byte*)data.ToPointer();

            for (int i = 0; i < data_size; i++)
            {
                GetData_Buffer[i] = ptr[i];
            }
        }

        return new ArraySegment<byte>(GetData_Buffer, 0, data_size).Array;
    }

    public void SetData(byte[] data)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_SetData(Pointer, data, data.Length);
    }

    public void Write(byte value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteByte(Pointer, value);
    }

    public void WriteDelta(byte value, byte last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteByteDelta(Pointer, value, last_value);
    }
    public void WriteCompressed(byte value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteByteCompressed(Pointer, value);
    }
    public void WriteCompressedDelta(byte value, byte last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteByteCompressedDelta(Pointer, value, last_value);
    }

    public void Write(sbyte value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Write((byte)value);
    }
    public void WriteDelta(sbyte value, sbyte last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteDelta((byte)value, (byte)last_value);
    }
    public void WriteCompressed(sbyte value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressed((byte)value);
    }
    public void WriteCompressedDelta(sbyte value, sbyte last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressedDelta((byte)value, (byte)last_value);
    }
    public void Write(bool value)
    {
        Write(value ? (byte)1 : (byte)0);
    }

    public void Write(short value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteShort(Pointer, value);
    }
    public void WriteDelta(short value, short last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteShortDelta(Pointer, value, last_value);
    }
    public void WriteCompressed(short value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteShortCompressed(Pointer, value);
    }
    public void WriteCompressedDelta(short value, short last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteShortCompressedDelta(Pointer, value, last_value);
    }

    public void Write(ushort value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Write((short)value);
    }
    public void WriteDelta(ushort value, ushort last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteDelta((short)value, (short)last_value);
    }
    public void WriteCompressed(ushort value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressed((short)value);
    }
    public void WriteCompressedDelta(ushort value, ushort last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressedDelta((short)value, (short)last_value);
    }

    public void Write(int value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteInt(Pointer, value);
    }
    public void WriteDelta(int value, int last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteIntDelta(Pointer, value, last_value);
    }
    public void WriteCompressed(int value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteIntCompressed(Pointer, value);
    }
    public void WriteCompressedDelta(int value, int last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteIntCompressedDelta(Pointer, value, last_value);
    }

    public void Write(uint value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Write((int)value);
    }
    public void WriteDelta(uint value, uint last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteDelta((int)value, (int)last_value);
    }
    public void WriteCompressed(uint value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressed((int)value);
    }
    public void WriteCompressedDelta(uint value, uint last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressedDelta((int)value, (int)last_value);
    }

    public void Write(float value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteFloat(Pointer, value);
    }
    public void WriteDelta(float value, float last_value, float diff = 0.001f)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteFloatDelta(Pointer, value, last_value, diff);
    }
    public void WriteCompressed(float value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteFloatCompressed(Pointer, value);
    }
    public void WriteCompressedDelta(float value, float last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteFloatCompressedDelta(Pointer, value, last_value);
    }

    public void Write(float value, float min, float max)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteFloat16(Pointer, value, min, max);
    }

    public void Write(long value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteLong(Pointer, value);
    }
    public void WriteDelta(long value, long last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteLongDelta(Pointer, value, last_value);
    }
    public void WriteCompressed(long value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteLongCompressed(Pointer, value);
    }
    public void WriteCompressedDelta(long value, long last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteLongCompressedDelta(Pointer, value, last_value);
    }

    public void Write(ulong value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Write((long)value);
    }
    public void WriteDelta(ulong value, ulong last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteDelta((long)value, (long)last_value);
    }
    public void WriteCompressed(ulong value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressed((long)value);
    }
    public void WriteCompressedDelta(ulong value, ulong last_value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressedDelta((long)value, (long)last_value);
    }

    public void Write(string value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        if (string.IsNullOrEmpty(value) || value.Length <= 0)
        {
            Write((ushort)0);
            return;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(value);
        Write((ushort)bytes.Length);
        for (int i = 0; i < bytes.Length; i++)
        {
            Write(bytes[i]);
        }
    }

    public void Write(Vector2 value)
    {
        Write(value.x);
        Write(value.y);
    }

    public void Write(Vector3 value, bool compressed = false)
    {
        if (!compressed)
        {
            Write(value.x);
            Write(value.y);
            Write(value.z);
        }
        else
        {
            float m = value.magnitude;
            Write(m);
            WriteCompressed(value.x / m);
            WriteCompressed(value.y / m);
            WriteCompressed(value.z / m);
        }
    }

    public void WriteDelta(Vector3 value, Vector3 last_value, float diff = 0.001f)
    {
        short changeMask = 0;
        if (Mathf.Abs(value.x - last_value.x) >= diff) { changeMask |= 1 << 0; }
        if (Mathf.Abs(value.y - last_value.y) >= diff) { changeMask |= 1 << 1; }
        if (Mathf.Abs(value.z - last_value.z) >= diff) { changeMask |= 1 << 2; }

        Write(changeMask);
        if ((changeMask & 1 << 0) != 0) { Write(value.x); }
        if ((changeMask & 1 << 1) != 0) { Write(value.y); }
        if ((changeMask & 1 << 2) != 0) { Write(value.z); }
    }

    public void Write(Vector4 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }

    public void Write(Quaternion value, bool compressed = false)
    {
        if (!compressed)
        {
            Write(value.eulerAngles);
        }
        else
        {
            Write(value.x, -1f, 1f);
            Write(value.y, -1f, 1f);
            Write(value.z, -1f, 1f);
            Write(value.w, -1f, 1f);
        }
    }

    public void WriteDelta(Quaternion value, Quaternion last_value, float diff = 0.001f)
    {
        WriteDelta(value.eulerAngles, last_value.eulerAngles, diff);
    }

    public byte ReadByte()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadByte(Pointer);
    }
    public byte ReadByteDelta(byte last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadByteDelta(Pointer, last_value);
    }
    public byte ReadByteCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadByteCompressed(Pointer);
    }
    public byte ReadByteCompressedDelta(byte last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadByteCompressedDelta(Pointer, last_value);
    }

    public sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }
    public sbyte ReadSByteDelta(sbyte last_value)
    {
        return (sbyte)ReadByteDelta((byte)last_value);
    }
    public sbyte ReadSByteCompressed()
    {
        return (sbyte)ReadByteCompressed();
    }
    public sbyte ReadSByteCompressedDelta(sbyte last_value)
    {
        return (sbyte)ReadByteCompressedDelta((byte)last_value);
    }
    public bool ReadBool()
    {
        return ReadByte() >= 1 ? true : false;
    }

    public short ReadShort()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadShort(Pointer);
    }
    public short ReadShortDelta(short last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadShortDelta(Pointer, last_value);
    }
    public short ReadShortCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadShortCompressed(Pointer);
    }
    public short ReadShortCompressedDelta(short last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadShortCompressedDelta(Pointer, last_value);
    }

    public ushort ReadUShort()
    {
        return (ushort)ReadShort();
    }
    public ushort ReadUShortDelta(ushort last_value)
    {
        return (ushort)ReadShortDelta((short)last_value);
    }
    public ushort ReadUShortCompressed()
    {
        return (ushort)ReadShortCompressed();
    }
    public ushort ReadUShortCompressedDelta(ushort last_value)
    {
        return (ushort)ReadShortCompressedDelta((short)last_value);
    }

    public int ReadInt()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadInt(Pointer);
    }
    public int ReadIntDelta(int last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadIntDelta(Pointer, last_value);
    }
    public int ReadIntCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadIntCompressed(Pointer);
    }
    public int ReadIntCompressedDelta(int last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadIntCompressedDelta(Pointer, last_value);
    }

    public uint ReadUInt()
    {
        return (uint)ReadInt();
    }
    public uint ReadUIntDelta(uint last_value)
    {
        return (uint)ReadIntDelta((int)last_value);
    }
    public uint ReadUIntCompressed()
    {
        return (uint)ReadIntCompressed();
    }
    public uint ReadUIntCompressedDelta(uint last_value)
    {
        return (uint)ReadIntCompressedDelta((int)last_value);
    }

    public float ReadFloat()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadFloat(Pointer);
    }
    public float ReadFloatDelta(float last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadFloatDelta(Pointer, last_value);
    }
    public float ReadFloatCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadFloatCompressed(Pointer);
    }
    public float ReadFloatCompressedDelta(float last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadFloatCompressedDelta(Pointer, last_value);
    }

    public float ReadFloat16(float min, float max)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadFloat16(Pointer, min, max);
    }

    public long ReadLong()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadLong(Pointer);
    }
    public long ReadLongDelta(long last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadLongDelta(Pointer, last_value);
    }
    public long ReadLongCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadLongCompressed(Pointer);
    }
    public long ReadLongCompressedDelta(long last_value)
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadLongCompressedDelta(Pointer, last_value);
    }

    public ulong ReadULong()
    {
        return (ulong)ReadLong();
    }
    public ulong ReadULongDelta(ulong last_value)
    {
        return (ulong)ReadLongDelta((long)last_value);
    }
    public ulong ReadULongCompressed()
    {
        return (ulong)ReadLongCompressed();
    }
    public ulong ReadULongCompressedDelta(ulong last_value)
    {
        return (ulong)ReadLongCompressedDelta((long)last_value);
    }

    byte[] buffered_StringBytes = new byte[0];

    public string ReadString()
    {
        if (Pointer == IntPtr.Zero)
            return string.Empty;

        ushort count = ReadUShort();

        if (count <= 0)
        {
            return string.Empty;
        }

        buffered_StringBytes = new byte[count];

        for (int i = 0; i < buffered_StringBytes.Length; i++)
        {
            buffered_StringBytes[i] = ReadByte();
        }

        return Encoding.UTF8.GetString(buffered_StringBytes);
    }

    public Vector2 ReadVector2()
    {
        float x = ReadFloat();
        float y = ReadFloat();
        return new Vector2(x, y);
    }

    public Vector3 ReadVector3(bool compressed = false)
    {
        if (!compressed)
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();

            return new Vector3(x, y, z);
        }
        else
        {
            float m = ReadFloat();
            float x = ReadFloatCompressed() * m;
            float y = ReadFloatCompressed() * m;
            float z = ReadFloatCompressed() * m;
            return new Vector3(x, y, z);
        }
    }

    public Vector3 ReadVector3Delta(Vector3 last_value)
    {
        short changeMask = ReadShort();

        Vector3 value = last_value;
        if ((changeMask & 1 << 0) != 0) { value.x = ReadFloat(); }
        if ((changeMask & 1 << 1) != 0) { value.y = ReadFloat(); }
        if ((changeMask & 1 << 2) != 0) { value.z = ReadFloat(); }

        return value;
    }

    public Vector4 ReadVector4()
    {
        float x = ReadFloat();
        float y = ReadFloat();
        float z = ReadFloat();
        float w = ReadFloat();
        return new Vector4(x, y, z, w);
    }

    public Quaternion ReadQuaternion(bool compressed = false)
    {
        if (!compressed)
        {
            return Quaternion.Euler(ReadVector3());
        }
        else
        {
            return new Quaternion(ReadFloat16(-1f, 1f), ReadFloat16(-1f, 1f), ReadFloat16(-1f, 1f), ReadFloat16(-1f, 1f));
        }
    }

    public Quaternion ReadQuaternionDelta(Quaternion last_value)
    {
        return Quaternion.Euler(ReadVector3Delta(last_value.eulerAngles));
    }

    /* ARRAYS */
    byte[] bArray = new byte[0];
    float[] flArray = new float[0];

    //BYTE[]
    public void WriteArray(byte[] array)
    {
        Write((short)array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            Write(array[i]);
        }
    }

    public byte[] ReadBytes()
    {
        short count = ReadShort();

        if (bArray.Length < count)
        {
            Array.Resize(ref bArray, count);
        }

        for (int i = 0; i < count; i++)
        {
            bArray[i] = ReadByte();
        }

        return new ArraySegment<byte>(bArray, 0, count).Array;
    }

    //FLOAT[]
    public void WriteArray(float[] array)
    {
        Write((short)array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            Write(array[i]);
        }
    }

    public float[] ReadFloats()
    {
        short count = ReadShort();

        if (flArray.Length < count)
        {
            Array.Resize(ref flArray, count);
        }

        for (int i = 0; i < count; i++)
        {
            flArray[i] = ReadFloat();
        }

        return new ArraySegment<float>(flArray, 0, count).Array;
    }
}
