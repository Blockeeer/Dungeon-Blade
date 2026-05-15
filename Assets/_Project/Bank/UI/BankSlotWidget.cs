using DungeonBlade.Inventory;
using DungeonBlade.Inventory.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonBlade.Bank.UI
{
    public class BankSlotWidget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] Image background;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text quantityText;

        [Header("Slot palette (menu theme)")]
        [SerializeField] Color emptyColor = new Color(0.045f, 0.050f, 0.075f, 0.78f);
        [SerializeField] Color filledColor = new Color(0.080f, 0.095f, 0.130f, 0.92f);
        [SerializeField] Color hoverColor = new Color(0.95f, 0.18f, 0.18f, 1f);

        [Header("Hover feedback")]
        [SerializeField, Tooltip("Local scale applied when hovering a filled slot.")]
        float hoverScale = 1.08f;
        [SerializeField, Tooltip("Color / scale lerp speed. Higher = snappier.")]
        float tweenSpeed = 14f;

        public int Index { get; private set; }
        public BankUI Owner { get; private set; }
        public InventorySlot Data { get; private set; } = InventorySlot.Empty;

        Color _targetColor;
        Vector3 _targetScale = Vector3.one;
        bool _isHovered;

        public void Bind(BankUI owner, int index)
        {
            Owner = owner;
            Index = index;
            Refresh();
        }

        public void Refresh()
        {
            if (BankManager.Instance == null) return;
            Data = BankManager.Instance.GetSlot(Index);

            UpdateTargetColor();
            if (iconImage != null)
            {
                iconImage.enabled = !Data.IsEmpty && Data.Item.Icon != null;
                if (!Data.IsEmpty && Data.Item.Icon != null) iconImage.sprite = Data.Item.Icon;
            }
            if (quantityText != null)
            {
                bool show = !Data.IsEmpty && Data.Quantity > 1;
                quantityText.gameObject.SetActive(show);
                if (show) quantityText.text = Data.Quantity.ToString();
            }
        }

        void UpdateTargetColor()
        {
            if (_isHovered && !Data.IsEmpty) _targetColor = hoverColor;
            else _targetColor = Data.IsEmpty ? emptyColor : filledColor;
        }

        void OnEnable()
        {
            UpdateTargetColor();
            if (background != null) background.color = _targetColor;
            transform.localScale = _targetScale;
        }

        void Update()
        {
            float t = Time.unscaledDeltaTime * tweenSpeed;
            if (background != null) background.color = Color.Lerp(background.color, _targetColor, t);
            if (transform.localScale != _targetScale)
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, t);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            _isHovered = true;
            UpdateTargetColor();
            if (!Data.IsEmpty)
            {
                _targetScale = Vector3.one * hoverScale;
                Owner?.Tooltip?.Show(Data.Item, transform.position);
            }
        }

        public void OnPointerExit(PointerEventData e)
        {
            _isHovered = false;
            UpdateTargetColor();
            _targetScale = Vector3.one;
            Owner?.Tooltip?.Hide();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (Data.IsEmpty) return;
            Owner?.BeginDrag(this);
            DragRouter.BeginBankDrag(Index, Data.Item);
        }

        public void OnDrag(PointerEventData e) => Owner?.UpdateDrag(e.position);

        public void OnEndDrag(PointerEventData e)
        {
            Owner?.EndDrag();
            DragRouter.End();
        }

        public void OnDrop(PointerEventData e)
        {
            if (BankManager.Instance == null) return;

            if (DragRouter.SourceKind == DragSourceKind.Bank)
            {
                if (DragRouter.SourceIndex == Index) return;
                BankManager.Instance.MoveOrSwap(DragRouter.SourceIndex, Index);
                return;
            }

            if (DragRouter.SourceKind == DragSourceKind.Inventory)
            {
                BankManager.Instance.DepositFromInventory(DragRouter.InventorySourceKind, DragRouter.SourceIndex);
                return;
            }
        }
    }
}
