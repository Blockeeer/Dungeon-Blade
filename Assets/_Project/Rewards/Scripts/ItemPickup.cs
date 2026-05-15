using DungeonBlade.Bank;
using DungeonBlade.Inventory;
using UnityEngine;

namespace DungeonBlade.Rewards
{
    [RequireComponent(typeof(Collider))]
    public class ItemPickup : Interactable
    {
        [SerializeField] Item item;
        [SerializeField] int quantity = 1;
        [SerializeField] float bobAmplitude = 0.15f;
        [SerializeField] float bobSpeed = 2f;
        [SerializeField] float spinSpeed = 60f;
        [Tooltip("Optional MeshRenderer (or Image on a child world-space canvas) tinted to match item rarity. Leave empty to skip.")]
        [SerializeField] Renderer rarityTintRenderer;

        Vector3 _basePosition;

        public Item Item => item;
        public int Quantity => quantity;

        // Override the parent's PromptText so each pickup auto-builds a
        // rarity-colored "Press [F] to pick up <item name>" line.
        public override string PromptText
        {
            get
            {
                if (item == null) return base.PromptText;
                string hex = ItemRankColors.Hex(item.Rank);
                return $"Press [F] to pick up <color=#{hex}>{item.DisplayName}</color>"
                     + (quantity > 1 ? $" x{quantity}" : "");
            }
        }

        void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            _basePosition = transform.position;
            ApplyRarityTint();
        }

        public void Initialize(Item it, int qty)
        {
            item = it;
            quantity = Mathf.Max(1, qty);
            _basePosition = transform.position;
            ApplyRarityTint();
        }

        void ApplyRarityTint()
        {
            if (rarityTintRenderer == null || item == null) return;
            // Use a property block so we don't allocate a per-instance material.
            var block = new MaterialPropertyBlock();
            rarityTintRenderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", ItemRankColors.For(item.Rank));
            block.SetColor("_Color", ItemRankColors.For(item.Rank));
            block.SetColor("_EmissionColor", ItemRankColors.For(item.Rank) * 0.6f);
            rarityTintRenderer.SetPropertyBlock(block);
        }

        void Update()
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = _basePosition + Vector3.up * bob;
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        public override void OnInteract(GameObject player)
        {
            if (item == null || InventoryManager.Instance == null) return;
            int leftover = InventoryManager.Instance.AddItem(item, quantity);
            if (leftover >= quantity)
            {
                Debug.Log($"[Pickup] Inventory full — could not pick up {item.DisplayName}.");
                return;
            }

            int picked = quantity - leftover;
            Debug.Log($"[Pickup] Picked up {picked}× {item.DisplayName}.");
            quantity = leftover;
            if (quantity <= 0) Destroy(gameObject);
        }
    }
}
