using System;
using System.Collections.Generic;
using System.Security;

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
        bitStream.ResetReadPointer();
        bitStream.ResetWritePointer();
        return bitStream;
    }
    public static PooledBitStream GetBitStream(byte[] data)
    {
        PooledBitStream bitStream = Take();
        bitStream.Reset();
        bitStream.ResetReadPointer();
        bitStream.ResetWritePointer();
        bitStream.SetData(data);
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

    public void WriteCompressed(byte value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteByteCompressed(Pointer, value);
    }

    public void Write(sbyte value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Write((byte)value);
    }

    public void WriteCompressed(sbyte value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressed((byte)value);
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

    public void WriteCompressed(short value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteShortCompressed(Pointer, value);
    }

    public void Write(ushort value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Write((short)value);
    }

    public void WriteCompressed(ushort value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressed((short)value);
    }

    public void Write(int value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteInt(Pointer, value);
    }

    public void WriteCompressed(int value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteIntCompressed(Pointer, value);
    }

    public void Write(uint value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Write((int)value);
    }

    public void WriteCompressed(uint value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressed((int)value);
    }

    public void Write(float value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteFloat(Pointer, value);
    }

    public void WriteCompressed(float value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteFloatCompressed(Pointer, value);
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

    public void WriteCompressed(long value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Imports.BitStream_WriteLongCompressed(Pointer, value);
    }

    public void Write(ulong value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        Write((long)value);
    }

    public void WriteCompressed(ulong value)
    {
        if (Pointer == IntPtr.Zero)
            return;

        WriteCompressed((long)value);
    }

    public void Write(string value, bool compressed = false, ushort languageId = 0, bool writeLanguage = false)
    {
        if (Pointer == IntPtr.Zero)
            return;

        try
        {
            if (!compressed)
            {
                Imports.BitStream_WriteString(Pointer, value);
            }
            else
            {
                Imports.BitStream_WriteCompressedString(Pointer, value, languageId, writeLanguage);
            }
        }
        catch { }
    }

    public byte ReadByte()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadByte(Pointer);
    }

    public byte ReadByteCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadByteCompressed(Pointer);
    }

    public sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }

    public sbyte ReadSByteCompressed()
    {
        return (sbyte)ReadByteCompressed();
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

    public short ReadShortCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadShortCompressed(Pointer);
    }

    public ushort ReadUShort()
    {
        return (ushort)ReadShort();
    }

    public ushort ReadUShortCompressed()
    {
        return (ushort)ReadShortCompressed();
    }

    public int ReadInt()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadInt(Pointer);
    }

    public int ReadIntCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadIntCompressed(Pointer);
    }

    public uint ReadUInt()
    {
        return (uint)ReadInt();
    }

    public uint ReadUIntCompressed()
    {
        return (uint)ReadIntCompressed();
    }

    public float ReadFloat()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadFloat(Pointer);
    }

    public float ReadFloatCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadFloatCompressed(Pointer);
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

    public long ReadLongCompressed()
    {
        if (Pointer == IntPtr.Zero)
            return 0;

        return Imports.BitStream_ReadLongCompressed(Pointer);
    }

    public ulong ReadULong()
    {
        return (ulong)ReadLong();
    }

    public ulong ReadULongCompressed()
    {
        return (ulong)ReadLongCompressed();
    }

    public string ReadString(bool unicode = false, bool compressed = false, bool readLanguageId = false)
    {
        if (Pointer == IntPtr.Zero)
            return string.Empty;

        try
        {
            IntPtr string_ptr = IntPtr.Zero;
            if (!compressed)
            {
                string_ptr = Imports.BitStream_ReadString(Pointer);
            }
            else
            {
                string_ptr = Imports.BitStream_ReadCompressedString(Pointer, readLanguageId);
            }

            if (unicode)
            {
                return Imports.IntPtrToStringUnicode(string_ptr);
            }
            else
            {
                return Imports.IntPtrToStringAnsi(string_ptr);
            }
        }
        catch { }

        return string.Empty;
    }

    public void WriteBytes(byte[] array)
    {
        try
        {
            Imports.BitStream_WriteArray(Pointer, array);
        }
        catch { }
    }

    static readonly byte[] NullArray = new byte[0];
    byte[] BytesArray = new byte[0];

    public byte[] ReadBytes()
    {
        if (Pointer == IntPtr.Zero)
            return NullArray;

        try
        {
            Imports.BitStream_ReadArray(Pointer, ref BytesArray);
            return BytesArray;
        }
        catch { }
        return NullArray;
    }
}
