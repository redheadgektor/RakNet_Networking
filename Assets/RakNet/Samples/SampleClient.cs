using UnityEngine;

public class SampleClient : MonoBehaviour, IRakClient
{
    public string playerName = "Player";

    void Awake()
    {
        /* Registering the interface for processing packets and receiving events when connecting and disconnecting clients */
        RakClient.RegisterInterface(this);
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(450);
        GUILayout.BeginVertical();
        if(RakClient.State == ClientState.IS_DISCONNECTED)
        {
            if(GUILayout.Button("Connect to server"))
            {
                RakClient.Connect("127.0.0.1", 7777);
            }
        }
        else if(RakClient.State == ClientState.IS_CONNECTING)
        {
            GUILayout.Label("Client connecting...");
        }
        else
        {
            if (GUILayout.Button("Disconnect"))
            {
                RakClient.Disconnect();
            }

            GUILayout.Box("Ping: "+RakClient.Ping);
            GUILayout.Box("Average ping: "+RakClient.AveragePing);
            GUILayout.Box("Lowest ping: "+RakClient.LowestPing);
            GUILayout.Box("Connection time: "+RakClient.Statistics.ConnectionTime());
            GUILayout.Box("Bytes received: "+RakClient.Statistics.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED));
            GUILayout.Box("Bytes sended: "+RakClient.Statistics.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_BYTES_SENT));
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void IRakClient.OnConnecting(string address, ushort port, string password)
    {
        Debug.Log("[SampleClient] Connecting to " + address + ":" + port);
    }

    void IRakClient.OnConnected(string address, ushort port, string password)
    {
        Debug.Log("[SampleClient] Connected to "+address+":"+port);
    }

    void IRakClient.OnDisconnected(DisconnectReason reason)
    {
        Debug.Log("[SampleClient] Disconnected "+reason);
    }

    void IRakClient.OnReceived(byte packet_id, uint packet_size, BitStream bitStream)
    {
        switch ((SamplePacketID)packet_id)
        {
            /* The server requests information about the client, and we will send him the name of the player */
            case SamplePacketID.CLIENT_DATA_REQUEST:

                /* Recommended design when writing data for sending */
                using (PooledBitStream bsOut = PooledBitStream.GetBitStream())
                {
                    /* 
                     * Always write the first byte as the packet number before sending data! (range from 134 to 255), 
                     * this is necessary so that the receiving data knows how to process it 
                     */
                    bsOut.Write((byte)SamplePacketID.CLIENT_DATA_REPLY);
                    bsOut.Write(playerName);

                    /* We send data to the server with priority for immediate sending, reliable transmission over channel 0 */
                    RakClient.Send(bsOut, PacketPriority.IMMEDIATE_PRIORITY, PacketReliability.RELIABLE, 0);
                }
                break;

            /* The server notify that the data has been processed */
            case SamplePacketID.CLIENT_DATA_ACCEPTED:
                playerName = bitStream.ReadString();//read the changed name of the player by the server
                Debug.Log("[SampleClient] Client data accepted by server... My name is "+playerName);
                break;
        }
    }
}
