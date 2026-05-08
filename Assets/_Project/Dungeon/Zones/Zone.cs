using System;
using System.Collections.Generic;
using DungeonBlade.Enemies;
using UnityEngine;

namespace DungeonBlade.Dungeon
{
    [RequireComponent(typeof(Collider))]
    public class Zone : MonoBehaviour
    {
        [SerializeField] string zoneName = "Zone";
        [SerializeField] string zoneId = "zone_1";
        [Tooltip("Enemies belonging to this zone. Drag every EnemyBase that lives inside the zone here. The zone is cleared when all of them die.")]
        [SerializeField] List<EnemyBase> enemies = new();

        public string ZoneName => zoneName;
        public string ZoneId => zoneId;
        public bool HasBeenEntered { get; private set; }
        public bool IsCleared { get; private set; }
        public int LivingEnemyCount { get; private set; }

        public event Action<Zone> Cleared;
        public event Action<Zone, int> EnemyCountChanged;

        readonly HashSet<EnemyBase> _alive = new();

        void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void Awake()
        {
            // Subscribe to each enemy's death. Empty list = zone is "cleared"
            // immediately (useful for arena zones with no trash mobs).
            foreach (var e in enemies)
            {
                if (e == null) continue;
                _alive.Add(e);
                HookEnemy(e);
            }
            LivingEnemyCount = _alive.Count;
            if (LivingEnemyCount == 0) MarkCleared();
        }

        void OnDestroy()
        {
            foreach (var e in _alive) UnhookEnemy(e);
        }

        void HookEnemy(EnemyBase e)
        {
            // EnemyBase doesn't expose a death event yet — poll in Update.
            // Cheap because we only check enemies in this zone, ~3-5 per zone.
        }

        void UnhookEnemy(EnemyBase e) { }

        void Update()
        {
            if (IsCleared) return;
            if (_alive.Count == 0) return;

            // Sweep for newly-dead enemies. EnemyBase.IsAlive becomes false
            // when health hits zero.
            _justDied.Clear();
            foreach (var e in _alive)
            {
                if (e == null || !e.IsAlive) _justDied.Add(e);
            }
            foreach (var e in _justDied)
            {
                _alive.Remove(e);
                LivingEnemyCount = _alive.Count;
                EnemyCountChanged?.Invoke(this, LivingEnemyCount);
                Debug.Log($"[Zone] {zoneName}: enemy down. {LivingEnemyCount} remaining.");
            }
            if (_alive.Count == 0) MarkCleared();
        }

        readonly List<EnemyBase> _justDied = new();

        void MarkCleared()
        {
            if (IsCleared) return;
            IsCleared = true;
            Cleared?.Invoke(this);
            Debug.Log($"[Zone] {zoneName} CLEARED.");
        }

        void OnTriggerEnter(Collider other)
        {
            var manager = ZoneManager.Instance;
            if (manager == null) return;

            if (other.GetComponentInParent<Player.PlayerStats>() == null) return;

            HasBeenEntered = true;
            manager.NotifyZoneEntered(this);
        }

        void OnTriggerExit(Collider other)
        {
            var manager = ZoneManager.Instance;
            if (manager == null) return;
            if (other.GetComponentInParent<Player.PlayerStats>() == null) return;
            manager.NotifyZoneExited(this);
        }
    }
}
