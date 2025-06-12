using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Memory : IRewardExecutor
{
    private readonly Color _color = Color.magenta;
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //记忆残片 记忆
        Dew.CreateActor<Shrine_Memory>(gold.position,Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"发现一座<color={MiningUtil.ColorToHex(_color)}>记忆残片祭坛</color>!");
    }
}