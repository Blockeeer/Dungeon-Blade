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

        Image _logoImage;
        RectTransform _logoRT;
        CanvasGroup _logoCG;
        Image _logoHalo;
        Image _logoEcho;
        RectTransform _logoEchoRT;
        Image _logoEchoCyan;
        RectTransform _logoEchoCyanRT;
        Image _flashBurst;
        Image _slashSweep;
        RectTransform _slashSweepRT;
        Image _shineSweep;
        RectTransform _shineSweepRT;
        TMP_Text _subtitle;
        TMP_Text _prompt;
        CanvasGroup _promptCG;
        TMP_Text _legalText;

        Vector2 _logoBasePos;
        Vector2 _logoSize;
        bool _jolting;
        static readonly Color AccentCyan = new Color(0.35f, 0.85f, 1f, 1f);

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

            // Hide the legacy "DUNGEON BLADE" title — the logo now carries the wordmark.
            if (existingTitle != null) existingTitle.gameObject.SetActive(false);

            // Frame the logo top/bottom — well clear of the artwork.
            _accentTop = CreateImage(canvasRT, "AccentBarTop", siblingIndex: titleSibling);
            ConfigureAccent(_accentTop.rectTransform, yOffset: 310f);
            _accentTop.color = new Color(AccentRed.r, AccentRed.g, AccentRed.b, 0.65f);
            _accentTop.raycastTarget = false;

            _accentBottom = CreateImage(canvasRT, "AccentBarBottom", siblingIndex: titleSibling);
            ConfigureAccent(_accentBottom.rectTransform, yOffset: -260f);
            _accentBottom.color = new Color(AccentRed.r, AccentRed.g, AccentRed.b, 0.65f);
            _accentBottom.raycastTarget = false;

            TMP_FontAsset titleFont = null;
            var titleTextRef = existingTitle != null ? existingTitle.GetComponent<TMP_Text>() : null;
            if (titleTextRef != null) titleFont = titleTextRef.font;

            var existingLogo = _canvas.transform.Find("Logo");
            if (existingLogo != null)
            {
                _logoRT = existingLogo as RectTransform;
                _logoImage = existingLogo.GetComponent<Image>();
                _logoCG = existingLogo.GetComponent<CanvasGroup>();
                if (_logoCG == null) _logoCG = existingLogo.gameObject.AddComponent<CanvasGroup>();

                _logoSize = new Vector2(900f, 495f);
                _logoBasePos = new Vector2(0f, 40f);
                _logoRT.anchorMin = new Vector2(0.5f, 0.5f);
                _logoRT.anchorMax = new Vector2(0.5f, 0.5f);
                _logoRT.pivot = new Vector2(0.5f, 0.5f);
                _logoRT.anchoredPosition = _logoBasePos;
                _logoRT.sizeDelta = _logoSize;

                if (_logoImage != null)
                {
                    var logoSprite = Resources.Load<Sprite>("UI/Logo-no-BG");
                    if (logoSprite != null) _logoImage.sprite = logoSprite;
                    _logoImage.preserveAspect = true;
                    _logoImage.color = Color.white;
                    _logoImage.raycastTarget = false;
                }

                // Cyan echo (chromatic aberration: cyan offset opposite to red).
                _logoEchoCyan = CreateImage(canvasRT, "LogoEchoCyan", siblingIndex: _logoRT.GetSiblingIndex());
                _logoEchoCyanRT = _logoEchoCyan.rectTransform;
                AnchorCenter(_logoEchoCyanRT, _logoBasePos + new Vector2(-6f, 4f), _logoSize);
                if (_logoImage != null && _logoImage.sprite != null) _logoEchoCyan.sprite = _logoImage.sprite;
                _logoEchoCyan.preserveAspect = true;
                _logoEchoCyan.color = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.30f);
                _logoEchoCyan.raycastTarget = false;

                // Red-tinted echo of the logo (chromatic aberration, sits behind logo).
                _logoEcho = CreateImage(canvasRT, "LogoEcho", siblingIndex: _logoRT.GetSiblingIndex());
                _logoEchoRT = _logoEcho.rectTransform;
                AnchorCenter(_logoEchoRT, _logoBasePos + new Vector2(6f, -4f), _logoSize);
                if (_logoImage != null && _logoImage.sprite != null) _logoEcho.sprite = _logoImage.sprite;
                _logoEcho.preserveAspect = true;
                _logoEcho.color = new Color(AccentRed.r, AccentRed.g, AccentRed.b, 0.40f);
                _logoEcho.raycastTarget = false;
            }

            // Horizontal slash sweep that whips across the logo on entrance.
            _slashSweep = CreateImage(canvasRT, "SlashSweep", siblingIndex: titleSibling);
            _slashSweepRT = _slashSweep.rectTransform;
            AnchorCenter(_slashSweepRT, _logoBasePos, new Vector2(2200f, 8f));
            _slashSweep.sprite = SpriteFromTexture(BuildHorizontalSweepTexture());
            _slashSweep.color = new Color(1f, 1f, 1f, 0f);
            _slashSweep.raycastTarget = false;

            // Mask container shaped like the logo — clips the shine to the lettering silhouette.
            if (_logoImage != null && _logoImage.sprite != null)
            {
                var maskGO = new GameObject("LogoShineMask",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
                maskGO.layer = LayerMask.NameToLayer("UI");
                var maskRT = (RectTransform)maskGO.transform;
                maskRT.SetParent(canvasRT, false);
                AnchorCenter(maskRT, _logoBasePos, _logoSize);
                maskRT.SetSiblingIndex(_logoRT.GetSiblingIndex() + 1);

                var maskImg = maskGO.GetComponent<Image>();
                maskImg.sprite = _logoImage.sprite;
                maskImg.preserveAspect = true;
                maskImg.raycastTarget = false;
                maskImg.color = Color.white;

                var mask = maskGO.GetComponent<Mask>();
                mask.showMaskGraphic = false;

                // Recurring diagonal shine — child of the mask, so it only renders over the lettering.
                _shineSweep = CreateImage(maskRT, "ShineSweep", siblingIndex: 0);
                _shineSweepRT = _shineSweep.rectTransform;
                AnchorCenter(_shineSweepRT, Vector2.zero, new Vector2(140f, _logoSize.y * 1.8f));
                _shineSweep.sprite = SpriteFromTexture(BuildHorizontalSweepTexture());
                _shineSweep.color = new Color(1f, 1f, 1f, 0f);
                _shineSweepRT.localRotation = Quaternion.Euler(0f, 0f, 18f);
                _shineSweep.raycastTarget = false;
            }

            // White flash burst that explodes outward on entrance.
            _flashBurst = CreateImage(canvasRT, "FlashBurst", siblingIndex: titleSibling);
            AnchorCenter(_flashBurst.rectTransform, _logoBasePos, new Vector2(900f, 900f));
            _flashBurst.sprite = SpriteFromTexture(BuildRadialGlow(Color.white));
            _flashBurst.color = new Color(1f, 1f, 1f, 0f);
            _flashBurst.raycastTarget = false;

            _subtitle = CreateText(canvasRT, "Subtitle", titleFont);
            if (_subtitle != null)
            {
                AnchorCenter(_subtitle.rectTransform, new Vector2(0f, -210f), new Vector2(1200f, 40f));
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

            _legalText = CreateText(canvasRT, "Legal", titleFont);
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
            if (_subtitle != null) _subtitle.alpha = 0f;
            if (_promptCG != null) _promptCG.alpha = 0f;
            if (_accentTop != null) _accentTop.rectTransform.localScale = new Vector3(0f, 1f, 1f);
            if (_accentBottom != null) _accentBottom.rectTransform.localScale = new Vector3(0f, 1f, 1f);
            if (_logoHalo != null) _logoHalo.color = new Color(1f, 1f, 1f, 0f);
            if (_logoEcho != null)
            {
                var c = _logoEcho.color; c.a = 0f; _logoEcho.color = c;
            }
            if (_logoEchoCyan != null)
            {
                var c = _logoEchoCyan.color; c.a = 0f; _logoEchoCyan.color = c;
            }
            if (_flashBurst != null) _flashBurst.color = new Color(1f, 1f, 1f, 0f);
            if (_slashSweep != null) _slashSweep.color = new Color(1f, 1f, 1f, 0f);

            // Logo fades in while scaling down from 1.25 → 1.0.
            yield return Tween(0.55f, t =>
            {
                float e = EaseOutCubic(t);
                if (_logoCG != null) _logoCG.alpha = e;
                if (_logoRT != null) _logoRT.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, e);
                if (_logoEcho != null)
                {
                    var c = _logoEcho.color; c.a = 0.40f * e; _logoEcho.color = c;
                }
                if (_logoEchoCyan != null)
                {
                    var c = _logoEchoCyan.color; c.a = 0.30f * e; _logoEchoCyan.color = c;
                }
            });

            // Flash burst — bright pop that fades out as it expands.
            StartCoroutine(FlashBurstRoutine());
            // Slash sweep — horizontal whip across the logo.
            StartCoroutine(SlashSweepRoutine());

            yield return Tween(0.35f, t =>
            {
                float s = EaseOutCubic(t);
                if (_accentTop != null) _accentTop.rectTransform.localScale = new Vector3(s, 1f, 1f);
                if (_accentBottom != null) _accentBottom.rectTransform.localScale = new Vector3(s, 1f, 1f);
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
            StartCoroutine(HaloPulseRoutine());
            StartCoroutine(LogoEchoDriftRoutine());
            StartCoroutine(ElectricJoltRoutine());
            StartCoroutine(ShineSweepLoopRoutine());
        }

        IEnumerator ShineSweepLoopRoutine()
        {
            if (_shineSweep == null || _shineSweepRT == null) yield break;
            // Initial delay so the recurring shine doesn't fight the entrance slash.
            float wait = 2.4f;
            while (!_advancing)
            {
                float elapsed = 0f;
                while (elapsed < wait && !_advancing)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                if (_advancing) yield break;

                float dur = 0.9f;
                float t = 0f;
                float halfWidth = _logoSize.x * 0.65f;
                while (t < dur)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / dur);
                    float e = EaseOutCubic(k);
                    float x = Mathf.Lerp(-halfWidth, halfWidth, e);
                    _shineSweepRT.anchoredPosition = new Vector2(x, 0f);
                    float a = Mathf.Sin(k * Mathf.PI) * 0.9f;
                    _shineSweep.color = new Color(1f, 0.97f, 0.88f, a);
                    yield return null;
                }
                _shineSweep.color = new Color(1f, 1f, 1f, 0f);
                wait = Random.Range(3.4f, 5.2f);
            }
        }

        IEnumerator FlashBurstRoutine()
        {
            if (_flashBurst == null) yield break;
            var rt = _flashBurst.rectTransform;
            float dur = 0.45f;
            float t = 0f;
            Vector2 start = new Vector2(600f, 600f);
            Vector2 end = new Vector2(1500f, 1500f);
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float e = EaseOutCubic(k);
                rt.sizeDelta = Vector2.Lerp(start, end, e);
                _flashBurst.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.85f, 0f, e));
                yield return null;
            }
            _flashBurst.color = new Color(1f, 1f, 1f, 0f);
        }

        IEnumerator HaloPulseRoutine()
        {
            if (_logoHalo == null) yield break;
            float t0 = Time.unscaledTime;
            while (!_advancing)
            {
                float k = (Time.unscaledTime - t0) * 1.1f;
                float a = 0.55f + Mathf.Sin(k) * 0.20f;
                float s = 1f + Mathf.Sin(k * 0.85f) * 0.05f;
                _logoHalo.color = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
                _logoHalo.rectTransform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }

        IEnumerator LogoEchoDriftRoutine()
        {
            if (_logoEchoRT == null && _logoEchoCyanRT == null) yield break;
            float t0 = Time.unscaledTime;
            while (!_advancing)
            {
                float k = (Time.unscaledTime - t0) * 1.6f;
                float dx = 5f + Mathf.Sin(k) * 3f;
                float dy = -3f + Mathf.Cos(k * 0.9f) * 2.5f;
                if (_logoEchoRT != null)
                    _logoEchoRT.anchoredPosition = _logoBasePos + new Vector2(dx, dy);
                if (_logoEchoCyanRT != null)
                    _logoEchoCyanRT.anchoredPosition = _logoBasePos + new Vector2(-dx, -dy);
                yield return null;
            }
        }

        IEnumerator SlashSweepRoutine()
        {
            if (_slashSweep == null || _slashSweepRT == null) yield break;
            float dur = 0.55f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float e = EaseOutCubic(k);
                // Slide a thick horizontal bar of light across the canvas.
                float x = Mathf.Lerp(-1400f, 1400f, e);
                _slashSweepRT.anchoredPosition = _logoBasePos + new Vector2(x, 0f);
                float a = Mathf.Sin(k * Mathf.PI);
                _slashSweep.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
            _slashSweep.color = new Color(1f, 1f, 1f, 0f);
        }

        IEnumerator ElectricJoltRoutine()
        {
            // Periodic "electric pulse" — boosts halo, briefly nudges echoes apart, tiny logo kick.
            float wait = 3.2f;
            float elapsed = 0f;
            while (!_advancing)
            {
                elapsed += Time.unscaledDeltaTime;
                if (elapsed < wait) { yield return null; continue; }
                elapsed = 0f;
                wait = Random.Range(2.6f, 4.2f);
                _jolting = true;

                float dur = 0.28f;
                float t = 0f;
                while (t < dur)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / dur);
                    float pulse = Mathf.Sin(k * Mathf.PI);
                    if (_logoHalo != null)
                    {
                        var c = _logoHalo.color; c.a = Mathf.Clamp01(c.a + pulse * 0.35f); _logoHalo.color = c;
                    }
                    if (_logoRT != null)
                    {
                        // Tiny kick: scale + shake.
                        float shake = (Random.value - 0.5f) * 4f * pulse;
                        _logoRT.localScale = Vector3.one * (1f + 0.025f * pulse);
                        _logoRT.anchoredPosition = _logoBasePos + new Vector2(shake, 0f);
                    }
                    yield return null;
                }
                if (_logoRT != null) _logoRT.localScale = Vector3.one;
                _jolting = false;
            }
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
                if (!_jolting && _logoRT != null)
                {
                    float k = (Time.unscaledTime - t0) * 1.4f;
                    float s = 1f + Mathf.Sin(k) * 0.018f;
                    float yFloat = Mathf.Sin(k * 0.7f) * 4f;
                    float tiltZ = Mathf.Sin(k * 0.55f) * 0.6f;
                    _logoRT.localScale = new Vector3(s, s, 1f);
                    _logoRT.anchoredPosition = _logoBasePos + new Vector2(0f, yFloat);
                    _logoRT.localRotation = Quaternion.Euler(0f, 0f, tiltZ);
                }
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

        static Texture2D BuildHorizontalSweepTexture()
        {
            // Soft horizontal bar — opaque white center fading to transparent at the edges.
            const int w = 256;
            const int h = 8;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1);
                float a = Mathf.Pow(Mathf.Sin(u * Mathf.PI), 2.5f);
                for (int y = 0; y < h; y++) tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
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
