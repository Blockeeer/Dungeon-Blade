using UnityEngine;

namespace DungeonBlade.Dungeon
{
    // A wall/door that blocks the player until a specific zone is cleared.
    // Drop one between two zones, wire the "required" zone, and the gate
    // disables itself the moment that zone's enemies are all dead.
    public class ZoneGate : MonoBehaviour
    {
        [SerializeField] Zone requiredZone;
        [Tooltip("GameObject(s) that physically block the player (walls, doors, force-field meshes). Disabled when the gate opens.")]
        [SerializeField] GameObject[] blockers;
        [Tooltip("Optional GameObject to enable when the gate opens (e.g. a particle effect or open-door visual).")]
        [SerializeField] GameObject openVisual;
        [Tooltip("Delay before opening the gate after the zone clears. Lets death VFX/SFX play first.")]
        [SerializeField] float openDelay = 0.5f;

        bool _opened;

        void Awake()
        {
            if (openVisual != null) openVisual.SetActive(false);
            SetBlockers(true);
        }

        void OnEnable()
        {
            if (requiredZone == null)
            {
                Debug.LogWarning($"[ZoneGate] {name}: no Required Zone wired. Gate stays closed.");
                return;
            }

            // If the zone was already cleared by the time we enable (e.g. no
            // enemies in it), open immediately.
            if (requiredZone.IsCleared)
            {
                Open();
                return;
            }

            requiredZone.Cleared += OnZoneCleared;
        }

        void OnDisable()
        {
            if (requiredZone != null) requiredZone.Cleared -= OnZoneCleared;
        }

        void OnZoneCleared(Zone z)
        {
            Invoke(nameof(Open), openDelay);
        }

        void Open()
        {
            if (_opened) return;
            _opened = true;
            SetBlockers(false);
            if (openVisual != null) openVisual.SetActive(true);
            Debug.Log($"[ZoneGate] {name} opened — {requiredZone.ZoneName} cleared.");
        }

        void SetBlockers(bool active)
        {
            if (blockers == null) return;
            foreach (var b in blockers)
            {
                if (b != null) b.SetActive(active);
            }
        }
    }
}
