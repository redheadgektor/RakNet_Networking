using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static class RakQuerySample
{
    public static bool StartQuery(string address, ushort port, out byte[] QueryData)
    {
        //Create UDP
        UdpClient udp = new UdpClient();
        udp.Client.ReceiveTimeout = 5000;
        udp.Client.SendTimeout = 5000;

        //Set Query Header
        byte[] header = Encoding.UTF8.GetBytes("RakNetQuery");

        //Send Header to server
        udp.Send(header, header.Length, address, port);

        QueryData = new byte[0];

        try
        {
            IPEndPoint serverAddress = null;

            //Receive Query Data
            QueryData = udp.Receive(ref serverAddress);
            Debug.Log("[RakQuery] Responce " + Encoding.UTF8.GetString(QueryData));
        }
        catch (SocketException ex)
        {
            Debug.LogError("[RakQuery] " + ex);
        }
        finally
        {
            udp.Close();
        }

        return QueryData.Length > 0;
    }
}
