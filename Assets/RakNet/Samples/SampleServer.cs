using System.Collections.Generic;
using UnityEngine;

public struct SampleClientData
{
    public ulong guid;
    public string playerName;

    public SampleClientData(ulong guid, string username)
    {
        this.guid = guid;
        this.playerName = username;
    }
}

public class SampleServer : MonoBehaviour, IRakServer
{

    void Awake()
    {
        /* Registering the interface for processing packets and receiving events when connecting and disconnecting clients */
        RakServer.RegisterInterface(this);
    }

    void OnGUI()
    {
        if(RakServer.State == ServerState.NOT_STARTED || RakServer.State == ServerState.STOPPED)
        {
            if (GUILayout.Button("Start Server"))
            {
                RakServer.Start();
            }
        }
        else
        {
            if(GUILayout.Button("Stop Server"))
            {
                RakServer.Stop();
            }

            GUILayout.Box("Connected clients");

            foreach(SampleClientData data in Clients.Values)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Box(data.playerName);
                if (GUILayout.Button("Kick"))
                {
                    RakServer.CloseConnection(data.guid, true);
                }
                if (GUILayout.Button("Ban"))
                {
                    RakServer.AddBanIP(RakServer.GetAddress(data.guid, false));
                    RakServer.CloseConnection(data.guid, true);
                }
                GUILayout.EndHorizontal();
            }
        }
    }

    public Dictionary<ulong, SampleClientData> Clients = new Dictionary<ulong, SampleClientData>();//accepted clients dictionary

    void IRakServer.OnConnected(ulong guid)
    {
        Debug.Log("[SampleServer] Client connected with guid "+guid + " [IP: "+RakServer.GetAddress(guid,true)+"]");

        /* Immediately after connecting requesting client data */
        using(PooledBitStream bitStream = PooledBitStream.GetBitStream())
        {
            bitStream.Write((byte)SamplePacketID.CLIENT_DATA_REQUEST);
            RakServer.SendToClient(bitStream, guid, PacketPriority.IMMEDIATE_PRIORITY, PacketReliability.RELIABLE, 0);
        }
    }

    void IRakServer.OnDisconnected(ulong guid, DisconnectReason reason)
    {
        /* Removing client data from the dictionary */
        if (Clients.ContainsKey(guid))
        {
            Debug.Log("[Server] Client " + Clients[guid].playerName + " disconnected! (" + reason + ")");
            Clients.Remove(guid);
        }

    }

    void IRakServer.OnReceived(byte packet_id, ulong guid, BitStream bitStream)
    {
        switch ((SamplePacketID)packet_id)
        {
            /* Processing the client's response */
            case SamplePacketID.CLIENT_DATA_REPLY:
                string playerName = bitStream.ReadString();

                /* Adding the client data in the dictionary for further manipulations */
                Clients.Add(guid, new SampleClientData() { guid = guid, playerName = playerName });

                /* Notify the client that the data is accepted */
                using(PooledBitStream bsOut = PooledBitStream.GetBitStream())
                {
                    /* 
                     * Always write the first byte as the packet number before sending data! (range from 134 to 255), 
                     * this is necessary so that the receiving data knows how to process it 
                     */
                    bsOut.Write((byte)SamplePacketID.CLIENT_DATA_ACCEPTED);
                    bsOut.Write("edited_"+playerName);//Slightly modifying the player's name :)

                    /* Send the client data from the bitstream with low priority, reliable delivery on channel 0 */
                    RakServer.SendToClient(bsOut, guid, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE, 0);
                }
                break;
        }
    }
}
