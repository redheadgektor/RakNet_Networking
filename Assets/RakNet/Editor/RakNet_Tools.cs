#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

public class RakNet_Tools : EditorWindow
{
    public static RakNet_Tools Instance;

    [MenuItem("RakNet/Tools", priority = 2056)]
    static void ShowWindow()
    {
        Instance = (RakNet_Tools)GetWindow(typeof(RakNet_Tools), true);
        Instance.Show();
        Instance.titleContent = new GUIContent("RakNet Tools");
        Instance.minSize = new Vector2(640, 380);
    }

    enum WindowType { Client, Server }
    WindowType m_WindowType = WindowType.Client;

    StringBuilder m_DebugString = new StringBuilder(1024);

    string m_DisconnectMessage = "Connection closed by Unity Editor";
    Vector2 m_Scroll;

    private void OnGUI()
    {
        m_WindowType = (WindowType)EditorGUILayout.EnumPopup(m_WindowType);

        if (m_WindowType == WindowType.Client)
        {
            GUILayout.Box("Client Instance at 0x" + RakClient.Pointer.ToString("X"));
            GUILayout.Box("State " + RakClient.State);

            if (RakClient.State == ClientState.IS_CONNECTED)
            {
                m_DebugString.Clear();
                m_DebugString.AppendFormat(
                    "FPS {0}\n" +
                    "Ping {1}\n" +
                    "Recv bytes:(current/total) {2}/{3} packets: (current/total) {4}/{5}\n" +
                    "Sent bytes:(current/total) {6}/{7} packets: (current/total) {8}/{9}\n" +
                    "Connection Time {10}",

                    (int)(1f / Time.deltaTime), //0
                    RakClient.Ping,//1

                    /* RECV (current/total)*/
                    RakClient.Statistics.GetStatsLastSecond(RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED), RakClient.Statistics.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED),//3
                    RakClient.Statistics.GetStatsLastSecond(RNSPerSecondMetrics.ACTUAL_MESSAGES_RECEIVED), RakClient.Statistics.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_MESSAGES_RECEIVED),//5

                    /* SENT (current/total)*/
                    RakClient.Statistics.GetStatsLastSecond(RNSPerSecondMetrics.ACTUAL_BYTES_SENT), RakClient.Statistics.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_BYTES_SENT),//7
                    RakClient.Statistics.GetStatsLastSecond(RNSPerSecondMetrics.ACTUAL_MESSAGES_SENT), RakClient.Statistics.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_MESSAGES_SENT),//9

                    RakClient.Statistics.ConnectionTime()//10
                    );
                GUILayout.Space(3);
                GUILayout.Label("Statistics");
                GUILayout.BeginVertical("box");
                GUILayout.Label(m_DebugString.ToString());
                GUILayout.EndVertical();
            }
        }
        else if (m_WindowType == WindowType.Server)
        {
            GUILayout.Box("Server Instance at 0x" + RakServer.Pointer.ToString("X"));
            GUILayout.Box("State " + RakServer.State);
            EditorGUI.ProgressBar(new Rect(3, 75, position.width - 15, 20), RakServer.NumberOfConnections / (float)RakServer.GetMaxConnections(), "Connections " + RakServer.NumberOfConnections + "/" + RakServer.GetMaxConnections());

            EditorGUILayout.Space(30);
            m_Scroll = GUILayout.BeginScrollView(m_Scroll);

            for (uint i = 0; i < RakServer.NumberOfConnections; i++)
            {
                ulong guid = RakServer.GetGuidFromIndex((int)i);
                GUILayout.BeginHorizontal("box");

                GUILayout.BeginVertical("box");
                GUILayout.Box("Index: " + i);
                GUILayout.Box("GUID: " + guid);
                GUILayout.Box("IP: " + RakServer.GetAddress(guid, true));
                GUILayout.EndVertical();


                GUILayout.BeginVertical("box");
                GUILayout.Box("Ping: " + RakServer.GetPing(guid));
                GUILayout.Box("Average Ping: " + RakServer.GetAveragePing(guid));
                GUILayout.Box("Lowest Ping: " + RakServer.GetLowestPing(guid));
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical("box");
                RakNetStatistics stats = new RakNetStatistics();
                if (RakServer.GetStatistics(i, ref stats))
                {
                    GUILayout.Box("Connection time: " + stats.ConnectionTime());
                    GUILayout.Box("Recv (current/total): " + stats.GetStatsLastSecond(RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED) + "/" + stats.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED)+" ("+stats.GetStatsLastSecond(RNSPerSecondMetrics.ACTUAL_MESSAGES_RECEIVED)+"/"+stats.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_MESSAGES_RECEIVED));
                    GUILayout.Box("Sent (current/total): " + stats.GetStatsLastSecond(RNSPerSecondMetrics.ACTUAL_BYTES_SENT) + "/" + stats.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_BYTES_SENT) + " (" + stats.GetStatsLastSecond(RNSPerSecondMetrics.ACTUAL_MESSAGES_SENT) + "/" + stats.GetStatsTotal(RNSPerSecondMetrics.ACTUAL_MESSAGES_SENT));
                }
                else
                {
                    GUILayout.Box("Stats unavailable!");
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical("box");
                if (GUILayout.Button("Close connection"))
                {
                    RakServer.CloseConnection(guid, true, m_DisconnectMessage);
                }

                if (GUILayout.Button("Close connection (silent)"))
                {
                    RakServer.CloseConnection(guid, false, string.Empty);
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }


        Repaint();
    }
}
#endif
