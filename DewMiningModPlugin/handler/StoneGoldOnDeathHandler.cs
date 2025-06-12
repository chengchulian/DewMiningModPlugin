using System.Collections;
using DewMiningModPlugin.executor;
using DewMiningModPlugin.manager;
using UnityEngine;

namespace DewMiningModPlugin.handler;

public static class StoneGoldOnDeathHandler
{
    private static readonly WaitForSeconds rewardDelayWait = new(0.8f);

    public static void ProcessDeathRewards(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
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