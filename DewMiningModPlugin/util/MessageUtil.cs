using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DewMiningModPlugin.util;

public class MessageUtil
{
    
    // 聊天消息批处理
    private static readonly List<string> pendingMessages = new(5);
    private static Coroutine messageFlushCoroutine;
    private static readonly object chatLock = new();
    
    private static readonly Stack<StringBuilder> stringBuilderPool = new(4);
    
    // 稀有度颜色查找表
    private static readonly Dictionary<Rarity, string> RarityColors = new Dictionary<Rarity, string>
    {
        { Rarity.Common, "#FFFFFF" },
        { Rarity.Rare, "#00FFFF" },
        { Rarity.Epic, "#FF33FF" },
        { Rarity.Legendary, "#FFA500" }
    };
    public static void SendChatMessageOptimized(string content)
    {
        ChatManager _chatManager = SingletonDewNetworkBehaviour<ChatManager>.instance;
        lock (chatLock)
        {
            pendingMessages.Add(content);
            
            if (messageFlushCoroutine == null && _chatManager != null)
            {
                messageFlushCoroutine = _chatManager.StartCoroutine(FlushChatMessages());
            }
        }
    }
    
      
    // 刷新聊天消息
    private static IEnumerator FlushChatMessages()
    {
        ChatManager _chatManager = SingletonDewNetworkBehaviour<ChatManager>.instance;
        
        yield return new WaitForSeconds(0.2f); // 批量延迟
        
        lock (chatLock)
        {
            if (pendingMessages.Count == 0)
            {
                messageFlushCoroutine = null;
                yield break;
            }
            
            if (_chatManager == null) 
            {
                pendingMessages.Clear();
                messageFlushCoroutine = null;
                yield break;
            }
            
            if (pendingMessages.Count == 1)
            {
                _chatManager.BroadcastChatMessage(new ChatManager.Message
                {
                    type = ChatManager.MessageType.Raw,
                    content = pendingMessages[0]
                });
            }
            else
            {
                var sb = GetStringBuilder();
                sb.AppendLine("<color=#AAAAFF>多重发现:</color>");
                
                for (int i = 0; i < pendingMessages.Count; i++)
                {
                    sb.AppendLine($"• {pendingMessages[i]}");
                }
                
                _chatManager.BroadcastChatMessage(new ChatManager.Message
                {
                    type = ChatManager.MessageType.Raw,
                    content = sb.ToString()
                });
                
                ReturnStringBuilder(sb);
            }
            
            pendingMessages.Clear();
            messageFlushCoroutine = null;
        }
    }
    
    public static void ReturnStringBuilder(StringBuilder sb)
    {
        if (sb != null)
        {
            sb.Clear();
            if (stringBuilderPool.Count < 4)
            {
                stringBuilderPool.Push(sb);
            }
        }
    }

    // 获取StringBuilder
    public static StringBuilder GetStringBuilder()
    {
        if (stringBuilderPool.Count > 0)
        {
            var sb = stringBuilderPool.Pop();
            sb.Clear();
            return sb;
        }
        return new StringBuilder(256);
    }
    public static void BroadcastLootMessage(string playerName, string itemName, Rarity rarity, int value, bool isSkill)
    {
        var sb = GetStringBuilder();
        string cleanName = itemName.Contains("_") ? itemName.Substring(itemName.LastIndexOf('_') + 1) : itemName;
        
        sb.AppendFormat("<b><color=#62BCE2>{0}</color></b>: 挖到了 <color={1}>{2} {3}: {4} ({5}{6})</color>",
            playerName, RarityColors[rarity], rarity, isSkill ? "技能" : "宝石", 
            cleanName, isSkill ? "等级 " : "", value, isSkill ? "" : "%");
        
        SendChatMessageOptimized(sb.ToString());
        ReturnStringBuilder(sb);
    }


}