using System.Collections.Concurrent;
using DewMiningModPlugin.entry;
using DewMiningModPlugin.executor;
using DewMiningModPlugin.executor.impl;

namespace DewMiningModPlugin.config;

public static class RewardExecutorFactory
{
    private static readonly ConcurrentDictionary<MiningRewardEnum, IRewardExecutor> _executorCache = new();

    public static IRewardExecutor Create(MiningRewardEnum type)
    {
        return _executorCache.GetOrAdd(type, CreateNewExecutor);
    }

    private static IRewardExecutor CreateNewExecutor(MiningRewardEnum type)
    {
        return type switch
        {
            MiningRewardEnum.Monster => new RewardExecutor_Monster(),
            MiningRewardEnum.MiniBoss => new RewardExecutor_MiniBoss(),
            MiningRewardEnum.Boss => new RewardExecutor_Boss(),
            MiningRewardEnum.Merchant => new RewardExecutor_Merchant(),
            MiningRewardEnum.Concept => new RewardExecutor_Concept(),
            MiningRewardEnum.Memory => new RewardExecutor_Memory(),
            MiningRewardEnum.Enlightenment => new RewardExecutor_Enlightenment(),
            MiningRewardEnum.Retrospection => new RewardExecutor_Retrospection(),
            MiningRewardEnum.Chaos => new RewardExecutor_Chaos(),
            MiningRewardEnum.AltarOfCleansing => new RewardExecutor_AltarOfCleansing(),
            MiningRewardEnum.UpgradeWell => new RewardExecutor_UpgradeWell(),
            MiningRewardEnum.Disintegration => new RewardExecutor_Disintegration(),
            MiningRewardEnum.FragmentOfRadiance => new RewardExecutor_FragmentOfRadiance(),
            MiningRewardEnum.Guidance => new RewardExecutor_Guidance(),
            MiningRewardEnum.Hatred => new RewardExecutor_Hatred(),
            MiningRewardEnum.LoopCat => new RewardExecutor_LoopCat(),
            MiningRewardEnum.MerchantBackpack => new RewardExecutor_MerchantBackpack(),
            MiningRewardEnum.StarCookie => new RewardExecutor_StarCookie(),
            MiningRewardEnum.Gemslot => new RewardExecutor_Gemslot(),
            MiningRewardEnum.Nothing => new RewardExecutor_Nothing(),
            _ => new RewardExecutor_Nothing()
        };
    }

    public static void ClearCache()
    {
        _executorCache.Clear();
    }
}
