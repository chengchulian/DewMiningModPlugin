using DewCustomizeMod.config;
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
            
            //挂载每进一个房间重置房间计数
            NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded +=
                MiningRewardUtil.ResetRoomMonsterCount;
            

            //设置精华槽数量
            if (Constant.ENABLE_GEMSLOT_REWARD)
            {
                AttrCustomizeResources.Config.skillQGemCount  = 2;
                AttrCustomizeResources.Config.skillWGemCount  = 1;
                AttrCustomizeResources.Config.skillEGemCount  = 1;
                AttrCustomizeResources.Config.skillRGemCount  = 1;
                AttrCustomizeResources.Config.skillIdentityGemCount = 1;
            }

        }
    }
}