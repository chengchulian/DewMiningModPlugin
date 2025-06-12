using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DewMiningModPlugin.entry;
using DewMiningModPlugin.manager;
using DewMiningModPlugin.util;
using Mirror;
using Sirenix.Utilities;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl
{
    public class RewardExecutor_Monster : IRewardExecutor
    {
        [Tooltip("怪物奖励触发时生成的最小怪物数量。")] private int minMonsterSpawnCount = 2;

        [Tooltip("怪物奖励触发时生成的最大怪物数量。")] private int maxMonsterSpawnCount = 5;

        [Tooltip("每个房间允许的黄金石怪物最大数量")] private int maxStoneMonstersPerRoom = 15;

        [Tooltip("每个生成的怪物成为元素生物的概率(0-1)。")] private float elementalChance = 0.035f;

        [Tooltip("普通生成变为中级怪物的概率(0-1)。")] private float midMobChance = 0.2f;

        [Tooltip("石头周围生成怪物的散射半径。")] private float spawnRadius = 2f;

        [Tooltip("每个生成的怪物获得幻影皮肤效果的概率(0-1)。")] private float mirageSkinChance = 0.05f;


        public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
        {
            var basePos = gold.transform.position;

            int count = UnityEngine.Random.Range(minMonsterSpawnCount, maxMonsterSpawnCount + 1);
            int[] elementalCounts = new int[3];

            for (int i = 0; i < count; i++)
            {
                var data = MonsterSpawnDataProvider.ChooseRandomMonsterData(
                    basePos,
                    spawnRadius,
                    elementalChance,
                    midMobChance,
                    mirageSkinChance,
                    elementalCounts
                );

                StoneMonsterCountManager.Enqueue(data);
            }

            if (elementalCounts.Sum() > 0)
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

            yield break;
        }


    }
}