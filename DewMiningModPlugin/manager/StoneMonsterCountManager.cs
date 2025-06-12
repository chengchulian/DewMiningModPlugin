using System.Collections.Generic;
using DewMiningModPlugin.entry;

namespace DewMiningModPlugin.manager
{
    public static class StoneMonsterCountManager
    {
        public static int MonsterCount;
        private static readonly object _lock = new();

        private static readonly Queue<MonsterSpawnData> SpawnQueue = new(64);

        public static int GetAvailableSpawnCount(int maxPerRoom)
        {
            lock (_lock)
            {
                return maxPerRoom - MonsterCount;
            }
        }

        public static void Increment()
        {
            lock (_lock)
            {
                MonsterCount++;
            }
        }

        public static void Decrement()
        {
            lock (_lock)
            {
                MonsterCount--;
            }
        }

        public static void ResetRoomMonsterCount()
        {
            lock (_lock)
            {
                MonsterCount = 0;
                SpawnQueue.Clear();
            }
        }

        public static void Enqueue(MonsterSpawnData data)
        {
            lock (_lock)
            {
                SpawnQueue.Enqueue(data);
            }
        }

        public static bool TryDequeue(out MonsterSpawnData data)
        {
            lock (_lock)
            {
                if (SpawnQueue.Count > 0)
                {
                    data = SpawnQueue.Dequeue();
                    return true;
                }

                data = default;
                return false;
            }
        }

        public static void ClearSpawnQueue()
        {
            lock (_lock)
            {
                SpawnQueue.Clear();
            }
        }

        public static int GetSpawnQueueCount()
        {
            lock (_lock)
            {
                return SpawnQueue.Count;
            }
        }

        public static void ResetRoomMonsterCount(EventInfoLoadRoom obj)
        {
            ResetRoomMonsterCount();
        }
    }
}