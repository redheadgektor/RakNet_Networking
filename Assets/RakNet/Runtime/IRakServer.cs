public interface IRakServer
{
    /// <summary>
    /// Called when client connected
    /// </summary>
    /// <param name="connectionIndex">Connection index</param>
    /// <param name="guid">Unique client GUID</param>
    void OnConnected(ushort connectionIndex, ulong guid);

    /// <summary>
    /// Called when client disconnected
    /// </summary>
    /// <param name="connectionIndex">Connection index</param>
    /// <param name="guid">Unique client GUID</param>
    void OnDisconnected(ushort connectionIndex, ulong guid, DisconnectReason reason, string message);

    /// <summary>
    /// Called when received data
    /// </summary>
    /// <param name="packet_id">Data packet number</param>
    /// <param name="connectionIndex">Connection index</param>
    /// <param name="guid">Unique client GUID</param>
    /// <param name="bitStream">Bitstream for reading data from a packet</param>
    /// <param name="local_time">local time when the packet is received (in ms)</param>
    void OnReceived(byte packet_id, ushort connectionIndex, ulong guid, BitStream bitStream, ulong local_time);
}
