using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.UI.Menus
{
    public class CharacterSelectController
    {
        [Serializable]
        public class HeroModelEntry
        {
            public string id;
            public GameObject prefab;
            public AnimationClip previewClip;
        }

        public class Character
        {
            public string id;
            public string name;
            public string role;
            public Color accent;
            public string lore;
            public int str, agi, fin, arc;
            public Sprite portrait;
            public Sprite headSprite;
        }

        class CoroutineRunner : MonoBehaviour { }

        public static readonly Character[] Roster =
        {
            new Character {
                id = "lyra", name = "LYRA", role = "PHANTOM  RAZOR",
                accent = new Color(0.42f, 0.22f, 0.58f),
                lore = "Few have seen Lyra and lived to describe her. The Brotherhood took her in as a child and trained her in the silence between heartbeats.\n\nShe moves through the dungeon's deepest shadows with twin razors and a hood that has never been pulled back in daylight. The dead, if they could speak, would call her by no name at all.",
                str = 5, agi = 9, fin = 10, arc = 4
            },
            new Character {
                id = "kaelen", name = "KAELEN", role = "SILENT  AVENGER",
                accent = new Color(0.45f, 0.30f, 0.62f),
                lore = "Once a sworn knight of the Crimson Vale, Kaelen renounced his oaths after watching his order betray its own. He walks the dungeon hooded and masked — a single sword in one hand, a pistol in the other.\n\nHis trainers said he could vanish between heartbeats. The dead would agree, if any could speak.",
                str = 6, agi = 8, fin = 9, arc = 5
            },
            new Character {
                id = "mira", name = "MIRA", role = "OUTLAW  SLINGER",
                accent = new Color(0.78f, 0.18f, 0.30f),
                lore = "Mira walked out of the Lower Quarters with a stolen revolver and a grin, and she has not stopped moving since. The dungeon, she says, is just another back alley with bigger rats.\n\nShe has always known how to handle rats. The choker at her throat is a souvenir; she doesn't say from whom.",
                str = 5, agi = 8, fin = 9, arc = 4
            },
            new Character {
                id = "wayne", name = "WAYNE", role = "FREE  STRIDER",
                accent = new Color(0.86f, 0.56f, 0.20f),
                lore = "Wayne walks the Fringe, the wasteland between kingdoms, taking work where coin is offered and asking few questions. He came to the dungeon for the bounty and stayed for the duels.\n\nA sabre at his hip, a revolver in his hand, and the same easy half-smile for every challenger he meets.",
                str = 7, agi = 7, fin = 7, arc = 6
            },
            new Character {
                id = "aurelia", name = "AURELIA", role = "DAWN  CAPTAIN",
                accent = new Color(0.92f, 0.80f, 0.35f),
                lore = "Captain Aurelia of the Eighth Order serves the throne as much as the throne serves her. She enters the dungeon under royal seal, sword and pistol drawn, on a hunt for a relic the Crown will not name.\n\nWhatever she finds down there, she has sworn to bring back. Or to bury, if the Crown gives that order instead.",
                str = 8, agi = 6, fin = 7, arc = 7
            },
            new Character {
                id = "varion", name = "VARION", role = "CRIMSON  REAVER",
                accent = new Color(0.72f, 0.12f, 0.18f),
                lore = "Varion lost his eye at the Siege of Black Brook and his name a year later. He wears the red-lined coat of a banner he no longer serves, and carries a longsword too heavy for any but him.\n\nThe dungeon's deepest dwellers know him not by sight, but by the silence that follows wherever he passes.",
                str = 10, agi = 4, fin = 7, arc = 6
            },
        };

        GameObject _root;
        int _selected;
        Image[] _cardBgs;
        Image[] _cardSelOutline;
        Image _previewBadge;
        Image _previewPortrait;
        RawImage _previewModelImage;
        TMP_Text _previewLetter;
        TMP_Text _previewName;
        TMP_Text _previewRole;
        TMP_Text _loreText;
        TMP_Text[] _statValues = new TMP_Text[4];
        Image[] _statBars = new Image[4];
        TMP_FontAsset _font;

        // Loading overlay
        CanvasGroup _loadOverlayGroup;
        Image _loadOverlayBg;
        Image _loadScanBar;
        Image _loadScanGlow;
        TMP_Text _loadingText;

        Hero3DPreview _hero3D;
        Dictionary<string, HeroModelEntry> _heroEntries;
        AnimationClip _idleClip;
        Avatar _heroAvatar;
        CoroutineRunner _runner;
        Coroutine _loadCo;

        Action<string> _onConfirm;
        Action _onCancel;

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Open(Canvas canvas, TMP_FontAsset font, Action<string> onConfirm, Action onCancel)
        {
            Open(canvas, font, null, null, null, onConfirm, onCancel);
        }

        public void Open(Canvas canvas, TMP_FontAsset font,
                         HeroModelEntry[] heroModels, AnimationClip idleClip,
                         Action<string> onConfirm, Action onCancel)
        {
            Open(canvas, font, heroModels, idleClip, null, onConfirm, onCancel);
        }

        public void Open(Canvas canvas, TMP_FontAsset font,
                         HeroModelEntry[] heroModels, AnimationClip idleClip, Avatar heroAvatar,
                         Action<string> onConfirm, Action onCancel)
        {
            if (canvas == null) return;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _font = font;

            _heroEntries = new Dictionary<string, HeroModelEntry>();
            if (heroModels != null)
            {
                foreach (var entry in heroModels)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.id) || entry.prefab == null) continue;
                    _heroEntries[entry.id] = entry;
                }
            }
            _idleClip = idleClip;
            _heroAvatar = heroAvatar;

            LoadPortraits();

            if (_hero3D == null && _heroEntries.Count > 0)
                _hero3D = Hero3DPreview.Create();

            if (_root == null) Build(canvas);
            _root.SetActive(true);
            _root.transform.SetAsLastSibling();
            SelectIndex(0);
        }

        static bool _portraitsLoaded;
        static void LoadPortraits()
        {
            if (_portraitsLoaded) return;
            _portraitsLoaded = true;
            foreach (var c in Roster)
            {
                if (c.portrait != null) continue;
                var full = Resources.Load<Sprite>("Characters/" + c.id);
                if (full == null) continue;
                c.portrait = full;

                if (full.texture != null)
                {
                    var tex = full.texture;
                    const float headFracY = 0.30f;
                    const float xMargin = 0.18f;
                    float w = tex.width * (1f - 2f * xMargin);
                    float h = tex.height * headFracY;
                    float x = tex.width * xMargin;
                    float y = tex.height - h;
                    c.headSprite = Sprite.Create(tex, new Rect(x, y, w, h), new Vector2(0.5f, 0.5f), 100f);
                }
            }
        }

        public void Close()
        {
            if (_runner != null && _loadCo != null)
            {
                _runner.StopCoroutine(_loadCo);
                _loadCo = null;
            }
            if (_hero3D != null)
            {
                _hero3D.Hide();
                UnityEngine.Object.Destroy(_hero3D.gameObject);
                _hero3D = null;
            }
            if (_root != null) _root.SetActive(false);
        }

        // ---------- Build ----------

        void Build(Canvas canvas)
        {
            var canvasRT = canvas.transform as RectTransform;

            _root = new GameObject("CharacterSelectRoot",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) _root.layer = uiLayer;
            var rootRT = (RectTransform)_root.transform;
            rootRT.SetParent(canvasRT, false);
            MenuFx.StretchFull(rootRT);

            var bgImg = _root.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.78f);
            bgImg.raycastTarget = true;

            BuildBackdropLayers(rootRT);
            BuildHeader(rootRT);
            BuildCardColumn(rootRT);
            BuildPreview(rootRT);
            BuildLorePanel(rootRT);
            BuildBottomBar(rootRT);

            _runner = _root.GetComponent<CoroutineRunner>();
            if (_runner == null) _runner = _root.AddComponent<CoroutineRunner>();
        }

        void BuildBackdropLayers(RectTransform parent)
        {
            var glow = MenuFx.CreateImage(parent, "RedGlow", parent.childCount);
            glow.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            glow.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            glow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            glow.rectTransform.anchoredPosition = new Vector2(0f, -200f);
            glow.rectTransform.sizeDelta = new Vector2(2200f, 1100f);
            glow.sprite = MenuFx.RedGlowSprite();
            glow.color = new Color(1f, 1f, 1f, 0.30f);
            glow.raycastTarget = false;

            var scan = MenuFx.CreateImage(parent, "Scanlines", parent.childCount);
            MenuFx.StretchFull(scan.rectTransform);
            scan.sprite = MenuFx.ScanlineSprite();
            scan.type = Image.Type.Tiled;
            scan.color = new Color(0f, 0f, 0f, 0.14f);
            scan.raycastTarget = false;

            var vig = MenuFx.CreateImage(parent, "Vignette", parent.childCount);
            MenuFx.StretchFull(vig.rectTransform);
            vig.sprite = MenuFx.VignetteSprite();
            vig.color = new Color(0f, 0f, 0f, 0.7f);
            vig.raycastTarget = false;
        }

        void BuildHeader(RectTransform parent)
        {
            MenuFx.AddText(parent, "Eyebrow", _font,
                new Vector2(0f, 440f), new Vector2(800f, 24f),
                "//  CHOOSE  YOUR  PATH",
                fontSize: 16,
                color: new Color(MenuFx.AccentRed.r, MenuFx.AccentRed.g, MenuFx.AccentRed.b, 0.95f),
                spacing: 22f, style: FontStyles.Bold);

            MenuFx.AddText(parent, "Header", _font,
                new Vector2(0f, 400f), new Vector2(1400f, 80f),
                "SELECT  HERO",
                fontSize: 56,
                color: MenuFx.SteelTint,
                spacing: 22f, style: FontStyles.Bold);

            var bar = MenuFx.CreateImage(parent, "HeaderBar", parent.childCount);
            bar.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            bar.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            bar.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            bar.rectTransform.anchoredPosition = new Vector2(0f, 348f);
            bar.rectTransform.sizeDelta = new Vector2(540f, 2f);
            bar.color = MenuFx.AccentRed;
            bar.raycastTarget = false;
        }

        void BuildCardColumn(RectTransform parent)
        {
            int n = Roster.Length;
            _cardBgs = new Image[n];
            _cardSelOutline = new Image[n];

            float colX = -650f;
            float topY = 230f;
            const float cardW = 380f;
            const float cardH = 96f;
            const float gap = 10f;

            for (int i = 0; i < n; i++)
            {
                var c = Roster[i];

                var cardGO = new GameObject("Card_" + c.id,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Button));
                int uiLayer = LayerMask.NameToLayer("UI");
                if (uiLayer >= 0) cardGO.layer = uiLayer;
                cardGO.transform.SetParent(parent, false);

                var cardRT = (RectTransform)cardGO.transform;
                cardRT.anchorMin = new Vector2(0.5f, 0.5f);
                cardRT.anchorMax = new Vector2(0.5f, 0.5f);
                cardRT.pivot = new Vector2(0.5f, 1f);
                cardRT.anchoredPosition = new Vector2(colX, topY - i * (cardH + gap));
                cardRT.sizeDelta = new Vector2(cardW, cardH);

                // BG: child of card. Holds the Mask so gradient/text get clipped to rounded shape.
                var bgGO = new GameObject("CardBG",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                if (uiLayer >= 0) bgGO.layer = uiLayer;
                bgGO.transform.SetParent(cardRT, false);
                var bgRT = (RectTransform)bgGO.transform;
                MenuFx.StretchFull(bgRT);

                var bg = bgGO.GetComponent<Image>();
                bg.sprite = MenuFx.RoundedSlabSprite();
                bg.type = Image.Type.Sliced;
                bg.pixelsPerUnitMultiplier = 1f;
                bg.color = MultColor(c.accent, 0.30f, 0.95f);
                bg.raycastTarget = true;
                _cardBgs[i] = bg;

                bgGO.AddComponent<Mask>().showMaskGraphic = true;

                var gradient = MenuFx.CreateImage(bgRT, "Gradient", bgRT.childCount);
                gradient.rectTransform.anchorMin = Vector2.zero;
                gradient.rectTransform.anchorMax = Vector2.one;
                gradient.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                gradient.rectTransform.offsetMin = Vector2.zero;
                gradient.rectTransform.offsetMax = Vector2.zero;
                gradient.sprite = MenuFx.HorizontalGradientSprite();
                gradient.color = MultColor(c.accent, 1.20f, 1.0f);
                gradient.raycastTarget = false;

                var btn = cardGO.GetComponent<Button>();
                btn.targetGraphic = bg;
                var colors = btn.colors;
                colors.normalColor = MultColor(c.accent, 0.30f, 0.95f);
                colors.highlightedColor = MultColor(c.accent, 0.55f, 1.00f);
                colors.pressedColor = MultColor(c.accent, 0.20f, 1.00f);
                colors.selectedColor = MultColor(c.accent, 0.55f, 1.00f);
                colors.disabledColor = MultColor(c.accent, 0.18f, 0.65f);
                colors.fadeDuration = 0.10f;
                btn.colors = colors;
                int captured = i;
                btn.onClick.AddListener(() => SelectIndex(captured));

                var stripe = MenuFx.CreateImage(bgRT, "Stripe", bgRT.childCount);
                stripe.rectTransform.anchorMin = new Vector2(0f, 0f);
                stripe.rectTransform.anchorMax = new Vector2(0f, 1f);
                stripe.rectTransform.pivot = new Vector2(0f, 0.5f);
                stripe.rectTransform.anchoredPosition = Vector2.zero;
                stripe.rectTransform.sizeDelta = new Vector2(4f, 0f);
                stripe.color = c.accent;
                stripe.raycastTarget = false;

                MenuFx.AddText(bgRT, "Name", _font,
                    new Vector2(0f, 18f), new Vector2(cardW - 120f, 32f),
                    c.name, fontSize: 22,
                    color: MenuFx.SteelTint, spacing: 10f, style: FontStyles.Bold)
                    .alignment = TextAlignmentOptions.Left;

                MenuFx.AddText(bgRT, "Role", _font,
                    new Vector2(0f, -16f), new Vector2(cardW - 120f, 22f),
                    c.role, fontSize: 13,
                    color: new Color(MenuFx.SteelTint.r, MenuFx.SteelTint.g, MenuFx.SteelTint.b, 0.78f),
                    spacing: 14f, style: FontStyles.Bold)
                    .alignment = TextAlignmentOptions.Left;

                FixLabelInsets(bgRT, "Name", left: 24f, right: 150f);
                FixLabelInsets(bgRT, "Role", left: 24f, right: 150f);

                var outline = MenuFx.CreateImage(bgRT, "SelOutline", 1);
                MenuFx.StretchFull(outline.rectTransform);
                outline.color = new Color(1f, 1f, 1f, 0f);
                outline.raycastTarget = false;
                _cardSelOutline[i] = outline;

                // Portrait: child of cardRT (NOT bgRT) so head can poke above the rounded card.
                BuildCardAvatar(cardRT, c, cardW, cardH);

                // Border: rounded outline on top of everything, also outside the mask.
                var border = MenuFx.CreateImage(cardRT, "Border", cardRT.childCount);
                MenuFx.StretchFull(border.rectTransform);
                border.sprite = MenuFx.RoundedBorderSprite();
                border.type = Image.Type.Sliced;
                border.pixelsPerUnitMultiplier = 1f;
                border.color = new Color(1f, 1f, 1f, 0.20f);
                border.raycastTarget = false;
            }
        }

        void BuildCardAvatar(RectTransform cardRT, Character c, float cardW, float cardH)
        {
            if (c.headSprite != null)
            {
                var portrait = MenuFx.CreateImage(cardRT, "Portrait", cardRT.childCount);
                portrait.rectTransform.anchorMin = new Vector2(1f, 0.5f);
                portrait.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                portrait.rectTransform.pivot = new Vector2(1f, 0.5f);
                portrait.rectTransform.anchoredPosition = new Vector2(-4f, 8f);
                portrait.rectTransform.sizeDelta = new Vector2(214f, 100f);
                portrait.preserveAspect = true;
                portrait.sprite = c.headSprite;
                portrait.color = Color.white;
                portrait.raycastTarget = false;
            }
            else
            {
                var initial = MenuFx.AddText(cardRT, "Initial", _font,
                    new Vector2(-cardW * 0.5f + 70f, 0f), new Vector2(80f, cardH),
                    c.name.Substring(0, 1),
                    fontSize: 56,
                    color: new Color(0.05f, 0.06f, 0.08f, 0.95f),
                    spacing: 0f, style: FontStyles.Bold);
                initial.alignment = TextAlignmentOptions.Center;
                initial.rectTransform.anchorMin = new Vector2(1f, 0.5f);
                initial.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                initial.rectTransform.pivot = new Vector2(1f, 0.5f);
                initial.rectTransform.anchoredPosition = new Vector2(-30f, 0f);
            }
        }

        void BuildPreview(RectTransform parent)
        {
            var col = new GameObject("PreviewColumn", typeof(RectTransform));
            col.transform.SetParent(parent, false);
            var colRT = (RectTransform)col.transform;
            colRT.anchorMin = new Vector2(0.5f, 0.5f);
            colRT.anchorMax = new Vector2(0.5f, 0.5f);
            colRT.pivot = new Vector2(0.5f, 0.5f);
            colRT.anchoredPosition = new Vector2(-100f, 0f);
            colRT.sizeDelta = new Vector2(540f, 760f);

            _previewBadge = MenuFx.CreateImage(colRT, "BigBadge", colRT.childCount);
            _previewBadge.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _previewBadge.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _previewBadge.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _previewBadge.rectTransform.anchoredPosition = new Vector2(0f, 110f);
            _previewBadge.rectTransform.sizeDelta = new Vector2(320f, 440f);
            _previewBadge.sprite = MenuFx.SlabSprite();
            _previewBadge.color = Color.white;
            _previewBadge.raycastTarget = false;

            _previewLetter = MenuFx.AddText(colRT, "BigLetter", _font,
                new Vector2(0f, 110f), new Vector2(320f, 440f),
                "L", fontSize: 220,
                color: new Color(0.05f, 0.06f, 0.08f, 0.9f),
                spacing: 0f, style: FontStyles.Bold);
            _previewLetter.alignment = TextAlignmentOptions.Center;

            _previewPortrait = MenuFx.CreateImage(colRT, "BigPortrait", colRT.childCount);
            _previewPortrait.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _previewPortrait.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _previewPortrait.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _previewPortrait.rectTransform.anchoredPosition = new Vector2(0f, 110f);
            _previewPortrait.rectTransform.sizeDelta = new Vector2(320f, 440f);
            _previewPortrait.preserveAspect = true;
            _previewPortrait.color = Color.white;
            _previewPortrait.raycastTarget = false;
            _previewPortrait.gameObject.SetActive(false);

            BuildModelFrame(colRT);
            BuildLoadingOverlay(colRT);

            _previewName = MenuFx.AddText(colRT, "PreviewName", _font,
                new Vector2(0f, -160f), new Vector2(540f, 70f),
                "LYRA", fontSize: 56,
                color: MenuFx.SteelTint, spacing: 22f, style: FontStyles.Bold);

            _previewRole = MenuFx.AddText(colRT, "PreviewRole", _font,
                new Vector2(0f, -210f), new Vector2(540f, 28f),
                "PHANTOM  RAZOR", fontSize: 18,
                color: MenuFx.AccentRed, spacing: 22f, style: FontStyles.Bold);

            BuildStats(colRT);
        }

        void BuildStats(RectTransform parent)
        {
            string[] labels = { "STR", "AGI", "FIN", "ARC" };
            float startY = -260f;
            float rowH = 26f;
            float barW = 360f;

            for (int i = 0; i < 4; i++)
            {
                MenuFx.AddText(parent, labels[i] + "Label", _font,
                    new Vector2(-200f, startY - i * rowH),
                    new Vector2(60f, 24f),
                    labels[i], fontSize: 14,
                    color: new Color(MenuFx.MutedSteel.r, MenuFx.MutedSteel.g, MenuFx.MutedSteel.b, 0.8f),
                    spacing: 8f, style: FontStyles.Bold)
                    .alignment = TextAlignmentOptions.Left;

                var trough = MenuFx.CreateImage(parent, labels[i] + "Trough", parent.childCount);
                trough.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                trough.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                trough.rectTransform.pivot = new Vector2(0f, 0.5f);
                trough.rectTransform.anchoredPosition = new Vector2(-130f, startY - i * rowH);
                trough.rectTransform.sizeDelta = new Vector2(barW, 6f);
                trough.color = new Color(0.10f, 0.13f, 0.18f, 0.9f);
                trough.raycastTarget = false;

                var fill = MenuFx.CreateImage(parent, labels[i] + "Fill", parent.childCount);
                fill.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                fill.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                fill.rectTransform.pivot = new Vector2(0f, 0.5f);
                fill.rectTransform.anchoredPosition = new Vector2(-130f, startY - i * rowH);
                fill.rectTransform.sizeDelta = new Vector2(barW * 0.5f, 6f);
                fill.color = MenuFx.AccentRed;
                fill.raycastTarget = false;
                _statBars[i] = fill;

                _statValues[i] = MenuFx.AddText(parent, labels[i] + "Value", _font,
                    new Vector2(260f, startY - i * rowH),
                    new Vector2(40f, 24f),
                    "5", fontSize: 14,
                    color: MenuFx.SteelTint, spacing: 0f, style: FontStyles.Bold);
                _statValues[i].alignment = TextAlignmentOptions.Right;
            }
        }

        void BuildModelFrame(RectTransform colRT)
        {
            var modelGO = new GameObject("HeroModel",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) modelGO.layer = uiLayer;
            modelGO.transform.SetParent(colRT, false);

            var rt = (RectTransform)modelGO.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 110f);
            rt.sizeDelta = new Vector2(320f, 440f);

            _previewModelImage = modelGO.GetComponent<RawImage>();
            _previewModelImage.color = Color.white;
            _previewModelImage.raycastTarget = false;
            if (_hero3D != null) _previewModelImage.texture = _hero3D.RT;
            _previewModelImage.gameObject.SetActive(_hero3D != null);

            var border = MenuFx.CreateImage(colRT, "PreviewBorder", colRT.childCount);
            border.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            border.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            border.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            border.rectTransform.anchoredPosition = new Vector2(0f, 110f);
            border.rectTransform.sizeDelta = new Vector2(324f, 444f);
            border.sprite = MenuFx.RoundedBorderSprite();
            border.type = Image.Type.Sliced;
            border.pixelsPerUnitMultiplier = 1f;
            border.color = new Color(1f, 1f, 1f, 0.18f);
            border.raycastTarget = false;
        }

        void BuildLoadingOverlay(RectTransform colRT)
        {
            var overlayGO = new GameObject("LoadingOverlay",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(CanvasGroup));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) overlayGO.layer = uiLayer;
            overlayGO.transform.SetParent(colRT, false);

            var rt = (RectTransform)overlayGO.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 110f);
            rt.sizeDelta = new Vector2(320f, 440f);

            _loadOverlayGroup = overlayGO.GetComponent<CanvasGroup>();
            _loadOverlayGroup.alpha = 0f;
            _loadOverlayGroup.blocksRaycasts = false;
            _loadOverlayGroup.interactable = false;

            _loadOverlayBg = MenuFx.CreateImage(rt, "Bg", rt.childCount);
            MenuFx.StretchFull(_loadOverlayBg.rectTransform);
            _loadOverlayBg.color = new Color(0.02f, 0.025f, 0.04f, 0.92f);
            _loadOverlayBg.raycastTarget = false;

            var scan = MenuFx.CreateImage(rt, "Scanlines", rt.childCount);
            MenuFx.StretchFull(scan.rectTransform);
            scan.sprite = MenuFx.ScanlineSprite();
            scan.type = Image.Type.Tiled;
            scan.color = new Color(0f, 0f, 0f, 0.5f);
            scan.pixelsPerUnitMultiplier = 1f;
            scan.raycastTarget = false;

            _loadScanGlow = MenuFx.CreateImage(rt, "ScanGlow", rt.childCount);
            _loadScanGlow.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            _loadScanGlow.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            _loadScanGlow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _loadScanGlow.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            _loadScanGlow.rectTransform.sizeDelta = new Vector2(0f, 80f);
            _loadScanGlow.color = new Color(MenuFx.AccentRed.r, MenuFx.AccentRed.g, MenuFx.AccentRed.b, 0.18f);
            _loadScanGlow.raycastTarget = false;

            _loadScanBar = MenuFx.CreateImage(rt, "ScanBar", rt.childCount);
            _loadScanBar.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            _loadScanBar.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            _loadScanBar.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _loadScanBar.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            _loadScanBar.rectTransform.sizeDelta = new Vector2(0f, 2f);
            _loadScanBar.color = new Color(1f, 1f, 1f, 0.95f);
            _loadScanBar.raycastTarget = false;

            _loadingText = MenuFx.AddText(rt, "LoadingText", _font,
                new Vector2(0f, -180f), new Vector2(300f, 24f),
                "// LOADING", fontSize: 14,
                color: new Color(MenuFx.AccentRed.r, MenuFx.AccentRed.g, MenuFx.AccentRed.b, 0.92f),
                spacing: 16f, style: FontStyles.Bold);
            _loadingText.alignment = TextAlignmentOptions.Center;
        }

        void PlayLoadingEffect(Character c)
        {
            if (_runner == null) return;
            if (_loadCo != null) _runner.StopCoroutine(_loadCo);
            _loadCo = _runner.StartCoroutine(LoadingEffectCo(c));
        }

        IEnumerator LoadingEffectCo(Character c)
        {
            const float halfH = 220f;
            const float sweepDur = 0.45f;
            const float holdDur = 0.06f;
            const float fadeDur = 0.20f;

            if (_loadOverlayGroup != null) _loadOverlayGroup.alpha = 1f;
            if (_loadScanBar != null) _loadScanBar.rectTransform.anchoredPosition = new Vector2(0f, halfH);
            if (_loadScanGlow != null) _loadScanGlow.rectTransform.anchoredPosition = new Vector2(0f, halfH);
            if (_loadingText != null) _loadingText.text = "// LOADING";

            yield return null;

            if (_hero3D != null && _heroEntries.TryGetValue(c.id, out var entry))
            {
                var clip = entry.previewClip != null ? entry.previewClip : _idleClip;
                _hero3D.Show(c.id, entry.prefab, clip, _heroAvatar, c.accent);
                if (_previewModelImage != null)
                {
                    _previewModelImage.gameObject.SetActive(true);
                    _previewModelImage.texture = _hero3D.RT;
                }
            }

            float t = 0f;
            while (t < sweepDur)
            {
                float u = Mathf.Clamp01(t / sweepDur);
                float y = Mathf.Lerp(halfH, -halfH, u);
                if (_loadScanBar != null) _loadScanBar.rectTransform.anchoredPosition = new Vector2(0f, y);
                if (_loadScanGlow != null) _loadScanGlow.rectTransform.anchoredPosition = new Vector2(0f, y);

                int dots = Mathf.Clamp(Mathf.FloorToInt(u * 16f), 0, 16);
                if (_loadingText != null)
                    _loadingText.text = "// LOADING  " + new string('|', dots) + new string('.', 16 - dots);

                if (_loadOverlayBg != null)
                {
                    bool flicker = (Mathf.FloorToInt(t * 60f) % 7) == 0;
                    var col = _loadOverlayBg.color;
                    col.a = flicker ? 0.74f : 0.92f;
                    _loadOverlayBg.color = col;
                }

                t += Time.unscaledDeltaTime;
                yield return null;
            }

            if (_loadingText != null) _loadingText.text = "// READY";
            if (_loadOverlayBg != null)
            {
                var col = _loadOverlayBg.color;
                col.a = 0.92f;
                _loadOverlayBg.color = col;
            }

            yield return new WaitForSecondsRealtime(holdDur);

            float f = 0f;
            while (f < fadeDur)
            {
                float u = Mathf.Clamp01(f / fadeDur);
                if (_loadOverlayGroup != null) _loadOverlayGroup.alpha = 1f - u;
                f += Time.unscaledDeltaTime;
                yield return null;
            }
            if (_loadOverlayGroup != null) _loadOverlayGroup.alpha = 0f;
            _loadCo = null;
        }

        void BuildLorePanel(RectTransform parent)
        {
            var panelGO = new GameObject("LorePanel",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) panelGO.layer = uiLayer;
            panelGO.transform.SetParent(parent, false);
            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = new Vector2(640f, 30f);
            panelRT.sizeDelta = new Vector2(540f, 600f);

            var panelImg = panelGO.GetComponent<Image>();
            panelImg.sprite = MenuFx.SlabSprite();
            panelImg.color = new Color(0.06f, 0.075f, 0.105f, 0.78f);
            panelImg.raycastTarget = false;

            var stripe = MenuFx.CreateImage(panelRT, "Stripe", panelRT.childCount);
            stripe.rectTransform.anchorMin = new Vector2(0f, 0f);
            stripe.rectTransform.anchorMax = new Vector2(0f, 1f);
            stripe.rectTransform.pivot = new Vector2(0f, 0.5f);
            stripe.rectTransform.anchoredPosition = Vector2.zero;
            stripe.rectTransform.sizeDelta = new Vector2(4f, 0f);
            stripe.color = MenuFx.AccentRed;
            stripe.raycastTarget = false;

            MenuFx.AddText(panelRT, "LoreHeader", _font,
                new Vector2(20f, 260f), new Vector2(480f, 28f),
                "//  BACKGROUND", fontSize: 14,
                color: new Color(MenuFx.AccentRed.r, MenuFx.AccentRed.g, MenuFx.AccentRed.b, 0.95f),
                spacing: 18f, style: FontStyles.Bold)
                .alignment = TextAlignmentOptions.Left;

            _loreText = MenuFx.AddText(panelRT, "LoreBody", _font,
                new Vector2(20f, 0f), new Vector2(480f, 460f),
                "", fontSize: 16,
                color: new Color(MenuFx.SteelTint.r, MenuFx.SteelTint.g, MenuFx.SteelTint.b, 0.92f),
                spacing: 4f, style: FontStyles.Normal);
            _loreText.alignment = TextAlignmentOptions.TopLeft;
            _loreText.enableWordWrapping = true;
            _loreText.lineSpacing = 4f;
        }

        void BuildBottomBar(RectTransform parent)
        {
            BuildActionButton(parent, "BackButton", "BACK",
                new Vector2(-220f, 80f), 280f,
                normal: new Color(0.10f, 0.13f, 0.18f, 0.75f),
                hover: new Color(0.20f, 0.24f, 0.32f, 0.95f),
                press: new Color(0.06f, 0.08f, 0.12f, 1f),
                () =>
                {
                    Close();
                    _onCancel?.Invoke();
                });

            BuildActionButton(parent, "ConfirmButton", "BEGIN  ADVENTURE",
                new Vector2(220f, 80f), 360f,
                normal: new Color(MenuFx.AccentRed.r * 0.55f, MenuFx.AccentRed.g * 0.16f, MenuFx.AccentRed.b * 0.16f, 0.92f),
                hover: MenuFx.AccentRed,
                press: new Color(MenuFx.AccentRed.r * 0.45f, MenuFx.AccentRed.g * 0.10f, MenuFx.AccentRed.b * 0.10f, 1f),
                () =>
                {
                    var id = Roster[_selected].id;
                    Close();
                    _onConfirm?.Invoke(id);
                });
        }

        void BuildActionButton(RectTransform parent, string name, string label,
                               Vector2 anchoredPos, float width,
                               Color normal, Color hover, Color press, Action onClick)
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) go.layer = uiLayer;
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(width, 56f);

            var img = go.GetComponent<Image>();
            img.sprite = MenuFx.SlabSprite();
            img.type = Image.Type.Simple;
            img.color = Color.white;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = normal;
            colors.highlightedColor = hover;
            colors.pressedColor = press;
            colors.selectedColor = hover;
            colors.fadeDuration = 0.10f;
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var labelTmp = MenuFx.AddText(rt, "Label", _font,
                Vector2.zero, new Vector2(width, 56f),
                label, fontSize: 22,
                color: MenuFx.SteelTint, spacing: 18f, style: FontStyles.Bold);
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.enableWordWrapping = false;
            labelTmp.overflowMode = TextOverflowModes.Overflow;
            labelTmp.raycastTarget = false;
        }

        // ---------- Selection ----------

        void SelectIndex(int i)
        {
            if (i < 0 || i >= Roster.Length) return;
            _selected = i;
            var c = Roster[i];

            for (int k = 0; k < _cardSelOutline.Length; k++)
            {
                bool sel = k == i;
                _cardSelOutline[k].color = new Color(1f, 1f, 1f, sel ? 0.18f : 0f);
                var cardRT = _cardBgs[k].rectTransform.parent as RectTransform;
                if (cardRT != null)
                    cardRT.localScale = sel ? new Vector3(1.03f, 1.03f, 1f) : Vector3.one;
            }

            _previewBadge.color = c.accent;
            _previewLetter.text = c.name.Substring(0, 1);
            _previewName.text = c.name;
            _previewRole.text = c.role;
            _loreText.text = c.lore;

            bool has3D = _hero3D != null && _heroEntries != null && _heroEntries.ContainsKey(c.id);

            if (_previewPortrait != null)
            {
                if (has3D)
                {
                    _previewPortrait.gameObject.SetActive(false);
                    _previewLetter.gameObject.SetActive(false);
                    _previewBadge.color = MultColor(c.accent, 0.18f, 0.85f);
                }
                else if (c.portrait != null)
                {
                    _previewPortrait.sprite = c.portrait;
                    _previewPortrait.gameObject.SetActive(true);
                    _previewLetter.gameObject.SetActive(false);
                    _previewBadge.color = MultColor(c.accent, 0.55f, 0.85f);
                }
                else
                {
                    _previewPortrait.gameObject.SetActive(false);
                    _previewLetter.gameObject.SetActive(true);
                }
            }

            if (_previewModelImage != null)
                _previewModelImage.gameObject.SetActive(has3D);

            if (has3D) PlayLoadingEffect(c);
            else if (_loadOverlayGroup != null) _loadOverlayGroup.alpha = 0f;

            int[] stats = { c.str, c.agi, c.fin, c.arc };
            for (int k = 0; k < 4; k++)
            {
                _statValues[k].text = stats[k].ToString();
                var fillRT = _statBars[k].rectTransform;
                fillRT.sizeDelta = new Vector2(360f * Mathf.Clamp01(stats[k] / 10f), 6f);
                _statBars[k].color = c.accent;
            }
        }

        // ---------- Helpers ----------

        static Color MultColor(Color c, float v, float a)
        {
            return new Color(c.r * v, c.g * v, c.b * v, a);
        }

        static void FixLabelInsets(RectTransform parentRT, string childName, float left, float right)
        {
            var t = parentRT.Find(childName);
            if (t == null) return;
            var rt = t as RectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }
    }
}
