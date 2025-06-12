using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Chaos : IRewardExecutor
{
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //混沌
        Shrine_Chaos shrineChaos = Dew.CreateActor<Shrine_Chaos>(gold.position, Quaternion.identity);
        Rarity shrineChaosRarity = shrineChaos.rarity;

        Color color = MiningUtil.GetColorByRarity(shrineChaosRarity);

        MessageUtil.SendChatMessageOptimized($"哇！发现了一颗<color={MiningUtil.ColorToHex(color)}>混沌宝珠</color>!");
    }
}