using DungeonBlade.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.Bank
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float interactRange = 2.5f;
        [SerializeField] LayerMask interactMask = ~0;
        [SerializeField] TMP_Text promptLabel;
        [SerializeField] Camera lookCamera;

        [Header("Prompt style")]
        [Tooltip("Action key name shown in the [ ] badge. F is the default Interact key.")]
        [SerializeField] string actionKeyLabel = "F";
        [Tooltip("Color of the action-key badge text.")]
        [SerializeField] Color keyColor = new Color(1f, 0.85f, 0.25f);
        [Tooltip("How fast the prompt fades in / out (seconds).")]
        [SerializeField, Range(0.05f, 0.6f)] float fadeDuration = 0.18f;

        PlayerInputActions _input;
        Interactable _current;
        CanvasGroup _promptGroup;
        float _targetAlpha;
        string _lastFormatted;

        void Start()
        {
            _input = InputManager.Instance != null ? InputManager.Instance.Actions : new PlayerInputActions();
            if (InputManager.Instance == null) _input.Enable();
            if (lookCamera == null) lookCamera = Camera.main;
            if (promptLabel != null)
            {
                promptLabel.richText = true;
                // CanvasGroup MUST go on the prompt label's own GameObject — putting it
                // on the parent would also hide siblings (BankPanel, ShopPanel, HUD, etc.)
                // if the prompt is parented directly under the Canvas.
                _promptGroup = promptLabel.GetComponent<CanvasGroup>();
                if (_promptGroup == null) _promptGroup = promptLabel.gameObject.AddComponent<CanvasGroup>();
                _promptGroup.alpha = 0f;
                _promptGroup.blocksRaycasts = false;
                _promptGroup.interactable = false;
                promptLabel.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            if (IsAnyMenuOpen()) { ClearPrompt(); return; }

            UpdateNearestInteractable();

            if (_current != null && _input.Interact.WasPressedThisFrame())
            {
                _current.OnInteract(gameObject);
            }
        }

        bool IsAnyMenuOpen()
        {
            if (Inventory.InventoryController.Instance != null && Inventory.InventoryController.Instance.IsOpen) return true;
            if (UI.BankController.Instance != null && UI.BankController.Instance.IsOpen) return true;
            if (UI.ShopController.Instance != null && UI.ShopController.Instance.IsOpen) return true;
            if (PortalConfirmDialog.Instance != null && PortalConfirmDialog.Instance.IsOpen) return true;
            return false;
        }

        void UpdateNearestInteractable()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactMask, QueryTriggerInteraction.Collide);

            Interactable best = null;
            float bestDot = -1f;
            Vector3 fwd = lookCamera != null ? lookCamera.transform.forward : transform.forward;
            Vector3 origin = lookCamera != null ? lookCamera.transform.position : transform.position;

            foreach (var h in hits)
            {
                var inter = h.GetComponentInParent<Interactable>();
                if (inter == null) continue;
                // Skip anything that handles its own proximity trigger — F-key UX is redundant for those.
                // But only if the proximity component is enabled; a disabled one means "use F-key instead".
                var pai = inter.GetComponent<ProximityAutoInteract>();
                if (pai != null && pai.enabled) continue;
                Vector3 to = (inter.transform.position - origin).normalized;
                float dot = Vector3.Dot(fwd, to);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = inter;
                }
            }

            if (best != _current)
            {
                _current = best;
                ShowPrompt(_current?.PromptText);
            }
            else if (_current != null)
            {
                ShowPrompt(_current.PromptText);
            }
        }

        void ShowPrompt(string text)
        {
            if (promptLabel == null) return;
            if (string.IsNullOrEmpty(text))
            {
                _targetAlpha = 0f;
                return;
            }
            // Format with a colored, bold [F] key badge in front of the action label.
            string keyHex = ColorUtility.ToHtmlStringRGB(keyColor);
            string formatted = $"<b><color=#{keyHex}>[{actionKeyLabel}]</color></b>  {text}";
            if (formatted != _lastFormatted)
            {
                promptLabel.text = formatted;
                _lastFormatted = formatted;
            }
            _targetAlpha = 1f;
        }

        void ClearPrompt() => ShowPrompt(null);

        void LateUpdate()
        {
            if (_promptGroup == null) return;
            float step = (fadeDuration <= 0.001f) ? 1f : Time.unscaledDeltaTime / fadeDuration;
            _promptGroup.alpha = Mathf.MoveTowards(_promptGroup.alpha, _targetAlpha, step);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
