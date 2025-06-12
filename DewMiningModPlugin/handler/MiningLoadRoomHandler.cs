using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DewMiningModPlugin.handler;

public static class MiningLoadRoomHandler
{
    public static void AddRandomRoomMod(EventInfoLoadRoom obj)
    {
        AddRandomRoomMod();
    }

    public static void SpawnGoldStones(EventInfoLoadRoom obj)
    {
        SpawnGoldStones();
        //移除房间屏障
        RemoveBarrier();
    }


    private static void AddRandomRoomMod()
    {
        List<string> list =
        [
            "RoomMod_ArcticTerritory", "RoomMod_AcceleratedTime", "RoomMod_DistantMemories", "RoomMod_EngulfedInFlame",
            "RoomMod_GoldEverywhere", "RoomMod_LingeringAuraOfGuidance", "RoomMod_PureDream", "RoomMod_StarCookie",
            "RoomMod_Hunted"
        ];
        if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss)
        {
            list.Remove("RoomMod_GravityTraining");
            list.Remove("RoomMod_LeafPuppies");
            list.Remove("RoomMod_StarCookie");
            list.Remove("RoomMod_DistantMemories");
        }

        if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Merchant)
        {
            list.Clear();
        }

        string roommod = list[Mathf.FloorToInt(Random.value * list.Count)];
        if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss &&
            roommod == "RoomMod_LingeringAuraOfGuidance")
        {
            roommod = "";
        }

        if (NetworkedManagerBase<ZoneManager>.instance.loopIndex == 0 && roommod == "RoomMod_Hunted")
        {
            roommod = "";
        }

        if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex % 3 == 0 && roommod == "RoomMod_LeafPuppies")
        {
            roommod = "";
        }

        if (roommod.Length > 5)
        {
            Dew.CallDelayed(delegate
            {
                Debug.Log($"[MiningLoadRoomHandler] Adding modifier: {roommod}");

                NetworkedManagerBase<ZoneManager>.instance.AddModifier(
                    NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, new ModifierData
                    {
                        type = roommod,
                        clientData = null
                    });
            }, 60);
        }
    }

    private static void SpawnGoldStones()
    {
        var _room = SingletonDewNetworkBehaviour<Room>.instance;


        Dew.CallDelayed(delegate
        {
            if (!SingletonDewNetworkBehaviour<Room>.instance.isRevisit &&
                NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.Combat)
            {
                _room.StartCoroutine(SpawnGoldStonesDelayed());
            }
        }, 60);
    }

    private static void RemoveBarrier()
    {
        Dew.CallDelayed(delegate
        {
            Room_Barrier[] array = Object.FindObjectsOfType<Room_Barrier>();
            foreach (var barrier in array)
            {
                NetworkServer.Destroy(barrier.gameObject);
            }
        }, 60);
    }

    private static IEnumerator SpawnGoldStonesDelayed()
    {
        while (!SingletonDewNetworkBehaviour<Room>.instance.isActive)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        SingletonDewNetworkBehaviour<Room>.instance.ClearRoom();
        Room room = SingletonDewNetworkBehaviour<Room>.instance;
        if (room.props == null)
        {
            Debug.LogError("[MiningLoadRoomHandler] Room.props is still null after delay!");
            yield break;
        }

        Actor stonePrefab = DewResources.FindOneByTypeSubstring<Actor>("PropEnt_Stone_Gold");
        if (stonePrefab == null)
        {
            Debug.LogError("[MiningLoadRoomHandler] Could not find PropEnt_Stone_Gold prefab!");
            yield break;
        }

        HashSet<Vector3> exactPositions = new HashSet<Vector3>();
        int actualSpawned = 0;
        int totalAttempts = 0;
        int maxTotalAttempts = 500;
        while (actualSpawned < 50 && totalAttempts < maxTotalAttempts)
        {
            int num = totalAttempts;
            totalAttempts = num + 1;
            Vector3 vector = Vector3.zero;
            bool flag = false;
            if (room.props != null)
            {
                flag = room.props.TryGetGoodNodePosition(out vector);
            }

            if (!flag && room.sections is { Count: > 0 })
            {
                RoomSection roomSection = room.sections[Random.Range(0, room.sections.Count)];
                if (roomSection != null)
                {
                    flag = roomSection.TryGetGoodNodePosition(out vector);
                    if (!flag)
                    {
                        vector = roomSection.GetRandomWorldPosition();
                        flag = true;
                    }
                }
            }

            if (!flag && room.playerPathablePoints is { Count: > 0 })
            {
                Vector3 vector2 = room.playerPathablePoints[Random.Range(0, room.playerPathablePoints.Count)];
                float num2 = Random.Range(0f, 360f) * 0.017453292f;
                float num3 = Random.Range(1f, 10f);
                vector = vector2 + new Vector3(Mathf.Sin(num2) * num3, 0f, Mathf.Cos(num2) * num3);
                flag = true;
            }

            if (!flag)
            {
                Vector3 vector3 = room.transform.position;
                if (room.sections is { Count: > 0 })
                {
                    Vector3 vector4 = Vector3.zero;
                    int num4 = 0;
                    foreach (RoomSection roomSection2 in room.sections)
                    {
                        if (roomSection2 != null)
                        {
                            vector4 += roomSection2.transform.position;
                            num4++;
                        }
                    }

                    if (num4 > 0)
                    {
                        vector3 = vector4 / num4;
                    }
                }

                float num5 = Random.Range(0f, 360f) * 0.017453292f;
                float num6 = Random.Range(2f, 20f);
                vector = vector3 + new Vector3(Mathf.Sin(num5) * num6, 0f, Mathf.Cos(num5) * num6);
            }

            {
                vector = Dew.GetPositionOnGround(vector);
                Vector3 vector5 = new Vector3(Mathf.Round(vector.x * 10f) / 10f, Mathf.Round(vector.y * 10f) / 10f,
                    Mathf.Round(vector.z * 10f) / 10f);
                if (!exactPositions.Contains(vector5))
                {
                    Actor actor = Dew.CreateActor(stonePrefab, vector, Quaternion.identity);
                    if (actor != null)
                    {
                        exactPositions.Add(vector5);
                        num = actualSpawned;
                        actualSpawned = num + 1;
                        actor.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    }

                    if (actualSpawned % 5 == 0)
                    {
                        yield return null;
                    }
                }
            }
        }

        Debug.Log(
            $"[MiningLoadRoomHandler] Successfully spawned {actualSpawned}/50 gold stones (attempts: {totalAttempts})");
        ChatManager instance = NetworkedManagerBase<ChatManager>.instance;
        if (instance != null)
        {
            ChatManager.Message message = new ChatManager.Message
            {
                type = ChatManager.MessageType.Raw,
                content = $"你进入了一个矿洞<color=#FFD700>{actualSpawned}块金矿</color>等待你挖掘!"
            };
            instance.BroadcastChatMessage(message);
        }
    }
}