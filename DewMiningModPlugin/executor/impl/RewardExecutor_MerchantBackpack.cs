using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_MerchantBackpack : IRewardExecutor
{
    private readonly Color _color = new(0.85f, 0.65f, 0.13f);

    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //商人背包
        Dew.CreateActor<Shrine_Merchant_Backpack>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"恭喜！发现一个<color={MiningUtil.ColorToHex(_color)}>商人背包</color>!");
    }
}