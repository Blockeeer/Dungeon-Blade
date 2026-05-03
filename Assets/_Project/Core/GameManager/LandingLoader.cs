using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DungeonBlade.Core
{
    public class LandingLoader : MonoBehaviour
    {
        [SerializeField] string nextScene = SceneLoader.MainMenu;
        [SerializeField] float minDisplaySeconds = 1.25f;
        [Tooltip("0 or negative = wait indefinitely for input. Set > 0 to auto-advance after this many seconds.")]
        [SerializeField] float autoAdvanceSeconds = 0f;

        static readonly Color BgTop      = new Color(0.020f, 0.024f, 0.035f, 1f);
        static readonly Color BgMid      = new Color(0.045f, 0.050f, 0.075f, 1f);
        static readonly Color BgBottom   = new Color(0.110f, 0.030f, 0.030f, 1f);
        static readonly Color AccentRed  = new Color(0.95f, 0.18f, 0.18f, 1f);
        static readonly Color SteelTint  = new Color(0.88f, 0.90f, 0.95f, 1f);
        static readonly Color MutedSteel = new Color(0.70f, 0.74f, 0.82f, 1f);

        float _startTime;
        bool _advancing;
        Canvas _canvas;

        Image _bgGradient;
        Image _scanlines;
        Image _vignette;
        Image _accentTop;
        Image _accentBottom;
        Image _redGlow;

        RectTransform _logoRT;
        CanvasGroup _logoCG;
        RectTransform _titleRT;
        CanvasGroup _titleCG;
        TMP_Text _titleText;
        CanvasGroup _titleEchoCG;
        TMP_Text _subtitle;
        TMP_Text _prompt;
        CanvasGroup _promptCG;
        TMP_Text _legalText;

        void Start()
        {
            _startTime = Time.unscaledTime;
            EnsureSystem<GameManager>("[GameManager]");
            EnsureSystem<InputManager>("[InputManager]");

            BuildScreen();
            StartCoroutine(EntranceRoutine());
        }

        void Update()
        {
            if (_advancing) return;

            float held = Time.unscaledTime - _startTime;
            bool minHeld = held >= minDisplaySeconds;

            if (minHeld && AnyInputPressed())
            {
                Advance();
                return;
            }

            if (autoAdvanceSeconds > 0f && held >= autoAdvanceSeconds)
            {
                Advance();
            }
        }

        void Advance()
        {
            _advancing = true;
            SceneLoader.Load(nextScene);
            enabled = false;
        }

        static bool AnyInputPressed()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
            if (Mouse.current != null &&
                (Mouse.current.leftButton.wasPressedThisFrame ||
                 Mouse.current.rightButton.wasPressedThisFrame ||
                 Mouse.current.middleButton.wasPressedThisFrame)) return true;
            if (Gamepad.current != null)
            {
                var pad = Gamepad.current;
                if (pad.buttonSouth.wasPressedThisFrame ||
                    pad.buttonEast.wasPressedThisFrame ||
                    pad.buttonNorth.wasPressedThisFrame ||
                    pad.buttonWest.wasPressedThisFrame ||
                    pad.startButton.wasPressedThisFrame ||
                    pad.selectButton.wasPressedThisFrame) return true;
            }
            return false;
        }

        Canvas ResolveLandingCanvas()
        {
            var all = FindObjectsOfType<Canvas>();
            var myScene = gameObject.scene;
            foreach (var c in all)
            {
                if (c == null) continue;
                if (c.gameObject.scene != myScene) continue;
                if (c.name == "FadeCanvas") continue;
                return c;
            }
            foreach (var c in all)
            {
                if (c != null && c.gameObject.scene == myScene) return c;
            }
            return null;
        }

        // ---------- Build ----------

        void BuildScreen()
        {
            _canvas = ResolveLandingCanvas();
            if (_canvas == null) return;

            var canvasRT = _canvas.transform as RectTransform;

            var existingBg = _canvas.transform.Find("Background");
            if (existingBg != null) _bgGradient = existingBg.GetComponent<Image>();
            if (_bgGradient != null)
            {
                _bgGradient.sprite = SpriteFromTexture(BuildVerticalGradient(BgTop, BgMid, BgBottom));
                _bgGradient.color = Color.white;
                _bgGradient.type = Image.Type.Simple;
            }

            _redGlow = CreateImage(canvasRT, "RedGlow", siblingIndex: 1);
            StretchAnchored(_redGlow.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, -200f), new Vector2(1600f, 900f));
            _redGlow.sprite = SpriteFromTexture(BuildRadialGlow(AccentRed));
            _redGlow.color = new Color(1f, 1f, 1f, 0.55f);
            _redGlow.raycastTarget = false;

            _scanlines = CreateImage(canvasRT, "Scanlines", siblingIndex: 2);
            StretchFull(_scanlines.rectTransform);
            var scanTex = BuildScanlineTexture();
            _scanlines.sprite = SpriteFromTexture(scanTex);
            _scanlines.type = Image.Type.Tiled;
            _scanlines.color = new Color(0f, 0f, 0f, 0.18f);
            _scanlines.pixelsPerUnitMultiplier = 1f;
            _scanlines.raycastTarget = false;

            _vignette = CreateImage(canvasRT, "Vignette", siblingIndex: 3);
            StretchFull(_vignette.rectTransform);
            _vignette.sprite = SpriteFromTexture(BuildVignetteTexture());
            _vignette.color = new Color(0f, 0f, 0f, 0.85f);
            _vignette.raycastTarget = false;

            var existingTitle = _canvas.transform.Find("Title");
            int titleSibling = existingTitle != null ? existingTitle.GetSiblingIndex() : _canvas.transform.childCount;

            _accentTop = CreateImage(canvasRT, "AccentBarTop", siblingIndex: titleSibling);
            ConfigureAccent(_accentTop.rectTransform, yOffset: 95f);
            _accentTop.color = AccentRed;
            _accentTop.raycastTarget = false;

            _accentBottom = CreateImage(canvasRT, "AccentBarBottom", siblingIndex: titleSibling);
            ConfigureAccent(_accentBottom.rectTransform, yOffset: -95f);
            _accentBottom.color = new Color(AccentRed.r, AccentRed.g, AccentRed.b, 0.55f);
            _accentBottom.raycastTarget = false;

            var existingLogo = _canvas.transform.Find("Logo");
            if (existingLogo != null)
            {
                _logoRT = existingLogo as RectTransform;
                _logoCG = existingLogo.GetComponent<CanvasGroup>();
                if (_logoCG == null) _logoCG = existingLogo.gameObject.AddComponent<CanvasGroup>();
                _logoRT.anchoredPosition = new Vector2(0f, 220f);
                _logoRT.sizeDelta = new Vector2(360f, 360f);
            }

            if (existingTitle != null)
            {
                _titleRT = existingTitle as RectTransform;
                _titleText = existingTitle.GetComponent<TMP_Text>();
                _titleCG = existingTitle.GetComponent<CanvasGroup>();
                if (_titleCG == null) _titleCG = existingTitle.gameObject.AddComponent<CanvasGroup>();
                if (_titleText != null)
                {
                    _titleText.text = "DUNGEON  BLADE";
                    _titleText.fontSize = 110;
                    _titleText.fontStyle = FontStyles.Bold;
                    _titleText.color = SteelTint;
                    _titleText.characterSpacing = 18f;
                    _titleText.enableVertexGradient = true;
                    _titleText.colorGradient = new VertexGradient(
                        SteelTint, SteelTint, MutedSteel, MutedSteel);
                }
                _titleRT.anchoredPosition = new Vector2(0f, -40f);
                _titleRT.sizeDelta = new Vector2(1400f, 200f);
            }

            var titleEcho = CreateText(canvasRT, "TitleEcho", _titleText != null ? _titleText.font : null);
            if (titleEcho != null && _titleText != null)
            {
                AnchorCenter(titleEcho.rectTransform, new Vector2(4f, -44f), _titleRT.sizeDelta);
                titleEcho.text = _titleText.text;
                titleEcho.fontSize = _titleText.fontSize;
                titleEcho.fontStyle = _titleText.fontStyle;
                titleEcho.alignment = _titleText.alignment;
                titleEcho.characterSpacing = _titleText.characterSpacing;
                titleEcho.color = new Color(AccentRed.r, AccentRed.g, AccentRed.b, 0.35f);
                titleEcho.raycastTarget = false;
                titleEcho.transform.SetSiblingIndex(_titleRT.GetSiblingIndex());
                _titleEchoCG = titleEcho.gameObject.AddComponent<CanvasGroup>();
            }

            _subtitle = CreateText(canvasRT, "Subtitle", _titleText != null ? _titleText.font : null);
            if (_subtitle != null)
            {
                AnchorCenter(_subtitle.rectTransform, new Vector2(0f, -130f), new Vector2(1200f, 40f));
                _subtitle.text = "— A   B L A D E   &   G U N   D U E L —";
                _subtitle.fontSize = 24;
                _subtitle.alignment = TextAlignmentOptions.Center;
                _subtitle.color = new Color(MutedSteel.r, MutedSteel.g, MutedSteel.b, 0.85f);
                _subtitle.characterSpacing = 22f;
                _subtitle.fontStyle = FontStyles.Bold;
                _subtitle.raycastTarget = false;
            }

            var existingLoading = _canvas.transform.Find("Loading");
            if (existingLoading != null)
            {
                _prompt = existingLoading.GetComponent<TMP_Text>();
                _promptCG = existingLoading.GetComponent<CanvasGroup>();
                if (_promptCG == null) _promptCG = existingLoading.gameObject.AddComponent<CanvasGroup>();
                if (_prompt != null)
                {
                    _prompt.text = "PRESS ANY KEY TO CONTINUE";
                    _prompt.fontSize = 26;
                    _prompt.color = SteelTint;
                    _prompt.characterSpacing = 14f;
                    _prompt.fontStyle = FontStyles.Bold;
                    _prompt.raycastTarget = false;
                }
                var promptRT = existingLoading as RectTransform;
                promptRT.anchorMin = new Vector2(0.5f, 0f);
                promptRT.anchorMax = new Vector2(0.5f, 0f);
                promptRT.pivot = new Vector2(0.5f, 0.5f);
                promptRT.anchoredPosition = new Vector2(0f, 110f);
                promptRT.sizeDelta = new Vector2(700f, 40f);
            }

            _legalText = CreateText(canvasRT, "Legal", _titleText != null ? _titleText.font : null);
            if (_legalText != null)
            {
                _legalText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
                _legalText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
                _legalText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                _legalText.rectTransform.anchoredPosition = new Vector2(0f, 36f);
                _legalText.rectTransform.sizeDelta = new Vector2(900f, 28f);
                _legalText.text = "v" + Application.version + "    //    DUNGEON  BLADE";
                _legalText.fontSize = 14;
                _legalText.alignment = TextAlignmentOptions.Center;
                _legalText.color = new Color(MutedSteel.r, MutedSteel.g, MutedSteel.b, 0.45f);
                _legalText.characterSpacing = 18f;
                _legalText.raycastTarget = false;
            }
        }

        // ---------- Animation ----------

        IEnumerator EntranceRoutine()
        {
            if (_logoCG != null) _logoCG.alpha = 0f;
            if (_titleCG != null) _titleCG.alpha = 0f;
            if (_titleEchoCG != null) _titleEchoCG.alpha = 0f;
            if (_subtitle != null) _subtitle.alpha = 0f;
            if (_promptCG != null) _promptCG.alpha = 0f;
            if (_accentTop != null) _accentTop.rectTransform.localScale = new Vector3(0f, 1f, 1f);
            if (_accentBottom != null) _accentBottom.rectTransform.localScale = new Vector3(0f, 1f, 1f);

            yield return Tween(0.4f, t =>
            {
                if (_logoCG != null) _logoCG.alpha = t;
                if (_logoRT != null) _logoRT.localScale = Vector3.one * Mathf.Lerp(1.15f, 1f, t);
            });

            yield return Tween(0.35f, t =>
            {
                float s = EaseOutCubic(t);
                if (_accentTop != null) _accentTop.rectTransform.localScale = new Vector3(s, 1f, 1f);
                if (_accentBottom != null) _accentBottom.rectTransform.localScale = new Vector3(s, 1f, 1f);
            });

            yield return Tween(0.5f, t =>
            {
                if (_titleCG != null) _titleCG.alpha = t;
                if (_titleEchoCG != null) _titleEchoCG.alpha = t;
                if (_titleRT != null)
                    _titleRT.anchoredPosition = new Vector2(0f, Mathf.Lerp(-80f, -40f, EaseOutCubic(t)));
            });

            yield return Tween(0.35f, t =>
            {
                if (_subtitle != null) _subtitle.alpha = t;
            });

            yield return Tween(0.25f, t =>
            {
                if (_promptCG != null) _promptCG.alpha = t;
            });

            StartCoroutine(PromptPulseRoutine());
            StartCoroutine(LogoBreathRoutine());
        }

        IEnumerator PromptPulseRoutine()
        {
            while (!_advancing)
            {
                yield return Tween(0.55f, t =>
                {
                    if (_promptCG != null) _promptCG.alpha = Mathf.Lerp(1f, 0.25f, t);
                });
                yield return Tween(0.55f, t =>
                {
                    if (_promptCG != null) _promptCG.alpha = Mathf.Lerp(0.25f, 1f, t);
                });
            }
        }

        IEnumerator LogoBreathRoutine()
        {
            float t0 = Time.unscaledTime;
            while (!_advancing)
            {
                float k = (Time.unscaledTime - t0) * 1.4f;
                float s = 1f + Mathf.Sin(k) * 0.012f;
                if (_logoRT != null) _logoRT.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }

        static IEnumerator Tween(float duration, System.Action<float> apply)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                apply(Mathf.Clamp01(t / duration));
                yield return null;
            }
            apply(1f);
        }

        static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        // ---------- UI helpers ----------

        static Image CreateImage(RectTransform parent, string name, int siblingIndex)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));
            return go.GetComponent<Image>();
        }

        static TMP_Text CreateText(RectTransform parent, string name, TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            return tmp;
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        static void StretchAnchored(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static void AnchorCenter(RectTransform rt, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static void ConfigureAccent(RectTransform rt, float yOffset)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, yOffset);
            rt.sizeDelta = new Vector2(820f, 3f);
        }

        // ---------- Procedural textures ----------

        static Sprite SpriteFromTexture(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        static Texture2D BuildVerticalGradient(Color top, Color mid, Color bottom)
        {
            const int h = 256;
            var tex = new Texture2D(2, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                Color c = t < 0.5f
                    ? Color.Lerp(bottom, mid, t * 2f)
                    : Color.Lerp(mid, top, (t - 0.5f) * 2f);
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
                float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2.5f);
                tex.SetPixel(x, y, new Color(tint.r, tint.g, tint.b, a));
            }
            tex.Apply();
            return tex;
        }

        static T EnsureSystem<T>(string objectName) where T : Component
        {
            var existing = FindObjectOfType<T>();
            if (existing != null) return existing;

            var go = new GameObject(objectName);
            DontDestroyOnLoad(go);
            return go.AddComponent<T>();
        }
    }
}
