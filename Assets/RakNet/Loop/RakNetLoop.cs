using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

//   process_packets
//   all_updates
//   send_packets
internal sealed class RakNetLoop
{
    static bool InsertNewSystem<T>(int index, PlayerLoopSystem.UpdateFunction function, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType)
    {
        if (playerLoop.type == playerLoopSystemType)
        {
            var list = new List<PlayerLoopSystem>(playerLoop.subSystemList);
            PlayerLoopSystem sys = new PlayerLoopSystem();
            sys.type = typeof(T);
            sys.updateDelegate = function;
            list.Insert(index, sys);
            playerLoop.subSystemList = list.ToArray();
            return true;
        }

        if (playerLoop.subSystemList != null)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (InsertNewSystem<T>(index, function, ref playerLoop.subSystemList[i], playerLoopSystemType))
                    return true;
            }
        }
        return false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Initialize()
    {
#if UNITY_EDITOR
        UnityEditor.Compilation.CompilationPipeline.compilationFinished += delegate (object o)
        {
            DestroyAll();
        };
        //if app non playing dont init RakNet and loop
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return;
        }
#endif
        RakServer.Init();
        RakClient.Init();

        PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        InsertNewSystem<RakNetLoop>(0, Update, ref playerLoop, typeof(EarlyUpdate));
        PlayerLoop.SetPlayerLoop(playerLoop);

        Application.quitting += DestroyAll;

    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("RakNet/Destroy all")]
#endif
    static void DestroyAll()
    {
        RakServer.Destroy();
        RakClient.Destroy();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("RakNet/Destroy client")]
#endif
    static void DestroyClient()
    {
        RakClient.Destroy();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("RakNet/Destroy server")]
#endif
    static void DestroyServer()
    {
        RakServer.Destroy();
    }

    static void Update()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return;
        }
#endif
        RakServer.Update();
        RakClient.Update();
    }
}
