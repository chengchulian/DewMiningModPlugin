using System.Collections;

namespace DewMiningModPlugin.executor;

public interface IRewardExecutor
{

    IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info);
    
}
