using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Disintegration : IRewardExecutor
{
    private readonly Color _color = Color.yellow;
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //分解祭坛
        Dew.CreateActor<Shrine_Disintegration>(gold.position,Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"发现了一座<color={MiningUtil.ColorToHex(_color)}>分解祭坛</color>!");
    }
}