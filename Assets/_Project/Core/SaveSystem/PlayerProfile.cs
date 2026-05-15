using System;
using System.Collections.Generic;
using DungeonBlade.Inventory;

namespace DungeonBlade.Core
{
    [Serializable]
    public class PlayerProfile
    {
        public string playerName = "Hero";
        public string characterId = "lyra";
        public int level = 1;
        public int experience = 0;
        public int gold = 0;
        public List<string> ownedItemIds = new List<string>();
        public List<SerializedSlot> inventory = new List<SerializedSlot>();

        // Per-account flags for loot gating. Per GDD §6.2:
        // Warlord's Blade is a "first kill only" drop — once granted, never
        // rolls again on subsequent boss kills.
        public bool warlordsBladeDropped = false;
    }
}
