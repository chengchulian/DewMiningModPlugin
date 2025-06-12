using System.Collections;
using DewMiningModPlugin.entry;
using DewMiningModPlugin.executor.impl;
using Mirror;
using UnityEngine;

namespace DewMiningModPlugin.manager;

public static class MonsterSpawnQueueListener
{

    [Tooltip("每帧最多生成多少只怪物")] public static int maxPerFrame = 2;
    [Tooltip("每批之间等待时间（秒）")] public static float spawnBatchDelay = 0.1f;
    [Tooltip("同房间最多怪物数量")] public static int maxPerRoom = 15;
    [Tooltip("生成失败时的重试等待（秒）")] public static float retryWaitTime = 0.5f;
    [Tooltip("单个怪物最大等待时间（秒）")] public static float maxWaitPerMonster = 30f;

    private static WaitForSeconds _spawnBatchWait = new WaitForSeconds(spawnBatchDelay);
    private static WaitForSeconds _spawnRetryWait = new WaitForSeconds((retryWaitTime));
    
    
    public static IEnumerator DequeueLoop()
    {
        while (true)
        {
            int spawnedThisFrame = 0;

            while (StoneMonsterCountManager.TryDequeue(out var data))
            {
                bool success = false;
                float start = Time.time;

                while (Time.time - start < maxWaitPerMonster)
                {
                    if (StoneMonsterCountManager.GetAvailableSpawnCount(maxPerRoom) > 0)
                    {
                        SpawnMonster(data);
                        success = true;
                        break;
                    }

                    yield return _spawnRetryWait;
                }

                if (!success)
                {
                    Debug.LogWarning($"[MonsterQueue] 怪物 [{data.mobKey}] 超时未能生成！");
                }

                spawnedThisFrame++;
                if (spawnedThisFrame >= maxPerFrame)
                {
                    spawnedThisFrame = 0;
                    yield return _spawnBatchWait;
                }
            }

            yield return null; // 一帧只尝试一次出队
        }
    }

    private static void SpawnMonster(MonsterSpawnData data)
    {
        var _actorManager = NetworkedManagerBase<ActorManager>.instance;
        var _gameManager = NetworkedManagerBase<GameManager>.instance;
        var _room = SingletonDewNetworkBehaviour<Room>.instance;

        Monster prefab = MonsterSpawnDataProvider.GetMonsterPrefab(data.mobKey);
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
                e.Visual.NetworkskipSpawning = true;
                _room?.monsters?.onBeforeSpawn?.Invoke(e);
            }
        ) as Monster;

        if (monster != null)
        {
            if (data.hasMirage)
            {
                monster.CreateStatusEffect<Se_MirageSkin_Delusion>(monster, new CastInfo(monster));
            }

            _room?.monsters?.onAfterSpawn?.Invoke(monster);
            StoneMonsterCountManager.Increment();
            AddMonsterTracker(monster.gameObject);
        }
    }

    private static void AddMonsterTracker(GameObject monster)
    {
        if (!monster) return;
        monster.AddComponent<StoneMonsterTracker>();
    }
    internal class StoneMonsterTracker : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (!NetworkServer.active) return;
            StoneMonsterCountManager.Decrement();
        }
    }
}