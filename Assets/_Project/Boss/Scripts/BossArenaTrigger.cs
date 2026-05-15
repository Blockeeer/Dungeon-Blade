using DungeonBlade.Core.Audio;
using DungeonBlade.Dungeon;
using UnityEngine;

namespace DungeonBlade.Boss
{
    [RequireComponent(typeof(Collider))]
    public class BossArenaTrigger : MonoBehaviour
    {
        [SerializeField] BossBase boss;
        [SerializeField] GameObject arenaSealWall;
        [SerializeField] string playerTag = "Player";
        [SerializeField] float sealDelay = 2.5f;
        [Tooltip("Optional. If wired, the player respawns here after dying inside the arena instead of going back to the previous checkpoint.")]
        [SerializeField] Checkpoint arenaCheckpoint;
        [Tooltip("Optional. If wired, the trigger listens for player respawn events to auto-reset the arena (drop seal wall + reset boss).")]
        [SerializeField] RespawnManager respawnManager;
        [Tooltip("Optional. If wired, the boss-arena music swaps in when the seal triggers and back out when the boss dies or the arena resets.")]
        [SerializeField] MusicTrigger musicTrigger;

        bool _triggered;
        bool _bossDefeated;
        float _sealAtTime = -1f;

        void Awake()
        {
            if (arenaSealWall != null) arenaSealWall.SetActive(false);
            if (boss != null) boss.OnBossDefeated += OnBossDefeated;
            if (respawnManager != null) respawnManager.OnPlayerRespawned += OnPlayerRespawned;
        }

        void Update()
        {
            if (_sealAtTime > 0f && Time.time >= _sealAtTime)
            {
                _sealAtTime = -1f;
                if (arenaSealWall != null) arenaSealWall.SetActive(true);
                Debug.Log("[Boss] Arena sealed — fight begins.");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (!other.CompareTag(playerTag)) return;

            _triggered = true;
            Debug.Log($"[Boss] Player entered arena — sealing in {sealDelay:F1}s.");

            _sealAtTime = Time.time + sealDelay;
            if (boss != null) boss.ActivateBoss();
            // SwitchToArena is idempotent — if boss music is already playing
            // (player retrying after death), MusicPlayer.Play() skips replay
            // when the same clip is already active.
            if (musicTrigger != null) musicTrigger.SwitchToArena();
        }

        void OnBossDefeated()
        {
            _bossDefeated = true;
            Debug.Log("[Boss] Defeated — arena unsealed.");
            if (arenaSealWall != null) arenaSealWall.SetActive(false);
            if (musicTrigger != null) musicTrigger.RestoreMain();
        }

        void OnPlayerRespawned()
        {
            // Boss already dead → arena stays open, nothing to reset.
            if (_bossDefeated) return;

            // If the player was respawned to the arena checkpoint, the fight
            // is still on. Reset boss state so it's a clean retry.
            bool respawnedInsideArena = arenaCheckpoint != null
                && respawnManager != null
                && respawnManager.Current == arenaCheckpoint;

            if (respawnedInsideArena)
            {
                if (arenaSealWall != null) arenaSealWall.SetActive(true);
                if (boss != null)
                {
                    boss.ResetForRetry();
                    boss.ActivateBoss();
                }
                _sealAtTime = -1f;
                Debug.Log("[Boss] Player respawned inside arena — boss reset, fight resumes.");
            }
            else
            {
                // Player respawned at a checkpoint outside the arena. Drop
                // the seal so they can re-enter through the trigger.
                // Boss music keeps playing during the walk-back (Option Y) —
                // only restored on actual victory in OnBossDefeated.
                _triggered = false;
                _sealAtTime = -1f;
                if (arenaSealWall != null) arenaSealWall.SetActive(false);
                if (boss != null) boss.ResetForRetry();
                Debug.Log("[Boss] Player respawned outside arena — arena reset, awaiting re-entry.");
            }
        }

        void OnDestroy()
        {
            if (boss != null) boss.OnBossDefeated -= OnBossDefeated;
            if (respawnManager != null) respawnManager.OnPlayerRespawned -= OnPlayerRespawned;
        }

        void OnDrawGizmosSelected()
        {
            var col = GetComponent<Collider>();
            if (col == null) return;
            Gizmos.color = new Color(1f, 0f, 0.5f, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider box) Gizmos.DrawCube(box.center, box.size);
        }
    }
}
