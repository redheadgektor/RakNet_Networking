using System;
using System.Collections.Generic;
using UnityEngine;

public class RakServer
{
    static IntPtr Pointer = IntPtr.Zero;

    /// <summary>
    /// Server initialized and ready to start
    /// </summary>
    public static bool Initialized { get; private set; } = false;

    public delegate void OnInitializedCallback();
    /// <summary>
    /// On initalized server
    /// </summary>
    public static OnInitializedCallback OnInitialized;

    /// <summary>
    /// Current server state
    /// </summary>
    public static ServerState State { get; private set; } = ServerState.NOT_STARTED;

    public delegate void OnServerStartResultCallback(ServerStartResult result);
    /// <summary>
    /// On server start result
    /// </summary>
    public static OnServerStartResultCallback OnServerStartResult;

    public delegate void OnServerStopCallback();
    /// <summary>
    /// On server stop
    /// </summary>
    public static OnServerStopCallback OnServerStop;

    static IntPtr packet_ptr = IntPtr.Zero;

    static ulong receiver_guid = 0;
    static byte packet_id = 0;
    static uint packet_size = 0;

    static List<IRakServer> interfaces = new List<IRakServer>();

    public static void RegisterInterface(IRakServer server_interface)
    {
        interfaces.Add(server_interface);
    }

    public static void UnRegisterInterface(IRakServer server_interface)
    {
        interfaces.Remove(server_interface);
    }

    internal static void EarlyUpdate()
    {
        if (Initialized)
        {
            try
            {
                while (Imports.Shared_HasPackets(Pointer))
                {
                    packet_ptr = Imports.Server_GetPacket(Pointer, ref receiver_guid, ref packet_size);

                    using (PooledBitStream bitStream = PooledBitStream.GetBitStream())
                    {
                        bitStream.ReadPacket(packet_ptr);

                        packet_id = bitStream.ReadByte();

                        if ((InternalPacketID)packet_id < InternalPacketID.ID_USER_PACKET_ENUM)
                        {
                            switch ((InternalPacketID)packet_id)
                            {
                                case InternalPacketID.ID_NEW_INCOMING_CONNECTION:
                                    for (int i = 0; i < interfaces.Count; i++)
                                    {
                                        interfaces[i].OnConnected(receiver_guid);
                                    }
                                    break;

                                case InternalPacketID.ID_DISCONNECTION_NOTIFICATION:
                                    for (int i = 0; i < interfaces.Count; i++)
                                    {
                                        interfaces[i].OnDisconnected(receiver_guid, DisconnectReason.ConnectionClosed);
                                    }
                                    break;

                                case InternalPacketID.ID_CONNECTION_LOST:
                                    for (int i = 0; i < interfaces.Count; i++)
                                    {
                                        interfaces[i].OnDisconnected(receiver_guid, DisconnectReason.ConnectionLost);
                                    }
                                    break;
                            }
                        }
                        else if((InternalPacketID)packet_id >= InternalPacketID.ID_USER_PACKET_ENUM)
                        {
                            for (int i = 0; i < interfaces.Count; i++)
                            {
                                interfaces[i].OnReceived(packet_id, receiver_guid, bitStream);
                            }
                        }
                    }
                }
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakServer] " + dll_ex);
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakServer] " + entry_ex);
            }
        }
    }

    internal static void PreLateUpdate()
    {

    }

    internal static void Init()
    {
        if (!Initialized)
        {
            try
            {
                Pointer = Imports.Server_Init();
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakServer] " + dll_ex);
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakServer] " + entry_ex);
            }
            finally
            {
                Initialized = Pointer != IntPtr.Zero;

                if (Initialized)
                {
                    Debug.Log("[RakServer] Initialized"/* 0x" + Pointer.ToString("X")*/);

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
                Stop();
                Imports.Server_Uninit(Pointer);
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakServer] " + dll_ex);
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakServer] " + entry_ex);
            }
            finally
            {
                Pointer = IntPtr.Zero;
                Initialized = false;

                Debug.Log("[RakServer] Unitialized...");
            }
        }
    }

    /// <summary>
    /// Start server
    /// </summary>
    public static ServerStartResult Start(string address = "", ushort port = 7777, string password = "", ushort max_connections = 32, bool insecure = false)
    {
        ServerStartResult result = ServerStartResult.ServerInitError;
        try
        {
            result = Imports.Server_Start(Pointer, address, port, max_connections, insecure);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
        finally
        {
            if (result == ServerStartResult.Started)
            {
                State = ServerState.STARTED;
            }
        }

        return result;
    }

    /// <summary>
    /// Stop server
    /// </summary>
    public static void Stop()
    {
        if (State == ServerState.NOT_STARTED || State == ServerState.STOPPED)
            return;

        try
        {
            Imports.Server_Stop(Pointer);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
        State = ServerState.STOPPED;

        if (OnServerStop != null)
            OnServerStop();
    }

    /// <summary>
    /// Bound Address
    /// </summary>
    public static string BoundAddress
    {
        get
        {
            try
            {
                return Imports.IntPtrToStringAnsi(Imports.Server_IP(Pointer));
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakServer] " + dll_ex);
                return string.Empty;
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakServer] " + entry_ex);
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Send to client using guid
    /// </summary>
    public static uint SendToClient(BitStream bitStream, ulong guid, PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY, PacketReliability reliability = PacketReliability.UNRELIABLE, byte channel = 0)
    {
        return Imports.Server_SendToClient(Pointer, bitStream.Pointer, guid, priority, reliability, channel);
    }

    /// <summary>
    /// Send to all connected clients
    /// </summary>
    public static uint SendToAll(BitStream bitStream, PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY, PacketReliability reliability = PacketReliability.UNRELIABLE, byte channel = 0)
    {
        return Imports.Server_SendToAll(Pointer, bitStream.Pointer, priority, reliability, channel);
    }

    /// <summary>
    /// Send to all connected clients ignoring specified guid
    /// </summary>
    public static uint SendToAllIgnore(BitStream bitStream, ulong guid_ignore, PacketPriority priority = PacketPriority.IMMEDIATE_PRIORITY, PacketReliability reliability = PacketReliability.UNRELIABLE, byte channel = 0)
    {
        return Imports.Server_SendToAllIgnore(Pointer, bitStream.Pointer, guid_ignore, priority, reliability, channel);
    }

    /// <summary>
    /// Close connection with the specified client guid
    /// </summary>
    /// <param name="send_disconnect_notify">Notify the client that the server has closed the connection?</param>
    public static void CloseConnection(ulong guid, bool send_disconnect_notify = true)
    {
        try
        {
            Imports.Server_CloseConnection(Pointer, guid, send_disconnect_notify);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Enable or disable allowing frequent connections from the same IP adderss
    /// </summary>
    public static void SetLimitIPConnectionFrequency(bool enabled)
    {
        try
        {
            Imports.Server_SetLimitIPConnectionFrequency(Pointer, enabled);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Limits how much outgoing bandwidth can be sent per-connection. This limit does not apply to the sum of all connections! Exceeding the limit queues up outgoing traffic
    /// </summary>
    /// <param name="bytes_per_sec">0 - Unlimited</param>
    public static void SetLimitBandwidth(uint bytes_per_sec)
    {
        try
        {
            Imports.Server_LimitBandwidth(Pointer, bytes_per_sec);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Allow server queries
    /// </summary>
    public static void AllowQuery(bool enabled)
    {
        try
        {
            Imports.Server_AllowQuery(Pointer, enabled);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Server queries is allowed?
    /// </summary>
    public static bool IsQueryAllow(bool enabled)
    {
        try
        {
            return Imports.Server_IsQueryAllowed(Pointer);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return false;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return false;
        }
    }

    /// <summary>
    /// Query responce data
    /// </summary>
    public static void SetQueryResponce(byte[] data)
    {
        try
        {
            Imports.Server_SetQueryResponce(Pointer, data);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Set a password, clients who do not know the password will not be able to connect
    /// </summary>
    public static void SetPassword(string password)
    {
        try
        {
            Imports.Server_SetPassword(Pointer, password);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Server has password?
    /// </summary>
    public static bool HasPassword()
    {
        try
        {
            return Imports.Server_HasPassword(Pointer);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return false;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return false;
        }
    }

    /// <summary>
    /// Maximum connections
    /// </summary>
    public static void SetMaxConnections(ushort max_connections)
    {
        try
        {
            Imports.Server_SetMaxConnections(Pointer, max_connections);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Maximum connections
    /// </summary>
    public static ushort GetMaxConnections()
    {
        try
        {
            return Imports.Server_GetMaximumConnections(Pointer);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return 0;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return 0;
        }
    }

    /// <summary>
    /// Currently connections count
    /// </summary>
    public static ushort NumberOfConnections
    {
        get
        {
            try
            {
                return Imports.Server_NumberOfConnections(Pointer);
            }
            catch (DllNotFoundException dll_ex)
            {
                Debug.LogError("[RakServer] " + dll_ex);
                return 0;
            }
            catch (EntryPointNotFoundException entry_ex)
            {
                Debug.LogError("[RakServer] " + entry_ex);
                return 0;
            }
        }
    }

    /// <summary>
    /// Get client ping
    /// </summary>
    public static int GetPing(ulong guid)
    {
        try
        {
            return Imports.Server_GetPing(Pointer, guid);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return 0;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return 0;
        }
    }

    /// <summary>
    /// Get average client ping
    /// </summary>
    public static int GetAveragePing(ulong guid)
    {
        try
        {
            return Imports.Server_GetAveragePing(Pointer, guid);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return 0;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return 0;
        }
    }

    /// <summary>
    /// Get lowest client ping
    /// </summary>
    public static int GetLowestPing(ulong guid)
    {
        try
        {
            return Imports.Server_GetLowestPing(Pointer, guid);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return 0;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return 0;
        }
    }

    /// <summary>
    /// Get Client statistics
    /// </summary>
    public bool GetStatistics(uint index, ref RakNetStatistics statistics)
    {
        try
        {
            return Imports.Shared_Statistics(Pointer, index, ref statistics);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return false;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return false;
        }
    }

    /// <summary>
    /// Add ban
    /// </summary>
    /// <param name="second">ban length in seconds   0 - is infinity</param>
    /// <returns></returns>
    public static void AddBanIP(string address, int second = 0)
    {
        try
        {
            Imports.Server_AddBanIP(Pointer, address, second);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Remove ban
    /// </summary>
    public static void RemoveBanIP(string address)
    {
        try
        {
            Imports.Server_RemoveBanIP(Pointer, address);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
        }
    }

    /// <summary>
    /// Remove ban
    /// </summary>
    public static bool IsBanned(string address)
    {
        try
        {
            return Imports.Server_IsBannedIP(Pointer, address);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return false;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return false;
        }
    }

    /// <summary>
    /// Get client address
    /// </summary>
    public static string GetAddress(ulong guid, bool with_port = false)
    {
        try
        {
            return Imports.IntPtrToStringAnsi(Imports.Shared_GetAddress(Pointer, guid, with_port));
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return string.Empty;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return string.Empty;
        }
    }

    /// <summary>
    /// Get client port
    /// </summary>
    /// <param name="guid"></param>
    public static ushort GetPort(ulong guid)
    {
        try
        {
            return Imports.Shared_GetPort(Pointer, guid);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return 0;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return 0;
        }
    }

    /// <summary>
    /// Get connection index from guid
    /// </summary>
    public static int GetIndexFromGuid(ulong guid)
    {
        try
        {
            return Imports.Server_GetIndexFromGuid(Pointer, guid);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return -1;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return -1;
        }
    }

    /// <summary>
    /// Get connection guid from index
    /// </summary>
    public static ulong GetGuidFromIndex(int index)
    {
        try
        {
            return Imports.Server_GetGuidFromIndex(Pointer, index);
        }
        catch (DllNotFoundException dll_ex)
        {
            Debug.LogError("[RakServer] " + dll_ex);
            return 0;
        }
        catch (EntryPointNotFoundException entry_ex)
        {
            Debug.LogError("[RakServer] " + entry_ex);
            return 0;
        }
    }
}
