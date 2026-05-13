using System;
using System.Collections.Generic;
using DungeonBlade.Core;
using DungeonBlade.Inventory;
using UnityEngine;

namespace DungeonBlade.Rewards
{
    // Gates a LootRoll behind a one-time profile flag. NoGate = always rolls
    // normally. WarlordsBladeFirstKill = rolls only if the profile hasn't
    // already received the Warlord's Blade (GDD §6.2 "first kill only").
    // After the roll succeeds and the item drops, the flag is set on the
    // profile so subsequent rolls skip this entry.
    public enum LootRollGate
    {
        None,
        WarlordsBladeFirstKill,
    }

    [CreateAssetMenu(menuName = "DungeonBlade/Loot Table", fileName = "LootTable")]
    public class LootTable : ScriptableObject
    {
        [Serializable]
        public struct LootRoll
        {
            public Item Item;
            [Range(0f, 1f)] public float DropChance;
            [Min(1)] public int MinQuantity;
            [Min(1)] public int MaxQuantity;
            [Tooltip("Optional profile-level gate. NoGate = always rolls. Used for one-time/first-kill-only drops.")]
            public LootRollGate Gate;
        }

        [Header("Item Rolls")]
        [Tooltip("Each row is rolled independently against its DropChance.")]
        [SerializeField] List<LootRoll> rolls = new List<LootRoll>();

        [Header("Gold")]
        [SerializeField] int minGold = 0;
        [SerializeField] int maxGold = 0;

        [Header("Experience")]
        [Min(0)] [SerializeField] int experience = 0;

        public IReadOnlyList<LootRoll> Rolls => rolls;
        public int MinGold => minGold;
        public int MaxGold => maxGold;
        public int Experience => experience;

        public int RollGold()
        {
            return UnityEngine.Random.Range(minGold, maxGold + 1);
        }

        public List<(Item item, int qty)> RollItems()
        {
            var result = new List<(Item, int)>(rolls.Count);
            var saveSystem = GameManager.Instance != null ? GameManager.Instance.SaveSystem : null;
            var profile = saveSystem?.Profile;
            foreach (var r in rolls)
            {
                if (r.Item == null) continue;

                // Profile-level gate: skip if the one-time flag is already set.
                if (!ProfilePermitsRoll(r.Gate, profile)) continue;

                if (UnityEngine.Random.value > r.DropChance) continue;
                int qty = UnityEngine.Random.Range(Mathf.Max(1, r.MinQuantity), Mathf.Max(r.MinQuantity, r.MaxQuantity) + 1);
                result.Add((r.Item, qty));

                // Roll succeeded — set the gate's flag so it never rolls again.
                MarkRollConsumed(r.Gate, profile, saveSystem);
            }
            return result;
        }

        static bool ProfilePermitsRoll(LootRollGate gate, PlayerProfile profile)
        {
            if (gate == LootRollGate.None) return true;
            if (profile == null) return true; // No save loaded — allow (e.g. testing).
            return gate switch
            {
                LootRollGate.WarlordsBladeFirstKill => !profile.warlordsBladeDropped,
                _ => true,
            };
        }

        static void MarkRollConsumed(LootRollGate gate, PlayerProfile profile, SaveSystem saveSystem)
        {
            if (gate == LootRollGate.None || profile == null) return;
            switch (gate)
            {
                case LootRollGate.WarlordsBladeFirstKill:
                    profile.warlordsBladeDropped = true;
                    saveSystem?.Save();
                    break;
            }
        }
    }
}
