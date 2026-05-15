using DungeonBlade.UI.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.Bank.UI
{
    public class BankPanelSkin : MonoBehaviour
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
        [SerializeField] float buttonLabelSpacing = 14f;
        [SerializeField] bool replaceButtonSprite = true;

        [Header("Input fields")]
        [SerializeField] bool replaceInputSprite = true;

        [Header("Header")]
        [SerializeField] bool createHeader = true;
        [SerializeField] string headerText = "BANK";
        [SerializeField] float headerFontSize = 56f;
        [SerializeField] float headerSpacing = 22f;
        [SerializeField] float headerOffsetY = -40f;
        [SerializeField] bool addHeaderRedEcho = true;

        [Header("Layout & cards")]
        [SerializeField] bool repositionContent = true;
        [SerializeField, Tooltip("Side-by-side (grid left, form right) instead of stacked.")]
        bool twoColumnLayout = true;
        [SerializeField, Tooltip("Place grid on left and form on right. Flip for the opposite.")]
        bool gridOnLeft = true;
        [SerializeField, Tooltip("Top of the cards in pixels from the top of the screen.")]
        float layoutTopY = -150f;
        [SerializeField, Tooltip("Horizontal gap between the two columns (in two-column mode).")]
        float columnGap = 40f;
        [SerializeField, Tooltip("Vertical gap between cards when stacked (single-column mode).")]
        float cardGap = 36f;
        [SerializeField] bool addContentCards = true;
        [SerializeField] float cardPadding = 24f;
        [SerializeField, Tooltip("Match both cards to the wider content width (only used in stacked mode).")]
        bool unifyCardWidths = true;
        [SerializeField] bool addCardAccentStripe = true;
        [SerializeField] Color cardColor = new Color(0.060f, 0.075f, 0.105f, 0.85f);

        const string HeaderName = "BankHeader_AutoSkin";
        const string ScanlinesName = "BankScanlines_AutoSkin";
        const string VignetteName = "BankVignette_AutoSkin";
        const string FormCardName = "FormCard_AutoSkin";
        const string GridCardName = "GridCard_AutoSkin";
        bool _applied;

        void OnEnable() { Apply(); }

        void Apply()
        {
            if (_applied) return;
            _applied = true;

            var rootImg = GetComponent<Image>();
            if (rootImg != null) rootImg.color = overlayColor;

            if (addScanlines) EnsureScanlines();
            if (addVignette) EnsureVignette();

            var bankUI = GetComponent<BankUI>();
            RectTransform formRT = null;
            if (bankUI != null && bankUI.DepositInput != null)
                formRT = bankUI.DepositInput.transform.parent as RectTransform;
            RectTransform gridRT = bankUI != null ? bankUI.GridParent : null;

            if (gridRT != null) FitGridToContent(gridRT);

            float overrideWidth = -1f;
            if (repositionContent && formRT != null && gridRT != null)
            {
                formRT.anchorMin = new Vector2(0.5f, 1f);
                formRT.anchorMax = new Vector2(0.5f, 1f);
                formRT.pivot = new Vector2(0.5f, 0.5f);

                gridRT.anchorMin = new Vector2(0.5f, 1f);
                gridRT.anchorMax = new Vector2(0.5f, 1f);
                gridRT.pivot = new Vector2(0.5f, 0.5f);

                if (twoColumnLayout)
                {
                    float formCardWidth = formRT.sizeDelta.x + cardPadding * 2f;
                    float gridCardWidth = gridRT.sizeDelta.x + cardPadding * 2f;
                    float totalWidth = formCardWidth + columnGap + gridCardWidth;
                    float leftCenterX = -totalWidth * 0.5f + (gridOnLeft ? gridCardWidth : formCardWidth) * 0.5f;
                    float rightCenterX = +totalWidth * 0.5f - (gridOnLeft ? formCardWidth : gridCardWidth) * 0.5f;

                    float contentTopFromScreenTop = -layoutTopY + cardPadding;
                    float formCenterY = -(contentTopFromScreenTop + formRT.sizeDelta.y * 0.5f);
                    float gridCenterY = -(contentTopFromScreenTop + gridRT.sizeDelta.y * 0.5f);

                    formRT.anchoredPosition = new Vector2(gridOnLeft ? rightCenterX : leftCenterX, formCenterY);
                    gridRT.anchoredPosition = new Vector2(gridOnLeft ? leftCenterX : rightCenterX, gridCenterY);
                }
                else
                {
                    formRT.anchoredPosition = new Vector2(0f, layoutTopY - formRT.sizeDelta.y * 0.5f - cardPadding);
                    float formBottomFromTop = -layoutTopY + formRT.sizeDelta.y + cardPadding * 2f;
                    float gridTopFromTop = formBottomFromTop + cardGap;
                    float gridCenterFromTop = gridTopFromTop + cardPadding + gridRT.sizeDelta.y * 0.5f;
                    gridRT.anchoredPosition = new Vector2(0f, -gridCenterFromTop);

                    if (unifyCardWidths)
                        overrideWidth = Mathf.Max(formRT.sizeDelta.x, gridRT.sizeDelta.x) + cardPadding * 2f;
                }
            }

            if (addContentCards)
            {
                if (formRT != null) EnsureCard(FormCardName, formRT, overrideWidth);
                if (gridRT != null) EnsureCard(GridCardName, gridRT, overrideWidth);
            }

            foreach (var btn in GetComponentsInChildren<Button>(true))
                SkinButton(btn);

            foreach (var input in GetComponentsInChildren<TMP_InputField>(true))
                SkinInput(input);

            if (themeFont != null)
            {
                foreach (var txt in GetComponentsInChildren<TMP_Text>(true))
                {
                    if (txt.transform.parent != null && txt.transform.parent.name.StartsWith("BankHeader_AutoSkin")) continue;
                    if (txt.GetComponentInParent<Button>() != null) continue;
                    if (txt.GetComponentInParent<TMP_InputField>() != null) continue;
                    txt.font = themeFont;
                }
            }

            if (createHeader) EnsureHeader();
        }

        void FitGridToContent(RectTransform gridRT)
        {
            var glg = gridRT.GetComponent<GridLayoutGroup>();
            if (glg == null) return;

            int slotCount = BankManager.BankSize;
            int cols, rows;
            if (glg.constraint == GridLayoutGroup.Constraint.FixedColumnCount && glg.constraintCount > 0)
            {
                cols = glg.constraintCount;
                rows = Mathf.CeilToInt(slotCount / (float)cols);
            }
            else if (glg.constraint == GridLayoutGroup.Constraint.FixedRowCount && glg.constraintCount > 0)
            {
                rows = glg.constraintCount;
                cols = Mathf.CeilToInt(slotCount / (float)rows);
            }
            else
            {
                return;
            }

            float width = glg.padding.horizontal + cols * glg.cellSize.x + Mathf.Max(0, cols - 1) * glg.spacing.x;
            float height = glg.padding.vertical + rows * glg.cellSize.y + Mathf.Max(0, rows - 1) * glg.spacing.y;
            gridRT.sizeDelta = new Vector2(width, height);
        }

        void EnsureCard(string name, RectTransform target, float widthOverride = -1f)
        {
            if (transform.Find(name) != null) return;

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = target.anchorMin;
            rt.anchorMax = target.anchorMax;
            rt.pivot = target.pivot;
            rt.anchoredPosition = target.anchoredPosition;
            float width = widthOverride > 0f ? widthOverride : target.sizeDelta.x + cardPadding * 2f;
            float height = target.sizeDelta.y + cardPadding * 2f;
            rt.sizeDelta = new Vector2(width, height);

            var img = go.AddComponent<Image>();
            img.sprite = MenuFx.RoundedSlabSprite();
            img.type = Image.Type.Sliced;
            img.color = cardColor;
            img.pixelsPerUnitMultiplier = 1f;
            img.raycastTarget = false;

            rt.SetSiblingIndex(target.GetSiblingIndex());

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
                if (replaceInputSprite)
                {
                    img.sprite = MenuFx.SlabSprite();
                    img.type = Image.Type.Sliced;
                    img.pixelsPerUnitMultiplier = 1f;
                }
                img.color = MenuFx.SlabFill;
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

        void EnsureHeader()
        {
            if (transform.Find(HeaderName) != null) return;

            var go = new GameObject(HeaderName, typeof(RectTransform));
            go.layer = gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, headerOffsetY);
            rt.sizeDelta = new Vector2(600f, 80f);

            var ignore = go.AddComponent<LayoutElement>();
            ignore.ignoreLayout = true;

            if (addHeaderRedEcho)
            {
                var echoGo = new GameObject(HeaderName + "_Echo", typeof(RectTransform));
                echoGo.layer = gameObject.layer;
                var echoRT = (RectTransform)echoGo.transform;
                echoRT.SetParent(rt, false);
                echoRT.anchorMin = Vector2.zero;
                echoRT.anchorMax = Vector2.one;
                echoRT.pivot = new Vector2(0.5f, 0.5f);
                echoRT.anchoredPosition = new Vector2(4f, -3f);
                echoRT.offsetMin = Vector2.zero;
                echoRT.offsetMax = Vector2.zero;

                var echoTxt = echoGo.AddComponent<TextMeshProUGUI>();
                if (themeFont != null) echoTxt.font = themeFont;
                echoTxt.text = headerText;
                echoTxt.fontSize = headerFontSize;
                echoTxt.color = new Color(MenuFx.AccentRed.r, MenuFx.AccentRed.g, MenuFx.AccentRed.b, 0.45f);
                echoTxt.fontStyle = FontStyles.Bold;
                echoTxt.alignment = TextAlignmentOptions.Center;
                echoTxt.characterSpacing = headerSpacing;
                echoTxt.raycastTarget = false;
                echoRT.SetAsFirstSibling();
            }

            var txt = go.AddComponent<TextMeshProUGUI>();
            if (themeFont != null) txt.font = themeFont;
            txt.text = headerText;
            txt.fontSize = headerFontSize;
            txt.color = MenuFx.SteelTint;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.characterSpacing = headerSpacing;
            txt.raycastTarget = false;
            txt.enableVertexGradient = true;
            txt.colorGradient = new VertexGradient(MenuFx.SteelTint, MenuFx.SteelTint, MenuFx.MutedSteel, MenuFx.MutedSteel);

            rt.SetAsLastSibling();
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
}
