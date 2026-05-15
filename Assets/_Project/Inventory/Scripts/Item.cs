using UnityEngine;

namespace DungeonBlade.Inventory
{
    public enum ItemType { Misc, Weapon, Consumable, Material, KeyItem }

    // GDD §7.2 specifies 9 equipment slots (Head/Chest/Legs/Boots/MainHand/
    // OffHand/Ring×2/Amulet). Phase 1 ships with 4 active slots; Legs/Boots/
    // Ring1/Ring2/Amulet are reserved enum values so future armor items can
    // be authored without enum-shift breakage of existing weapon assets
    // (which serialize equipSlot as int values 1-4).
    public enum EquipmentSlot
    {
        None = 0,
        Head = 1,
        Chest = 2,    // was "Body" pre-Tier-3 — renamed for GDD §7.2 compliance.
        MainHand = 3,
        OffHand = 4,
        Legs = 5,     // reserved — no UI slot yet
        Boots = 6,    // reserved
        Ring1 = 7,    // reserved
        Ring2 = 8,    // reserved
        Amulet = 9,   // reserved
    }

    // Per Dungeon Blade GDD §6.1 — 5 rarity tiers required for Boss Reward
    // Table (§6.2) loot rolls. Values are in ascending power order so
    // (int)rank comparisons and CompareTo work for sorts.
    public enum ItemRank
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
    }

    public static class ItemRankColors
    {
        // GDD §6.1 specifies these colors. Edit here to retune the whole game.
        static readonly Color CommonColor    = new Color(0.85f, 0.85f, 0.85f, 1f); // White / Gray
        static readonly Color UncommonColor  = new Color(0.30f, 0.85f, 0.30f, 1f); // Green
        static readonly Color RareColor      = new Color(0.30f, 0.55f, 1.00f, 1f); // Blue
        static readonly Color EpicColor      = new Color(0.70f, 0.35f, 1.00f, 1f); // Purple
        static readonly Color LegendaryColor = new Color(1.00f, 0.55f, 0.10f, 1f); // Orange / Gold

        public static Color For(ItemRank rank) => rank switch
        {
            ItemRank.Common    => CommonColor,
            ItemRank.Uncommon  => UncommonColor,
            ItemRank.Rare      => RareColor,
            ItemRank.Epic      => EpicColor,
            ItemRank.Legendary => LegendaryColor,
            _ => CommonColor,
        };

        public static string Hex(ItemRank rank) => ColorUtility.ToHtmlStringRGB(For(rank));
    }

    [CreateAssetMenu(menuName = "DungeonBlade/Item/Misc Item", fileName = "NewItem")]
    public class Item : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] string itemId = "item_id";
        [SerializeField] string displayName = "New Item";
        [TextArea(2, 5)]
        [SerializeField] string description = "";
        [SerializeField] Sprite icon;

        [Header("Type")]
        [SerializeField] ItemType type = ItemType.Misc;
        [SerializeField] EquipmentSlot equipSlot = EquipmentSlot.None;
        [SerializeField] ItemRank rank = ItemRank.Common;

        [Header("Stack")]
        [SerializeField] bool stackable = true;
        [SerializeField] int maxStack = 99;

        [Header("Economy")]
        [SerializeField] int sellValue = 1;
        [SerializeField] int buyValue = 5;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemType Type => type;
        public EquipmentSlot EquipSlot => equipSlot;
        public ItemRank Rank => rank;
        public bool Stackable => stackable;
        public int MaxStack => stackable ? Mathf.Max(1, maxStack) : 1;
        public int SellValue => sellValue;
        public int BuyValue => buyValue;

        public virtual void OnUse(GameObject user) { }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId)) itemId = name.ToLowerInvariant().Replace(' ', '_');
        }
#endif
    }
}
