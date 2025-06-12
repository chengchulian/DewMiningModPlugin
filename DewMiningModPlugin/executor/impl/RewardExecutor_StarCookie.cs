using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_StarCookie : IRewardExecutor
{
    private readonly Color _color = Color.magenta;
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        Dew.CreateActor<Shrine_StarCookie>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"恭喜！发现了一颗<color={MiningUtil.ColorToHex(_color)}>星形饼干</color>!");
    }
}