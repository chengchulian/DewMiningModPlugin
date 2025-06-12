using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DewMiningModPlugin.config;
using DewMiningModPlugin.entry;
using DewMiningModPlugin.executor;
using DewMiningModPlugin.executor.impl;

namespace DewMiningModPlugin.manager;

public static class MiningRewardManager
{
    private static readonly IRewardExecutor DefaultExecutor = new RewardExecutor_Nothing();

    public static IRewardExecutor SelectReward()
    {
        List<RewardConfigEntry> rewardConfigEntries = MiningResources.Config;

        float totalWeight = rewardConfigEntries.Sum(entry => entry.weight);


        float roll = UnityEngine.Random.value * totalWeight;
        float accumulator = 0f;

        foreach (var entry in rewardConfigEntries)
        {
            if (!entry.enabled) continue;
            
            accumulator += entry.weight;
            if (roll <= accumulator)
            {
                if (!Enum.TryParse(entry.type, out MiningRewardEnum rewardType)) continue;
                return RewardExecutorFactory.Create(rewardType);
            }
        }
        return DefaultExecutor;
    }
}