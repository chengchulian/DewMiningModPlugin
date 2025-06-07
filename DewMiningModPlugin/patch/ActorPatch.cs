using Cysharp.Threading.Tasks.Triggers;
using HarmonyLib;

namespace DewMiningModPlugin.patch;

[HarmonyPatch(typeof(Actor))]
public class ActorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("OnDestroy")]
    public static void OnDestroyPostfix(PropEntity __instance)
    {
        if (!__instance.isServer)
        {
            return;
        }
        if (__instance is not PropEnt_Stone_Gold gold) return;
        gold.StopAllCoroutines();
    }
}