using System.IO;
using DungeonBlade.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonBlade.UI.Menus
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] Button newGameButton;
        [SerializeField] Button continueButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button quitButton;
        [SerializeField] GameObject settingsPanel;
        [SerializeField] TMP_Text versionLabel;
        [SerializeField] TMP_Text titleLabel;

        CharacterSelectController _charSelect;

        void Start()
        {
            if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

            if (continueButton != null) continueButton.interactable = SaveExists();
            if (versionLabel != null) versionLabel.text = $"v{Application.version}";
            if (settingsPanel != null) settingsPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ApplyStyle();
        }

        bool SaveExists()
        {
            string path = Path.Combine(Application.persistentDataPath, "DungeonBlade", "profile.json");
            return File.Exists(path);
        }

        void OnNewGame()
        {
            var canvas = ResolveMenuCanvas();
            if (canvas == null)
            {
                DeleteSave();
                LoadScene(SceneLoader.Lobby);
                return;
            }

            if (_charSelect == null) _charSelect = new CharacterSelectController();

            var font = titleLabel != null ? titleLabel.font : null;
            _charSelect.Open(canvas, font,
                onConfirm: id =>
                {
                    DeleteSave();
                    SaveChosenCharacter(id);
                    LoadScene(SceneLoader.Lobby);
                },
                onCancel: () => { });
        }

        static void SaveChosenCharacter(string characterId)
        {
            var save = new SaveSystem();
            save.Load();
            save.Profile.characterId = characterId;
            save.Save();
        }

        void OnContinue()
        {
            LoadScene(SceneLoader.Lobby);
        }

        void OnSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void DeleteSave()
        {
            string root = Path.Combine(Application.persistentDataPath, "DungeonBlade");
            string profile = Path.Combine(root, "profile.json");
            string bank = Path.Combine(root, "bank.json");
            if (File.Exists(profile)) File.Delete(profile);
            if (File.Exists(bank)) File.Delete(bank);
            Debug.Log("[MainMenu] New game — old save deleted.");
        }

        void LoadScene(string sceneName)
        {
            if (FadeLoader.Instance != null) FadeLoader.Instance.LoadScene(sceneName);
            else SceneLoader.Load(sceneName);
        }

        // --- Styling ---

        void ApplyStyle()
        {
            Canvas canvas = ResolveMenuCanvas();
            if (canvas != null)
            {
                MenuFx.BuildBackdrop(canvas, existingBgToReplace: null);
            }

            var titleText = titleLabel;
            if (titleText == null && canvas != null)
            {
                var titleT = FindDescendant(canvas.transform, "Title");
                if (titleT != null) titleText = titleT.GetComponent<TMP_Text>();
            }

            if (titleText != null)
            {
                MenuFx.StylizeTitle(titleText, "DUNGEON  BLADE", fontSize: 88, spacing: 18f);
                var titleRT = titleText.rectTransform;
                titleRT.sizeDelta = new Vector2(1500f, 120f);
                MenuFx.AddTitleEcho(titleText, new Vector2(3f, -3f));

                float halfH = titleRT.sizeDelta.y * 0.5f;

                var sub = MenuFx.AddText(
                    titleRT.parent as RectTransform, "Subtitle", titleText.font,
                    Vector2.zero,
                    new Vector2(900f, 22f),
                    "A   B L A D E   &   G U N   D U E L",
                    fontSize: 16,
                    color: new Color(MenuFx.MutedSteel.r, MenuFx.MutedSteel.g, MenuFx.MutedSteel.b, 0.75f),
                    spacing: 22f,
                    style: FontStyles.Bold);

                AlignToTitleFrame(sub != null ? sub.rectTransform : null, titleRT,
                    new Vector2(titleRT.anchoredPosition.x,
                                titleRT.anchoredPosition.y - halfH - 20f));

                AddAccentBar(titleRT, yOffset: -halfH - 6f, width: 380f, alpha: 0.9f);
            }

            ConfigureButtonGroup();

            var font = titleText != null ? titleText.font : null;
            StylizeMenuButton(newGameButton, font, "NEW  GAME");
            StylizeMenuButton(continueButton, font, "CONTINUE");
            StylizeMenuButton(settingsButton, font, "SETTINGS");
            StylizeMenuButton(quitButton, font, "QUIT");

            if (versionLabel != null)
            {
                versionLabel.color = new Color(MenuFx.MutedSteel.r, MenuFx.MutedSteel.g, MenuFx.MutedSteel.b, 0.5f);
                versionLabel.fontSize = 14;
                versionLabel.characterSpacing = 6f;
                versionLabel.fontStyle = FontStyles.Bold;
            }
        }

        Canvas ResolveMenuCanvas()
        {
            if (newGameButton != null)
            {
                var c = newGameButton.GetComponentInParent<Canvas>();
                if (c != null) return c;
            }
            if (titleLabel != null)
            {
                var c = titleLabel.GetComponentInParent<Canvas>();
                if (c != null) return c;
            }
            var all = FindObjectsOfType<Canvas>();
            foreach (var c in all)
            {
                if (c.gameObject.scene == gameObject.scene) return c;
            }
            return null;
        }

        void ConfigureButtonGroup()
        {
            if (newGameButton == null) return;
            var groupRT = newGameButton.transform.parent as RectTransform;
            if (groupRT == null) return;

            var fitter = groupRT.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit   = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            }

            var vlg = groupRT.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 14f;
                vlg.padding = new RectOffset(12, 12, 12, 12);
                vlg.childAlignment = TextAnchor.MiddleCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = false;
                vlg.childForceExpandHeight = false;
            }
        }

        static void StylizeMenuButton(Button btn, TMP_FontAsset font, string label)
        {
            if (btn == null) return;
            MenuFx.StylizeButton(btn, font, label, width: 460f, height: 64f);
        }

        static void AlignToTitleFrame(RectTransform target, RectTransform titleRT, Vector2 anchoredPos)
        {
            if (target == null || titleRT == null) return;
            target.anchorMin = titleRT.anchorMin;
            target.anchorMax = titleRT.anchorMax;
            target.pivot = titleRT.pivot;
            target.anchoredPosition = anchoredPos;
        }

        static Transform FindDescendant(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDescendant(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        static void AddAccentBar(RectTransform titleRT, float yOffset, float width, float alpha)
        {
            var parent = titleRT.parent as RectTransform;
            if (parent == null) return;

            var img = MenuFx.CreateImage(parent, "TitleAccent", titleRT.GetSiblingIndex());
            var rt = img.rectTransform;
            rt.anchorMin = titleRT.anchorMin;
            rt.anchorMax = titleRT.anchorMax;
            rt.pivot = titleRT.pivot;
            rt.anchoredPosition = new Vector2(titleRT.anchoredPosition.x,
                                              titleRT.anchoredPosition.y + yOffset);
            rt.sizeDelta = new Vector2(width, 2f);
            img.color = new Color(MenuFx.AccentRed.r, MenuFx.AccentRed.g, MenuFx.AccentRed.b, alpha);
            img.raycastTarget = false;
        }
    }
}
