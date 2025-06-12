using System;
using System.Collections;
using System.Reflection;
using DewCustomizeMod.config;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_MiniBoss : IRewardExecutor
{
    private readonly Color _color = Color.red;

    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        var pos = gold.transform.position;

        var _room = SingletonDewNetworkBehaviour<Room>.instance;

        if (_room?.monsters == null) return null;


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
                afterSpawn = e =>
                {
                    // 生成后恢复原始值
                    AttrCustomizeResources.Config.bossCount = originalBossCount;
                    AttrCustomizeResources.Config.bossCountAddByZone = originalBossCountAddByZone;
                    AttrCustomizeResources.Config.bossCountAddByLoop = originalBossCountAddByLoop;
                }
            };

            _room.monsters.SpawnMiniBoss(settings, null, null);
            MessageUtil.SendChatMessageOptimized($"<color={MiningUtil.ColorToHex(_color)}>一只迷你boss从金矿中出现!</color>");
        }
        catch (Exception ex)
        {
            Debug.LogError($"生成迷你boss错误: {ex.Message}\n{ex.StackTrace}");

            // 确保即使出错也恢复值
            AttrCustomizeResources.Config.bossCount = originalBossCount;
            AttrCustomizeResources.Config.bossCountAddByZone = originalBossCountAddByZone;
            AttrCustomizeResources.Config.bossCountAddByLoop = originalBossCountAddByLoop;
        }

        return null;
    }
}