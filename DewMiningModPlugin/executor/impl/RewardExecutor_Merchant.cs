using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Merchant : IRewardExecutor
{
    private readonly Color _color = new(0.85f, 0.65f, 0.13f);

    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        Dew.CreateActor<PropEnt_Merchant_Jonas>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"挖到了一位<color={MiningUtil.ColorToHex(_color)}>流浪商人-乔纳斯</color>！");
    }
}