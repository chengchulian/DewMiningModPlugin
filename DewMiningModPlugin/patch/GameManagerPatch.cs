using System;
using System.Linq;
using DewCustomizeMod.config;
using DewMiningModPlugin.config;
using DewMiningModPlugin.entry;
using DewMiningModPlugin.handler;
using DewMiningModPlugin.manager;
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
            MonsterSpawnDataProvider.InitializePrefabCache();
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameManager.OnStartServer))]
    public static void OnStartServerPostfix(GameManager __instance)
    {
        if (__instance.isServer)
        {
            
            //重置配置
            MiningResources.Reload();
            
            //生成随机RoomMod
            NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded +=
                MiningLoadRoomHandler.AddRandomRoomMod;

            //挂载生成石头
            NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded +=
                MiningLoadRoomHandler.SpawnGoldStones;

            //挂载每进一个房间重置房间计数
            NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded +=
                StoneMonsterCountManager.ResetRoomMonsterCount;



            //启动监听器
            __instance.StartCoroutine(MonsterSpawnQueueListener.DequeueLoop());
            
            
            foreach (var entry in MiningResources.Config.Where(entry => entry.enabled))
            {
                if (!Enum.TryParse(entry.type, out MiningRewardEnum rewardType)) continue;
                //设置精华槽数量
                if (rewardType != MiningRewardEnum.Gemslot) continue;
                AttrCustomizeResources.Config.skillQGemCount = 2;
                AttrCustomizeResources.Config.skillWGemCount = 1;
                AttrCustomizeResources.Config.skillEGemCount = 1;
                AttrCustomizeResources.Config.skillRGemCount = 1;
                AttrCustomizeResources.Config.skillIdentityGemCount = 1;
            }
        }
    }
}