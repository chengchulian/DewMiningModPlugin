using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Hatred : IRewardExecutor
{
    private readonly Color _color = Color.red;

    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //仇恨碎片
        Dew.CreateActor<Shrine_Hatred>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"发现一个<color={MiningUtil.ColorToHex(_color)}>仇恨碎片</color>,它的出现并不是很美好!");
    }
}