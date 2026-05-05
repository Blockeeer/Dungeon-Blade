using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DungeonBlade.UI.Menus
{
    public class Hero3DPreview : MonoBehaviour
    {
        public RenderTexture RT { get; private set; }

        const int RT_WIDTH = 512;
        const int RT_HEIGHT = 704;
        const int PREVIEW_LAYER = 31;
        static readonly Vector3 RIG_WORLD_POS = new Vector3(10000f, 10000f, 10000f);

        Camera _camera;
        Light _keyLight, _rimLight, _fillLight;
        Transform _modelRoot;
        GameObject _currentModel;
        PlayableGraph _graph;
        AnimationClipPlayable _clipPlayable;
        double _clipLength;
        bool _graphValid;

        public static Hero3DPreview Create()
        {
            var go = new GameObject("Hero3DPreview");
            go.hideFlags = HideFlags.HideInHierarchy;
            return go.AddComponent<Hero3DPreview>();
        }

        void Awake()
        {
            transform.position = RIG_WORLD_POS;

            RT = new RenderTexture(RT_WIDTH, RT_HEIGHT, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            RT.Create();

            var pivot = new GameObject("ModelPivot");
            pivot.transform.SetParent(transform, false);
            pivot.transform.localPosition = Vector3.zero;
            pivot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            _modelRoot = pivot.transform;

            var camGO = new GameObject("PreviewCamera");
            camGO.transform.SetParent(transform, false);
            camGO.transform.localPosition = new Vector3(0f, 0.95f, -3.4f);
            camGO.transform.localRotation = Quaternion.Euler(2f, 0f, 0f);
            _camera = camGO.AddComponent<Camera>();
            _camera.targetTexture = RT;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _camera.fieldOfView = 28f;
            _camera.nearClipPlane = 0.05f;
            _camera.farClipPlane = 12f;
            _camera.allowHDR = false;
            _camera.allowMSAA = true;
            _camera.useOcclusionCulling = false;
            _camera.depth = -50;
            _camera.cullingMask = 1 << PREVIEW_LAYER;

            _keyLight = MakeLight("KeyLight",
                pos: new Vector3(1.6f, 2.4f, -1.4f),
                rot: Quaternion.Euler(35f, 200f, 0f),
                color: new Color(1f, 0.96f, 0.88f),
                intensity: 1.45f);

            _rimLight = MakeLight("RimLight",
                pos: new Vector3(-1.4f, 1.6f, 1.6f),
                rot: Quaternion.Euler(15f, 30f, 0f),
                color: new Color(0.95f, 0.30f, 0.30f),
                intensity: 1.30f);

            _fillLight = MakeLight("FillLight",
                pos: new Vector3(-1.0f, 1.0f, -1.4f),
                rot: Quaternion.Euler(10f, 160f, 0f),
                color: new Color(0.55f, 0.65f, 0.85f),
                intensity: 0.45f);
        }

        Light MakeLight(string name, Vector3 pos, Quaternion rot, Color color, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = color;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            l.cullingMask = 1 << PREVIEW_LAYER;
            return l;
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; i++)
                SetLayerRecursive(go.transform.GetChild(i).gameObject, layer);
        }

        public void Show(string id, GameObject prefab, AnimationClip idleClip, Avatar fallbackAvatar, Color accent)
        {
            DisposeGraph();
            if (_currentModel != null)
            {
                Destroy(_currentModel);
                _currentModel = null;
            }
            if (prefab == null) return;

            _currentModel = Instantiate(prefab, _modelRoot);
            _currentModel.name = "Model_" + id;
            _currentModel.transform.localPosition = Vector3.zero;
            _currentModel.transform.localRotation = Quaternion.identity;
            _currentModel.transform.localScale = Vector3.one;
            _currentModel.SetActive(true);
            SetLayerRecursive(_currentModel, PREVIEW_LAYER);

            _rimLight.color = Color.Lerp(accent, Color.white, 0.25f);
            _rimLight.intensity = 1.35f;

            var animator = _currentModel.GetComponent<Animator>();
            if (animator == null) animator = _currentModel.GetComponentInChildren<Animator>();
            if (animator == null) animator = _currentModel.AddComponent<Animator>();

            if (animator.avatar == null && fallbackAvatar != null)
                animator.avatar = fallbackAvatar;

            if (animator != null && idleClip != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.Normal;
                if (!animator.enabled) animator.enabled = true;

                _graph = PlayableGraph.Create("HeroPreview_" + id);
                _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

                _clipPlayable = AnimationClipPlayable.Create(_graph, idleClip);
                _clipPlayable.SetApplyFootIK(false);

                var output = AnimationPlayableOutput.Create(_graph, "Anim", animator);
                output.SetSourcePlayable(_clipPlayable);

                _clipLength = idleClip.length > 0f ? idleClip.length : 1.0;
                _clipPlayable.SetTime(0.0);

                _graph.Play();
                animator.Rebind();
                animator.Update(0f);
                _graphValid = true;
            }
        }

        void LateUpdate()
        {
            if (!_graphValid || !_graph.IsValid() || !_clipPlayable.IsValid()) return;
            if (_clipLength <= 0.0) return;

            double t = _clipPlayable.GetTime();
            if (t >= _clipLength) _clipPlayable.SetTime(t - _clipLength);
        }

        public void Hide()
        {
            DisposeGraph();
            if (_currentModel != null)
            {
                Destroy(_currentModel);
                _currentModel = null;
            }
        }

        void DisposeGraph()
        {
            if (_graphValid && _graph.IsValid())
            {
                _graph.Destroy();
            }
            _graphValid = false;
        }

        void OnDestroy()
        {
            DisposeGraph();
            if (RT != null)
            {
                RT.Release();
                Destroy(RT);
                RT = null;
            }
        }
    }
}
