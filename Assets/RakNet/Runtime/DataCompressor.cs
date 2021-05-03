public class DataCompressor
{
	//Don't compress small amounts of data!
    public static void Compress(BitStream bitStream, byte[] data)
    {
        try
        {
            Imports.DataCompressor_Compress(data, data.Length, bitStream.Pointer);
        }
        catch { }
    }

    public static int Decompress(BitStream bitStream, ref byte[] data)
    {
        try
        {
            return Imports.DataCompressor_Decompress(bitStream.Pointer, ref data);
        }
        catch
        {
            return 0;
        }
    }
}