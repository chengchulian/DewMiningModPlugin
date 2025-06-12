using System.Collections;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Enlightenment : IRewardExecutor
{
    private readonly Color _color = Color.cyan;

    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);
        //启蒙残片 精华
        Shrine_Enlightenment byType = DewResources.GetByType<Shrine_Enlightenment>();
        Dew.CreateActor(byType, gold.position, Quaternion.identity, null,
            delegate(Shrine_Enlightenment shrine)
            {
                shrine.baseGoldCost  = 45;
            });


        MessageUtil.SendChatMessageOptimized($"恭喜！发现了一座<color={MiningUtil.ColorToHex(_color)}>启蒙祭坛</color>!");
    }
}