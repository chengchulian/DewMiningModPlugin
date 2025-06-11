using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DewCustomizeMod.config;
using DewMiningModPlugin.entry;
using DewMiningModPlugin.util;
using HarmonyLib;
using UnityEngine;

namespace DewMiningModPlugin.handler;

public static class StoneGoldOnDeathHandler
{
    
    private static float totalWeightWithGemslot;
    private static float totalWeightWithoutGemslot;


    // 预计算的奖励权重用于O(1)选择
    private static readonly RewardEntry[] REWARDS =
    [
        new("nothing", 1.0f / 2.0f),
        new("monster", 1.0f / 5.0f),
        new("skill", 1.0f / 240.0f),
        new("gem", 1.0f / 240.0f),
        new("merchant", 1.0f / 1280.0f),
        new("well", 1.0f / 320.0f),
        new("cleansing", 1.0f / 640.0f),
        new("chaos", 1.0f / 500.0f),
        new("gemslot", 1.0f / 240.0f),
        new("boss", 1.0f / 1280.0f)
    ];


    // 奖励延迟时间
    private static readonly WaitForSeconds rewardDelayWait = new WaitForSeconds(0.8f);


    [Tooltip("生成迷你boss的概率(0-1)。")] private static float miniBossChance = 0.01f;

    public static void ProcessDeathRewards(PropEnt_Stone_Gold gold, EventInfoKill info)
    {
        var _room = SingletonDewNetworkBehaviour<Room>.instance;

        // 延迟选择和处理奖励
        string selectedReward = SelectRewardOptimized();
        if (!string.IsNullOrEmpty(selectedReward))
        {
            _room.StartCoroutine(ProcessRewardDelayed(selectedReward, gold));
        }
    }


    private static string SelectRewardOptimized()
    {
        float total = Constant.ENABLE_GEMSLOT_REWARD ? totalWeightWithGemslot : totalWeightWithoutGemslot;
        float roll = UnityEngine.Random.value * total;
        float accumulator = 0f;
        
        int endIndex = Constant.ENABLE_GEMSLOT_REWARD ? REWARDS.Length : REWARDS.Length - 1;
        
        for (int i = 0; i < endIndex; i++)
        {
            accumulator += REWARDS[i].weight;
            if (roll <= accumulator)
            {
                return REWARDS[i].key;
            }
        }
        
        return "nothing";
    }

    static StoneGoldOnDeathHandler()
    {
        // 预计算总权重
        totalWeightWithGemslot = 0f;
        totalWeightWithoutGemslot = 0f;
        
        for (int i = 0; i < REWARDS.Length; i++)
        {
            totalWeightWithGemslot += REWARDS[i].weight;
            if (REWARDS[i].key != "gemslot")
            {
                totalWeightWithoutGemslot += REWARDS[i].weight;
            }
        }
    }
    
    private static IEnumerator ProcessRewardDelayed(string reward, PropEnt_Stone_Gold gold)
    {
        yield return rewardDelayWait;
        ProcessReward(reward, gold);
    }

    private static void ProcessReward(string reward, PropEnt_Stone_Gold gold)
    {
        var _room = SingletonDewNetworkBehaviour<Room>.instance;
        var pos = gold.transform.position;

        switch (reward)
        {
            case "nothing":
                break;

            case "monster":
                if (UnityEngine.Random.value < miniBossChance)
                {
                    _room.StartCoroutine(MiningRewardUtil.SpawnMiniBossCoroutine(gold)) ;
                }
                else
                {
                    _room.StartCoroutine(MiningRewardUtil.SpawnMonstersBatched(gold));
                }

                break;

            case "skill":
                MiningRewardUtil.ProcessSkillRewardOptimized(gold);
                break;

            case "gem":
                MiningRewardUtil.ProcessGemRewardOptimized(gold);
                break;

            case "merchant":
                gold.CreateActor<PropEnt_Merchant_Jonas>(pos, Quaternion.identity);
                MessageUtil.SendChatMessageOptimized("挖到了一位流浪<color=#DAA520>商人</color>！");
                break;

            case "well":
                gold.CreateActor<Shrine_UpgradeWell>(pos, Quaternion.identity);
                MessageUtil.SendChatMessageOptimized("挖到了一口<color=#00BFFF>许愿井</color>！");
                break;

            case "cleansing":
                gold.CreateActor<Shrine_AltarOfCleansing>(pos, Quaternion.identity);
                MessageUtil.SendChatMessageOptimized("挖到了一座<color=#98FB98>净化祭坛</color>!");
                break;

            case "chaos":
                MiningRewardUtil.ProcessChaosReward(gold);
                break;

            case "gemslot":
                MiningRewardUtil.ProcessGemslotRewardOptimized(gold);
                break;

            case "boss":
                _room.StartCoroutine(MiningRewardUtil.SpawnBossFullyOptimized(gold));
                break;
        }
    }
}