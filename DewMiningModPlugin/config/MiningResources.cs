using System;
using System.Collections.Generic;
using System.IO;
using DewMiningModPlugin.entry;
using Newtonsoft.Json;
using UnityEngine;

namespace DewMiningModPlugin.config;

public static class MiningResources
{
    private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RewardsConfig.json");

    private static volatile List<RewardConfigEntry> _config;
    private static readonly object Lock = new();

    public static List<RewardConfigEntry> Config
    {
        get
        {
            if (_config != null)
                lock (Lock)
                    return _config;

            lock (Lock)
            {
                if (_config != null) return _config;

                try
                {
                    if (!File.Exists(FilePath))
                    {
                        _config = DefaultConfig;
                        SaveConfig();
                        return _config;
                    }

                    string content = File.ReadAllText(FilePath);
                    _config = JsonConvert.DeserializeObject<List<RewardConfigEntry>>(content);
                    if (_config == null || _config.Count == 0)
                        _config = DefaultConfig;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MiningResources] 加载配置失败: {ex}");
                    _config = DefaultConfig;
                }

                SaveConfig();
                return _config;
            }
        }
    }

    public static void SaveConfig()
    {
        lock (Lock)
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiningResources] 保存配置失败: {ex}");
            }
        }
    }

    public static void ResetConfig()
    {
        lock (Lock)
        {
            _config = DefaultConfig;
            SaveConfig();
        }
    }

    public static void Reload()
    {
        lock (Lock)
        {
            _config = null;
        }
    }

    private static List<RewardConfigEntry> DefaultConfig =>
    [
        new() { type = "Nothing", weight = 0.65f, enabled = true },
        new() { type = "Monster", weight = 0.3f, enabled = true },
        new() { type = "MiniBoss", weight = 0.01f, enabled = true },
        new() { type = "Boss", weight = 0.001f, enabled = true },
        new() { type = "Merchant", weight = 0.001f, enabled = true },
        new() { type = "Concept", weight = 0.003f, enabled = true },
        new() { type = "Memory", weight = 0.003f, enabled = true },
        new() { type = "Enlightenment", weight = 0.002f, enabled = true },
        new() { type = "Retrospection", weight = 0.002f, enabled = true },
        new() { type = "Chaos", weight = 0.003f, enabled = true },
        new() { type = "AltarOfCleansing", weight = 0.001f, enabled = true },
        new() { type = "UpgradeWell", weight = 0.002f, enabled = true },
        new() { type = "Disintegration", weight = 0.003f, enabled = true },
        new() { type = "FragmentOfRadiance", weight = 0.0005f, enabled = true },
        new() { type = "Guidance", weight = 0.003f, enabled = true },
        new() { type = "Hatred", weight = 0.003f, enabled = true },
        new() { type = "LoopCat", weight = 0.0005f, enabled = true },
        new() { type = "MerchantBackpack", weight = 0.003f, enabled = true },
        new() { type = "StarCookie", weight = 0.004f, enabled = true },
        new() { type = "Gemslot", weight = 0.004f, enabled = true }
    ];


}