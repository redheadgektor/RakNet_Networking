public interface IRakServer
{
    /// <summary>
    /// Called when client connected
    /// </summary>
    void OnConnected(ulong guid);

    /// <summary>
    /// Called when client disconnected
    /// </summary>
    void OnDisconnected(ulong guid, DisconnectReason reason);

    /// <summary>
    /// Called when received data
    /// </summary>
    void OnReceived(byte packet_id, ulong guid, BitStream bitStream);
}
