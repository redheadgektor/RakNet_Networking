/// <summary>
/// RakNet client interface
/// </summary>
public interface IRakClient
{
    /// <summary>
    /// On Connecting (string address, ushort port, string password)
    /// </summary>
    void OnConnecting(string address, ushort port, string password);

    /// <summary>
    /// On Connected (string address, ushort port, string password)
    /// </summary>
    void OnConnected(string address, ushort port, string password);

    /// <summary>
    /// On Disconnected (DisconnectReason reason)
    /// </summary>
    void OnDisconnected(DisconnectReason reason);

    /// <summary>
    /// On Received Data From Server (byte packet_id, uint packet_size, BitStream bitStream)
    /// </summary>
    void OnReceived(byte packet_id, uint packet_size, BitStream bitStream);
}
