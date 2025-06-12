using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_UpgradeWell : IRewardExecutor
{
    private readonly Color _color = new Color(0.76f, 1f, 1f);
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        Dew.CreateActor<Shrine_UpgradeWell>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"恭喜！发现了一座<color={MiningUtil.ColorToHex(_color)}>许愿井</color>!");
        
    }
}