using UnityEngine;

namespace DungeonBlade.Bank
{
    [ExecuteAlways]
    public class PortalVisual : MonoBehaviour
    {
        [Header("Existing mesh on this GameObject")]
        [SerializeField, Tooltip("Hide this object's own MeshRenderer (e.g. the placeholder cube) when you've parented a 3D model under it.")]
        bool hideOwnMesh = false;

        [Header("3D arch model (drag your FBX here)")]
        [SerializeField, Tooltip("Drag your portal arch FBX here. If set, this replaces the procedural stone frame.")]
        GameObject archModelPrefab;
        [SerializeField] Vector3 archModelPos = Vector3.zero;
        [SerializeField] Vector3 archModelEuler = Vector3.zero;
        [SerializeField, Tooltip("Local scale of the instantiated arch (compensates for portal's localScale).")]
        Vector3 archModelScale = new Vector3(0.5f, 0.333f, 2f);

        [Header("Procedural stone frame (used only if no arch model is assigned)")]
        [SerializeField] bool buildProceduralFrame = true;
        [SerializeField] Color frameColor = new Color(0.18f, 0.15f, 0.12f, 1f);
        [SerializeField] Color frameEmission = new Color(0.045f, 0.015f, 0.015f, 1f);
        [SerializeField, Tooltip("Frame thickness in world units.")]
        float frameThicknessWorld = 0.35f;
        [SerializeField, Tooltip("Frame depth multiplier vs the portal's depth (1 = same).")]
        float frameDepthMul = 1.6f;
        [SerializeField, Range(0f, 1f)] float frameSmoothness = 0.15f;
        [SerializeField, Range(0f, 1f)] float frameMetallic = 0.25f;

        [Header("Inner energy plane (the glowing portal surface)")]
        [SerializeField] bool buildEnergyPlane = true;
        [SerializeField] Color energyBaseColor = new Color(0.95f, 0.18f, 0.18f, 1f);
        [SerializeField, Tooltip("Local position of the energy plane relative to this GameObject. Ignored when 'Fit Energy To Arch Bounds' is on.")]
        Vector3 energyPlanePos = Vector3.zero;
        [SerializeField, Tooltip("Local euler rotation of the energy plane. Try (0,0,0) or (0,180,0) if it faces the wrong way.")]
        Vector3 energyPlaneEuler = Vector3.zero;
        [SerializeField, Tooltip("World-space size of the energy plane (width, height). Ignored when 'Fit Energy To Arch Bounds' is on.")]
        Vector2 energyPlaneSizeWorld = new Vector2(2f, 3f);

        [Header("Auto-fit energy to arch opening")]
        [SerializeField, Tooltip("When an arch model is assigned, auto-size the energy disc to fill the arch's renderer bounds.")]
        bool fitEnergyToArchBounds = true;
        [SerializeField, Tooltip("Energy disc size as a fraction of the arch bounds (x = width, y = height). 1 = full bounds, lower values shrink it inside the opening.")]
        Vector2 energyFitScale = new Vector2(0.42f, 0.55f);
        [SerializeField, Tooltip("Remove all colliders on the instantiated arch model so the player can walk through the opening. The interaction trigger lives on the parent Portal object.")]
        bool stripArchColliders = true;
        [SerializeField, Tooltip("Local-space offset added to the auto-fitted energy disc position (x = horizontal, y = vertical, z = depth).")]
        Vector3 energyFitOffset = Vector3.zero;

        [Header("Energy pulse")]
        [SerializeField] bool animateInnerEmission = true;
        [SerializeField] Color emissionLow = new Color(0.18f, 0.02f, 0.02f, 1f);
        [SerializeField] Color emissionHigh = new Color(1.50f, 0.25f, 0.25f, 1f);
        [SerializeField] float pulseSpeed = 1.2f;
        [SerializeField] Vector2 swirlScrollSpeed = new Vector2(0.05f, 0.15f);

        [Header("Glow light")]
        [SerializeField] bool addPointLight = true;
        [SerializeField] Color lightColor = new Color(0.95f, 0.18f, 0.18f, 1f);
        [SerializeField] float lightIntensityLow = 1.0f;
        [SerializeField] float lightIntensityHigh = 3.5f;
        [SerializeField] float lightRange = 8f;
        [SerializeField, Tooltip("Distance the light sits in front of the portal (local Z).")]
        float lightForwardOffset = -0.6f;

        [Header("Build")]
        [SerializeField] bool rebuildOnEnable = true;

        const string FrameTopName = "PortalFrame_Top_AutoVisual";
        const string FrameBottomName = "PortalFrame_Bottom_AutoVisual";
        const string FrameLeftName = "PortalFrame_Left_AutoVisual";
        const string FrameRightName = "PortalFrame_Right_AutoVisual";
        const string LightName = "PortalLight_AutoVisual";
        const string EnergyPlaneName = "PortalEnergy_AutoVisual";
        const string ArchInstanceName = "PortalArch_AutoVisual";

        Material _frameMat;
        Material _energyMat;
        Material _innerMatInstance;
        Light _portalLight;

        void OnEnable()
        {
            if (rebuildOnEnable) Build();
            CacheRuntimeRefs();
        }

        void Update()
        {
            if (!Application.isPlaying) return;
            Animate();
        }

        [ContextMenu("Rebuild Portal Visuals")]
        void RebuildContext() { Build(); CacheRuntimeRefs(); }

        void Build()
        {
            ApplyHideOwnMesh();

            if (archModelPrefab != null)
            {
                EnsureArchInstance();
                RemoveFrameIfExists();
            }
            else
            {
                RemoveArchInstanceIfExists();
                if (buildProceduralFrame) BuildFrame();
                else RemoveFrameIfExists();
            }

            if (buildEnergyPlane) BuildEnergyPlane();
            else RemoveEnergyPlaneIfExists();
            BuildLight();
        }

        void EnsureArchInstance()
        {
            Transform existing = transform.Find(ArchInstanceName);
            GameObject go;
            if (existing != null) go = existing.gameObject;
            else
            {
                go = Instantiate(archModelPrefab, transform);
                go.name = ArchInstanceName;
            }
            go.transform.localPosition = archModelPos;
            go.transform.localEulerAngles = archModelEuler;
            go.transform.localScale = archModelScale;

            if (stripArchColliders)
            {
                var cols = go.GetComponentsInChildren<Collider>(true);
                foreach (var c in cols) SafeDestroy(c);
            }
        }

        void RemoveArchInstanceIfExists()
        {
            var t = transform.Find(ArchInstanceName);
            if (t != null) SafeDestroy(t.gameObject);
        }

        void ApplyHideOwnMesh()
        {
            var r = GetComponent<Renderer>();
            if (r != null) r.enabled = !hideOwnMesh;
        }

        void RemoveFrameIfExists()
        {
            foreach (var name in new[] { FrameTopName, FrameBottomName, FrameLeftName, FrameRightName })
            {
                var t = transform.Find(name);
                if (t != null) SafeDestroy(t.gameObject);
            }
        }

        void RemoveEnergyPlaneIfExists()
        {
            var t = transform.Find(EnergyPlaneName);
            if (t != null) SafeDestroy(t.gameObject);
        }

        void BuildEnergyPlane()
        {
            Vector3 ps = transform.localScale;
            if (ps.x <= 0f || ps.y <= 0f) return;

            Transform existing = transform.Find(EnergyPlaneName);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(EnergyPlaneName, typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(transform, false);
            }

            var col = go.GetComponent<Collider>();
            if (col != null) SafeDestroy(col);

            var mf = go.GetComponent<MeshFilter>();
            if (mf == null) mf = go.AddComponent<MeshFilter>();
            if (mf.sharedMesh == null || mf.sharedMesh.name != "PortalDisc_Auto")
                mf.sharedMesh = GenerateDiscMesh(48);

            if (go.GetComponent<MeshRenderer>() == null) go.AddComponent<MeshRenderer>();

            Vector3 localPos = energyPlanePos;
            Vector2 sizeWorld = energyPlaneSizeWorld;

            if (fitEnergyToArchBounds && archModelPrefab != null
                && TryGetArchLocalBounds(out Vector3 archCenterLocal, out Vector3 archSizeLocal))
            {
                localPos = archCenterLocal + energyFitOffset;
                sizeWorld = new Vector2(archSizeLocal.x * ps.x * energyFitScale.x,
                                        archSizeLocal.y * ps.y * energyFitScale.y);
            }

            go.transform.localPosition = localPos;
            go.transform.localEulerAngles = energyPlaneEuler;
            go.transform.localScale = new Vector3(sizeWorld.x / ps.x, sizeWorld.y / ps.y, 1f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (_energyMat == null) _energyMat = CreateEnergyMaterial();
                renderer.sharedMaterial = _energyMat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        bool TryGetArchLocalBounds(out Vector3 centerLocal, out Vector3 sizeLocal)
        {
            centerLocal = Vector3.zero;
            sizeLocal = Vector3.zero;

            Transform archT = transform.Find(ArchInstanceName);
            if (archT == null) return false;

            var renderers = archT.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0) return false;

            bool initialized = false;
            Bounds localBounds = new Bounds();
            Matrix4x4 worldToLocal = transform.worldToLocalMatrix;

            foreach (var r in renderers)
            {
                if (r == null) continue;
                Bounds wb = r.bounds;
                Vector3 c = wb.center;
                Vector3 e = wb.extents;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = new Vector3(
                        c.x + (((i & 1) == 0) ? -e.x : e.x),
                        c.y + (((i & 2) == 0) ? -e.y : e.y),
                        c.z + (((i & 4) == 0) ? -e.z : e.z));
                    Vector3 lp = worldToLocal.MultiplyPoint3x4(corner);
                    if (!initialized) { localBounds = new Bounds(lp, Vector3.zero); initialized = true; }
                    else localBounds.Encapsulate(lp);
                }
            }

            if (!initialized) return false;
            centerLocal = localBounds.center;
            sizeLocal = localBounds.size;
            return true;
        }

        static Mesh GenerateDiscMesh(int segments)
        {
            var mesh = new Mesh { name = "PortalDisc_Auto" };
            var verts = new Vector3[segments + 1];
            var uvs = new Vector2[segments + 1];
            var tris = new int[segments * 3];

            verts[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < segments; i++)
            {
                float a = (i / (float)segments) * Mathf.PI * 2f;
                float x = Mathf.Cos(a) * 0.5f;
                float y = Mathf.Sin(a) * 0.5f;
                verts[i + 1] = new Vector3(x, y, 0f);
                uvs[i + 1] = new Vector2(x + 0.5f, y + 0.5f);

                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = ((i + 1) % segments) + 1;
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        Material CreateEnergyMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                var portalRenderer = GetComponent<Renderer>();
                if (portalRenderer != null && portalRenderer.sharedMaterial != null)
                    shader = portalRenderer.sharedMaterial.shader;
            }
            if (shader == null) shader = Shader.Find("Standard");

            var m = new Material(shader) { name = "PortalEnergy_AutoMaterial" };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", energyBaseColor);
            if (m.HasProperty("_Color")) m.color = energyBaseColor;
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emissionLow);

            var swirl = CreateSwirlTexture(256);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", swirl);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", swirl);
            if (m.HasProperty("_EmissionMap")) m.SetTexture("_EmissionMap", swirl);
            return m;
        }

        static Texture2D CreateSwirlTexture(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "PortalSwirl_Auto", wrapMode = TextureWrapMode.Repeat };
            float c = size * 0.5f;
            float maxR = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - c) / maxR;
                    float dy = (y - c) / maxR;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float ang = Mathf.Atan2(dy, dx);

                    float swirl1 = Mathf.Sin(ang * 6f + r * 10f);
                    float swirl2 = Mathf.Sin(ang * 3f - r * 8f + Mathf.PI * 0.5f);
                    float flow = Mathf.Sin(dx * 8f + dy * 4f) * Mathf.Cos(dy * 6f - dx * 2f);

                    float brightness = 0.70f + 0.18f * swirl1 + 0.12f * swirl2 + 0.10f * flow;
                    brightness = Mathf.Clamp01(brightness);

                    tex.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
                }
            }
            tex.Apply();
            return tex;
        }

        void BuildFrame()
        {
            Vector3 ps = transform.localScale;
            if (ps.x <= 0f || ps.y <= 0f || ps.z <= 0f) return;

            float tx = frameThicknessWorld / ps.x;
            float ty = frameThicknessWorld / ps.y;
            float dz = frameDepthMul;

            float fullWLocal = 1f + 2f * tx;
            float topY = 0.5f + ty * 0.5f;
            float bottomY = -0.5f - ty * 0.5f;
            float leftX = -0.5f - tx * 0.5f;
            float rightX = 0.5f + tx * 0.5f;

            EnsureFrameBar(FrameTopName, new Vector3(0f, topY, 0f), new Vector3(fullWLocal, ty, dz));
            EnsureFrameBar(FrameBottomName, new Vector3(0f, bottomY, 0f), new Vector3(fullWLocal, ty, dz));
            EnsureFrameBar(FrameLeftName, new Vector3(leftX, 0f, 0f), new Vector3(tx, 1f, dz));
            EnsureFrameBar(FrameRightName, new Vector3(rightX, 0f, 0f), new Vector3(tx, 1f, dz));
        }

        void EnsureFrameBar(string name, Vector3 localPos, Vector3 localScale)
        {
            Transform existing = transform.Find(name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = name;
                go.transform.SetParent(transform, false);
                var col = go.GetComponent<Collider>();
                if (col != null) SafeDestroy(col);
            }

            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (_frameMat == null) _frameMat = CreateLitMaterial(frameColor, frameEmission, frameSmoothness, frameMetallic);
                renderer.sharedMaterial = _frameMat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        void BuildLight()
        {
            Transform existing = transform.Find(LightName);
            if (!addPointLight)
            {
                if (existing != null) SafeDestroy(existing.gameObject);
                return;
            }

            GameObject go;
            if (existing != null) go = existing.gameObject;
            else
            {
                go = new GameObject(LightName);
                go.transform.SetParent(transform, false);
            }

            go.transform.localPosition = new Vector3(0f, 0f, lightForwardOffset);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var light = go.GetComponent<Light>();
            if (light == null) light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightColor;
            light.range = lightRange;
            light.intensity = lightIntensityLow;
            light.shadows = LightShadows.Soft;
        }

        void CacheRuntimeRefs()
        {
            var r = GetComponent<Renderer>();
            if (r != null) _innerMatInstance = Application.isPlaying ? r.material : r.sharedMaterial;

            var lightT = transform.Find(LightName);
            if (lightT != null) _portalLight = lightT.GetComponent<Light>();

            var energyT = transform.Find(EnergyPlaneName);
            if (energyT != null)
            {
                var er = energyT.GetComponent<Renderer>();
                if (er != null) _energyMat = Application.isPlaying ? er.material : er.sharedMaterial;
            }
        }

        void Animate()
        {
            float t = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            Color em = Color.Lerp(emissionLow, emissionHigh, t);

            if (animateInnerEmission && _innerMatInstance != null && !hideOwnMesh)
            {
                if (!_innerMatInstance.IsKeywordEnabled("_EMISSION"))
                    _innerMatInstance.EnableKeyword("_EMISSION");
                if (_innerMatInstance.HasProperty("_EmissionColor"))
                    _innerMatInstance.SetColor("_EmissionColor", em);
            }

            if (animateInnerEmission && _energyMat != null)
            {
                if (!_energyMat.IsKeywordEnabled("_EMISSION"))
                    _energyMat.EnableKeyword("_EMISSION");
                if (_energyMat.HasProperty("_EmissionColor"))
                    _energyMat.SetColor("_EmissionColor", em);
                if (_energyMat.HasProperty("_BaseColor"))
                    _energyMat.SetColor("_BaseColor", em);
                if (_energyMat.HasProperty("_Color"))
                    _energyMat.SetColor("_Color", em);

                Vector2 offset = swirlScrollSpeed * Time.time;
                if (_energyMat.HasProperty("_BaseMap")) _energyMat.SetTextureOffset("_BaseMap", offset);
                if (_energyMat.HasProperty("_MainTex")) _energyMat.SetTextureOffset("_MainTex", offset);
                if (_energyMat.HasProperty("_EmissionMap")) _energyMat.SetTextureOffset("_EmissionMap", offset);
            }

            if (_portalLight != null)
                _portalLight.intensity = Mathf.Lerp(lightIntensityLow, lightIntensityHigh, t);
        }

        Material CreateLitMaterial(Color baseColor, Color emission, float smoothness, float metallic)
        {
            var portalRenderer = GetComponent<Renderer>();
            Shader shader = null;
            if (portalRenderer != null && portalRenderer.sharedMaterial != null)
                shader = portalRenderer.sharedMaterial.shader;
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var m = new Material(shader) { name = "PortalFrame_AutoMaterial" };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", baseColor);
            if (m.HasProperty("_Color")) m.color = baseColor;
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            return m;
        }

        static void SafeDestroy(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Destroy(o); else DestroyImmediate(o);
        }
    }
}
