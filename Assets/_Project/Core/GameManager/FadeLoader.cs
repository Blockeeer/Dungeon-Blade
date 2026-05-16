using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonBlade.Core
{
    public class FadeLoader : MonoBehaviour
    {
        public static FadeLoader Instance { get; private set; }

        [SerializeField] CanvasGroup fadeGroup;
        [SerializeField] Image fadeImage;
        [SerializeField] float defaultFadeDuration = 0.8f;
        [SerializeField, Tooltip("Minimum time the loading screen stays visible (seconds), even on fast loads.")]
        float minLoadingDisplay = 12.0f;
        [SerializeField, Tooltip("How long the bar holds at 100% before fading out (seconds).")]
        float completeHoldTime = 1.5f;
        [SerializeField] string defaultDisplayName = "Loading";
        [SerializeField, TextArea(2, 4)]
        string defaultTip = "Tip: Block with right-click. Parry just before a hit lands to stagger your enemy.";

        bool _busy;

        Image _bgImage;
        TMP_Text _titleLabel;
        TMP_Text _loadingLabel;
        TMP_Text _tipLabel;
        RectTransform _spinner;
        Image _progressFill;
        RectTransform _progressTrack;
        CanvasGroup _loadingUIGroup;
        string _pendingDisplayName;
        string _pendingTip;
        float _displayedProgress;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoBootstrap()
        {
            if (Instance != null) return;
            var existing = FindObjectOfType<FadeLoader>();
            if (existing != null) return;
            var go = new GameObject("FadeLoader_AutoBootstrap");
            go.AddComponent<FadeLoader>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureFadeCanvas();
            if (fadeGroup != null) fadeGroup.alpha = 0f;
            if (fadeImage != null) fadeImage.raycastTarget = false;
            EnsureLoadingUI();
            SetLoadingUIVisible(false);
        }

        void EnsureFadeCanvas()
        {
            if (fadeGroup != null && fadeImage != null) return;

            var canvasGo = new GameObject("FadeCanvas_Auto", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9000;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            fadeGroup = canvasGo.GetComponent<CanvasGroup>();
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;

            var bg = new GameObject("FadeBackground", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(canvasGo.transform, false);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            fadeImage = bg.GetComponent<Image>();
            fadeImage.color = new Color(0.02f, 0.02f, 0.03f, 1f);
            fadeImage.raycastTarget = false;
        }

        public void LoadScene(string sceneName, float fadeDuration = -1f)
        {
            LoadScene(sceneName, null, null, fadeDuration);
        }

        public void LoadScene(string sceneName, string displayName, string tip, float fadeDuration = -1f)
        {
            if (_busy) return;
            float dur = fadeDuration > 0f ? fadeDuration : defaultFadeDuration;
            _pendingDisplayName = string.IsNullOrEmpty(displayName) ? PrettifySceneName(sceneName) : displayName;
            _pendingTip = string.IsNullOrEmpty(tip) ? defaultTip : tip;
            StartCoroutine(LoadRoutine(sceneName, dur));
        }

        IEnumerator LoadRoutine(string sceneName, float dur)
        {
            _busy = true;

            ApplyPendingLabels();
            _displayedProgress = 0f;
            SetProgressVisual(0f);
            SetLoadingUIVisible(true);
            yield return Fade(0f, 1f, dur);

            float displayTimer = 0f;
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null)
            {
                op.allowSceneActivation = false;
                while (!op.isDone)
                {
                    displayTimer += Time.unscaledDeltaTime;
                    float loadProgress = Mathf.Clamp01(op.progress / 0.9f);
                    float timeProgress = Mathf.Clamp01(displayTimer / minLoadingDisplay);
                    float target = Mathf.Min(loadProgress, timeProgress);
                    _displayedProgress = Mathf.MoveTowards(_displayedProgress, target, Time.unscaledDeltaTime * 0.6f);
                    SetProgressVisual(_displayedProgress);

                    if (op.progress >= 0.9f && displayTimer >= minLoadingDisplay)
                    {
                        SetProgressVisual(1f);
                        op.allowSceneActivation = true;
                    }
                    yield return null;
                }
            }
            else
            {
                while (displayTimer < minLoadingDisplay)
                {
                    displayTimer += Time.unscaledDeltaTime;
                    SetProgressVisual(displayTimer / minLoadingDisplay);
                    yield return null;
                }
                SetProgressVisual(1f);
            }

            yield return new WaitForSecondsRealtime(Mathf.Max(0f, completeHoldTime));

            yield return Fade(1f, 0f, dur);
            SetLoadingUIVisible(false);
            _busy = false;
        }

        void Update()
        {
            if (_spinner != null && _loadingUIGroup != null && _loadingUIGroup.alpha > 0.01f)
            {
                _spinner.Rotate(0f, 0f, -260f * Time.unscaledDeltaTime);
            }
            if (_loadingLabel != null && _loadingUIGroup != null && _loadingUIGroup.alpha > 0.01f)
            {
                int dots = Mathf.FloorToInt(Time.unscaledTime * 2.5f) % 4;
                _loadingLabel.text = "Loading" + new string('.', dots);
            }
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            if (fadeGroup == null) yield break;
            if (fadeImage != null) fadeImage.raycastTarget = to > 0.01f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            fadeGroup.alpha = to;
            if (fadeImage != null) fadeImage.raycastTarget = to > 0.01f;
        }

        void ApplyPendingLabels()
        {
            if (_titleLabel != null) _titleLabel.text = (_pendingDisplayName ?? defaultDisplayName).ToUpper();
            if (_tipLabel != null) _tipLabel.text = _pendingTip ?? defaultTip;
        }

        void SetProgressVisual(float t)
        {
            if (_progressFill == null || _progressTrack == null) return;
            var fillRT = _progressFill.rectTransform;
            fillRT.anchorMin = new Vector2(0f, 0f);
            fillRT.anchorMax = new Vector2(Mathf.Clamp01(t), 1f);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
        }

        void SetLoadingUIVisible(bool visible)
        {
            if (_loadingUIGroup == null) return;
            _loadingUIGroup.alpha = visible ? 1f : 0f;
            _loadingUIGroup.blocksRaycasts = visible;
        }

        static string PrettifySceneName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "Loading";
            string s = raw;
            int us = s.IndexOf('_');
            if (us >= 0 && us < s.Length - 1) s = s.Substring(us + 1);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(s[i - 1])) sb.Append(' ');
                sb.Append(c);
            }
            return sb.ToString();
        }

        void EnsureLoadingUI()
        {
            if (_loadingUIGroup != null) return;
            if (fadeGroup == null) return;

            if (fadeImage != null) _bgImage = fadeImage;

            var root = new GameObject("LoadingUI", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(fadeGroup.transform, false);
            var rootRT = root.GetComponent<RectTransform>();
            StretchFull(rootRT);
            _loadingUIGroup = root.GetComponent<CanvasGroup>();

            BuildGradientOverlay(root.transform);
            BuildTitleBlock(root.transform);
            BuildProgressBar(root.transform);
            BuildTipBlock(root.transform);
        }

        void BuildGradientOverlay(Transform parent)
        {
            var top = new GameObject("VignetteTop", typeof(RectTransform), typeof(Image));
            top.transform.SetParent(parent, false);
            var topImg = top.GetComponent<Image>();
            topImg.color = new Color(0f, 0f, 0f, 0.55f);
            topImg.raycastTarget = false;
            var topRT = top.GetComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0f, 0.7f);
            topRT.anchorMax = new Vector2(1f, 1f);
            topRT.offsetMin = Vector2.zero;
            topRT.offsetMax = Vector2.zero;

            var bot = new GameObject("VignetteBottom", typeof(RectTransform), typeof(Image));
            bot.transform.SetParent(parent, false);
            var botImg = bot.GetComponent<Image>();
            botImg.color = new Color(0f, 0f, 0f, 0.65f);
            botImg.raycastTarget = false;
            var botRT = bot.GetComponent<RectTransform>();
            botRT.anchorMin = new Vector2(0f, 0f);
            botRT.anchorMax = new Vector2(1f, 0.35f);
            botRT.offsetMin = Vector2.zero;
            botRT.offsetMax = Vector2.zero;
        }

        void BuildTitleBlock(Transform parent)
        {
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(parent, false);
            var tmp = titleGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "LOADING";
            tmp.fontSize = 96f;
            tmp.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.93f, 0.86f, 1f);
            tmp.characterSpacing = 18f;
            tmp.raycastTarget = false;
            var rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 90f);
            rt.sizeDelta = new Vector2(-200f, 130f);
            _titleLabel = tmp;

            var underline = new GameObject("Underline", typeof(RectTransform), typeof(Image));
            underline.transform.SetParent(parent, false);
            var ulImg = underline.GetComponent<Image>();
            ulImg.color = new Color(0.85f, 0.22f, 0.18f, 1f);
            ulImg.raycastTarget = false;
            var ulRT = underline.GetComponent<RectTransform>();
            ulRT.anchorMin = new Vector2(0.5f, 0.5f);
            ulRT.anchorMax = new Vector2(0.5f, 0.5f);
            ulRT.pivot = new Vector2(0.5f, 0.5f);
            ulRT.anchoredPosition = new Vector2(0f, 30f);
            ulRT.sizeDelta = new Vector2(240f, 4f);
        }

        void BuildProgressBar(Transform parent)
        {
            float barWidth = 900f;
            float barHeight = 8f;

            var rowGo = new GameObject("ProgressRow", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);
            var rowRT = rowGo.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.5f, 0f);
            rowRT.anchorMax = new Vector2(0.5f, 0f);
            rowRT.pivot = new Vector2(0.5f, 0f);
            rowRT.anchoredPosition = new Vector2(0f, 200f);
            rowRT.sizeDelta = new Vector2(barWidth + 90f, 60f);

            var spinnerGo = new GameObject("Spinner", typeof(RectTransform), typeof(Image));
            spinnerGo.transform.SetParent(rowGo.transform, false);
            var spinImg = spinnerGo.GetComponent<Image>();
            spinImg.sprite = BuildSpinnerSprite();
            spinImg.color = new Color(1f, 0.5f, 0.18f, 1f);
            spinImg.type = Image.Type.Simple;
            spinImg.raycastTarget = false;
            var spinRT = spinImg.rectTransform;
            spinRT.anchorMin = new Vector2(0f, 0.5f);
            spinRT.anchorMax = new Vector2(0f, 0.5f);
            spinRT.pivot = new Vector2(0.5f, 0.5f);
            spinRT.anchoredPosition = new Vector2(28f, 30f);
            spinRT.sizeDelta = new Vector2(40f, 40f);
            _spinner = spinRT;

            var loadingGo = new GameObject("LoadingText", typeof(RectTransform));
            loadingGo.transform.SetParent(rowGo.transform, false);
            var loadTmp = loadingGo.AddComponent<TextMeshProUGUI>();
            loadTmp.text = "Loading";
            loadTmp.fontSize = 28f;
            loadTmp.fontStyle = FontStyles.Bold;
            loadTmp.alignment = TextAlignmentOptions.Left;
            loadTmp.color = new Color(0.92f, 0.88f, 0.82f, 1f);
            loadTmp.raycastTarget = false;
            var loadRT = loadTmp.rectTransform;
            loadRT.anchorMin = new Vector2(0f, 0.5f);
            loadRT.anchorMax = new Vector2(0f, 0.5f);
            loadRT.pivot = new Vector2(0f, 0.5f);
            loadRT.anchoredPosition = new Vector2(60f, 30f);
            loadRT.sizeDelta = new Vector2(200f, 40f);
            _loadingLabel = loadTmp;

            var trackGo = new GameObject("ProgressTrack", typeof(RectTransform), typeof(Image));
            trackGo.transform.SetParent(parent, false);
            var trackImg = trackGo.GetComponent<Image>();
            trackImg.color = new Color(1f, 1f, 1f, 0.12f);
            trackImg.raycastTarget = false;
            var trackRT = trackGo.GetComponent<RectTransform>();
            trackRT.anchorMin = new Vector2(0.5f, 0f);
            trackRT.anchorMax = new Vector2(0.5f, 0f);
            trackRT.pivot = new Vector2(0.5f, 0f);
            trackRT.anchoredPosition = new Vector2(0f, 175f);
            trackRT.sizeDelta = new Vector2(barWidth, barHeight);
            _progressTrack = trackRT;

            var fillGo = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(trackGo.transform, false);
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.color = new Color(1f, 0.5f, 0.18f, 1f);
            fillImg.raycastTarget = false;
            var fillRT = fillGo.GetComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0f, 0f);
            fillRT.anchorMax = new Vector2(0f, 1f);
            fillRT.pivot = new Vector2(0f, 0.5f);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            _progressFill = fillImg;
        }

        void BuildTipBlock(Transform parent)
        {
            var go = new GameObject("Tip", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultTip;
            tmp.fontSize = 22f;
            tmp.fontStyle = FontStyles.Italic;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.78f, 0.75f, 0.7f, 1f);
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            var rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 90f);
            rt.sizeDelta = new Vector2(1100f, 70f);
            _tipLabel = tmp;
        }

        static Sprite BuildSpinnerSprite()
        {
            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "PortalSpinner_Auto", filterMode = FilterMode.Bilinear };
            float c = size * 0.5f;
            float outerR = size * 0.46f;
            float innerR = size * 0.32f;
            float taperStart = 0.15f * Mathf.PI * 2f;
            float taperEnd   = 1.85f * Mathf.PI * 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Atan2(dy, dx);
                    if (a < 0) a += Mathf.PI * 2f;

                    float ringMask = (r >= innerR && r <= outerR) ? 1f : 0f;
                    float edgeSoft = Mathf.SmoothStep(0f, 1f, (r - innerR) / 2f) *
                                     Mathf.SmoothStep(0f, 1f, (outerR - r) / 2f);

                    float taper;
                    if (a < taperStart) taper = 0f;
                    else if (a > taperEnd) taper = 0f;
                    else taper = Mathf.SmoothStep(0f, 1f, (a - taperStart) / (taperEnd - taperStart));

                    float alpha = Mathf.Clamp01(ringMask * edgeSoft * taper);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
