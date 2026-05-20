using UnityEngine;

namespace DungeonBlade.Bank
{
    /// <summary>
    /// Auto-fires this object's Interactable.OnInteract when the tagged player enters a radius.
    /// Manages its own child trigger collider so it doesn't fight the host's physics collider.
    /// Re-arms only after the player leaves the radius, so closing the panel while standing
    /// next to the merchant won't immediately reopen it.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class ProximityAutoInteract : MonoBehaviour
    {
        [SerializeField] float radius = 3.5f;
        [SerializeField] string playerTag = "Player";
        [Tooltip("Y offset for the trigger sphere relative to this object.")]
        [SerializeField] float yOffset = 1f;
        [Tooltip("If a UI panel is already open (inventory/bank/shop/pause/dialog), don't auto-trigger.")]
        [SerializeField] bool suppressWhenAnyMenuOpen = true;

        Interactable _interactable;
        SphereCollider _triggerCol;
        bool _playerInside;
        bool _armed = true;

        const string TriggerChildName = "_ProximityTrigger";

        void Awake()
        {
            _interactable = GetComponent<Interactable>();
            BuildTrigger();
        }

        void OnValidate()
        {
            if (_triggerCol != null)
            {
                _triggerCol.radius = radius;
                _triggerCol.center = new Vector3(0f, yOffset, 0f);
            }
        }

        void BuildTrigger()
        {
            // Reuse existing child if present (handles domain reload).
            var existing = transform.Find(TriggerChildName);
            GameObject host;
            if (existing != null)
            {
                host = existing.gameObject;
            }
            else
            {
                host = new GameObject(TriggerChildName);
                host.transform.SetParent(transform, false);
                host.transform.localPosition = Vector3.zero;
                host.transform.localRotation = Quaternion.identity;
                host.transform.localScale = Vector3.one;
                host.layer = gameObject.layer;
            }

            _triggerCol = host.GetComponent<SphereCollider>();
            if (_triggerCol == null) _triggerCol = host.AddComponent<SphereCollider>();
            _triggerCol.isTrigger = true;
            _triggerCol.radius = radius;
            _triggerCol.center = new Vector3(0f, yOffset, 0f);

            var relay = host.GetComponent<ProximityTriggerRelay>();
            if (relay == null) relay = host.AddComponent<ProximityTriggerRelay>();
            relay.Owner = this;
        }

        internal void HandleEnter(Collider other)
        {
            if (!IsPlayer(other)) return;
            _playerInside = true;
            TryFire();
        }

        internal void HandleStay(Collider other)
        {
            if (!IsPlayer(other)) return;
            // Stay covers the case where the player was already inside before this object spawned.
            if (!_playerInside) _playerInside = true;
            TryFire();
        }

        internal void HandleExit(Collider other)
        {
            if (!IsPlayer(other)) return;
            _playerInside = false;
            _armed = true;
        }

        void TryFire()
        {
            if (!_armed) return;
            if (suppressWhenAnyMenuOpen && IsAnyMenuOpen()) return;
            _armed = false;
            _interactable.OnInteract(_playerCached != null ? _playerCached : gameObject);
        }

        GameObject _playerCached;
        bool IsPlayer(Collider other)
        {
            if (other == null) return false;
            var go = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            if (!string.IsNullOrEmpty(playerTag) && !go.CompareTag(playerTag)) return false;
            _playerCached = go;
            return true;
        }

        static bool IsAnyMenuOpen()
        {
            if (Inventory.InventoryController.Instance != null && Inventory.InventoryController.Instance.IsOpen) return true;
            if (UI.BankController.Instance != null && UI.BankController.Instance.IsOpen) return true;
            if (UI.ShopController.Instance != null && UI.ShopController.Instance.IsOpen) return true;
            if (PortalConfirmDialog.Instance != null && PortalConfirmDialog.Instance.IsOpen) return true;
            return false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.25f);
            Gizmos.DrawWireSphere(transform.position + new Vector3(0f, yOffset, 0f), radius);
        }
    }

    // Internal relay so the trigger lives on a child without polluting the public component surface.
    [DisallowMultipleComponent]
    internal class ProximityTriggerRelay : MonoBehaviour
    {
        public ProximityAutoInteract Owner;
        void OnTriggerEnter(Collider other) { if (Owner != null) Owner.HandleEnter(other); }
        void OnTriggerStay(Collider other)  { if (Owner != null) Owner.HandleStay(other); }
        void OnTriggerExit(Collider other)  { if (Owner != null) Owner.HandleExit(other); }
    }
}
