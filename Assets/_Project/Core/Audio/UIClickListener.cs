using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonBlade.Core.Audio
{
    // Plays a click sound whenever any UI Button is pressed anywhere in the
    // game. Uses Unity's IPointerClickHandler dispatch — no per-button wiring.
    // Drop one of these into the LandingScene (DontDestroyOnLoad) and it
    // covers every Button in every scene.
    public class UIClickListener : MonoBehaviour, IPointerClickHandler
    {
        public static UIClickListener Instance { get; private set; }

        [SerializeField] AudioClip clickClip;
        [SerializeField] float volume = 0.5f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEnable()
        {
            // Subscribe to every Button found at startup, plus any that load later.
            HookAllButtonsInScene();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            HookAllButtonsInScene();
        }

        void HookAllButtonsInScene()
        {
            // FindObjectsOfType picks up disabled buttons too via includeInactive.
            var buttons = FindObjectsOfType<Button>(true);
            foreach (var b in buttons)
            {
                b.onClick.RemoveListener(PlayClick);
                b.onClick.AddListener(PlayClick);
            }
        }

        public void PlayClick()
        {
            if (clickClip != null) SfxPool.TryPlay(clickClip, volume);
        }

        // IPointerClickHandler is intentionally a no-op — kept so this class is
        // a valid pointer target if you ever want to dispatch events to it.
        public void OnPointerClick(PointerEventData eventData) { }
    }
}
