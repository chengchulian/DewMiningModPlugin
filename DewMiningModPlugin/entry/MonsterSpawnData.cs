using UnityEngine;

namespace DewMiningModPlugin.entry;

public struct MonsterSpawnData
{
    public string mobKey;
    public Vector3 position;
    public Quaternion rotation;
    public bool hasMirage;
    public int elementalType; // -1 = 非元素, 0 = 冰, 1 = 暗, 2 = 火
}