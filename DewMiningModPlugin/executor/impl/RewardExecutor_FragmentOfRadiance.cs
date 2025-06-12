using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;


public class RewardExecutor_FragmentOfRadiance : IRewardExecutor
{
    
    private readonly Color _color = Color.yellow;
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //光辉碎片
        Dew.CreateActor<Shrine_FragmentOfRadiance>(gold.position, Quaternion.identity);
        MessageUtil.SendChatMessageOptimized($"哇！发现了一个<color={MiningUtil.ColorToHex(_color)}>光辉碎片</color>!");
    }
}