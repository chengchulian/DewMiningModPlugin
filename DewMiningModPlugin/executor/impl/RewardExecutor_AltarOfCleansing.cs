using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_AltarOfCleansing : IRewardExecutor
{
    private readonly Color _color = Color.cyan;
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        // 短暂延迟
        yield return new WaitForSeconds(0.1f); 
        //净化祭坛
        Dew.CreateActor<Shrine_AltarOfCleansing>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"恭喜！发现了一座<color={MiningUtil.ColorToHex(_color)}>净化祭坛</color>!");

        yield break;
    }
}