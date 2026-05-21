using DungeonBlade.Core;
using DungeonBlade.Inventory;
using UnityEngine;

namespace DungeonBlade.Bank
{
    [RequireComponent(typeof(PortalVisual))]
    public class ScenePortal : Interactable
    {
        [SerializeField] string targetScene = SceneLoader.Dungeon1;
        [SerializeField] InventoryPersistence persistence;
        [SerializeField] bool saveBeforeTransition = true;
        [SerializeField, Tooltip("Auto-add the procedural portal door visuals (frame + glow light + pulse).")]
        bool autoAttachVisual = true;

        [Header("Activation")]
        [SerializeField, Tooltip("If on, simply walking into the portal triggers the transition (no F-press needed). The interaction collider must be a trigger.")]
        bool autoTriggerOnPlayerEnter = true;
        [SerializeField, Tooltip("Tag used to identify the player when auto-triggering. Leave blank to skip tag check.")]
        string playerTag = "Player";

        [Header("Confirm dialog")]
        [SerializeField] bool showConfirmDialog = true;
        [SerializeField] string dialogTitle = "Enter Forsaken Keep?";
        [SerializeField, TextArea(2, 5)]
        string dialogBody = "This dungeon contains a powerful boss. Make sure you are prepared before proceeding.";
        [SerializeField] string confirmButtonText = "Proceed";
        [SerializeField] string cancelButtonText = "Cancel";

        [Header("Loading screen")]
        [SerializeField, Tooltip("Title shown on the loading screen. Leave blank to auto-format the scene name.")]
        string loadingDisplayName = "Forsaken Keep";
        [SerializeField, TextArea(2, 4)]
        string loadingTip = "Tip: Block with right-click. Parry just before a hit lands to stagger your enemy.";

        bool _alreadyTriggered;

        void Awake()
        {
            if (autoAttachVisual && GetComponent<PortalVisual>() == null)
                gameObject.AddComponent<PortalVisual>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!autoTriggerOnPlayerEnter) return;
            if (_alreadyTriggered) return;
            if (!IsPlayer(other)) return;
            _alreadyTriggered = true;
            OnInteract(other.gameObject);
        }

        void OnTriggerExit(Collider other)
        {
            if (!autoTriggerOnPlayerEnter) return;
            if (!IsPlayer(other)) return;
            _alreadyTriggered = false;
        }

        bool IsPlayer(Collider c)
        {
            if (string.IsNullOrEmpty(playerTag)) return true;
            if (c.CompareTag(playerTag)) return true;
            var attached = c.attachedRigidbody;
            return attached != null && attached.CompareTag(playerTag);
        }

        public override void OnInteract(GameObject player)
        {
            if (PortalConfirmDialog.Instance != null && PortalConfirmDialog.Instance.IsOpen) return;

            if (showConfirmDialog)
            {
                PortalConfirmDialog.Show(dialogTitle, dialogBody, confirmButtonText, cancelButtonText, BeginTransition);
            }
            else
            {
                BeginTransition();
            }
        }

        void BeginTransition()
        {
            if (saveBeforeTransition)
            {
                // Auto-find persistence if the Inspector field is empty —
                // otherwise scene-specific portals that forget to wire it
                // silently lose all the player's run progress on transition.
                if (persistence == null)
                {
                    persistence = FindObjectOfType<InventoryPersistence>();
                }
                if (persistence != null) persistence.SaveNow();
                else Debug.LogWarning("[ScenePortal] No InventoryPersistence in scene — run progress will NOT be saved on transition.");
            }

            if (FadeLoader.Instance != null) FadeLoader.Instance.LoadScene(targetScene, loadingDisplayName, loadingTip);
            else SceneLoader.Load(targetScene);
        }
    }
}
