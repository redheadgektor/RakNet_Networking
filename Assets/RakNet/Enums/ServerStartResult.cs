/// <summary>
/// Server starting result
/// </summary>
public enum ServerStartResult
{
    ServerPointerIsNull,
    ServerInitError,
    SecurityInitError,
    Started,
    IsAlreadyStarted,
    PortIsAlreadyUse,
    BindingError
}