using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_LoopCat : IRewardExecutor
{
    private readonly Color _color = Color.black;
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        // 猫咪
        Dew.CreateActor<Shrine_LoopCat>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"发现一只<color={MiningUtil.ColorToHex(_color)}>猫咪</color>!");
    }
}