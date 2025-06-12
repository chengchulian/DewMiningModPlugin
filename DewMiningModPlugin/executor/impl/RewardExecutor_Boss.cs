using System;
using System.Collections;
using System.Collections.Generic;
using DewMiningModPlugin.executor;
using DewMiningModPlugin.manager;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Boss : IRewardExecutor
{

    private readonly Color _color = Color.red;

    
    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        
        // 短暂延迟
        yield return new WaitForSeconds(0.1f);
        
        var pos = gold.transform.position;
        var _actorManager = NetworkedManagerBase<ActorManager>.instance;
        var _gameManager = NetworkedManagerBase<GameManager>.instance;





        string monsterKey = MonsterSpawnDataProvider.BossList[UnityEngine.Random.Range(0, MonsterSpawnDataProvider.BossList.Count)];
        Monster bossPrefab = MonsterSpawnDataProvider.GetMonsterPrefab(monsterKey);

        if (bossPrefab != null)
        {
            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 spawnPos = Dew.GetPositionOnGround(pos + offset);

            Dew.SpawnEntity(
                bossPrefab,
                spawnPos,
                Quaternion.LookRotation((pos - spawnPos).Flattened()),
                _actorManager.serverActor,
                DewPlayer.creep,
                _gameManager.ambientLevel
            );
        }

        MessageUtil.SendChatMessageOptimized($"<color={MiningUtil.ColorToHex(_color)}>Boss！Boss！Boss!</color>被挖了出来!");
    }
}