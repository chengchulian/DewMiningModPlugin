using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DewCustomizeMod.config;
using DewCustomizeMod.util;
using DewMiningModPlugin.entry;
using Mirror;
using UnityEngine;

namespace DewMiningModPlugin.util;

public static class MiningRewardUtil
{
    [Tooltip("怪物奖励触发时生成的最小怪物数量。")] private static int minMonsterSpawnCount = 2;

    [Tooltip("怪物奖励触发时生成的最大怪物数量。")] private static int maxMonsterSpawnCount = 5;

    [Tooltip("每个区域允许的黄金石怪物最大数量")] private static int maxStoneMonstersPerZone = 15;


    [Tooltip("每个生成的怪物成为元素生物的概率(0-1)。")] private static float elementalChance = 0.035f;

    [Tooltip("普通生成变为中级怪物的概率(0-1)。")] private static float midMobChance = 0.2f;

    [Tooltip("每个生成的怪物获得幻影皮肤效果的概率(0-1)。")] private static float mirageSkinChance = 0.05f;

    [Tooltip("石头周围生成怪物的散射半径。")] private static float spawnRadius = 2f;

    [Tooltip("每帧最多生成的怪物数量以防止卡顿。")] private static int maxMonstersPerFrame = 2;

    [Tooltip("怪物生成批次之间的延迟(秒)。")] private static float monsterSpawnBatchDelay = 0.1f;

    

    // 线程安全锁
    private static readonly object _zoneCountLock = new();

    // 怪物预制体缓存用于性能
    private static readonly Dictionary<string, Monster> monsterPrefabCache = new();

    // 预分配对象
    private static WaitForSeconds spawnBatchWait = new(monsterSpawnBatchDelay);

    // 区域怪物计数器 (zoneIndex -> count)
    private static readonly Dictionary<int, int> _stoneMonsterCountByZone = new();

    private static readonly Stack<List<MonsterSpawnData>> spawnDataListPool = new Stack<List<MonsterSpawnData>>(2);


    private static readonly object cacheLock = new object();
    
    private static bool isPrefabCacheInitialized = false;
    // 怪物数组缓存为静态只读
    private static readonly string[] elementalMobs =
    {
        "Mon_SnowMountain_IceElemental",
        "Mon_DarkCave_DarkElemental",
        "Mon_LavaLand_FireElemental"
    };

    private static readonly string[] regularMobs =
    {
        "Mon_Forest_SpiderWarrior", "Mon_Forest_Scarab", "Mon_SnowMountain_LivingShards",
        "Mon_Forest_SpiderSpitter", "Mon_Sky_LittleBaam", "Mon_Forest_Treant",
        "Mon_Forest_Hound", "Mon_Sky_StarSeed", "Mon_SnowMountain_SnowWolf"
    };

    private static readonly string[] midTierMobs =
    {
        "Mon_Sky_BigBaam_Rooter", "Mon_Sky_BigBaam_Regular",
        "Mon_SnowMountain_Scavenger", "Mon_SnowMountain_VikingWarrior"
    };

    private static readonly string[] bossList =
    {
        "Mon_Forest_BossDemon", "Mon_Sky_BossNyx", "Mon_SnowMountain_BossSkoll",
        "Mon_Special_BossLightElemental", "Mon_Special_BossObliviax"
    };

    
    public static void InitializePrefabCache()
    {
        lock (cacheLock)
        {
            if (isPrefabCacheInitialized) return;
            
            // 预加载所有怪物预制体
            PreloadMonsterPrefabs(elementalMobs);
            PreloadMonsterPrefabs(regularMobs);
            PreloadMonsterPrefabs(midTierMobs);
            PreloadMonsterPrefabs(bossList);
            
            isPrefabCacheInitialized = true;
        }
    }

    // 预加载怪物预制体
    private static void PreloadMonsterPrefabs(string[] mobKeys)
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
    public static void SpawnMiniBossOptimized(PropEnt_Stone_Gold gold)
    {
        var pos = gold.transform.position;

        var _room = SingletonDewNetworkBehaviour<Room>.instance;
        var _zoneManager = NetworkedManagerBase<ZoneManager>.instance;

        if (_room?.monsters == null) return;

        // 检查区域怪物上限
        if (!CanSpawnStoneMonster())
        {
            MessageUtil.SendChatMessageOptimized("<color=#FF6B6B>区域怪物已达上限，无法生成迷你首领!</color>");
            return;
        }

        // 存储原始值以便稍后恢复
        int originalBossCount = AttrCustomizeResources.Config.bossCount;
        int originalBossCountAddByZone = AttrCustomizeResources.Config.bossCountAddByZone;
        int originalBossCountAddByLoop = AttrCustomizeResources.Config.bossCountAddByLoop;

        try
        {
            // 临时强制1个boss
            AttrCustomizeResources.Config.bossCount = 1;
            AttrCustomizeResources.Config.bossCountAddByZone = 0;
            AttrCustomizeResources.Config.bossCountAddByLoop = 0;

            // 使用优化生成设置
            var settings = new SpawnMonsterSettings
            {
                spawnPosGetter = () => pos,
                afterSpawn = (Entity e) =>
                {
                    // 生成后恢复原始值
                    AttrCustomizeResources.Config.bossCount = originalBossCount;
                    AttrCustomizeResources.Config.bossCountAddByZone = originalBossCountAddByZone;
                    AttrCustomizeResources.Config.bossCountAddByLoop = originalBossCountAddByLoop;

                    // 增加怪物计数
                    IncrementStoneMonsterCount();
                    int currentZone = _zoneManager.currentZoneIndex;

                    // 添加跟踪组件确保异常销毁时减少计数
                    if (e != null && e.gameObject != null)
                    {
                        AddMonsterTracker(e.gameObject, currentZone, gold);
                    }
                }
            };
            var isCutsceneSkippedRef = typeof(SpawnMonsterSettings).GetField("isCutsceneSkipped",
                BindingFlags.NonPublic | BindingFlags.Instance);
            isCutsceneSkippedRef.SetValue(settings, true);

            _room.monsters.SpawnMiniBoss(settings, null, null);
            MessageUtil.SendChatMessageOptimized("<color=#FF6B6B>一只迷你boss从金矿中出现!</color>");
        }
        catch (Exception ex)
        {
            Debug.LogError($"生成迷你boss错误: {ex.Message}\n{ex.StackTrace}");

            // 确保即使出错也恢复值
            AttrCustomizeResources.Config.bossCount = originalBossCount;
            AttrCustomizeResources.Config.bossCountAddByZone = originalBossCountAddByZone;
            AttrCustomizeResources.Config.bossCountAddByLoop = originalBossCountAddByLoop;
        }
    }

    // 批量生成怪物
    public static IEnumerator SpawnMonstersBatched(PropEnt_Stone_Gold gold)
    {
        var _zoneManager = NetworkedManagerBase<ZoneManager>.instance;

        var basePos = gold.transform.position;

        // 检查区域怪物上限
        if (!CanSpawnStoneMonster())
        {
            MessageUtil.SendChatMessageOptimized("<color=#FF6B6B>区域怪物已达上限，无法生成更多怪物!</color>");
            yield break;
        }

        int count = UnityEngine.Random.Range(minMonsterSpawnCount, maxMonsterSpawnCount + 1);

        // 动态调整生成数量（考虑区域上限）
        int availableInZone;
        lock (_zoneCountLock)
        {
            availableInZone = maxStoneMonstersPerZone -
                              (_stoneMonsterCountByZone.TryGetValue(_zoneManager.currentZoneIndex, out int currentCount)
                                  ? currentCount
                                  : 0);
        }

        int maxCount = availableInZone;

        if (count > maxCount)
        {
            count = maxCount;
            Debug.Log($"动态调整生成数量为: {count}");
        }

        if (count <= 0) yield break;

        // 从池中获取生成数据列表
        var spawnDataList = GetSpawnDataList();

        // 预计算所有生成数据
        int[] elementalCounts = new int[3]; // 冰, 暗, 火

        for (int i = 0; i < count; i++)
        {
            var data = new MonsterSpawnData();
            float roll = UnityEngine.Random.value;

            if (roll < elementalChance)
            {
                data.elementalType = UnityEngine.Random.Range(0, 3);
                data.mobKey = elementalMobs[data.elementalType];
                elementalCounts[data.elementalType]++;
            }
            else if (roll < elementalChance + midMobChance)
            {
                data.mobKey = midTierMobs[UnityEngine.Random.Range(0, midTierMobs.Length)];
            }
            else
            {
                data.mobKey = regularMobs[UnityEngine.Random.Range(0, regularMobs.Length)];
            }

            // 计算生成位置
            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            float dist = UnityEngine.Random.value * spawnRadius;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
            data.position = Dew.GetPositionOnGround(basePos + offset);
            data.position = Dew.GetValidAgentDestination_LinearSweep(basePos, data.position);
            data.rotation = Quaternion.LookRotation((basePos - data.position).Flattened());
            data.hasMirage = UnityEngine.Random.value < mirageSkinChance;

            spawnDataList.Add(data);
        }

        // 如果有元素生物生成则排队消息
        if (elementalCounts[0] + elementalCounts[1] + elementalCounts[2] > 0)
        {
            var sb = MessageUtil.GetStringBuilder();
            sb.Append("<color=#FFB6C1>元素生物来袭: ");

            bool first = true;
            if (elementalCounts[0] > 0)
            {
                sb.Append($"{elementalCounts[0]}只冰霜");
                first = false;
            }

            if (elementalCounts[1] > 0)
            {
                if (!first) sb.Append(", ");
                sb.Append($"{elementalCounts[1]}只暗影");
                first = false;
            }

            if (elementalCounts[2] > 0)
            {
                if (!first) sb.Append(", ");
                sb.Append($"{elementalCounts[2]}只烈焰");
            }

            sb.Append("元素出现!</color>");
            MessageUtil.SendChatMessageOptimized(sb.ToString());
            MessageUtil.ReturnStringBuilder(sb);
        }

        // 分批生成怪物
        int spawned = 0;
        foreach (var data in spawnDataList)
        {
            // 再次检查区域上限（防止生成过程中上限变化）
            if (!CanSpawnStoneMonster())
            {
                MessageUtil.SendChatMessageOptimized("<color=#FF6B6B>区域怪物已达上限，停止生成!</color>");
                break;
            }

            SpawnMonsterOptimized(data, gold);
            spawned++;

            // 批次后让步以防止帧率下降
            if (spawned >= maxMonstersPerFrame)
            {
                spawned = 0;
                yield return spawnBatchWait;
            }
        }

        // 将列表返回到池中
        ReturnSpawnDataList(spawnDataList);
    }

    private static void IncrementStoneMonsterCount()
    {
        var _zoneManager = NetworkedManagerBase<ZoneManager>.instance;
        int currentZone = _zoneManager.currentZoneIndex;
        lock (_zoneCountLock)
        {
            _stoneMonsterCountByZone[currentZone] = _stoneMonsterCountByZone.TryGetValue(currentZone, out int count)
                ? count + 1
                : 1;
        }
    }

    // 添加怪物跟踪组件
    private static void AddMonsterTracker(GameObject monster, int zoneIndex, PropEnt_Stone_Gold gold)
    {
        if (!monster) return;

        var tracker = monster.AddComponent<StoneMonsterTracker>();
        tracker.stoneRef = new WeakReference<PropEnt_Stone_Gold>(gold);
        tracker.zoneIndex = zoneIndex;
    }

    private class StoneMonsterTracker : MonoBehaviour
    {
        public WeakReference<PropEnt_Stone_Gold> stoneRef;
        public int zoneIndex;

        private void OnDestroy()
        {
            if (!NetworkServer.active) return;

            if (stoneRef != null && stoneRef.TryGetTarget(out var stone))
            {
                DecrementStoneMonsterCount(zoneIndex);
            }
        }
    }

    private static void DecrementStoneMonsterCount(int zoneIndex)
    {
        lock (_zoneCountLock)
        {
            if (_stoneMonsterCountByZone.TryGetValue(zoneIndex, out int count) && count > 0)
            {
                _stoneMonsterCountByZone[zoneIndex] = count - 1;
            }
        }
    }

    // 优化怪物生成
    private static void SpawnMonsterOptimized(MonsterSpawnData data, PropEnt_Stone_Gold gold)
    {
        var _actorManager = NetworkedManagerBase<ActorManager>.instance;
        var _gameManager = NetworkedManagerBase<GameManager>.instance;
        var _zoneManager = NetworkedManagerBase<ZoneManager>.instance;
        var _room = SingletonDewNetworkBehaviour<Room>.instance;

        // 从缓存中获取预制体
        Monster prefab = GetMonsterPrefab(data.mobKey);
        if (prefab == null) return;

        var monster = Dew.SpawnEntity(
            prefab,
            data.position,
            data.rotation,
            _actorManager.serverActor,
            DewPlayer.creep,
            _gameManager.ambientLevel,
            (Entity e) =>
            {
                // 跳过生成动画以提高性能
                e.Visual.NetworkskipSpawning = true;

                // 高效应用房间回调
                _room?.monsters?.onBeforeSpawn?.Invoke(e);
            }
        ) as Monster;

        if (monster != null)
        {
            // 生成后应用效果
            if (data.hasMirage)
            {
                // 确保效果在网络同步
                monster.CreateStatusEffect<Se_MirageSkin_Delusion>(monster, new CastInfo(monster));
            }

            _room?.monsters?.onAfterSpawn?.Invoke(monster);

            // 增加区域怪物计数
            IncrementStoneMonsterCount();
            int currentZone = _zoneManager.currentZoneIndex;

            // 添加跟踪组件确保异常销毁时减少计数
            AddMonsterTracker(monster.gameObject, currentZone, gold);
        }
    }

    private static Monster GetMonsterPrefab(string key)
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

    private static bool CanSpawnStoneMonster()
    {
        var _zoneManager = NetworkedManagerBase<ZoneManager>.instance;

        int currentZone = _zoneManager.currentZoneIndex;

        lock (_zoneCountLock)
        {
            // 获取当前区域计数
            if (!_stoneMonsterCountByZone.TryGetValue(currentZone, out int count))
            {
                count = 0;
                _stoneMonsterCountByZone[currentZone] = count;
            }

            return count < maxStoneMonstersPerZone;
        }
    }

    private static List<MonsterSpawnData> GetSpawnDataList()
    {
        if (spawnDataListPool.Count > 0)
        {
            return spawnDataListPool.Pop();
        }

        return new List<MonsterSpawnData>(10);
    }

    private static void ReturnSpawnDataList(List<MonsterSpawnData> list)
    {
        if (list != null)
        {
            list.Clear();
            if (spawnDataListPool.Count < 2)
            {
                spawnDataListPool.Push(list);
            }
        }
    }

// 优化技能奖励处理
    public static void ProcessSkillRewardOptimized(PropEnt_Stone_Gold gold)
    {
        var pos = gold.transform.position;

        var _lootManager = NetworkedManagerBase<LootManager>.instance;
        var _room = SingletonDewNetworkBehaviour<Room>.instance;
        var rewards = _room.rewards;
        var loot = _lootManager.GetLootInstance<Loot_Skill>();

        foreach (var player in DewPlayer.humanPlayers)
        {
            if (player.hero != null && !player.hero.IsNullInactiveDeadOrKnockedOut())
            {
                Rarity rarity = rewards.giveHighRarityReward ? loot.SelectRarityHigh() : loot.SelectRarityNormal();

                loot.SelectSkillAndLevel(rarity, out var skill, out var level);
                if (skill != null)
                {
                    level += rewards.skillBonusLevel;
                    Dew.CreateSkillTrigger(skill, pos, level, player);

                    // 只广播史诗级以上物品
                    if (rarity >= Rarity.Epic)
                    {
                        MessageUtil.BroadcastLootMessage(player.playerName, skill.name, rarity, level, true);
                    }
                }
            }
        }
    }


    // 优化宝石奖励处理
    public static void ProcessGemRewardOptimized(PropEnt_Stone_Gold gold)
    {
        var _room = SingletonDewNetworkBehaviour<Room>.instance;
        var _lootManager = NetworkedManagerBase<LootManager>.instance;

        var pos = gold.transform.position;
        var rewards = _room.rewards;
        var loot = _lootManager.GetLootInstance<Loot_Gem>();

        foreach (var player in DewPlayer.humanPlayers)
        {
            if (player.hero != null && !player.hero.IsNullInactiveDeadOrKnockedOut())
            {
                Rarity rarity = rewards.giveHighRarityReward ? loot.SelectRarityHigh() : loot.SelectRarityNormal();

                loot.SelectGemAndQuality(rarity, out var gem, out var quality);
                if (gem != null)
                {
                    quality += rewards.gemBonusQuality;
                    Dew.CreateGem(gem, pos, quality, player);

                    // 只广播史诗级以上物品
                    if (rarity >= Rarity.Epic)
                    {
                        MessageUtil.BroadcastLootMessage(player.playerName, gem.name, rarity, quality, false);
                    }
                }
            }
        }
    }

    // 处理混沌奖励
    public static void ProcessChaosReward(PropEnt_Stone_Gold gold)
    {
        var pos = gold.transform.position;
        float v = UnityEngine.Random.value;
        Rarity rarity = v > 0.9f ? Rarity.Legendary :
            v > 0.7f ? Rarity.Epic :
            v > 0.4f ? Rarity.Rare :
            Rarity.Common;

        gold.CreateActor(pos, Quaternion.identity, (Shrine_Chaos c) => c.rarity = rarity);
        MessageUtil.SendChatMessageOptimized("哇！发现了一颗<color=#8B0000>混沌宝珠</color>!");
    }

    // 优化宝石槽奖励处理
    public static void ProcessGemslotRewardOptimized(PropEnt_Stone_Gold gold)
    {
        var config = AttrCustomizeResources.Config;
        int[] slots =
        {
            config.skillQGemCount, config.skillWGemCount, config.skillEGemCount,
            config.skillRGemCount, config.skillIdentityGemCount
        };

        // 高效查找可升级的槽位
        List<int> upgradeableIndices = new List<int>(5);
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] < 4)
            {
                upgradeableIndices.Add(i);
            }
        }

        if (upgradeableIndices.Count > 0)
        {
            int upgradeIndex = upgradeableIndices[UnityEngine.Random.Range(0, upgradeableIndices.Count)];
            slots[upgradeIndex]++;

            // 更新配置
            config.skillQGemCount = slots[0];
            config.skillWGemCount = slots[1];
            config.skillEGemCount = slots[2];
            config.skillRGemCount = slots[3];
            config.skillIdentityGemCount = slots[4];

            // 构建消息
            var sb = MessageUtil.GetStringBuilder();
            sb.AppendFormat("Nice!!!挖到了新的<color=#16D7FF>宝石槽</color>! [ Q{0} W{1} E{2} R{3} I{4} ]",
                slots[0], slots[1], slots[2], slots[3], slots[4]);
            MessageUtil.SendChatMessageOptimized(sb.ToString());
            MessageUtil.ReturnStringBuilder(sb);

            // 与客户端同步

            GameManagerUtil.SendSynchronizeClient();
        }
    }

    public static IEnumerator SpawnBossFullyOptimized(PropEnt_Stone_Gold gold)
    {
        var pos = gold.transform.position;
        var _zoneManager = NetworkedManagerBase<ZoneManager>.instance;
        var _actorManager = NetworkedManagerBase<ActorManager>.instance;
        var _gameManager = NetworkedManagerBase<GameManager>.instance;

        // 检查区域怪物上限
        if (!CanSpawnStoneMonster())
        {
            MessageUtil.SendChatMessageOptimized("<color=#FF6B6B>区域怪物已达上限，无法生成首领!</color>");
            yield break;
        }
        
        yield return new WaitForSeconds(0.1f); // 短暂延迟
        
        string bossKey = bossList[UnityEngine.Random.Range(0, bossList.Length)];
        Monster bossPrefab = GetMonsterPrefab(bossKey);
        
        if (bossPrefab != null)
        {
            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 spawnPos = Dew.GetPositionOnGround(pos + offset);
            
            var boss = Dew.SpawnEntity(
                bossPrefab,
                spawnPos,
                Quaternion.LookRotation((pos - spawnPos).Flattened()),
                _actorManager.serverActor,
                DewPlayer.creep,
                _gameManager.ambientLevel,
                null
            ) as Monster;
            
            if (boss != null)
            {
                // 增加区域怪物计数
                IncrementStoneMonsterCount();
                int currentZone = _zoneManager.currentZoneIndex;
                
                // 添加跟踪组件确保异常销毁时减少计数
                AddMonsterTracker(boss.gameObject, currentZone,  gold);
            }
        }
        
        MessageUtil.SendChatMessageOptimized("<color=#FF4F4F>Boss！Boss！Boss!</color>被挖了出来!");
    }
    
}