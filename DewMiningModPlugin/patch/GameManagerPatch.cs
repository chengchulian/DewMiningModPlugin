using DewMiningModPlugin.handler;
using DewMiningModPlugin.util;
using HarmonyLib;

namespace DewMiningModPlugin.patch;

[HarmonyPatch(typeof(GameManager))]
public class GameManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameManager.OnStartServer))]
    public static bool OnStartServerPrefix(GameManager __instance)
    {
        if (__instance.isServer)
        {
            MiningRewardUtil.InitializePrefabCache();
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameManager.OnStartServer))]
    public static void OnStartServerPostfix(GameManager __instance)
    {
        if (__instance.isServer)
        {
            
            //生成随机RoomMod
            NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded +=
                MiningLoadRoomHandler.AddRandomRoomMod;
            
            //挂载生成石头
            NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded +=
                MiningLoadRoomHandler.SpawnGoldStones;

        }
    }
}