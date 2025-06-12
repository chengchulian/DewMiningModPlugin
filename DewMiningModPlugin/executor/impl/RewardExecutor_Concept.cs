using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Concept : IRewardExecutor
{
    private readonly Color _color = Color.cyan;

    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //概念残片 精华
        Dew.CreateActor<Shrine_Concept>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"恭喜！发现了一座<color={MiningUtil.ColorToHex(_color)}>概念祭坛</color>!");
    }
}