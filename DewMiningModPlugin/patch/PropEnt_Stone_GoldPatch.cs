using System.Reflection;
using DewMiningModPlugin.handler;
using HarmonyLib;
using UnityEngine;

namespace DewMiningModPlugin.patch;

[HarmonyPatch(typeof(PropEnt_Stone_Gold))]
public class PropEntStoneGoldPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("OnDeath")]
    public static void OnDeathPostfix(PropEnt_Stone_Gold __instance, EventInfoKill info)
    {
        // 2. 如果非服务器，不继续处理
        if (!__instance.isServer)
            return ;

        // 3. 自定义掉落逻辑（不使用默认协程）
        StoneGoldOnDeathHandler.ProcessDeathRewards(__instance, info);
        
    }
}