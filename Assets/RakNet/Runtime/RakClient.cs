using System;
using System.Collections.Generic;
using UnityEngine;

public class RakClient
{
    static IntPtr Pointer = IntPtr.Zero;

    /// <summary>
    /// Client initialized and ready to connect?
    /// </summary>
    public static bool Initialized { get; private set; } = false;

    public delegate void OnInitializedCallback();
    /// <summary>
    /// On initialized client
    /// </summary>
    public static OnInitializedCallback OnInitialized;

    /// <summary>
    /// Current client state
    /// </summary>
    public static ClientState State { get; private set; } = ClientState.IS_DISCONNECTED;

    static string server_address = string.Empty;
    static ushort server_port = 0;
    static string server_password = string.Empty;

    static List<IRakClient> interfaces = new List<IRakClient>();

    /// <summary>
    /// Registering interface
    /// </summary>
    public static void RegisterInterface(IRakClient client_interface)
    {
        interfaces.Add(client_interface);
    }

    /// <summary>
    /// UnRegistering interface
    /// </summary>
    public static void UnRegisterInterface(IRakClient client_interface)
    {
        interfaces.Remove(client_interface);
    }

    internal static void EarlyUpdate()
    {
        if (Initialized)
        {
            if (State == ClientState.IS_CONNECTING)
            {
                for (int i = 0; i < interfaces.Count; i++)
                {
                    if (interfaces[i] != null)
                    {
                        interfaces[i].OnConnecting(server_address, server_port, server_password);
                    }
                }
            }

            try
            {
                while (Imports.Shared_HasPackets(Pointer))
                {
                    IntPtr packet_ptr = Imports.Client_GetPacket(Pointer, out uint packet_size);

                    using (PooledBitStream bitStream = PooledBitStream.GetBitStream())
                    {
                        bitStream.ReadPacket(packet_ptr);

                        byte packet_id = bitStream.ReadByte();

                        if (packet_id < (byte)InternalPacketID.ID_USER_PACKET_ENUM)
                        {
                            //packet_id
                            InternalPacketID internal_packet_id = (InternalPacketID)packet_id;

                            switch (internal_packet_id)
                            {
                                case InternalPacketID.ID_CONNECTION_REQUEST_ACCEPTED:
                                    for (int i = 0; i < interfaces.Count; i++)
                                    {
                                        State = ClientState.IS_CONNECTED;
                                        if (interfaces[i] != null)
                                        {
                                            interfaces[i].OnConnected(server_address, server_port, server_password);
                                        }
                                    }
                                    break;

                                case InternalPacketID.ID_DISCONNECTION_NOTIFICATION:
                                    Disconnect(DisconnectReason.ConnectionClosed);
                                    break;

                                case InternalPacketID.ID_CONNECTION_LOST:
                                    Disconnect(DisconnectReason.ConnectionLost);
                                    break;

                                case InternalPacketID.ID_CONNECTION_BANNED:
                                    Disconnect(DisconnectReason.IsBanned);
                                    break;

                                case InternalPacketID.ID_INCOMPATIBLE_PROTOCOL_VERSION:
                                    Disconnect(DisconnectReason.IncompatibleProtocol);
                                    break;

                                case InternalPacketID.ID_INVALID_PASSWORD:
                                    Disconnect(DisconnectReason.InvalidPassword);
                                    break;

                                case InternalPacketID.ID_PUBLIC_KEY_MISMATCH:
                                    Disconnect(DisconnectReason.SecurityError);
                                    break;

                                case InternalPacketID.ID_REMOTE_SYSTEM_REQUIRES_PUBLIC_KEY:
                                    Disconnect(DisconnectReason.SecurityError);
                                    break;

                                case InternalPacketID.ID_OUR_SYSTEM_REQUIRES_SECURITY:
                                    Disconnect(DisconnectReason.SecurityError);
                                    break;

                                case InternalPacketID.ID_CONNECTION_ATTEMPT_FAILED:
                                    Disconnect(DisconnectReason.AttemptFailed);
                                    break;

                                case InternalPacketID.ID_NO_FREE_INCOMING_CONNECTIONS:
                                    Disconnect(DisconnectReason.ServerIsFull);
                                    break;

                                case InternalPacketID.ID_IP_RECENTLY_CONNECTED:
                                    Disconnect(DisconnectReason.ConnectionRecently);
                                    break;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < interfaces.Count; i++)
                            {
                                if (interfaces[i] != null)
                                {
                                    interfaces[i].OnReceived(packet_id, packet_size, bitStream);
                                }
                            }
                        }
                    }
                }

                /* UPDATE STATS */
                Imports.Shared_Statistics(Pointer, 0, ref Statistics);
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakClient] " + dll_ex);
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakClient] " + entry_ex);
            }
        }
    }

    internal static void Init()
    {
        if (!Initialized)
        {
            try
            {
                Pointer = Imports.Client_Init();
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakClient] " + dll_ex);
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakClient] " + entry_ex);
            }
            finally
            {
                Initialized = Pointer != IntPtr.Zero;

                if (Initialized)
                {
                    Debug.Log("[RakClient] Initialized 0x" + Pointer.ToString("X"));

                    if (OnInitialized != null)
                        OnInitialized();
                }
            }
        }
    }

    internal static void Uninit()
    {
        if (Initialized)
        {
            try
            {
                Disconnect();
                Imports.Client_Uninit(Pointer);
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakClient] " + dll_ex);
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakClient] " + entry_ex);
            }
            finally
            {
                Pointer = IntPtr.Zero;
                Initialized = false;

                Debug.Log("[RakClient] Unitialized...");
            }
        }
    }

    internal static uint NativeInstances()
    {
        try
        {
            return Imports.GetClientInstances();
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakClient] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakClient] " + entry_ex);
        }

        return 0;
    }

    internal static void UninitInstances()
    {
        try
        {
            Uninit();
            Imports.UninitClientInstances();
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakClient] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakClient] " + entry_ex);
        }
    }


    /* PUBLIC */

    /// <summary>
    /// Connect to server
    /// </summary>
    public static ClientConnectResult Connect(string address, ushort port, string password = "", int attemps = 10)
    {
        server_address = address;
        server_port = port;
        server_password = password;
        ClientConnectResult result = Imports.Client_Connect(Pointer, address, port, password, attemps);

        if (result == ClientConnectResult.Connecting) { State = ClientState.IS_CONNECTING; }

        return result;
    }

    /// <summary>
    /// Internal only
    /// </summary>
    static void Disconnect(DisconnectReason reason = DisconnectReason.ByUser)
    {
        try
        {
            Imports.Client_Disconnect(Pointer);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakClient] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakClient] " + entry_ex);
        }
        finally
        {
            State = ClientState.IS_DISCONNECTED;
            for (int i = 0; i < interfaces.Count; i++)
            {
                if (interfaces[i] != null)
                {
                    interfaces[i].OnDisconnected(reason);
                }
            }
        }
    }

    /// <summary>
    /// Disconnect
    /// </summary>
    public static void Disconnect()
    {
        Disconnect(DisconnectReason.ByUser);
    }

    /// <summary>
    /// Send to server
    /// </summary>
    public static uint Send(BitStream bitStream, PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY, PacketReliability reliability = PacketReliability.UNRELIABLE, byte channel = 0)
    {
        try
        {
            return Imports.Client_Send(Pointer, bitStream.Pointer, priority, reliability, channel);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakClient] " + dll_ex);
            return 0;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakClient] " + entry_ex);
            return 0;
        }
    }

    /// <summary>
    /// Client guid
    /// </summary>
    public static ulong Guid
    {
        get
        {
            try
            {
                return Imports.Client_Guid(Pointer);
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakClient] " + dll_ex);
                return 0;
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakClient] " + entry_ex);
                return 0;
            }
        }
    }

    /* STATS */

    public static int Ping
    {
        get
        {
            try
            {
                return Imports.Client_GetPing(Pointer);
            }
            catch
            {
                return 0;
            }
        }
    }

    public static int AveragePing
    {
        get
        {
            try
            {
                return Imports.Client_GetAveragePing(Pointer);
            }
            catch
            {
                return 0;
            }
        }
    }

    public static int LowestPing
    {
        get
        {
            try
            {
                return Imports.Client_GetLowestPing(Pointer);
            }
            catch
            {
                return 0;
            }
        }
    }

    public static RakNetStatistics Statistics = new RakNetStatistics();

    public static int Loss
    {
        get
        {
            if (State == ClientState.IS_CONNECTED)
            {
                return Statistics.PacketLoss();
            }
            else
            {
                return 0;
            }
        }
    }
}
