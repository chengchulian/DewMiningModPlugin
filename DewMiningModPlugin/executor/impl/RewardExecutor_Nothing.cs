using System.Collections;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Nothing : IRewardExecutor
{

    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        return null;
    }
}