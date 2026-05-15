using System.Collections.Generic;
using System.Globalization;
using DungeonBlade.Inventory.UI;
using TMPro;
using UnityEngine;

namespace DungeonBlade.Bank.UI
{
    public class ShopUI : MonoBehaviour
    {
        [Header("Stock list")]
        [SerializeField] ShopStockEntry stockPrefab;
        [SerializeField] RectTransform stockParent;

        [Header("Header / labels")]
        [SerializeField] TMP_Text shopNameText;
        [SerializeField] TMP_Text pocketGoldText;

        [Header("Tooltip (shared)")]
        [SerializeField] ItemTooltip tooltip;

        [Header("Gold formatting")]
        [SerializeField] string goldColorHex = "F22E2E";
        [SerializeField] bool boldGoldAmount = true;

        [Header("Auto skin")]
        [SerializeField] bool autoAttachSkin = true;

        public ItemTooltip Tooltip => tooltip;
        public RectTransform StockParent => stockParent;
        public TMP_Text ShopNameText => shopNameText;
        public TMP_Text PocketGoldText => pocketGoldText;

        readonly List<ShopStockEntry> _entries = new List<ShopStockEntry>();

        void OnEnable()
        {
            if (autoAttachSkin && GetComponent<ShopPanelSkin>() == null)
                gameObject.AddComponent<ShopPanelSkin>();

            if (ShopManager.Instance != null) ShopManager.Instance.OnShopChanged += Rebuild;
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.OnGoldChanged += _ => RefreshGold();
            Rebuild();
            RefreshGold();
        }

        void OnDisable()
        {
            if (ShopManager.Instance != null) ShopManager.Instance.OnShopChanged -= Rebuild;
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.OnGoldChanged -= _ => RefreshGold();
        }

        void Rebuild()
        {
            foreach (var e in _entries) if (e != null) Destroy(e.gameObject);
            _entries.Clear();

            var shop = ShopManager.Instance != null ? ShopManager.Instance.CurrentShop : null;
            if (shop == null) return;

            if (shopNameText != null) shopNameText.text = shop.ShopName;

            for (int i = 0; i < shop.StockList.Count; i++)
            {
                var s = shop.StockList[i];
                if (stockPrefab == null || stockParent == null) break;

                var entry = Instantiate(stockPrefab, stockParent);
                entry.name = $"Stock_{i}";
                entry.Bind(this, i, s, shop.BuyPriceFor(i));
                _entries.Add(entry);
            }
        }

        void RefreshGold()
        {
            int pocket = PlayerWallet.Instance != null ? PlayerWallet.Instance.Gold : 0;
            if (pocketGoldText != null)
            {
                string n = pocket.ToString("N0", CultureInfo.InvariantCulture);
                string inner = boldGoldAmount ? $"<b>{n}g</b>" : $"{n}g";
                pocketGoldText.text = $"Gold: <color=#{goldColorHex}>{inner}</color>";
            }
        }

        public void OnClosePressed()
        {
            if (ShopController.Instance != null) ShopController.Instance.Close();
        }
    }
}
