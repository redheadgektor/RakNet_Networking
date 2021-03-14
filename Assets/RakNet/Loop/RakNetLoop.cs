using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

//   process_packets
//   all_updates
//   send_packets
internal static class RakNetLoop
{
    internal enum AddMode { Beginning, End }
    internal static int FindPlayerLoopEntryIndex(PlayerLoopSystem.UpdateFunction function, PlayerLoopSystem playerLoop, Type playerLoopSystemType)
    {
        if (playerLoop.type == playerLoopSystemType)
            return Array.FindIndex(playerLoop.subSystemList, (elem => elem.updateDelegate == function));

        if (playerLoop.subSystemList != null)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                int index = FindPlayerLoopEntryIndex(function, playerLoop.subSystemList[i], playerLoopSystemType);
                if (index != -1) return index;
            }
        }
        return -1;
    }

    internal static bool AddToPlayerLoop(PlayerLoopSystem.UpdateFunction function, Type ownerType, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType, AddMode addMode)
    {
        if (playerLoop.type == playerLoopSystemType)
        {
            int oldListLength = (playerLoop.subSystemList != null) ? playerLoop.subSystemList.Length : 0;
            Array.Resize(ref playerLoop.subSystemList, oldListLength + 1);

            if (addMode == AddMode.Beginning)
            {
                Array.Copy(playerLoop.subSystemList, 0, playerLoop.subSystemList, 1, playerLoop.subSystemList.Length - 1);
                playerLoop.subSystemList[0].type = ownerType;
                playerLoop.subSystemList[0].updateDelegate = function;

            }
            else if (addMode == AddMode.End)
            {
                playerLoop.subSystemList[oldListLength].type = ownerType;
                playerLoop.subSystemList[oldListLength].updateDelegate = function;
            }
            return true;
        }

        if (playerLoop.subSystemList != null)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (AddToPlayerLoop(function, ownerType, ref playerLoop.subSystemList[i], playerLoopSystemType, addMode))
                    return true;
            }
        }
        return false;
    }

    [RuntimeInitializeOnLoadMethod]
    static void RuntimeInitializeOnLoad()
    {
#if UNITY_EDITOR
        //if app non playing dont init RakNet and loop
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return;
        }

        UnityEditor.EditorApplication.LockReloadAssemblies();
#endif

        RakServer.Init();
        RakClient.Init();

        PlayerLoopSystem playerLoop = PlayerLoop.GetDefaultPlayerLoop();
        AddToPlayerLoop(EarlyUpdate, typeof(RakNetLoop), ref playerLoop, typeof(EarlyUpdate), AddMode.End);
        AddToPlayerLoop(PreLateUpdate, typeof(RakNetLoop), ref playerLoop, typeof(PreLateUpdate), AddMode.End);
        PlayerLoop.SetPlayerLoop(playerLoop);

#if !UNITY_EDITOR
        Application.quitting += RakServer.Uninit;
        Application.quitting += RakClient.Uninit;
#endif

    }

    static void EarlyUpdate()
    {
        //De-initialize if non playing
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            RakServer.Uninit();
            RakClient.Uninit();

            //Compiling scripts after uninitalizing
            UnityEditor.EditorApplication.UnlockReloadAssemblies();

            return;
        }
#endif

        RakServer.EarlyUpdate();
        RakClient.EarlyUpdate();
    }

    static void PreLateUpdate()
    {
        //RakServer.PreLateUpdate();
        //RakClient.PreLateUpdate();
    }
}