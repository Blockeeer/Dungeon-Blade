using System.Collections.Generic;
using System.Globalization;
using DungeonBlade.Inventory.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.Bank.UI
{
    public class BankUI : MonoBehaviour
    {
        [Header("Slot prefab")]
        [SerializeField] BankSlotWidget slotPrefab;

        [Header("Containers")]
        [SerializeField] RectTransform gridParent;

        [Header("Drag ghost")]
        [SerializeField] Image dragGhost;

        [Header("Tooltip (shared with inventory)")]
        [SerializeField] ItemTooltip tooltip;

        [Header("Gold UI")]
        [SerializeField] TMP_Text pocketGoldText;
        [SerializeField] TMP_Text vaultGoldText;
        [SerializeField] TMP_InputField depositInput;
        [SerializeField] TMP_InputField withdrawInput;

        [Header("Gold formatting")]
        [SerializeField, Tooltip("Hex color (without #) for the gold amount in the Pocket/Vault labels.")]
        string goldColorHex = "F22E2E";
        [SerializeField, Tooltip("Bold the gold amount in rich text.")]
        bool boldGoldAmount = true;
        [SerializeField, Tooltip("Clear the deposit/withdraw input after pressing the button.")]
        bool clearInputAfterSubmit = true;

        [Header("Auto skin")]
        [SerializeField, Tooltip("Auto-add BankPanelSkin to the bank panel on enable (no Editor setup required).")]
        bool autoAttachSkin = true;

        public ItemTooltip Tooltip => tooltip;
        public BankSlotWidget DraggingFromBank { get; private set; }
        public RectTransform GridParent => gridParent;
        public TMP_InputField DepositInput => depositInput;
        public TMP_InputField WithdrawInput => withdrawInput;
        public TMP_Text PocketGoldText => pocketGoldText;
        public TMP_Text VaultGoldText => vaultGoldText;

        readonly List<BankSlotWidget> _slots = new List<BankSlotWidget>();

        void OnEnable()
        {
            if (autoAttachSkin && GetComponent<BankPanelSkin>() == null)
                gameObject.AddComponent<BankPanelSkin>();

            if (BankManager.Instance != null) BankManager.Instance.OnBankChanged += RefreshAll;
            if (BankManager.Instance != null) BankManager.Instance.OnGoldChanged += _ => RefreshGold();
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.OnGoldChanged += _ => RefreshGold();
            if (Inventory.InventoryManager.Instance != null) Inventory.InventoryManager.Instance.OnInventoryChanged += RefreshAll;
            EnsureBuilt();
            RefreshAll();
            RefreshGold();
        }

        void OnDisable()
        {
            if (BankManager.Instance != null) BankManager.Instance.OnBankChanged -= RefreshAll;
            if (BankManager.Instance != null) BankManager.Instance.OnGoldChanged -= _ => RefreshGold();
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.OnGoldChanged -= _ => RefreshGold();
            if (Inventory.InventoryManager.Instance != null) Inventory.InventoryManager.Instance.OnInventoryChanged -= RefreshAll;
            CancelDrag();
        }

        void EnsureBuilt()
        {
            if (slotPrefab == null || gridParent == null) return;
            if (_slots.Count > 0) return;

            for (int i = 0; i < BankManager.BankSize; i++)
            {
                var w = Instantiate(slotPrefab, gridParent);
                w.name = $"BankSlot_{i}";
                w.Bind(this, i);
                _slots.Add(w);
            }
        }

        void RefreshAll()
        {
            foreach (var s in _slots) s.Refresh();
        }

        void RefreshGold()
        {
            int pocket = PlayerWallet.Instance != null ? PlayerWallet.Instance.Gold : 0;
            int vault = BankManager.Instance != null ? BankManager.Instance.StoredGold : 0;
            if (pocketGoldText != null) pocketGoldText.text = FormatGold("Pocket", pocket);
            if (vaultGoldText != null) vaultGoldText.text = FormatGold("Vault", vault);
        }

        string FormatGold(string label, int amount)
        {
            string number = amount.ToString("N0", CultureInfo.InvariantCulture);
            string inner = boldGoldAmount ? $"<b>{number}g</b>" : $"{number}g";
            return $"{label}: <color=#{goldColorHex}>{inner}</color>";
        }

        public void OnDepositPressed()
        {
            if (BankManager.Instance == null) return;
            int amount = ParseAmount(depositInput);
            BankManager.Instance.DepositGold(amount);
            if (clearInputAfterSubmit && depositInput != null) depositInput.text = string.Empty;
        }

        public void OnWithdrawPressed()
        {
            if (BankManager.Instance == null) return;
            int amount = ParseAmount(withdrawInput);
            BankManager.Instance.WithdrawGold(amount);
            if (clearInputAfterSubmit && withdrawInput != null) withdrawInput.text = string.Empty;
        }

        public void OnClosePressed()
        {
            if (BankController.Instance != null) BankController.Instance.Close();
        }

        int ParseAmount(TMP_InputField field)
        {
            if (field == null) return 0;
            int.TryParse(field.text, out int v);
            return Mathf.Max(0, v);
        }

        public void BeginDrag(BankSlotWidget from)
        {
            DraggingFromBank = from;
            if (dragGhost != null && from != null && !from.Data.IsEmpty)
            {
                dragGhost.gameObject.SetActive(true);
                dragGhost.sprite = from.Data.Item.Icon;
                dragGhost.enabled = from.Data.Item.Icon != null;
            }
        }

        public void UpdateDrag(Vector2 screenPos)
        {
            if (dragGhost == null) return;
            dragGhost.rectTransform.position = screenPos;
        }

        public void EndDrag()
        {
            if (dragGhost != null) dragGhost.gameObject.SetActive(false);
            DraggingFromBank = null;
        }

        public void CancelDrag() => EndDrag();
    }
}
