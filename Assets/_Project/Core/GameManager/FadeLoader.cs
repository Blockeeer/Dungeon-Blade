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
        [SerializeField] float defaultFadeDuration = 0.4f;
        [SerializeField, Tooltip("Optional label shown while a scene loads. If null, one is built automatically into the fade canvas.")]
        TMP_Text loadingLabel;
        [SerializeField] string loadingText = "Loading...";

        bool _busy;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (fadeGroup != null) fadeGroup.alpha = 0f;
            if (fadeImage != null) fadeImage.raycastTarget = false;
            EnsureLoadingLabel();
            SetLoadingVisible(false);
        }

        public void LoadScene(string sceneName, float fadeDuration = -1f)
        {
            if (_busy) return;
            float dur = fadeDuration > 0f ? fadeDuration : defaultFadeDuration;
            StartCoroutine(LoadRoutine(sceneName, dur));
        }

        IEnumerator LoadRoutine(string sceneName, float dur)
        {
            _busy = true;
            yield return Fade(0f, 1f, dur);

            SetLoadingVisible(true);

            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null)
            {
                op.allowSceneActivation = true;
                while (!op.isDone) yield return null;
            }

            SetLoadingVisible(false);

            yield return Fade(1f, 0f, dur);
            _busy = false;
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

        void SetLoadingVisible(bool visible)
        {
            if (loadingLabel == null) return;
            loadingLabel.gameObject.SetActive(visible);
            if (visible) loadingLabel.text = string.IsNullOrEmpty(loadingText) ? "Loading..." : loadingText;
        }

        void EnsureLoadingLabel()
        {
            if (loadingLabel != null) return;
            if (fadeGroup == null) return;

            var go = new GameObject("LoadingLabel", typeof(RectTransform));
            go.transform.SetParent(fadeGroup.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800f, 120f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = loadingText;
            tmp.fontSize = 56f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.9f, 0.85f, 1f);
            tmp.raycastTarget = false;
            loadingLabel = tmp;
        }
    }
}
