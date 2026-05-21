using UnityEngine;

namespace DungeonBlade.Bank
{
    /// <summary>
    /// Scatters real prefabs (Synty PolygonStarter / PolygonGeneric, etc.) around the lobby
    /// instead of procedural primitives. Trees, bushes, grass tufts, rocks, and distant
    /// mountains are placed in rings around the courtyard, avoiding the courtyard rectangle.
    ///
    /// Attach to an empty GameObject at the world origin. Drag prefabs from the asset pack
    /// into the inspector slots. Disable the procedural toggles on LobbyEnvironmentDecor
    /// ("Build Distant Mountains" and "Build Outer Rocks") so the two systems don't double up.
    /// </summary>
    [DisallowMultipleComponent]
    public class LobbyAssetSpawner : MonoBehaviour
    {
        [Header("Courtyard keep-out (skip props in this rectangle)")]
        [Tooltip("World-space center X of the courtyard rectangle to skip.")]
        [SerializeField] float courtyardCenterX = 0f;
        [Tooltip("World-space center Z of the courtyard rectangle to skip.")]
        [SerializeField] float courtyardCenterZ = -4f;
        [SerializeField] float courtyardHalfWidth = 14f;
        [SerializeField] float courtyardHalfDepth = 18f;

        [Header("Deterministic seed (change to reshuffle layout)")]
        [SerializeField] int seed = 12345;

        [Header("Trees — woodland between courtyard and mountains")]
        [SerializeField] GameObject[] treePrefabs;
        [SerializeField] int treeCount = 50;
        [SerializeField] float treeInnerRadius = 16f;
        [SerializeField] float treeOuterRadius = 55f;
        [SerializeField] Vector2 treeScaleRange = new Vector2(0.9f, 1.5f);
        [SerializeField] float treeYOffset = 0f;

        [Header("Bushes")]
        [SerializeField] GameObject[] bushPrefabs;
        [SerializeField] int bushCount = 60;
        [SerializeField] float bushInnerRadius = 14f;
        [SerializeField] float bushOuterRadius = 50f;
        [SerializeField] Vector2 bushScaleRange = new Vector2(0.8f, 1.4f);

        [Header("Grass tufts")]
        [SerializeField] GameObject[] grassPrefabs;
        [SerializeField] int grassCount = 100;
        [SerializeField] float grassInnerRadius = 14f;
        [SerializeField] float grassOuterRadius = 55f;
        [SerializeField] Vector2 grassScaleRange = new Vector2(0.7f, 1.3f);

        [Header("Small rocks / pebbles")]
        [SerializeField] GameObject[] rockPrefabs;
        [SerializeField] int rockCount = 40;
        [SerializeField] float rockInnerRadius = 15f;
        [SerializeField] float rockOuterRadius = 55f;
        [SerializeField] Vector2 rockScaleRange = new Vector2(0.8f, 1.6f);

        [Header("Large boulders — placed in clusters at edges")]
        [SerializeField] GameObject[] boulderPrefabs;
        [SerializeField] int boulderCount = 18;
        [SerializeField] float boulderInnerRadius = 20f;
        [SerializeField] float boulderOuterRadius = 55f;
        [SerializeField] Vector2 boulderScaleRange = new Vector2(1.0f, 1.8f);

        [Header("Distant mountains — ring at the far horizon")]
        [SerializeField] GameObject[] mountainPrefabs;
        [SerializeField] int mountainCount = 16;
        [Tooltip("Radius the ring of mountains sits at.")]
        [SerializeField] float mountainRadius = 75f;
        [SerializeField] Vector2 mountainScaleRange = new Vector2(1.5f, 2.5f);
        [Tooltip("If true, mountains are aimed inward at the courtyard center; if false, random yaw.")]
        [SerializeField] bool mountainsFaceCenter = true;

        const string BuiltRootName = "_SpawnedAssets";

        void Awake()
        {
            // Idempotent rebuild on Awake.
            var existing = transform.Find(BuiltRootName);
            if (existing != null) Destroy(existing.gameObject);

            var root = new GameObject(BuiltRootName);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            // Use a per-spawner RNG seeded for repeatable placement.
            var rng = new System.Random(seed);

            ScatterRing(root.transform, "Trees",     treePrefabs,     treeCount,     treeInnerRadius,    treeOuterRadius,    treeScaleRange,    treeYOffset, rng);
            ScatterRing(root.transform, "Bushes",    bushPrefabs,     bushCount,     bushInnerRadius,    bushOuterRadius,    bushScaleRange,    0f,          rng);
            ScatterRing(root.transform, "Grass",     grassPrefabs,    grassCount,    grassInnerRadius,   grassOuterRadius,   grassScaleRange,   0f,          rng);
            ScatterRing(root.transform, "Rocks",     rockPrefabs,     rockCount,     rockInnerRadius,    rockOuterRadius,    rockScaleRange,    0f,          rng);
            ScatterRing(root.transform, "Boulders",  boulderPrefabs,  boulderCount,  boulderInnerRadius, boulderOuterRadius, boulderScaleRange, 0f,          rng);

            ScatterMountainRing(root.transform, rng);
        }

        void ScatterRing(Transform parent, string groupName, GameObject[] prefabs, int count, float innerR, float outerR, Vector2 scaleRange, float yOffset, System.Random rng)
        {
            if (prefabs == null || prefabs.Length == 0 || count <= 0) return;

            var group = new GameObject(groupName);
            group.transform.SetParent(parent, false);

            int placed = 0, attempts = 0;
            int maxAttempts = count * 8;

            while (placed < count && attempts < maxAttempts)
            {
                attempts++;
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float r = Mathf.Lerp(innerR, outerR, (float)rng.NextDouble());
                float x = Mathf.Cos(angle) * r;
                float z = Mathf.Sin(angle) * r;

                // Skip placements that overlap the courtyard rectangle.
                if (Mathf.Abs(x - courtyardCenterX) < courtyardHalfWidth &&
                    Mathf.Abs(z - courtyardCenterZ) < courtyardHalfDepth)
                {
                    continue;
                }

                var prefab = prefabs[rng.Next(prefabs.Length)];
                if (prefab == null) continue;

                GameObject go;
                try
                {
                    go = Instantiate(prefab, group.transform);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[LobbyAssetSpawner] Failed to instantiate '{prefab.name}' in group '{groupName}' — skipping this prefab. ({e.GetType().Name})");
                    continue;
                }
                if (go == null) continue;
                go.transform.localPosition = new Vector3(x, yOffset, z);
                go.transform.localRotation = Quaternion.Euler(0f, (float)(rng.NextDouble() * 360.0), 0f);
                float s = Mathf.Lerp(scaleRange.x, scaleRange.y, (float)rng.NextDouble());
                go.transform.localScale = prefab.transform.localScale * s;
                placed++;
            }
        }

        void ScatterMountainRing(Transform parent, System.Random rng)
        {
            if (mountainPrefabs == null || mountainPrefabs.Length == 0 || mountainCount <= 0) return;

            var group = new GameObject("Mountains");
            group.transform.SetParent(parent, false);

            float step = 360f / mountainCount;
            for (int i = 0; i < mountainCount; i++)
            {
                // Even angular spacing with light jitter so the ring doesn't look mechanical.
                float angleDeg = i * step + ((float)rng.NextDouble() - 0.5f) * (step * 0.4f);
                float angleRad = angleDeg * Mathf.Deg2Rad;
                float r = mountainRadius * (0.92f + (float)rng.NextDouble() * 0.16f);
                float x = Mathf.Cos(angleRad) * r;
                float z = Mathf.Sin(angleRad) * r;

                var prefab = mountainPrefabs[rng.Next(mountainPrefabs.Length)];
                if (prefab == null) continue;

                GameObject go;
                try
                {
                    go = Instantiate(prefab, group.transform);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[LobbyAssetSpawner] Failed to instantiate mountain '{prefab.name}' — skipping. ({e.GetType().Name})");
                    continue;
                }
                if (go == null) continue;
                go.transform.localPosition = new Vector3(x, 0f, z);

                // Yaw: face inward toward (0, 0, 0) or random.
                float yaw;
                if (mountainsFaceCenter)
                {
                    yaw = Mathf.Atan2(-x, -z) * Mathf.Rad2Deg + 180f; // face toward origin
                }
                else
                {
                    yaw = (float)(rng.NextDouble() * 360.0);
                }
                go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

                float s = Mathf.Lerp(mountainScaleRange.x, mountainScaleRange.y, (float)rng.NextDouble());
                go.transform.localScale = prefab.transform.localScale * s;
            }
        }

        void OnDrawGizmosSelected()
        {
            // Visualize courtyard keep-out + rings in the editor.
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.6f);
            var c = new Vector3(courtyardCenterX, 0.05f, courtyardCenterZ);
            Gizmos.DrawWireCube(c, new Vector3(courtyardHalfWidth * 2f, 0.1f, courtyardHalfDepth * 2f));

            Gizmos.color = new Color(0.3f, 0.8f, 0.3f, 0.5f);
            DrawRingGizmo(treeInnerRadius);
            DrawRingGizmo(treeOuterRadius);

            Gizmos.color = new Color(0.5f, 0.5f, 0.9f, 0.5f);
            DrawRingGizmo(mountainRadius);
        }

        void DrawRingGizmo(float radius)
        {
            const int segs = 48;
            Vector3 prev = new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segs; i++)
            {
                float a = i / (float)segs * Mathf.PI * 2f;
                Vector3 next = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
    }
}
