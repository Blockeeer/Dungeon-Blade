using DungeonBlade.Bank;
using DungeonBlade.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonBlade.Inventory
{
    public class InventoryPersistence : MonoBehaviour
    {
        [SerializeField] ItemDatabase database;
        [SerializeField] bool loadOnStart = true;
        [SerializeField] bool saveOnQuit = true;
        [Tooltip("Auto-save before every scene change. Belt-and-suspenders safety net so loot picked up in Dungeon persists when returning to Lobby even if a portal forgot to wire its persistence reference.")]
        [SerializeField] bool saveOnSceneUnload = true;

        SaveSystem _save;

        void Start()
        {
            _save = new SaveSystem();
            _save.Load();

            if (!loadOnStart || database == null) return;

            var profile = _save.Profile;
            if (_save.ProfileLoaded)
            {
                if (InventoryManager.Instance != null && profile.inventory != null && profile.inventory.Count > 0)
                {
                    InventoryManager.Instance.DeserializeAll(profile.inventory, database.Find);
                    Debug.Log("[Inventory] Loaded inventory from profile.json");
                }
                if (PlayerWallet.Instance != null)
                {
                    PlayerWallet.Instance.SetGold(profile.gold);
                    Debug.Log($"[Wallet] Loaded gold from profile.json: {profile.gold}g");
                }
                if (Rewards.ExperienceSystem.Instance != null)
                {
                    Rewards.ExperienceSystem.Instance.SetState(profile.level, profile.experience);
                    Debug.Log($"[EXP] Loaded level {profile.level} ({profile.experience} EXP).");
                }
            }

            if (_save.BankLoaded && BankManager.Instance != null)
            {
                BankManager.Instance.DeserializeAll(_save.Bank.bankSlots, database.Find);
                BankManager.Instance.SetStoredGold(_save.Bank.storedGold);
                Debug.Log("[Bank] Loaded bank from bank.json");
            }
        }

        void OnEnable()
        {
            if (saveOnSceneUnload) SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDisable()
        {
            if (saveOnSceneUnload) SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void OnSceneUnloaded(Scene scene)
        {
            // Fires for any scene unload (portal transitions, manual loads,
            // death respawns). Ensures the player's run progress survives
            // even if the outgoing scene's portal forgot to call SaveNow.
            if (!saveOnSceneUnload) return;
            if (InventoryManager.Instance == null) return; // already torn down
            SaveNow();
        }

        void OnApplicationQuit()
        {
            if (!saveOnQuit) return;
            SaveNow();
        }

        public void SaveNow()
        {
            if (_save == null) _save = new SaveSystem();

            if (InventoryManager.Instance != null)
                _save.Profile.inventory = InventoryManager.Instance.SerializeAll();
            if (PlayerWallet.Instance != null)
                _save.Profile.gold = PlayerWallet.Instance.Gold;
            if (Rewards.ExperienceSystem.Instance != null)
            {
                _save.Profile.level = Rewards.ExperienceSystem.Instance.Level;
                _save.Profile.experience = Rewards.ExperienceSystem.Instance.Experience;
            }

            if (BankManager.Instance != null)
            {
                _save.Bank.bankSlots = BankManager.Instance.SerializeAll();
                _save.Bank.storedGold = BankManager.Instance.StoredGold;
            }

            _save.Save();
            Debug.Log("[Save] Saved profile.json + bank.json");
        }
    }
}
