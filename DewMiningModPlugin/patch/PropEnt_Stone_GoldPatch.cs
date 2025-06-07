using System.Reflection;
using DewMiningModPlugin.handler;
using HarmonyLib;
using UnityEngine;

namespace DewMiningModPlugin.patch;

[HarmonyPatch(typeof(PropEnt_Stone_Gold))]
public class PropEntStoneGoldPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("OnDeath")]
    public static bool OnDeathPrefix(PropEnt_Stone_Gold __instance, EventInfoKill info)
    {
        // 2. 如果非服务器，不继续处理
        if (!__instance.isServer)
            return false;

        // 3. 自定义掉落逻辑（不使用默认协程）
        StoneGoldOnDeathHandler.ProcessDeathRewards(__instance, info);

        // 4. 阻止原方法执行（跳过金币掉落）
        return false;
    }
}