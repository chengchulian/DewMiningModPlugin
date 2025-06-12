using UnityEngine;

namespace DewMiningModPlugin.util;

public static class MiningUtil
{
    public static Color GetColorByRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Color.white;
            case Rarity.Rare:
                return Color.cyan;
            case Rarity.Epic:
                return Color.magenta;
            case Rarity.Legendary:
                return Color.red;
            case Rarity.Character:
                return Color.yellow;
            case Rarity.Identity:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    public static string ColorToHex(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static string ColorToHexWithAlpha(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        int a = Mathf.RoundToInt(color.a * 255);
        return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
    }
}