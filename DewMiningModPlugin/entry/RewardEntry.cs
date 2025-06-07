namespace DewMiningModPlugin.entry;

public struct RewardEntry
{
    public readonly string key;
    public readonly float weight;
        
    public RewardEntry(string key, float weight)
    {
        this.key = key;
        this.weight = weight;
    }
}