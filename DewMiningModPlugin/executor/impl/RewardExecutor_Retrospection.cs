using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Retrospection : IRewardExecutor
{
    private readonly Color _color = Color.magenta;


    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        // 回顾残片 记忆
        Shrine_Retrospection byType = DewResources.GetByType<Shrine_Retrospection>();
        Dew.CreateActor(byType, gold.position, Quaternion.identity, null,
            delegate(Shrine_Retrospection shrine) { shrine.maxUseCount = DewPlayer.humanPlayers.Count; });

        MessageUtil.SendChatMessageOptimized($"恭喜！发现了一座<color={MiningUtil.ColorToHex(_color)}>回顾祭坛</color>!");
    }
}