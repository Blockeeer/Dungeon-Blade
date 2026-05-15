using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonBlade.Bank
{
    public class PortalConfirmDialog : MonoBehaviour
    {
        public static PortalConfirmDialog Instance { get; private set; }

        Canvas _canvas;
        CanvasGroup _group;
        TMP_Text _titleLabel;
        TMP_Text _bodyLabel;
        TMP_Text _confirmLabel;
        TMP_Text _cancelLabel;
        Button _confirmBtn;
        Button _cancelBtn;
        Action _onConfirm;
        bool _open;

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
            _group.alpha = 1f;
            _group.interactable = true;
            _group.blocksRaycasts = true;
            UnlockCursor();
        }

        void HideImmediate()
        {
            _open = false;
            if (_canvas != null) _canvas.gameObject.SetActive(false);
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
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

            var dim = CreateChild(canvasGo.transform, "Dim");
            var dimImg = dim.AddComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.72f);
            StretchFull(dim.GetComponent<RectTransform>());

            var card = CreateChild(canvasGo.transform, "Card");
            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.10f, 0.08f, 0.07f, 0.96f);
            var cardRT = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(720f, 420f);
            cardRT.anchoredPosition = Vector2.zero;

            var border = CreateChild(card.transform, "Border");
            var borderImg = border.AddComponent<Image>();
            borderImg.color = new Color(0.85f, 0.25f, 0.18f, 1f);
            var borderRT = border.GetComponent<RectTransform>();
            StretchFull(borderRT);
            borderRT.offsetMin = new Vector2(-4f, -4f);
            borderRT.offsetMax = new Vector2(4f, 4f);
            border.transform.SetAsFirstSibling();

            _titleLabel = CreateText(card.transform, "Title", "Enter Dungeon?", 42, FontStyles.Bold,
                TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.85f, 1f));
            var titleRT = _titleLabel.rectTransform;
            titleRT.anchorMin = new Vector2(0f, 1f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -32f);
            titleRT.sizeDelta = new Vector2(-60f, 70f);

            _bodyLabel = CreateText(card.transform, "Body",
                "This dungeon contains a powerful boss. Make sure you are prepared.", 24, FontStyles.Normal,
                TextAlignmentOptions.Center, new Color(0.92f, 0.88f, 0.82f, 1f));
            var bodyRT = _bodyLabel.rectTransform;
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.pivot = new Vector2(0.5f, 0.5f);
            bodyRT.offsetMin = new Vector2(40f, 110f);
            bodyRT.offsetMax = new Vector2(-40f, -120f);
            _bodyLabel.enableWordWrapping = true;

            _confirmBtn = CreateButton(card.transform, "ConfirmBtn", "Proceed",
                new Color(0.78f, 0.18f, 0.14f, 1f), new Vector2(180f, 32f), out _confirmLabel);
            var cRT = _confirmBtn.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(1f, 0f);
            cRT.anchorMax = new Vector2(1f, 0f);
            cRT.pivot = new Vector2(1f, 0f);
            cRT.anchoredPosition = new Vector2(-40f, 32f);
            cRT.sizeDelta = new Vector2(240f, 64f);
            _confirmBtn.onClick.AddListener(OnConfirmClicked);

            _cancelBtn = CreateButton(card.transform, "CancelBtn", "Cancel",
                new Color(0.20f, 0.20f, 0.22f, 1f), new Vector2(-180f, 32f), out _cancelLabel);
            var caRT = _cancelBtn.GetComponent<RectTransform>();
            caRT.anchorMin = new Vector2(0f, 0f);
            caRT.anchorMax = new Vector2(0f, 0f);
            caRT.pivot = new Vector2(0f, 0f);
            caRT.anchoredPosition = new Vector2(40f, 32f);
            caRT.sizeDelta = new Vector2(240f, 64f);
            _cancelBtn.onClick.AddListener(OnCancelClicked);
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

        static Button CreateButton(Transform parent, string name, string label, Color bg, Vector2 _, out TMP_Text labelOut)
        {
            var go = CreateChild(parent, name);
            var img = go.AddComponent<Image>();
            img.color = bg;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = colors.highlightedColor;
            btn.colors = colors;

            labelOut = CreateText(go.transform, "Label", label, 26, FontStyles.Bold,
                TextAlignmentOptions.Center, Color.white);
            var lrt = labelOut.rectTransform;
            StretchFull(lrt);
            return btn;
        }
    }
}
