/// <summary>
/// Disconnect reasons
/// </summary>
public enum DisconnectReason
{
    None,
    IsBanned,
    IncompatibleProtocol,
    SecurityError,
    InvalidPassword,
    ServerIsFull,
    AttemptFailed,
    ConnectionRecently,
    ConnectionLost,
    ConnectionClosed,
    ByUser
}