using DungeonBlade.Core;
using DungeonBlade.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.UI.Menus
{
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }

        [SerializeField] GameObject pausePanel;
        [SerializeField] GameObject settingsPanel;
        [SerializeField] Button resumeButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button returnLobbyButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button quitButton;
        [SerializeField] InventoryPersistence persistence;

        bool _styled;

        PlayerInputActions _input;
        bool _isOpen;

        public bool IsOpen => _isOpen;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        void Start()
        {
            _input = InputManager.Instance != null ? InputManager.Instance.Actions : new PlayerInputActions();
            if (InputManager.Instance == null) _input.Enable();

            ResolveMissingButtons();

            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            if (resumeButton != null) resumeButton.onClick.AddListener(Close);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (returnLobbyButton != null) returnLobbyButton.onClick.AddListener(() => SaveAndLoad(SceneLoader.Lobby));
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() => SaveAndLoad(SceneLoader.MainMenu));
            if (quitButton != null) quitButton.onClick.AddListener(QuitToDesktop);

            ApplyStyle();
        }

        void ResolveMissingButtons()
        {
            if (pausePanel == null) return;
            var root = pausePanel.transform;
            if (resumeButton == null)      resumeButton      = FindButtonByName(root, "ResumeButton");
            if (settingsButton == null)    settingsButton    = FindButtonByName(root, "SettingsButton");
            if (returnLobbyButton == null) returnLobbyButton = FindButtonByName(root, "ReturnLobbyButton");
            if (mainMenuButton == null)    mainMenuButton    = FindButtonByName(root, "MainMenuButton");
            if (quitButton == null)        quitButton        = FindButtonByName(root, "QuitButton");
        }

        static Button FindButtonByName(Transform root, string name)
        {
            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b != null && b.gameObject.name == name) return b;
            }
            return null;
        }

        void Update()
        {
            if (_input == null) return;
            if (_input.Pause.WasPressedThisFrame())
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    settingsPanel.SetActive(false);
                    return;
                }
                Toggle();
            }
        }

        public void Toggle() { if (_isOpen) Close(); else Open(); }

        public void Open()
        {
            if (_isOpen) return;
            if (MenuState.IsAnyOpen) return;
            _isOpen = true;
            if (pausePanel != null) pausePanel.SetActive(true);
            MenuState.Push();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            MenuState.Pop();
            Time.timeScale = 1f;
            if (!MenuState.IsAnyOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void OpenSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        void SaveAndLoad(string sceneName)
        {
            Time.timeScale = 1f;
            if (persistence != null) persistence.SaveNow();
            if (FadeLoader.Instance != null) FadeLoader.Instance.LoadScene(sceneName);
            else SceneLoader.Load(sceneName);
        }

        void QuitToDesktop()
        {
            Time.timeScale = 1f;
            if (persistence != null) persistence.SaveNow();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---- Styling ----

        void ApplyStyle()
        {
            if (_styled || pausePanel == null) return;
            _styled = true;

            var panelRT = pausePanel.transform as RectTransform;
            if (panelRT == null) return;

            BuildDimAndCard(panelRT);
            var card = panelRT.Find("PauseCard") as RectTransform;

            var label = FindLabel(panelRT.transform, "PAUSED");
            if (label != null && card != null)
            {
                MenuFx.StylizeTitle(label, "PAUSED", fontSize: 48, spacing: 26f);
                label.color = MenuFx.SteelTint;

                var labelRT = label.rectTransform;
                labelRT.SetParent(card, worldPositionStays: false);
                labelRT.anchorMin = new Vector2(0.5f, 1f);
                labelRT.anchorMax = new Vector2(0.5f, 1f);
                labelRT.pivot = new Vector2(0.5f, 1f);
                labelRT.anchoredPosition = new Vector2(0f, -34f);
                labelRT.sizeDelta = new Vector2(420f, 60f);

                var le = label.GetComponent<LayoutElement>();
                if (le == null) le = label.gameObject.AddComponent<LayoutElement>();
                le.ignoreLayout = true;

                EnsureCardEdge(card, "PausedAccent",
                    anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f),
                    pivot: new Vector2(0.5f, 1f), pos: new Vector2(0f, -100f),
                    size: new Vector2(120f, 2f),
                    color: new Color(MenuFx.AccentRed.r, MenuFx.AccentRed.g, MenuFx.AccentRed.b, 0.95f));
            }

            ConfigureButtonGroup();

            TMP_FontAsset font = label != null ? label.font : null;
            MenuFx.StylizeButton(resumeButton,       font, "RESUME",            width: 440f, height: 56f);
            MenuFx.StylizeButton(settingsButton,     font, "SETTINGS",          width: 440f, height: 56f);
            MenuFx.StylizeButton(returnLobbyButton,  font, "RETURN  TO  LOBBY", width: 440f, height: 56f);
            MenuFx.StylizeButton(mainMenuButton,     font, "MAIN  MENU",        width: 440f, height: 56f);
            MenuFx.StylizeButton(quitButton,         font, "QUIT  TO  DESKTOP", width: 440f, height: 56f);
        }

        void BuildDimAndCard(RectTransform panelRT)
        {
            var panelFitter = panelRT.GetComponent<ContentSizeFitter>();
            if (panelFitter != null) panelFitter.enabled = false;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = Vector2.zero;

            var dimT = panelRT.Find("PauseDim");
            Image dim;
            if (dimT == null)
            {
                dim = MenuFx.CreateImage(panelRT, "PauseDim", 0);
            }
            else
            {
                dim = dimT.GetComponent<Image>() ?? dimT.gameObject.AddComponent<Image>();
                dimT.SetSiblingIndex(0);
            }
            var dimRT = dim.rectTransform;
            dimRT.anchorMin = Vector2.zero;
            dimRT.anchorMax = Vector2.one;
            dimRT.pivot = new Vector2(0.5f, 0.5f);
            dimRT.anchoredPosition = Vector2.zero;
            dimRT.sizeDelta = Vector2.zero;
            dim.color = new Color(0f, 0f, 0f, 0.42f);
            dim.raycastTarget = true;
            IgnoreFromLayout(dim.gameObject);

            var cardT = panelRT.Find("PauseCard");
            Image card;
            if (cardT == null)
            {
                card = MenuFx.CreateImage(panelRT, "PauseCard", 1);
            }
            else
            {
                card = cardT.GetComponent<Image>() ?? cardT.gameObject.AddComponent<Image>();
                cardT.SetSiblingIndex(1);
            }
            var cardRT = card.rectTransform;
            cardRT.anchorMin = new Vector2(0.5f, 0.5f);
            cardRT.anchorMax = new Vector2(0.5f, 0.5f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.anchoredPosition = Vector2.zero;
            cardRT.sizeDelta = new Vector2(560f, 640f);
            card.sprite = MenuFx.SlabSprite();
            card.type = Image.Type.Simple;
            card.color = new Color(0.055f, 0.075f, 0.105f, 0.55f);
            card.raycastTarget = true;
            IgnoreFromLayout(card.gameObject);

            EnsureCardEdge(card.transform, "CardStripe", anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(0f, 1f), pivot: new Vector2(0f, 0.5f),
                pos: Vector2.zero, size: new Vector2(4f, 0f), color: MenuFx.AccentRed);

            EnsureCardEdge(card.transform, "CardTopAccent", anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f), pivot: new Vector2(0.5f, 1f),
                pos: new Vector2(0f, -16f), size: new Vector2(-80f, 1f),
                color: new Color(MenuFx.MutedSteel.r, MenuFx.MutedSteel.g, MenuFx.MutedSteel.b, 0.35f));
        }

        static void EnsureCardEdge(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
                                   Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            var t = parent.Find(name);
            Image img;
            if (t == null)
            {
                img = MenuFx.CreateImage(parent as RectTransform, name, parent.childCount);
            }
            else
            {
                img = t.GetComponent<Image>() ?? t.gameObject.AddComponent<Image>();
            }
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            img.color = color;
            img.raycastTarget = false;
        }

        void ConfigureButtonGroup()
        {
            if (resumeButton == null) return;
            var groupRT = resumeButton.transform.parent as RectTransform;
            if (groupRT == null) return;

            var fitter = groupRT.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
            }

            var vlg = groupRT.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 12f;
                vlg.padding = new RectOffset(24, 24, 24, 24);
                vlg.childAlignment = TextAnchor.MiddleCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = false;
                vlg.childForceExpandHeight = false;
            }
        }

        static TMP_Text FindLabel(Transform root, string match)
        {
            var labels = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (var l in labels)
            {
                if (l == null) continue;
                if (l.GetComponentInParent<Button>() != null) continue;
                if (string.Equals(l.text?.Trim(), match, System.StringComparison.OrdinalIgnoreCase))
                    return l;
            }
            foreach (var l in labels)
            {
                if (l != null && l.GetComponentInParent<Button>() == null) return l;
            }
            return null;
        }

        static void IgnoreFromLayout(GameObject go)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
        }
    }
}
