using BepInEx;
using DewMiningModPlugin.util;
using HarmonyLib;

namespace DewMiningModPlugin;


[BepInPlugin(Constant.PluginGuid, Constant.PluginName, Constant.PluginVersion)]
[BepInDependency(Constant.AttrCustomizePluginGuid, BepInDependency.DependencyFlags.HardDependency)]
public class DewMiningModPlugin : BaseUnityPlugin
{

    private void Awake()
    {
        
        Logger.LogInfo($"{Constant.PluginName} 插件已加载");

        var harmony = new Harmony(Constant.PluginGuid);
        harmony.PatchAll();
    }
    
}