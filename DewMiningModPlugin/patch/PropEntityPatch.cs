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


        gold.Status.maxHealth = 10f;
        gold.Status.Network_currentHealth = 10f;
    }

}