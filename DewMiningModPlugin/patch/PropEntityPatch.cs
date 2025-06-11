using DewCustomizeMod.config;
using DewMiningModPlugin.util;
using HarmonyLib;

namespace DewMiningModPlugin.patch;

[HarmonyPatch(typeof(PropEntity))]
public class PropEntityPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("OnCreate")]
    public static void OnCreatePostfix(PropEntity __instance)
    {
        if (!__instance.isServer)
        {
            return;
        }

        if (__instance is not PropEnt_Stone_Gold gold) return;

        var _zoneManager = NetworkedManagerBase<ZoneManager>.instance;

        var health = _zoneManager.currentZoneIndex + 5;
        gold.Status.maxHealth = health;
        gold.Status.Network_currentHealth = health;
    }
}