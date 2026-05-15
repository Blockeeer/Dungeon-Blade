using System.Collections;
using DungeonBlade.UI.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.Bank.UI
{
    public class ShopPanelSkin : MonoBehaviour
    {
        [Header("Font (optional, matches menu)")]
        [SerializeField] TMP_FontAsset themeFont;

        [Header("Overlay")]
        [SerializeField] Color overlayColor = new Color(0.020f, 0.024f, 0.035f, 0.86f);
        [SerializeField] bool addScanlines = true;
        [SerializeField, Range(0f, 0.5f)] float scanlineAlpha = 0.14f;
        [SerializeField] bool addVignette = true;
        [SerializeField, Range(0f, 1f)] float vignetteAlpha = 0.55f;

        [Header("Buttons")]
        [SerializeField] bool uppercaseButtonLabels = true;
        [SerializeField] float buttonLabelSpacing = 10f;
        [SerializeField] bool replaceButtonSprite = true;

        [Header("Shop title")]
        [SerializeField] bool stylizeTitle = true;
        [SerializeField] float titleFontSize = 40f;
        [SerializeField] float titleSpacing = 8f;
        [SerializeField] bool addTitleRedEcho = true;
        [SerializeField] Vector2 titleEchoOffset = new Vector2(4f, -3f);

        [Header("Gold label")]
        [SerializeField] bool stylizeGold = true;
        [SerializeField] float goldFontSize = 22f;

        [Header("Layout & cards")]
        [SerializeField] bool addContentCards = true;
        [SerializeField] float cardPadding = 24f;
        [SerializeField] bool addCardAccentStripe = true;
        [SerializeField] Color cardColor = new Color(0.060f, 0.075f, 0.105f, 0.85f);
        [SerializeField] Color sellZoneColor = new Color(0.40f, 0.06f, 0.06f, 0.80f);

        const string ScanlinesName = "ShopScanlines_AutoSkin";
        const string VignetteName = "ShopVignette_AutoSkin";
        const string StockCardName = "StockCard_AutoSkin";
        const string SellCardName = "SellCard_AutoSkin";
        bool _applied;

        void OnEnable()
        {
            Apply();
            StartCoroutine(SkinEntriesNextFrame());
            if (ShopManager.Instance != null) ShopManager.Instance.OnShopChanged += OnShopChanged;
        }

        void OnDisable()
        {
            if (ShopManager.Instance != null) ShopManager.Instance.OnShopChanged -= OnShopChanged;
        }

        void OnShopChanged()
        {
            if (isActiveAndEnabled) StartCoroutine(SkinEntriesNextFrame());
        }

        IEnumerator SkinEntriesNextFrame()
        {
            yield return null;
            SkinAllButtons();
        }

        void SkinAllButtons()
        {
            foreach (var btn in GetComponentsInChildren<Button>(true))
                SkinButton(btn);
        }

        void Apply()
        {
            if (_applied) return;
            _applied = true;

            var rootImg = GetComponent<Image>();
            if (rootImg != null) rootImg.color = overlayColor;

            if (addScanlines) EnsureScanlines();
            if (addVignette) EnsureVignette();

            var shopUI = GetComponent<ShopUI>();
            RectTransform stockRT = shopUI != null ? shopUI.StockParent : null;
            RectTransform sellRT = FindSellZoneRect();

            if (addContentCards)
            {
                if (stockRT != null) EnsureCard(StockCardName, stockRT, cardColor);
                if (sellRT != null) EnsureCard(SellCardName, sellRT, sellZoneColor);
            }

            SkinAllButtons();

            foreach (var input in GetComponentsInChildren<TMP_InputField>(true))
                SkinInput(input);

            if (shopUI != null)
            {
                if (stylizeTitle && shopUI.ShopNameText != null) StylizeTitle(shopUI.ShopNameText);
                if (stylizeGold && shopUI.PocketGoldText != null) StylizeGold(shopUI.PocketGoldText);
            }

            if (themeFont != null)
            {
                foreach (var txt in GetComponentsInChildren<TMP_Text>(true))
                {
                    if (txt.GetComponentInParent<Button>() != null) continue;
                    if (txt.GetComponentInParent<TMP_InputField>() != null) continue;
                    if (txt.font != themeFont) txt.font = themeFont;
                }
            }
        }

        RectTransform FindSellZoneRect()
        {
            var sz = GetComponentInChildren<ShopSellZone>(true);
            return sz != null ? sz.transform as RectTransform : null;
        }

        void StylizeTitle(TMP_Text title)
        {
            if (themeFont != null) title.font = themeFont;
            title.fontSize = titleFontSize;
            title.fontStyle = FontStyles.Bold;
            title.color = MenuFx.SteelTint;
            title.characterSpacing = titleSpacing;
            title.alignment = TextAlignmentOptions.Center;
            title.enableVertexGradient = true;
            title.colorGradient = new VertexGradient(MenuFx.SteelTint, MenuFx.SteelTint, MenuFx.MutedSteel, MenuFx.MutedSteel);
            title.raycastTarget = false;

            if (!addTitleRedEcho || title.transform.parent == null) return;
            if (title.transform.parent.Find("TitleEcho_AutoSkin") != null) return;

            var echoGo = new GameObject("TitleEcho_AutoSkin", typeof(RectTransform), typeof(CanvasRenderer));
            echoGo.layer = title.gameObject.layer;
            var echoRT = (RectTransform)echoGo.transform;
            echoRT.SetParent(title.transform.parent, false);
            var srcRT = title.rectTransform;
            echoRT.anchorMin = srcRT.anchorMin;
            echoRT.anchorMax = srcRT.anchorMax;
            echoRT.pivot = srcRT.pivot;
            echoRT.anchoredPosition = srcRT.anchoredPosition + titleEchoOffset;
            echoRT.sizeDelta = srcRT.sizeDelta;
            var echo = echoGo.AddComponent<TextMeshProUGUI>();
            if (themeFont != null) echo.font = themeFont;
            echo.text = title.text;
            echo.fontSize = title.fontSize;
            echo.fontStyle = title.fontStyle;
            echo.alignment = title.alignment;
            echo.characterSpacing = title.characterSpacing;
            echo.color = new Color(MenuFx.AccentRed.r, MenuFx.AccentRed.g, MenuFx.AccentRed.b, 0.45f);
            echo.raycastTarget = false;
            echoRT.SetSiblingIndex(srcRT.GetSiblingIndex());

            var sync = echoGo.AddComponent<TitleEchoSync>();
            sync.Bind(title, echo);
        }

        void StylizeGold(TMP_Text gold)
        {
            if (themeFont != null) gold.font = themeFont;
            gold.fontSize = goldFontSize;
            gold.fontStyle = FontStyles.Bold;
            gold.characterSpacing = 4f;
            gold.raycastTarget = false;
        }

        void SkinButton(Button btn)
        {
            var img = btn.GetComponent<Image>();
            if (img != null && replaceButtonSprite)
            {
                img.sprite = MenuFx.SlabSprite();
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                img.pixelsPerUnitMultiplier = 1f;
            }

            var cb = btn.colors;
            cb.normalColor = MenuFx.SlabFill;
            cb.highlightedColor = MenuFx.SlabHover;
            cb.pressedColor = MenuFx.SlabPress;
            cb.selectedColor = MenuFx.SlabHover;
            cb.disabledColor = new Color(0.04f, 0.05f, 0.07f, 0.45f);
            cb.fadeDuration = 0.12f;
            cb.colorMultiplier = 1f;
            btn.colors = cb;
            btn.transition = Selectable.Transition.ColorTint;
            if (img != null) btn.targetGraphic = img;

            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                if (themeFont != null) label.font = themeFont;
                if (uppercaseButtonLabels && !string.IsNullOrEmpty(label.text))
                    label.text = label.text.ToUpperInvariant();
                label.fontStyle |= FontStyles.Bold;
                label.color = MenuFx.SteelTint;
                label.characterSpacing = buttonLabelSpacing;
            }
        }

        void SkinInput(TMP_InputField input)
        {
            var img = input.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = MenuFx.SlabSprite();
                img.type = Image.Type.Sliced;
                img.color = MenuFx.SlabFill;
                img.pixelsPerUnitMultiplier = 1f;
            }
            if (input.textComponent != null)
            {
                input.textComponent.color = MenuFx.SteelTint;
                if (themeFont != null) input.textComponent.font = themeFont;
            }
            if (input.placeholder is TMP_Text placeholder)
            {
                var c = MenuFx.MutedSteel;
                c.a = 0.5f;
                placeholder.color = c;
                if (themeFont != null) placeholder.font = themeFont;
            }
        }

        void EnsureCard(string name, RectTransform target, Color color)
        {
            var parent = target.parent as RectTransform;
            if (parent == null) return;
            if (parent.Find(name) != null) return;

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = target.anchorMin;
            rt.anchorMax = target.anchorMax;
            rt.pivot = target.pivot;
            rt.anchoredPosition = target.anchoredPosition;
            rt.sizeDelta = target.sizeDelta + new Vector2(cardPadding * 2f, cardPadding * 2f);

            var img = go.AddComponent<Image>();
            img.sprite = MenuFx.RoundedSlabSprite();
            img.type = Image.Type.Sliced;
            img.color = color;
            img.pixelsPerUnitMultiplier = 1f;
            img.raycastTarget = false;

            rt.SetSiblingIndex(target.GetSiblingIndex());

            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            if (addCardAccentStripe)
            {
                var stripeGo = new GameObject("AccentStripe", typeof(RectTransform), typeof(CanvasRenderer));
                stripeGo.layer = gameObject.layer;
                var stripeRT = (RectTransform)stripeGo.transform;
                stripeRT.SetParent(rt, false);
                stripeRT.anchorMin = new Vector2(0f, 0f);
                stripeRT.anchorMax = new Vector2(0f, 1f);
                stripeRT.pivot = new Vector2(0f, 0.5f);
                stripeRT.anchoredPosition = new Vector2(2f, 0f);
                stripeRT.sizeDelta = new Vector2(4f, -16f);
                var stripeImg = stripeGo.AddComponent<Image>();
                stripeImg.color = MenuFx.AccentRed;
                stripeImg.raycastTarget = false;
            }
        }

        void EnsureScanlines()
        {
            if (transform.Find(ScanlinesName) != null) return;
            var go = new GameObject(ScanlinesName, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.sprite = MenuFx.ScanlineSprite();
            img.type = Image.Type.Tiled;
            img.color = new Color(0f, 0f, 0f, scanlineAlpha);
            img.pixelsPerUnitMultiplier = 1f;
            img.raycastTarget = false;
            rt.SetAsFirstSibling();
        }

        void EnsureVignette()
        {
            if (transform.Find(VignetteName) != null) return;
            var go = new GameObject(VignetteName, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.sprite = MenuFx.VignetteSprite();
            img.color = new Color(0f, 0f, 0f, vignetteAlpha);
            img.raycastTarget = false;
            rt.SetAsFirstSibling();
        }
    }

    public class TitleEchoSync : MonoBehaviour
    {
        TMP_Text _source;
        TMP_Text _echo;
        string _lastText;

        public void Bind(TMP_Text source, TMP_Text echo)
        {
            _source = source;
            _echo = echo;
            _lastText = null;
        }

        void LateUpdate()
        {
            if (_source == null || _echo == null) return;
            if (_source.text == _lastText) return;
            _lastText = _source.text;
            _echo.text = _lastText;
        }
    }
}
