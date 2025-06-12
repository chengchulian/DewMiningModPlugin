using System.Collections;
using System.Collections.Generic;
using DewCustomizeMod.config;
using DewCustomizeMod.util;
using DewMiningModPlugin.util;
using UnityEngine;

namespace DewMiningModPlugin.executor.impl;

public class RewardExecutor_Gemslot : IRewardExecutor
{
    private readonly Color _color = Color.red;

    public IEnumerator ProcessRewardAsync(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        yield return new WaitForSeconds(0.1f);

        var config = AttrCustomizeResources.Config;
        int[] slots =
        [
            config.skillQGemCount, config.skillWGemCount, config.skillEGemCount,
            config.skillRGemCount, config.skillIdentityGemCount
        ];

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
            int upgradeIndex = upgradeableIndices[Random.Range(0, upgradeableIndices.Count)];
            slots[upgradeIndex]++;

            // 更新配置
            config.skillQGemCount = slots[0];
            config.skillWGemCount = slots[1];
            config.skillEGemCount = slots[2];
            config.skillRGemCount = slots[3];
            config.skillIdentityGemCount = slots[4];

            // 构建消息
            var sb = MessageUtil.GetStringBuilder();
            sb.Append(
                $"Nice!!!挖到了新的<color={MiningUtil.ColorToHex(_color)}>宝石槽</color>! [ Q{slots[0]} W{slots[1]} E{slots[2]} R{slots[3]} I{slots[4]} ]");
            MessageUtil.SendChatMessageOptimized(sb.ToString());
            MessageUtil.ReturnStringBuilder(sb);

            // 与客户端同步

            GameManagerUtil.SendSynchronizeClient();
        }
    }
}