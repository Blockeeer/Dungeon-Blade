using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonBlade.Bank
{
    public class PortalConfirmDialog : MonoBehaviour
    {
        public static PortalConfirmDialog Instance { get; private set; }

        static readonly Color AccentRed = new Color(0.78f, 0.20f, 0.18f, 1f);
        static readonly Color AccentRedHover = new Color(0.92f, 0.28f, 0.24f, 1f);
        static readonly Color CardBg = new Color(0.09f, 0.085f, 0.10f, 0.98f);
        static readonly Color HeaderBg = new Color(0.13f, 0.115f, 0.13f, 1f);
        static readonly Color DividerColor = new Color(0.85f, 0.25f, 0.18f, 0.55f);
        static readonly Color TitleColor = new Color(1f, 0.94f, 0.86f, 1f);
        static readonly Color BodyColor = new Color(0.86f, 0.83f, 0.78f, 1f);
        static readonly Color CancelBg = new Color(0.18f, 0.17f, 0.19f, 1f);
        static readonly Color CancelBgHover = new Color(0.26f, 0.25f, 0.27f, 1f);
        static readonly Color CancelBorder = new Color(1f, 1f, 1f, 0.18f);

        Canvas _canvas;
        CanvasGroup _group;
        RectTransform _cardRT;
        TMP_Text _titleLabel;
        TMP_Text _bodyLabel;
        TMP_Text _confirmLabel;
        TMP_Text _cancelLabel;
        Button _confirmBtn;
        Button _cancelBtn;
        Action _onConfirm;
        bool _open;
        Coroutine _animCo;

        public bool IsOpen => _open;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUI();
            HideImmediate();
        }

        public static void Show(string title, string body, string confirmText, string cancelText, Action onConfirm)
        {
            var inst = Instance;
            if (inst == null)
            {
                var go = new GameObject("PortalConfirmDialog");
                inst = go.AddComponent<PortalConfirmDialog>();
            }
            inst.ShowInternal(title, body, confirmText, cancelText, onConfirm);
        }

        void ShowInternal(string title, string body, string confirmText, string cancelText, Action onConfirm)
        {
            _onConfirm = onConfirm;
            if (_titleLabel != null) _titleLabel.text = title ?? string.Empty;
            if (_bodyLabel != null) _bodyLabel.text = body ?? string.Empty;
            if (_confirmLabel != null) _confirmLabel.text = string.IsNullOrEmpty(confirmText) ? "Proceed" : confirmText;
            if (_cancelLabel != null) _cancelLabel.text = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;
            _open = true;
            _canvas.gameObject.SetActive(true);
            _group.interactable = true;
            _group.blocksRaycasts = true;
            UnlockCursor();
            if (_animCo != null) StopCoroutine(_animCo);
            _animCo = StartCoroutine(AnimateIn());
            EventSystem.current?.SetSelectedGameObject(_confirmBtn.gameObject);
        }

        IEnumerator AnimateIn()
        {
            const float dur = 0.16f;
            float t = 0f;
            _group.alpha = 0f;
            var startScale = new Vector3(0.92f, 0.92f, 1f);
            _cardRT.localScale = startScale;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float e = 1f - (1f - k) * (1f - k);
                _group.alpha = e;
                _cardRT.localScale = Vector3.LerpUnclamped(startScale, Vector3.one, e);
                yield return null;
            }
            _group.alpha = 1f;
            _cardRT.localScale = Vector3.one;
            _animCo = null;
        }

        void HideImmediate()
        {
            _open = false;
            if (_animCo != null) { StopCoroutine(_animCo); _animCo = null; }
            if (_canvas != null) _canvas.gameObject.SetActive(false);
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
        }

        void Update()
        {
            if (!_open) return;
            if (Input.GetKeyDown(KeyCode.Escape)) OnCancelClicked();
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) OnConfirmClicked();
        }

        void OnConfirmClicked()
        {
            var cb = _onConfirm;
            _onConfirm = null;
            HideImmediate();
            cb?.Invoke();
        }

        void OnCancelClicked()
        {
            _onConfirm = null;
            HideImmediate();
        }

        void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void BuildUI()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 5000;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            _group = canvasGo.GetComponent<CanvasGroup>();

            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(es);
            }

            // Backdrop with click-to-cancel
            var dim = CreateChild(canvasGo.transform, "Dim");
            var dimImg = dim.AddComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.78f);
            StretchFull(dim.GetComponent<RectTransform>());
            var dimBtn = dim.AddComponent<Button>();
            var dimColors = dimBtn.colors;
            dimColors.highlightedColor = Color.white;
            dimColors.pressedColor = Color.white;
            dimColors.selectedColor = Color.white;
            dimBtn.colors = dimColors;
            dimBtn.transition = Selectable.Transition.None;
            dimBtn.onClick.AddListener(OnCancelClicked);

            // Drop shadow (sibling behind card)
            var shadow = CreateChild(canvasGo.transform, "Shadow");
            var shadowImg = shadow.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0.55f);
            shadowImg.raycastTarget = false;
            var shadowRT = shadow.GetComponent<RectTransform>();
            shadowRT.anchorMin = new Vector2(0.5f, 0.5f);
            shadowRT.anchorMax = new Vector2(0.5f, 0.5f);
            shadowRT.pivot = new Vector2(0.5f, 0.5f);
            shadowRT.sizeDelta = new Vector2(740f, 440f);
            shadowRT.anchoredPosition = new Vector2(0f, -10f);

            // Card root
            var card = CreateChild(canvasGo.transform, "Card");
            var cardImg = card.AddComponent<Image>();
            cardImg.color = CardBg;
            _cardRT = card.GetComponent<RectTransform>();
            _cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            _cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            _cardRT.pivot = new Vector2(0.5f, 0.5f);
            _cardRT.sizeDelta = new Vector2(720f, 420f);
            _cardRT.anchoredPosition = Vector2.zero;

            // Header strip
            var header = CreateChild(card.transform, "Header");
            var headerImg = header.AddComponent<Image>();
            headerImg.color = HeaderBg;
            headerImg.raycastTarget = false;
            var headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(0f, 96f);
            headerRT.anchoredPosition = Vector2.zero;

            // Accent stripe on the left edge of the header
            var accent = CreateChild(header.transform, "Accent");
            var accentImg = accent.AddComponent<Image>();
            accentImg.color = AccentRed;
            accentImg.raycastTarget = false;
            var accentRT = accent.GetComponent<RectTransform>();
            accentRT.anchorMin = new Vector2(0f, 0f);
            accentRT.anchorMax = new Vector2(0f, 1f);
            accentRT.pivot = new Vector2(0f, 0.5f);
            accentRT.sizeDelta = new Vector2(6f, 0f);
            accentRT.anchoredPosition = Vector2.zero;

            // Title sits inside the header, left-aligned
            _titleLabel = CreateText(header.transform, "Title", "Enter Dungeon?", 36, FontStyles.Bold,
                TextAlignmentOptions.Left, TitleColor);
            var titleRT = _titleLabel.rectTransform;
            titleRT.anchorMin = new Vector2(0f, 0f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.offsetMin = new Vector2(36f, 0f);
            titleRT.offsetMax = new Vector2(-32f, 0f);
            _titleLabel.characterSpacing = 2f;

            // Divider beneath the header
            var divider = CreateChild(card.transform, "Divider");
            var divImg = divider.AddComponent<Image>();
            divImg.color = DividerColor;
            divImg.raycastTarget = false;
            var divRT = divider.GetComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0f, 1f);
            divRT.anchorMax = new Vector2(1f, 1f);
            divRT.pivot = new Vector2(0.5f, 1f);
            divRT.sizeDelta = new Vector2(0f, 2f);
            divRT.anchoredPosition = new Vector2(0f, -96f);

            // Body text — sits between header and button row
            _bodyLabel = CreateText(card.transform, "Body",
                "This dungeon contains a powerful boss. Make sure you are prepared before proceeding.",
                22, FontStyles.Normal, TextAlignmentOptions.Center, BodyColor);
            var bodyRT = _bodyLabel.rectTransform;
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.pivot = new Vector2(0.5f, 0.5f);
            bodyRT.offsetMin = new Vector2(48f, 120f);
            bodyRT.offsetMax = new Vector2(-48f, -118f);
            _bodyLabel.enableWordWrapping = true;
            _bodyLabel.lineSpacing = 8f;

            // Cancel button (left, subdued outlined style)
            _cancelBtn = CreateButton(card.transform, "CancelBtn", "Cancel",
                CancelBg, CancelBgHover, CancelBorder, out _cancelLabel);
            var caRT = _cancelBtn.GetComponent<RectTransform>();
            caRT.anchorMin = new Vector2(0f, 0f);
            caRT.anchorMax = new Vector2(0f, 0f);
            caRT.pivot = new Vector2(0f, 0f);
            caRT.anchoredPosition = new Vector2(40f, 36f);
            caRT.sizeDelta = new Vector2(240f, 60f);
            _cancelBtn.onClick.AddListener(OnCancelClicked);

            // Confirm button (right, accent solid)
            _confirmBtn = CreateButton(card.transform, "ConfirmBtn", "Proceed",
                AccentRed, AccentRedHover, new Color(1f, 1f, 1f, 0.0f), out _confirmLabel);
            var cRT = _confirmBtn.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(1f, 0f);
            cRT.anchorMax = new Vector2(1f, 0f);
            cRT.pivot = new Vector2(1f, 0f);
            cRT.anchoredPosition = new Vector2(-40f, 36f);
            cRT.sizeDelta = new Vector2(240f, 60f);
            _confirmBtn.onClick.AddListener(OnConfirmClicked);
        }

        static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static TMP_Text CreateText(Transform parent, string name, string text, float size,
            FontStyles style, TextAlignmentOptions align, Color color)
        {
            var go = CreateChild(parent, name);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        static Button CreateButton(Transform parent, string name, string label,
            Color bg, Color hoverBg, Color borderColor, out TMP_Text labelOut)
        {
            var go = CreateChild(parent, name);
            var img = go.AddComponent<Image>();
            img.color = bg;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(hoverBg.r / Mathf.Max(0.001f, bg.r), hoverBg.g / Mathf.Max(0.001f, bg.g), hoverBg.b / Mathf.Max(0.001f, bg.b), 1f);
            // Simpler: use color multiplier approach — set highlighted as a brighter tint
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;

            // Optional outline (only visible if alpha > 0)
            if (borderColor.a > 0f)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = borderColor;
                outline.effectDistance = new Vector2(1f, -1f);
            }

            labelOut = CreateText(go.transform, "Label", label, 24, FontStyles.Bold,
                TextAlignmentOptions.Center, Color.white);
            labelOut.characterSpacing = 4f;
            var lrt = labelOut.rectTransform;
            StretchFull(lrt);
            return btn;
        }
    }
}
