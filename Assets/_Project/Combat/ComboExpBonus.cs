using DungeonBlade.Rewards;
using UnityEngine;

namespace DungeonBlade.Combat
{
    // Grants bonus EXP per Dungeon Blade GDD §9.2 — "+25 EXP per 10-hit
    // milestone." Subscribes to ComboSystem.OnComboChanged; when the combo
    // crosses a multiple of `milestoneEvery`, calls ExperienceSystem.GrantExperience.
    // Resets the awarded-milestone tracker when the combo ends so the next
    // combo earns bonuses again from 10.
    [RequireComponent(typeof(ComboSystem))]
    public class ComboExpBonus : MonoBehaviour
    {
        [Tooltip("How many hits between EXP bonuses. GDD §9.2 spec = 10.")]
        [Min(1)]
        [SerializeField] int milestoneEvery = 10;

        [Tooltip("EXP granted per milestone. GDD §9.2 spec = 25.")]
        [Min(1)]
        [SerializeField] int expPerMilestone = 25;

        ComboSystem _combo;
        int _lastMilestoneAwarded;

        void Awake()
        {
            _combo = GetComponent<ComboSystem>();
        }

        void OnEnable()
        {
            if (_combo != null)
            {
                _combo.OnComboChanged += OnComboChanged;
                _combo.OnComboReset += OnComboReset;
            }
        }

        void OnDisable()
        {
            if (_combo != null)
            {
                _combo.OnComboChanged -= OnComboChanged;
                _combo.OnComboReset -= OnComboReset;
            }
        }

        void OnComboChanged(int currentCombo)
        {
            // Compute the milestone number this combo qualifies for (e.g. 23 hits → milestone 2 at 20).
            int milestone = currentCombo / milestoneEvery;
            if (milestone <= _lastMilestoneAwarded) return;

            // Award one bonus per crossed milestone (handles edge case where
            // combo jumps multiple milestones at once, though unlikely).
            int newMilestones = milestone - _lastMilestoneAwarded;
            _lastMilestoneAwarded = milestone;

            if (ExperienceSystem.Instance == null) return;
            int totalBonus = newMilestones * expPerMilestone;
            ExperienceSystem.Instance.GrantExperience(totalBonus);
            Debug.Log($"[Combo] {currentCombo}-hit milestone! +{totalBonus} bonus EXP.");
        }

        void OnComboReset(int finalCombo)
        {
            // Combo broke — reset the tracker so the next combo can earn bonuses again from 10.
            _lastMilestoneAwarded = 0;
        }
    }
}
