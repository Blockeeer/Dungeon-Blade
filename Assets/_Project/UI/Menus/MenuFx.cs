using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.UI.Menus
{
    public static class MenuFx
    {
        public static readonly Color BgTop      = new Color(0.020f, 0.024f, 0.035f, 1f);
        public static readonly Color BgMid      = new Color(0.045f, 0.050f, 0.075f, 1f);
        public static readonly Color BgBottom   = new Color(0.110f, 0.030f, 0.030f, 1f);
        public static readonly Color AccentRed  = new Color(0.95f, 0.18f, 0.18f, 1f);
        public static readonly Color SteelTint  = new Color(0.92f, 0.94f, 0.98f, 1f);
        public static readonly Color MutedSteel = new Color(0.70f, 0.74f, 0.82f, 1f);
        public static readonly Color SlabFill   = new Color(0.080f, 0.095f, 0.130f, 0.78f);
        public static readonly Color SlabHover  = new Color(0.180f, 0.220f, 0.290f, 0.92f);
        public static readonly Color SlabPress  = new Color(0.040f, 0.050f, 0.070f, 1.00f);

        static Sprite _gradientSprite;
        static Sprite _scanlineSprite;
        static Sprite _vignetteSprite;
        static Sprite _redGlowSprite;
        static Sprite _slabSprite;
        static Sprite _hGradientSprite;
        static Sprite _roundedSlabSprite;
        static Sprite _roundedBorderSprite;

        public static Sprite GradientSprite()
        {
            if (_gradientSprite == null) _gradientSprite = Make(BuildVerticalGradient(BgTop, BgMid, BgBottom));
            return _gradientSprite;
        }
        public static Sprite ScanlineSprite()
        {
            if (_scanlineSprite == null) _scanlineSprite = Make(BuildScanlineTexture());
            return _scanlineSprite;
        }
        public static Sprite VignetteSprite()
        {
            if (_vignetteSprite == null) _vignetteSprite = Make(BuildVignetteTexture());
            return _vignetteSprite;
        }
        public static Sprite RedGlowSprite()
        {
            if (_redGlowSprite == null) _redGlowSprite = Make(BuildRadialGlow(AccentRed));
            return _redGlowSprite;
        }
        public static Sprite SlabSprite()
        {
            if (_slabSprite == null) _slabSprite = MakeSliced(BuildSlabTexture(), 6f);
            return _slabSprite;
        }
        public static Sprite HorizontalGradientSprite()
        {
            if (_hGradientSprite == null) _hGradientSprite = Make(BuildHorizontalGradient());
            return _hGradientSprite;
        }
        public static Sprite RoundedSlabSprite()
        {
            if (_roundedSlabSprite == null) _roundedSlabSprite = MakeSliced(BuildRoundedSlab(64, 14, 0), 18f);
            return _roundedSlabSprite;
        }
        public static Sprite RoundedBorderSprite()
        {
            if (_roundedBorderSprite == null) _roundedBorderSprite = MakeSliced(BuildRoundedSlab(64, 14, 2), 18f);
            return _roundedBorderSprite;
        }

        public static void BuildBackdrop(Canvas canvas, Transform existingBgToReplace = null)
        {
            if (canvas == null) return;
            var rt = canvas.transform as RectTransform;

            if (existingBgToReplace != null)
            {
                var img = existingBgToReplace.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = GradientSprite();
                    img.color = Color.white;
                    img.type = Image.Type.Simple;
                    img.raycastTarget = false;
                }
            }
            else
            {
                var bg = CreateImage(rt, "Backdrop", 0);
                StretchFull(bg.rectTransform);
                bg.sprite = GradientSprite();
                bg.color = Color.white;
                bg.raycastTarget = false;
            }

            int idx = existingBgToReplace != null ? existingBgToReplace.GetSiblingIndex() + 1 : 1;

            var glow = CreateImage(rt, "RedGlow", idx++);
            glow.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            glow.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            glow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            glow.rectTransform.anchoredPosition = new Vector2(0f, -100f);
            glow.rectTransform.sizeDelta = new Vector2(2600f, 1700f);
            glow.sprite = RedGlowSprite();
            glow.color = new Color(1f, 1f, 1f, 0.32f);
            glow.raycastTarget = false;

            var scan = CreateImage(rt, "Scanlines", idx++);
            StretchFull(scan.rectTransform);
            scan.sprite = ScanlineSprite();
            scan.type = Image.Type.Tiled;
            scan.color = new Color(0f, 0f, 0f, 0.16f);
            scan.pixelsPerUnitMultiplier = 1f;
            scan.raycastTarget = false;

            var vig = CreateImage(rt, "Vignette", idx);
            StretchFull(vig.rectTransform);
            vig.sprite = VignetteSprite();
            vig.color = new Color(0f, 0f, 0f, 0.85f);
            vig.raycastTarget = false;
        }

        public static void StylizeTitle(TMP_Text title, string textOverride = null,
                                        int fontSize = 96, float spacing = 18f)
        {
            if (title == null) return;
            if (!string.IsNullOrEmpty(textOverride)) title.text = textOverride;
            title.fontSize = fontSize;
            title.fontStyle = FontStyles.Bold;
            title.color = SteelTint;
            title.characterSpacing = spacing;
            title.enableVertexGradient = true;
            title.colorGradient = new VertexGradient(SteelTint, SteelTint, MutedSteel, MutedSteel);
            title.alignment = TextAlignmentOptions.Center;
            title.enableWordWrapping = false;
            title.overflowMode = TextOverflowModes.Overflow;
            title.raycastTarget = false;
        }

        public static TMP_Text AddTitleEcho(TMP_Text source, Vector2 offset)
        {
            if (source == null) return null;
            var parent = source.transform.parent as RectTransform;
            if (parent == null) return null;

            var go = new GameObject(source.name + "Echo", typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = source.gameObject.layer;
            var echoRT = (RectTransform)go.transform;
            echoRT.SetParent(parent, false);
            var srcRT = source.rectTransform;
            echoRT.anchorMin = srcRT.anchorMin;
            echoRT.anchorMax = srcRT.anchorMax;
            echoRT.pivot = srcRT.pivot;
            echoRT.anchoredPosition = srcRT.anchoredPosition + offset;
            echoRT.sizeDelta = srcRT.sizeDelta;

            var echo = go.AddComponent<TextMeshProUGUI>();
            echo.font = source.font;
            echo.text = source.text;
            echo.fontSize = source.fontSize;
            echo.fontStyle = source.fontStyle;
            echo.alignment = source.alignment;
            echo.characterSpacing = source.characterSpacing;
            echo.color = new Color(AccentRed.r, AccentRed.g, AccentRed.b, 0.35f);
            echo.raycastTarget = false;
            echoRT.SetSiblingIndex(srcRT.GetSiblingIndex());
            return echo;
        }

        public static TMP_Text AddText(RectTransform parent, string name, TMP_FontAsset font,
                                       Vector2 anchoredPos, Vector2 size, string content,
                                       int fontSize, Color color, float spacing = 0f,
                                       FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.characterSpacing = spacing;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static void StylizeButton(Button btn, TMP_FontAsset font, string labelOverride = null,
                                         float width = 460f, float height = 64f)
        {
            if (btn == null) return;

            btn.gameObject.SetActive(true);
            btn.enabled = true;

            var btnRT = btn.transform as RectTransform;
            var img = btn.GetComponent<Image>();
            if (img == null) img = btn.gameObject.AddComponent<Image>();
            img.enabled = true;

            img.sprite = SlabSprite();
            img.type = Image.Type.Simple;
            img.pixelsPerUnitMultiplier = 1f;
            img.color = Color.white;
            img.raycastTarget = true;

            var colors = btn.colors;
            colors.normalColor      = SlabFill;
            colors.highlightedColor = SlabHover;
            colors.pressedColor     = SlabPress;
            colors.selectedColor    = SlabHover;
            colors.disabledColor    = new Color(0.04f, 0.05f, 0.07f, 0.45f);
            colors.colorMultiplier  = 1f;
            colors.fadeDuration     = 0.12f;
            btn.colors = colors;
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;

            if (btnRT != null)
            {
                btnRT.sizeDelta = new Vector2(width, height);

                var le = btn.GetComponent<LayoutElement>();
                if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = width;
                le.preferredHeight = height;
                le.minWidth = width;
                le.minHeight = height;
                le.flexibleWidth = 0f;
                le.flexibleHeight = 0f;
            }

            EnsureChildImage(btn.transform, "AccentStripe", out var stripe, out var stripeRT);
            stripeRT.anchorMin = new Vector2(0f, 0f);
            stripeRT.anchorMax = new Vector2(0f, 1f);
            stripeRT.pivot = new Vector2(0f, 0.5f);
            stripeRT.anchoredPosition = new Vector2(0f, 0f);
            stripeRT.sizeDelta = new Vector2(4f, 0f);
            stripe.color = AccentRed;
            stripe.raycastTarget = false;
            stripe.transform.SetAsFirstSibling();

            EnsureChildImage(btn.transform, "Marker", out var marker, out var markerRT);
            markerRT.anchorMin = new Vector2(0f, 0.5f);
            markerRT.anchorMax = new Vector2(0f, 0.5f);
            markerRT.pivot = new Vector2(0f, 0.5f);
            markerRT.anchoredPosition = new Vector2(20f, 0f);
            markerRT.sizeDelta = new Vector2(8f, 8f);
            marker.color = AccentRed;
            marker.raycastTarget = false;

            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.enabled = true;
                if (font != null) label.font = font;
                if (!string.IsNullOrEmpty(labelOverride)) label.text = labelOverride;
                else if (!string.IsNullOrEmpty(label.text)) label.text = label.text.ToUpperInvariant();
                label.fontStyle = FontStyles.Bold;
                label.fontSize = 24;
                label.color = SteelTint;
                label.characterSpacing = 18f;
                label.alignment = TextAlignmentOptions.Left;
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Overflow;
                label.raycastTarget = false;

                var labelRT = label.rectTransform;
                labelRT.anchorMin = new Vector2(0f, 0f);
                labelRT.anchorMax = new Vector2(1f, 1f);
                labelRT.pivot = new Vector2(0.5f, 0.5f);
                labelRT.anchoredPosition = Vector2.zero;
                labelRT.sizeDelta = Vector2.zero;
                labelRT.offsetMin = new Vector2(44f, 0f);
                labelRT.offsetMax = new Vector2(-20f, 0f);
            }
        }

        static void EnsureChildImage(Transform parent, string name, out Image img, out RectTransform rt)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                img = existing.GetComponent<Image>();
                if (img == null) img = existing.gameObject.AddComponent<Image>();
                rt = existing as RectTransform;
                return;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) go.layer = uiLayer;
            go.transform.SetParent(parent, false);
            img = go.GetComponent<Image>();
            rt = (RectTransform)go.transform;
        }

        // ---- helpers ----

        public static Image CreateImage(RectTransform parent, string name, int siblingIndex)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) go.layer = uiLayer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));
            return go.GetComponent<Image>();
        }

        public static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        // ---- procedural textures ----

        static Sprite Make(Texture2D tex) =>
            Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);

        static Sprite MakeSliced(Texture2D tex, float border) =>
            Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f),
                          100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));

        static Texture2D BuildVerticalGradient(Color top, Color mid, Color bottom)
        {
            const int h = 512;
            var tex = new Texture2D(2, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                float s = Mathf.SmoothStep(0f, 1f, t);
                float u = 1f - s;
                Color c = new Color(
                    u * u * bottom.r + 2f * u * s * mid.r + s * s * top.r,
                    u * u * bottom.g + 2f * u * s * mid.g + s * s * top.g,
                    u * u * bottom.b + 2f * u * s * mid.b + s * s * top.b,
                    1f);
                tex.SetPixel(0, y, c);
                tex.SetPixel(1, y, c);
            }
            tex.Apply();
            return tex;
        }

        static Texture2D BuildScanlineTexture()
        {
            const int size = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool dark = y == 0 || y == 1;
                tex.SetPixel(x, y, dark ? new Color(0f, 0f, 0f, 1f) : new Color(0f, 0f, 0f, 0f));
            }
            tex.Apply();
            return tex;
        }

        static Texture2D BuildVignetteTexture()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxD = c.magnitude;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / maxD;
                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.55f) / 0.45f));
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, a));
            }
            tex.Apply();
            return tex;
        }

        static Texture2D BuildRadialGlow(Color tint)
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxD = c.magnitude;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / maxD;
                float k = Mathf.Clamp01(1f - d);
                float a = Mathf.SmoothStep(0f, 1f, k);
                a = Mathf.Pow(a, 1.4f);
                tex.SetPixel(x, y, new Color(tint.r, tint.g, tint.b, a));
            }
            tex.Apply();
            return tex;
        }

        static Texture2D BuildRoundedSlab(int size, int radius, float strokeWidth)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };

            float aaWidth = 1.0f;
            float r = radius;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Min(x, size - 1 - x);
                float dy = Mathf.Min(y, size - 1 - y);

                float alpha;
                if (dx >= r || dy >= r)
                {
                    alpha = 1f;
                }
                else
                {
                    float dist = Mathf.Sqrt((r - dx) * (r - dx) + (r - dy) * (r - dy));
                    if (dist > r) alpha = 0f;
                    else if (dist > r - aaWidth) alpha = (r - dist) / aaWidth;
                    else alpha = 1f;
                }

                if (strokeWidth > 0f)
                {
                    float distFromEdge;
                    if (dx >= r || dy >= r)
                    {
                        distFromEdge = Mathf.Min(dx, dy);
                    }
                    else
                    {
                        float dist = Mathf.Sqrt((r - dx) * (r - dx) + (r - dy) * (r - dy));
                        distFromEdge = r - dist;
                    }

                    if (distFromEdge > strokeWidth + aaWidth) alpha = 0f;
                    else if (distFromEdge > strokeWidth) alpha *= 1f - (distFromEdge - strokeWidth) / aaWidth;
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            tex.Apply();
            return tex;
        }

        static Texture2D BuildHorizontalGradient()
        {
            const int w = 256;
            const int h = 4;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int x = 0; x < w; x++)
            {
                float t = x / (float)(w - 1);
                float a = Mathf.SmoothStep(0f, 1f, t);
                Color c = new Color(1f, 1f, 1f, a);
                for (int y = 0; y < h; y++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return tex;
        }

        static Texture2D BuildSlabTexture()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Color c = Color.white;
                if (y == size - 1) c = new Color(1f, 1f, 1f, 1f);
                else if (y == 0)   c = new Color(0.55f, 0.58f, 0.66f, 1f);
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return tex;
        }
    }
}
