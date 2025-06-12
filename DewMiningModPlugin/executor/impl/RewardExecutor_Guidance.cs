using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Guidance : IRewardExecutor
{
    private readonly Color _color = Color.green;
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //引导祭坛
        Dew.CreateActor<Shrine_Guidance>(gold.position, Quaternion.identity);
        
        MessageUtil.SendChatMessageOptimized($"发现了一座<color={MiningUtil.ColorToHex(_color)}>引导祭坛</color>!");
    }
}