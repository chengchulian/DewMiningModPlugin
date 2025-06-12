using System;
using System.Collections.Generic;
using System.Linq;
using DewMiningModPlugin.entry;
using Sirenix.Utilities;
using UnityEngine;

namespace DewMiningModPlugin.manager;

public static class MonsterSpawnDataProvider
{
    private static readonly Dictionary<string, Monster> monsterPrefabCache = new();


    public static void InitializePrefabCache()
    {
        PreloadMonsterPrefabs(GetMonsterList());
    }

    public static List<string> GetMonsterList()
    {
        var appendWith =
            ElementalMobs
                .AppendWith(RegularMobs)
                .AppendWith(BossList)
                .AppendWith(MidTierMobs);
        return appendWith.ToList();
    }


    private static void PreloadMonsterPrefabs(List<string> mobKeys)
    {
        foreach (var key in mobKeys)
        {
            try
            {
                if (!monsterPrefabCache.ContainsKey(key))
                {
                    var prefab = DewResources.FindOneByTypeSubstring<Actor>(key) as Monster;
                    if (prefab != null)
                    {
                        monsterPrefabCache[key] = prefab;
                    }
                    else
                    {
                        Debug.LogError($"Failed to load monster prefab: {key}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading prefab {key}: {ex.Message}");
            }
        }
    }


    public static Monster GetMonsterPrefab(string key)
    {
        if (monsterPrefabCache.Count == 0)
        {
            InitializePrefabCache();
        }

        if (monsterPrefabCache.TryGetValue(key, out var prefab))
        {
            return prefab;
        }

        // 如果不在缓存中则回退
        try
        {
            var newPrefab = DewResources.FindOneByTypeSubstring<Actor>(key) as Monster;
            if (newPrefab != null)
            {
                monsterPrefabCache[key] = newPrefab;
                return newPrefab;
            }

            Debug.LogError($"Monster prefab not found: {key}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting prefab {key}: {ex.Message}");
            return null;
        }
    }
    public static readonly List<string> BossList =
    [
        "Mon_Forest_BossDemon", "Mon_Sky_BossNyx", "Mon_SnowMountain_BossSkoll",
        "Mon_Special_BossLightElemental", "Mon_Special_BossObliviax"
    ];
    public static readonly List<string> ElementalMobs = new()
    {
        "Mon_SnowMountain_IceElemental",
        "Mon_DarkCave_DarkElemental",
        "Mon_LavaLand_FireElemental"
    };

    public static readonly List<string> RegularMobs = new()
    {
        "Mon_Forest_SpiderWarrior", "Mon_Forest_Scarab", "Mon_SnowMountain_LivingShards",
        "Mon_Forest_SpiderSpitter", "Mon_Sky_LittleBaam", "Mon_Forest_Treant",
        "Mon_Forest_Hound", "Mon_Sky_StarSeed", "Mon_SnowMountain_SnowWolf"
    };

    public static readonly List<string> MidTierMobs = new()
    {
        "Mon_Sky_BigBaam_Rooter", "Mon_Sky_BigBaam_Regular",
        "Mon_SnowMountain_Scavenger", "Mon_SnowMountain_VikingWarrior"
    };
    
    
    public static MonsterSpawnData ChooseRandomMonsterData(
        Vector3 center,
        float radius,
        float elementalChance,
        float midMobChance,
        float mirageChance,
        int[] elementalCounter = null)
    {
        float roll = UnityEngine.Random.value;

        var data = new MonsterSpawnData();

        if (roll < elementalChance)
        {
            int type = UnityEngine.Random.Range(0, ElementalMobs.Count);
            data.mobKey = ElementalMobs[type];
            data.elementalType = type;

            elementalCounter?.Let(c => c[type]++);
        }
        else if (roll < elementalChance + midMobChance)
        {
            data.mobKey = MidTierMobs[UnityEngine.Random.Range(0, MidTierMobs.Count)];
        }
        else
        {
            data.mobKey = RegularMobs[UnityEngine.Random.Range(0, RegularMobs.Count)];
        }

        float angle = UnityEngine.Random.value * Mathf.PI * 2f;
        float dist = UnityEngine.Random.value * radius;
        Vector3 offset = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);

        Vector3 raw = center + offset;
        data.position = Dew.GetValidAgentDestination_LinearSweep(center, Dew.GetPositionOnGround(raw));
        data.rotation = Quaternion.LookRotation((center - data.position).Flattened());
        data.hasMirage = UnityEngine.Random.value < mirageChance;

        return data;
    }
}

internal static class Extensions
{
    public static void Let<T>(this T self, Action<T> block) => block(self);
}