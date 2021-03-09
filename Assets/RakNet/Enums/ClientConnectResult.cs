/// <summary>
/// Client connect result
/// </summary>
public enum ClientConnectResult
{
    ClientPointerIsNull,
    ClientInitError,
    CannotResolveDomainName,
    Connecting,
    AlreadyConnected,
    AlreadyConnecting
}