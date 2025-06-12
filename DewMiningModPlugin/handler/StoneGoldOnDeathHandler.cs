using System.Collections;
using DewMiningModPlugin.executor;
using DewMiningModPlugin.manager;
using UnityEngine;

namespace DewMiningModPlugin.handler;

public static class StoneGoldOnDeathHandler
{
    private static readonly WaitForSeconds rewardDelayWait = new(0.8f);
    
    [Tooltip("区域缩放前应用的基础金币乘数。")] private static float baseGoldMultiplier = 1.5f;

    [Tooltip("每个区域索引的额外金币乘数。")] private static float perZoneGoldMultiplier = 0.6f;


    public static void ProcessDeathRewards(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        
        var _zoneManager = NetworkedManagerBase<ZoneManager>.instance;
        var _pickupManager = NetworkedManagerBase<PickupManager>.instance;
        // 立即掉落金币
        float goldValue = (baseGoldMultiplier + perZoneGoldMultiplier * _zoneManager.currentZoneIndex)
                          * DewPlayer.humanPlayers.Count;

        _pickupManager.DropGold(
            isKillGold: false,
            isGivenByOtherPlayer: false,
            DewMath.RandomRoundToInt(goldValue),
            gold.transform.position
        );

        
        var room = SingletonDewNetworkBehaviour<Room>.instance;

        var executor = MiningRewardManager.SelectReward();
        room.StartCoroutine(ProcessRewardDelayed(executor, gold, info));
    }

    private static IEnumerator ProcessRewardDelayed(IRewardExecutor executor, PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return rewardDelayWait;
        yield return executor.ProcessRewardAsync(gold, info);
    }
}