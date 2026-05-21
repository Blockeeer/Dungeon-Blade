using UnityEngine;

namespace DungeonBlade.Bank
{
    /// <summary>
    /// Builds GunZ-inspired courtyard dressing around the lobby — stone pillars,
    /// braziers with warm point lights, hanging banners, scattered barrels/crates,
    /// and a cobblestone tile overlay on the floor.
    ///
    /// Attach to an empty GameObject (e.g. "Environment") at the world origin.
    /// All props are spawned as children with their colliders stripped, so they
    /// can never block the player.
    /// </summary>
    [DisallowMultipleComponent]
    public class LobbyEnvironmentDecor : MonoBehaviour
    {
        [Header("Layout")]
        [Tooltip("Half-width of the courtyard in X. Pillars go on each side at ±this distance.")]
        [SerializeField] float courtyardHalfWidth = 11f;
        [Tooltip("Z position of the front pillar row (near the portal end).")]
        [SerializeField] float frontRowZ = -18f;
        [Tooltip("Z position of the back pillar row (behind the merchants).")]
        [SerializeField] float backRowZ = 10f;
        [Tooltip("Z spacing between intermediate pillar rows.")]
        [SerializeField] float pillarSpacingZ = 8f;

        [Header("Floor tiling")]
        [SerializeField] bool buildFloorOverlay = true;
        [Tooltip("Y position for the cobblestone overlay. Should be a hair above the existing floor (existing Floor is at Y=-0.25 with scale 0.5 → top at Y=0).")]
        [SerializeField] float overlayY = 0.02f;
        [Tooltip("Size of each cobblestone tile in world units.")]
        [SerializeField] float tileSize = 2.4f;
        [Tooltip("Per-tile shrink factor — leaves a thin grout-line gap between cobblestones (0.96 = 4% gap).")]
        [Range(0.7f, 1.0f)] [SerializeField] float tileFill = 0.94f;
        [Tooltip("Buffer in world units added outside the pillar bounds, so the cobblestone visibly extends past the walls.")]
        [SerializeField] float floorBuffer = 3.0f;
        [Tooltip("Skip cobblestone tiles within this radius of the portal so its rune circle reads cleanly.")]
        [SerializeField] float portalClearRadius = 4.5f;
        [Tooltip("World-space portal position used for the tile cutout (override if your portal is moved).")]
        [SerializeField] Vector3 portalPosition = new Vector3(0f, 0f, -20f);
        [Tooltip("Optional PBR cobblestone material A (e.g. Yughues M_YFCM_*). If null, falls back to procedural flat color.")]
        [SerializeField] Material cobbleMaterialOverrideA;
        [Tooltip("Optional PBR cobblestone material B (alternates with A in the checkerboard pattern). If null, uses A or falls back to procedural.")]
        [SerializeField] Material cobbleMaterialOverrideB;
        [Tooltip("UV scale on the cobble material per tile (1 = one full texture per tile, 2 = repeat twice across each tile, etc).")]
        [SerializeField] float cobbleTextureScale = 1f;

        [Header("Barriers")]
        [Tooltip("If true, invisible BoxColliders are placed around the perimeter so the player can't walk off the playable area.")]
        [SerializeField] bool buildInvisibleWalls = true;
        [Tooltip("How tall the invisible walls are.")]
        [SerializeField] float wallHeight = 6f;
        [Tooltip("How thick the invisible walls are.")]
        [SerializeField] float wallThickness = 1f;
        [Tooltip("Extra padding from the pillars to where the wall sits.")]
        [SerializeField] float wallPadding = 2f;

        [Header("Backdrop")]
        [Tooltip("If true, builds the grass/dirt ground plane around the courtyard so the original white Floor is hidden. Independent of distant mountains.")]
        [SerializeField] bool buildWasteland = true;
        [Tooltip("If true, builds visible stone curtain walls around the courtyard with crenellations on top. Off by default — pillars alone define the courtyard.")]
        [SerializeField] bool buildCurtainWalls = false;
        [Tooltip("Height of the visible stone walls (when enabled).")]
        [SerializeField] float curtainWallHeight = 5f;
        [Tooltip("If true, scatters rocks, boulders, and broken pillar stumps beyond the courtyard.")]
        [SerializeField] bool buildOuterRocks = true;
        [Tooltip("How far past the courtyard the rock formations extend.")]
        [SerializeField] float rockSpread = 6f;
        [Tooltip("If true, builds layered distant mountain silhouettes around the lobby so the horizon doesn't read as an empty void.")]
        [SerializeField] bool buildDistantMountains = true;
        [Tooltip("Inner radius where the mountain ring starts.")]
        [SerializeField] float mountainInnerRadius = 55f;
        [Tooltip("How many mountains per ring layer.")]
        [SerializeField] int mountainsPerRing = 24;
        [Tooltip("Number of concentric mountain rings (each further out + taller for parallax depth).")]
        [Range(1, 4)] [SerializeField] int mountainRings = 3;

        [Header("Lighting")]
        [Tooltip("Color of brazier point lights.")]
        [SerializeField] Color torchColor = new Color(1f, 0.55f, 0.22f);
        [SerializeField, Range(0.5f, 6f)] float torchIntensity = 3.2f;
        [SerializeField, Range(2f, 20f)] float torchRange = 8f;

        [Header("Fire VFX (optional, big quality jump)")]
        [Tooltip("Particle prefab spawned on top of the central fire pit (e.g. Synty FX_Fire_01). Replaces the procedural flame spheres visually.")]
        [SerializeField] GameObject firePitVfxPrefab;
        [Tooltip("Particle prefab spawned on each brazier (e.g. Synty FX_Candle_Flame_01 or smaller FX_Fire). Replaces the procedural flame spheres visually.")]
        [SerializeField] GameObject brazierVfxPrefab;
        [Tooltip("Scale applied to the brazier VFX. Bump up if the fire looks too small.")]
        [SerializeField] float brazierVfxScale = 1f;
        [Tooltip("Scale applied to the fire pit VFX. Bump up for a more dramatic central fire.")]
        [SerializeField] float firePitVfxScale = 1.4f;
        [Tooltip("If true, the procedural flame spheres are hidden when a VFX prefab is assigned (recommended).")]
        [SerializeField] bool hideProcFlamesWhenVfx = true;

        [Header("Atmospheric particles")]
        [Tooltip("Spawn a slow dust-mote ambient particle system across the courtyard.")]
        [SerializeField] bool buildDustMotes = true;
        [Tooltip("Spawn rising ember sparks off each brazier + the central fire pit.")]
        [SerializeField] bool buildEmberSparks = true;
        [Tooltip("Fake volumetric god rays — long thin streak particles falling from above. URP doesn't support true volumetric lighting; this approximates it.")]
        [SerializeField] bool buildGodRays = true;
        [Tooltip("Direction the god rays travel (typically the sun's down vector). Y is the dominant axis.")]
        [SerializeField] Vector3 godRayDirection = new Vector3(0.3f, -1f, -0.5f);
        [Tooltip("Color tint applied to the rays. Warm gold = sunset, cool blue = morning, white = noon.")]
        [SerializeField] Color godRayColor = new Color(1f, 0.85f, 0.55f, 0.35f);
        [Range(2f, 30f)] [SerializeField] float godRayEmissionRate = 5f;
        [Range(0.5f, 5f)] [SerializeField] float godRayWidth = 1.2f;

        const string BuiltRootName = "_LobbyDecor";

        // Cached materials shared between props for fewer draw calls.
        Material _stone, _stoneDark, _stoneLight, _wood, _woodDark, _iron, _rust, _flame, _bannerAmber, _bannerTeal, _bannerTrim, _cobbleA, _cobbleB;
        // Outdoor environment materials (mountains + wasteland + trees)
        Material _earth, _grass, _dirtRock, _mtnBase, _mtnMid, _mtnTop, _mtnSnow;
        Material _treeTrunk, _treeFoliageDark, _treeFoliageMid, _treeFoliageLight, _treeFoliageOlive;

        void Awake()
        {
            // Idempotent rebuild.
            var existing = transform.Find(BuiltRootName);
            if (existing != null) Destroy(existing.gameObject);

            var root = new GameObject(BuiltRootName);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            BuildMaterials();

            if (buildFloorOverlay) BuildCobblestoneFloor(root.transform);
            if (buildWasteland) BuildWasteland(root.transform);
            if (buildCurtainWalls) BuildCurtainWalls(root.transform);
            BuildPerimeterPillars(root.transform);
            BuildSceneryProps(root.transform);
            BuildCentralFirePit(root.transform);
            if (buildOuterRocks) BuildOuterRocks(root.transform);
            if (buildDistantMountains) BuildDistantMountains(root.transform);
            if (buildInvisibleWalls) BuildInvisibleWalls(root.transform);
            if (buildDustMotes) BuildDustMotes(root.transform);
            if (buildGodRays) BuildGodRays(root.transform);
        }

        // Standalone wasteland — the grass donut around the courtyard that hides the
        // original white Floor. Separated from BuildDistantMountains so users can have
        // the ground green without the procedural mountains.
        void BuildWasteland(Transform root)
        {
            const float outerHalf = 130f;
            const float wastelandY = 0.03f;
            const float wastelandThick = 0.10f;

            float cMinX = -courtyardHalfWidth - floorBuffer;
            float cMaxX =  courtyardHalfWidth + floorBuffer;
            float cMinZ = frontRowZ - floorBuffer;
            float cMaxZ = backRowZ  + floorBuffer;

            var ground = new GameObject("Wasteland");
            ground.transform.SetParent(root, false);

            MakeCube(ground.transform, "Wasteland_N",
                new Vector3(0f, wastelandY, (cMaxZ + outerHalf) * 0.5f),
                new Vector3(outerHalf * 2f, wastelandThick, outerHalf - cMaxZ), _earth);
            MakeCube(ground.transform, "Wasteland_S",
                new Vector3(0f, wastelandY, (-outerHalf + cMinZ) * 0.5f),
                new Vector3(outerHalf * 2f, wastelandThick, cMinZ + outerHalf), _earth);
            MakeCube(ground.transform, "Wasteland_W",
                new Vector3((-outerHalf + cMinX) * 0.5f, wastelandY, (cMinZ + cMaxZ) * 0.5f),
                new Vector3(cMinX + outerHalf, wastelandThick, cMaxZ - cMinZ), _earth);
            MakeCube(ground.transform, "Wasteland_E",
                new Vector3((outerHalf + cMaxX) * 0.5f, wastelandY, (cMinZ + cMaxZ) * 0.5f),
                new Vector3(outerHalf - cMaxX, wastelandThick, cMaxZ - cMinZ), _earth);

            // Patches scattered on top for natural variation. All earth-toned so they blend
            // with the sandy ground instead of standing out as bright green tiles.
            var patches = new GameObject("WastelandPatches");
            patches.transform.SetParent(ground.transform, false);
            for (int i = 0; i < 100; i++)
            {
                int seed = i * 91 + 7;
                float a = (seed * 7) % 360 * Mathf.Deg2Rad;
                float r = (courtyardHalfWidth + floorBuffer + 4f) + (seed % 65);
                float x = Mathf.Cos(a) * r;
                float z = Mathf.Sin(a) * r;
                // Hard skip: never place a patch inside the cobblestone rectangle.
                if (x > cMinX - 0.5f && x < cMaxX + 0.5f && z > cMinZ - 0.5f && z < cMaxZ + 0.5f) continue;
                // Skip mountain ring area.
                if (r > mountainInnerRadius - 4f && r < mountainInnerRadius + mountainRings * 22f + 4f) continue;
                float scale = 0.6f + (seed % 6) * 0.20f;       // smaller patches (was up to 3.1)
                int kind = seed % 3;
                Material m;
                float thick;
                if (kind == 0)      { m = _dirtRock; thick = 0.08f; }
                else                { m = _earth;    thick = 0.04f; }
                MakeCube(patches.transform, $"Patch_{i}",
                    new Vector3(x, wastelandY + wastelandThick * 0.5f + thick * 0.5f, z),
                    new Vector3(scale, thick, scale * 0.8f), m);
            }
        }

        void BuildMaterials()
        {
            _stone       = MakeMat(new Color(0.30f, 0.28f, 0.26f), 0.05f, 0.30f);
            _stoneDark   = MakeMat(new Color(0.16f, 0.14f, 0.13f), 0.05f, 0.30f);
            _stoneLight  = MakeMat(new Color(0.40f, 0.38f, 0.35f), 0.05f, 0.30f);
            _wood        = MakeMat(new Color(0.24f, 0.15f, 0.08f), 0.05f, 0.30f);
            _woodDark    = MakeMat(new Color(0.14f, 0.09f, 0.05f), 0.05f, 0.35f);
            _iron        = MakeMat(new Color(0.16f, 0.14f, 0.13f), 0.85f, 0.40f);
            _rust        = MakeMatEmissive(new Color(0.40f, 0.22f, 0.10f), new Color(0.06f, 0.02f, 0f), 0.55f, 0.25f);
            _flame       = MakeMatEmissive(new Color(1f, 0.55f, 0.18f), new Color(3.5f, 1.6f, 0.4f), 0f, 0.20f);
            _bannerAmber = MakeMat(new Color(0.52f, 0.24f, 0.08f), 0.05f, 0.45f);
            _bannerTeal  = MakeMat(new Color(0.10f, 0.30f, 0.32f), 0.05f, 0.45f);
            _bannerTrim  = MakeMat(new Color(0.10f, 0.07f, 0.04f), 0.20f, 0.40f);
            _cobbleA     = MakeMat(new Color(0.24f, 0.22f, 0.20f), 0.05f, 0.20f);
            _cobbleB     = MakeMat(new Color(0.30f, 0.28f, 0.25f), 0.05f, 0.20f);

            // Outdoor — dry earth/sand palette so the surrounding land reads as worn dirt or arid path.
            _earth    = MakeMat(new Color(0.55f, 0.44f, 0.28f), 0f,    0.20f);   // warm sandy dirt (main ground)
            _grass    = MakeMat(new Color(0.40f, 0.52f, 0.22f), 0f,    0.15f);   // occasional grass tuft for variation
            _dirtRock = MakeMat(new Color(0.42f, 0.34f, 0.22f), 0.05f, 0.20f);   // darker dirt/rock patches
            // Mountain color zones — dirt base, dry stone middle, lighter exposed rock at top, snow on tallest.
            _mtnBase  = MakeMat(new Color(0.20f, 0.16f, 0.12f), 0.03f, 0.15f);   // dark dirt-stained slopes
            _mtnMid   = MakeMat(new Color(0.28f, 0.24f, 0.20f), 0.04f, 0.18f);   // weathered brown stone
            _mtnTop   = MakeMat(new Color(0.42f, 0.40f, 0.38f), 0.05f, 0.22f);   // bare lighter stone
            _mtnSnow  = MakeMat(new Color(0.93f, 0.94f, 0.96f), 0.05f, 0.55f);   // snowcap

            // Trees — warm trunk + several green foliage shades for variety.
            _treeTrunk        = MakeMat(new Color(0.22f, 0.13f, 0.07f), 0.05f, 0.20f);
            _treeFoliageDark  = MakeMat(new Color(0.10f, 0.22f, 0.08f), 0f, 0.15f);
            _treeFoliageMid   = MakeMat(new Color(0.16f, 0.32f, 0.12f), 0f, 0.15f);
            _treeFoliageLight = MakeMat(new Color(0.26f, 0.40f, 0.18f), 0f, 0.15f);
            _treeFoliageOlive = MakeMat(new Color(0.36f, 0.42f, 0.20f), 0f, 0.15f);
        }

        // ------------------------------------------------------------------
        // Cobblestone floor overlay — large dark plaza centred on the player path.
        // Built as a grid of slightly-varying tiles for texture without textures.
        // ------------------------------------------------------------------
        void BuildCobblestoneFloor(Transform root)
        {
            var floor = new GameObject("Cobblestone");
            floor.transform.SetParent(root, false);

            // Cover the full playable area + a buffer so the cobblestone visibly extends past the pillars.
            float minX = -courtyardHalfWidth - floorBuffer;
            float maxX = courtyardHalfWidth + floorBuffer;
            float minZ = frontRowZ - floorBuffer;
            float maxZ = backRowZ + floorBuffer;

            int cols = Mathf.CeilToInt((maxX - minX) / tileSize);
            int rows = Mathf.CeilToInt((maxZ - minZ) / tileSize);

            // Re-center so coverage is symmetric even after rounding up.
            float actualWidth = cols * tileSize;
            float actualDepth = rows * tileSize;
            float startX = (minX + maxX) * 0.5f - actualWidth * 0.5f;
            float startZ = (minZ + maxZ) * 0.5f - actualDepth * 0.5f;
            float portalClearSq = portalClearRadius * portalClearRadius;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = startX + (c + 0.5f) * tileSize;
                    float z = startZ + (r + 0.5f) * tileSize;
                    // Cut a circular hole around the portal so its rune circle reads cleanly.
                    float dx = x - portalPosition.x;
                    float dz = z - portalPosition.z;
                    if (dx * dx + dz * dz < portalClearSq) continue;

                    bool useA = ((r + c) & 1) == 0;
                    Material mat;
                    if (cobbleMaterialOverrideA != null)
                    {
                        // Use the assigned PBR material(s). If only A is set, every tile uses A.
                        mat = useA ? cobbleMaterialOverrideA
                                   : (cobbleMaterialOverrideB != null ? cobbleMaterialOverrideB : cobbleMaterialOverrideA);
                    }
                    else
                    {
                        mat = useA ? _cobbleA : _cobbleB;
                    }
                    // Shrink each tile slightly so a grout line shows between them — proper cobblestone look.
                    var tile = MakeCube(floor.transform, $"Tile_{r}_{c}",
                        new Vector3(x, overlayY, z),
                        new Vector3(tileSize * tileFill, 0.10f, tileSize * tileFill), mat);
                    // Subtle rotation jitter so the grid doesn't look mechanical.
                    tile.transform.localRotation = Quaternion.Euler(0f, ((r * 7) + (c * 11)) % 4 * 1.5f, 0f);
                    // If using a PBR material, set texture tiling so each tile shows one (or N) full pattern repeat.
                    if (cobbleMaterialOverrideA != null && cobbleTextureScale > 0f)
                    {
                        var mr = tile.GetComponent<MeshRenderer>();
                        if (mr != null)
                        {
                            var mpb = new MaterialPropertyBlock();
                            mr.GetPropertyBlock(mpb);
                            Vector4 st = new Vector4(cobbleTextureScale, cobbleTextureScale, 0f, 0f);
                            mpb.SetVector("_BaseMap_ST", st);
                            mpb.SetVector("_MainTex_ST", st);
                            mr.SetPropertyBlock(mpb);
                        }
                    }
                }
            }

            // Center medallion — a slightly raised circle of darker stone where the player path goes.
            var medallion = MakePrimitive(floor.transform, "Medallion", PrimitiveType.Cylinder,
                new Vector3(0f, overlayY + 0.06f, -7f),
                new Vector3(4.5f, 0.04f, 4.5f), _stoneDark);
            medallion.transform.localRotation = Quaternion.identity;
            // Iron-strapped ring around the medallion
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f * Mathf.Deg2Rad;
                var seg = MakeCube(floor.transform, $"MedRing_{i}",
                    new Vector3(Mathf.Cos(a) * 2.25f, overlayY + 0.08f, -7f + Mathf.Sin(a) * 2.25f),
                    new Vector3(0.25f, 0.04f, 1.2f), _iron);
                seg.transform.localRotation = Quaternion.Euler(0f, -i * 30f, 0f);
            }
        }

        // ------------------------------------------------------------------
        // Visible stone curtain walls forming the courtyard enclosure.
        // Connects the perimeter pillars with stone-block segments, capped with
        // crenellations on top. Leaves gaps at the south (portal) and lets the
        // merchants stay accessible at the north end.
        // ------------------------------------------------------------------
        void BuildCurtainWalls(Transform root)
        {
            var walls = new GameObject("CurtainWalls");
            walls.transform.SetParent(root, false);

            float wallY = curtainWallHeight * 0.5f;
            float wallThick = 0.6f;
            float capY = curtainWallHeight + 0.20f;

            // Left + right side walls run the full courtyard depth.
            float sideSpan = (backRowZ - frontRowZ) + 1f;
            float sideMidZ = (frontRowZ + backRowZ) * 0.5f;
            BuildWallSegment(walls.transform, "WallW", new Vector3(-courtyardHalfWidth, wallY, sideMidZ),
                new Vector3(wallThick, curtainWallHeight, sideSpan));
            BuildWallSegment(walls.transform, "WallE", new Vector3( courtyardHalfWidth, wallY, sideMidZ),
                new Vector3(wallThick, curtainWallHeight, sideSpan));
            // Crenellations on top of side walls
            BuildCrenellations(walls.transform, "WallW_Cren", new Vector3(-courtyardHalfWidth, capY, sideMidZ),
                sideSpan, wallThick, false);
            BuildCrenellations(walls.transform, "WallE_Cren", new Vector3( courtyardHalfWidth, capY, sideMidZ),
                sideSpan, wallThick, false);

            // Back wall (north, behind merchants) — split into two segments with a central archway
            float backWidth = (courtyardHalfWidth * 2f) - 4f; // 2m gap each side accounted for by pillars
            float backHalfSeg = (courtyardHalfWidth - 1.5f) * 0.5f; // each segment span
            float backSegCenter = courtyardHalfWidth * 0.5f + 0.75f;
            BuildWallSegment(walls.transform, "WallN_L", new Vector3(-backSegCenter, wallY, backRowZ),
                new Vector3(courtyardHalfWidth - 1.5f, curtainWallHeight, wallThick));
            BuildWallSegment(walls.transform, "WallN_R", new Vector3( backSegCenter, wallY, backRowZ),
                new Vector3(courtyardHalfWidth - 1.5f, curtainWallHeight, wallThick));
            BuildCrenellations(walls.transform, "WallN_L_Cren", new Vector3(-backSegCenter, capY, backRowZ),
                courtyardHalfWidth - 1.5f, wallThick, true);
            BuildCrenellations(walls.transform, "WallN_R_Cren", new Vector3( backSegCenter, capY, backRowZ),
                courtyardHalfWidth - 1.5f, wallThick, true);
            // Lintel arch over the central back gap
            BuildWallSegment(walls.transform, "WallN_Lintel", new Vector3(0f, curtainWallHeight - 0.4f, backRowZ),
                new Vector3(3f, 0.8f, wallThick));

            // Front wall (south, behind portal) — same split with central gap for the portal arch
            BuildWallSegment(walls.transform, "WallS_L", new Vector3(-backSegCenter, wallY, frontRowZ),
                new Vector3(courtyardHalfWidth - 1.5f, curtainWallHeight, wallThick));
            BuildWallSegment(walls.transform, "WallS_R", new Vector3( backSegCenter, wallY, frontRowZ),
                new Vector3(courtyardHalfWidth - 1.5f, curtainWallHeight, wallThick));
            BuildCrenellations(walls.transform, "WallS_L_Cren", new Vector3(-backSegCenter, capY, frontRowZ),
                courtyardHalfWidth - 1.5f, wallThick, true);
            BuildCrenellations(walls.transform, "WallS_R_Cren", new Vector3( backSegCenter, capY, frontRowZ),
                courtyardHalfWidth - 1.5f, wallThick, true);

            _ = backWidth;
        }

        // A wall segment is a single block with an iron stripe at the base and middle,
        // plus a few faux block seams to break up the silhouette.
        void BuildWallSegment(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;

            MakeCube(go.transform, "Block", Vector3.zero, size, _stoneDark);
            // Base iron band on the visible (long) faces
            bool runsAlongX = size.x > size.z;
            if (runsAlongX)
            {
                MakeCube(go.transform, "BaseBand", new Vector3(0f, -size.y * 0.5f + 0.10f, -size.z * 0.5f - 0.02f),
                    new Vector3(size.x + 0.04f, 0.12f, 0.04f), _iron);
                MakeCube(go.transform, "MidSeam",  new Vector3(0f,  0f, -size.z * 0.5f - 0.02f),
                    new Vector3(size.x + 0.04f, 0.06f, 0.02f), _stone);
                // A few vertical seams along the front face
                for (int i = 1; i < 4; i++)
                {
                    float x = -size.x * 0.5f + (i / 4f) * size.x;
                    MakeCube(go.transform, $"Seam_{i}", new Vector3(x, 0f, -size.z * 0.5f - 0.03f),
                        new Vector3(0.04f, size.y - 0.20f, 0.02f), _stone);
                }
            }
            else
            {
                MakeCube(go.transform, "BaseBand", new Vector3(size.x * 0.5f + 0.02f, -size.y * 0.5f + 0.10f, 0f),
                    new Vector3(0.04f, 0.12f, size.z + 0.04f), _iron);
                MakeCube(go.transform, "MidSeam",  new Vector3(size.x * 0.5f + 0.02f,  0f, 0f),
                    new Vector3(0.02f, 0.06f, size.z + 0.04f), _stone);
                for (int i = 1; i < 4; i++)
                {
                    float z = -size.z * 0.5f + (i / 4f) * size.z;
                    MakeCube(go.transform, $"Seam_{i}", new Vector3(size.x * 0.5f + 0.03f, 0f, z),
                        new Vector3(0.02f, size.y - 0.20f, 0.04f), _stone);
                }
            }
        }

        // Battlement merlons on top of a wall.
        void BuildCrenellations(Transform parent, string name, Vector3 baseTopPos, float length, float thickness, bool runsAlongX)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = baseTopPos;

            float merlonW = 0.50f;
            float merlonGap = 0.40f;
            float step = merlonW + merlonGap;
            int count = Mathf.Max(2, Mathf.FloorToInt(length / step));
            float spanUsed = count * step - merlonGap;
            float start = -spanUsed * 0.5f + merlonW * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float offset = start + i * step;
                Vector3 pos = runsAlongX
                    ? new Vector3(offset, 0f, 0f)
                    : new Vector3(0f, 0f, offset);
                MakeCube(go.transform, $"Merlon_{i}", pos,
                    new Vector3(runsAlongX ? merlonW : thickness, 0.40f, runsAlongX ? thickness : merlonW), _stoneDark);
            }
        }

        // ------------------------------------------------------------------
        // Outer rocks + ruined props — scattered beyond the curtain walls for depth.
        // ------------------------------------------------------------------
        void BuildOuterRocks(Transform root)
        {
            var rocks = new GameObject("OuterRocks");
            rocks.transform.SetParent(root, false);

            float outX = courtyardHalfWidth + rockSpread * 0.5f;
            float outFrontZ = frontRowZ - rockSpread * 0.5f;
            float outBackZ = backRowZ + rockSpread * 0.5f;

            // Boulder clusters at the four outer corners
            BuildBoulderCluster(rocks.transform, new Vector3(-outX, 0f, outFrontZ), 4);
            BuildBoulderCluster(rocks.transform, new Vector3( outX, 0f, outFrontZ), 4);
            BuildBoulderCluster(rocks.transform, new Vector3(-outX, 0f, outBackZ),  5);
            BuildBoulderCluster(rocks.transform, new Vector3( outX, 0f, outBackZ),  5);

            // Boulders along the left + right walls (outside)
            for (int i = 0; i < 4; i++)
            {
                float t = (i + 0.5f) / 4f;
                float z = Mathf.Lerp(frontRowZ, backRowZ, t);
                BuildBoulder(rocks.transform, new Vector3(-courtyardHalfWidth - 2.2f - (i % 2) * 0.6f, 0f, z + (i % 2) * 1.2f), 1.0f + (i % 3) * 0.3f);
                BuildBoulder(rocks.transform, new Vector3( courtyardHalfWidth + 2.2f + (i % 2) * 0.6f, 0f, z - (i % 2) * 1.2f), 1.0f + ((i + 1) % 3) * 0.3f);
            }

            // Broken pillar stumps behind the back wall — implies ancient ruins
            BuildBrokenPillar(rocks.transform, new Vector3(-courtyardHalfWidth - 3f, 0f, outBackZ - 1f), 1.8f);
            BuildBrokenPillar(rocks.transform, new Vector3( courtyardHalfWidth + 3f, 0f, outBackZ - 1f), 2.4f);
            BuildBrokenPillar(rocks.transform, new Vector3(-2f, 0f, outBackZ + 1.5f), 1.4f);
            BuildBrokenPillar(rocks.transform, new Vector3( 3f, 0f, outBackZ + 2.0f), 2.0f);

            // A couple of broken stumps behind the portal end too
            BuildBrokenPillar(rocks.transform, new Vector3(-courtyardHalfWidth - 2.5f, 0f, outFrontZ + 1f), 2.1f);
            BuildBrokenPillar(rocks.transform, new Vector3( courtyardHalfWidth + 2.5f, 0f, outFrontZ + 1f), 1.6f);
        }

        void BuildBoulderCluster(Transform parent, Vector3 center, int count)
        {
            var cluster = new GameObject("BoulderCluster");
            cluster.transform.SetParent(parent, false);
            cluster.transform.localPosition = center;
            for (int i = 0; i < count; i++)
            {
                float a = (i * 137.5f) * Mathf.Deg2Rad;
                float r = 0.5f + (i % 3) * 0.6f;
                float scale = 1.2f + ((i * 7) % 5) * 0.25f;
                BuildBoulder(cluster.transform, new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r), scale);
            }
        }

        void BuildBoulder(Transform parent, Vector3 basePos, float scale)
        {
            var b = new GameObject("Boulder");
            b.transform.SetParent(parent, false);
            b.transform.localPosition = basePos;
            // Random-ish irregular rotation per position so they look natural without seeded randomness.
            int seed = Mathf.Abs(Mathf.RoundToInt(basePos.x * 13.7f + basePos.z * 7.3f));
            b.transform.localRotation = Quaternion.Euler((seed % 60) - 30f, (seed * 7) % 360f, (seed * 11 % 60) - 30f);

            // Boulder body = stretched cube, with two smaller cubes glued on for irregular silhouette.
            // Core is collidable so the player bumps off the boulder.
            MakeCube(b.transform, "Core", new Vector3(0f, scale * 0.5f, 0f), new Vector3(scale * 1.2f, scale, scale * 1.0f), _stone, keepCollider: true);
            MakeCube(b.transform, "Bump1", new Vector3(scale * 0.4f, scale * 0.85f, -scale * 0.2f),
                new Vector3(scale * 0.6f, scale * 0.5f, scale * 0.5f), _stoneDark);
            MakeCube(b.transform, "Bump2", new Vector3(-scale * 0.3f, scale * 0.30f, scale * 0.35f),
                new Vector3(scale * 0.55f, scale * 0.4f, scale * 0.5f), _stoneLight);
        }

        // ------------------------------------------------------------------
        // Distant mountain backdrop — layered rings of dark jagged silhouettes
        // around the lobby so the horizon doesn't read as an empty void.
        // Also lays down a wide dark ground plane to hide the original Floor's edge.
        // ------------------------------------------------------------------
        void BuildDistantMountains(Transform root)
        {
            var mountains = new GameObject("DistantMountains");
            mountains.transform.SetParent(root, false);

            float cMinX = -courtyardHalfWidth - floorBuffer;
            float cMaxX =  courtyardHalfWidth + floorBuffer;
            float cMinZ = frontRowZ - floorBuffer;
            float cMaxZ = backRowZ  + floorBuffer;

            // Trees — scattered woodland between the courtyard and the mountains.
            BuildWoodland(mountains.transform, cMinX, cMaxX, cMinZ, cMaxZ);

            // Mountain rings — each layer steps further out and slightly taller for parallax depth.
            for (int ring = 0; ring < mountainRings; ring++)
            {
                float radius = mountainInnerRadius + ring * 22f;
                float baseHeight = 6f + ring * 3.5f;

                int count = mountainsPerRing + ring * 6;
                float angleOffset = ring * 8f;

                for (int i = 0; i < count; i++)
                {
                    float a = (i * (360f / count) + angleOffset) * Mathf.Deg2Rad;
                    int seed = Mathf.Abs(ring * 91 + i * 17);
                    float radiusJitter = ((seed % 30) - 15) * 0.20f;
                    float heightVar = baseHeight + ((seed * 7) % 100) * 0.04f;
                    float baseWidth = 6.5f + ((seed * 13) % 50) * 0.14f;
                    float baseYaw = ((seed * 5) % 360);

                    Vector3 pos = new Vector3(
                        Mathf.Cos(a) * (radius + radiusJitter),
                        0f,
                        Mathf.Sin(a) * (radius + radiusJitter)
                    );

                    BuildMountainPeak(mountains.transform, $"Peak_R{ring}_{i}",
                        pos, baseWidth, heightVar, baseYaw, seed);
                }
            }
        }

        // A mountain peak built from STACKED FLATTENED SPHERES with extra detail:
        //   • main peak: 6 layers tapering up with color zones (dirt → stone → rock → snow)
        //   • 2 sub-peaks at different heights and offsets for irregular ridges
        //   • jagged rocky outcrops (rotated cubes) sticking out of the mid slopes
        //   • snow patches drifting down from the cap on tall peaks
        void BuildMountainPeak(Transform parent, string name, Vector3 pos, float baseWidth, float baseHeight, float baseYaw, int seed)
        {
            var peak = new GameObject(name);
            peak.transform.SetParent(parent, false);
            peak.transform.localPosition = pos;
            peak.transform.localRotation = Quaternion.Euler(0f, baseYaw, 0f);

            bool snowy = baseHeight >= 8.5f;

            // ---- Main peak ----
            BuildPeakSpheres(peak.transform, Vector3.zero, baseWidth, baseHeight, snowy, seed);

            // ---- Two secondary peaks ----
            // Sub-peak A (larger, taller)
            float subA_W = baseWidth * 0.62f;
            float subA_H = baseHeight * 0.70f;
            var subA = new GameObject("SubPeakA");
            subA.transform.SetParent(peak.transform, false);
            subA.transform.localPosition = new Vector3(baseWidth * 0.40f, 0f, -baseWidth * 0.18f);
            subA.transform.localRotation = Quaternion.Euler(0f, ((seed * 3) % 90) - 45f, 0f);
            BuildPeakSpheres(subA.transform, Vector3.zero, subA_W, subA_H, snowy && subA_H >= 7f, seed * 31);

            // Sub-peak B (smaller, lower, on the other side)
            float subB_W = baseWidth * 0.45f;
            float subB_H = baseHeight * 0.50f;
            var subB = new GameObject("SubPeakB");
            subB.transform.SetParent(peak.transform, false);
            subB.transform.localPosition = new Vector3(-baseWidth * 0.35f, 0f, baseWidth * 0.22f);
            subB.transform.localRotation = Quaternion.Euler(0f, ((seed * 5) % 90) + 20f, 0f);
            BuildPeakSpheres(subB.transform, Vector3.zero, subB_W, subB_H, false, seed * 47);

            // ---- Jagged rocky outcrops on the slopes ----
            int outcrops = 3 + (seed % 3); // 3..5
            for (int i = 0; i < outcrops; i++)
            {
                int s = seed + i * 211;
                float a = (s * 7) % 360 * Mathf.Deg2Rad;
                float dist = baseWidth * Mathf.Lerp(0.30f, 0.55f, (s % 100) * 0.01f);
                float hFrac = Mathf.Lerp(0.15f, 0.65f, (s % 89) * 0.011f);
                float y = baseHeight * hFrac;
                float w = baseWidth * Mathf.Lerp(0.10f, 0.22f, (s % 73) * 0.014f);
                float h = w * Mathf.Lerp(0.6f, 1.3f, (s % 53) * 0.019f);
                Vector3 oPos = new Vector3(Mathf.Cos(a) * dist, y, Mathf.Sin(a) * dist);
                var rock = MakeCube(peak.transform, $"Outcrop_{i}", oPos,
                    new Vector3(w, h, w * 0.85f), PickPeakMaterial(hFrac, false));
                rock.transform.localRotation = Quaternion.Euler(((s * 7) % 60) - 30f, (s * 11) % 360, ((s * 13) % 60) - 30f);
            }

            // ---- Snow patches drifting down from the cap ----
            if (snowy)
            {
                int patchCount = 4 + (seed % 4);
                for (int i = 0; i < patchCount; i++)
                {
                    int s = seed + i * 173;
                    float a = (s * 11) % 360 * Mathf.Deg2Rad;
                    float hFrac = Mathf.Lerp(0.60f, 0.92f, (s % 80) * 0.0125f);
                    float dist = baseWidth * Mathf.Lerp(0.05f, 0.25f, (s % 60) * 0.0166f) * (1f - hFrac);
                    float w = baseWidth * Mathf.Lerp(0.12f, 0.28f, (s % 90) * 0.0111f);
                    Vector3 sPos = new Vector3(Mathf.Cos(a) * dist, baseHeight * hFrac, Mathf.Sin(a) * dist);
                    MakePrimitive(peak.transform, $"SnowPatch_{i}", PrimitiveType.Sphere, sPos,
                        new Vector3(w, w * 0.30f, w * 0.80f), _mtnSnow);
                }
            }
        }

        // Helper: build the stacked-sphere body of a peak (no sub-peaks/outcrops).
        void BuildPeakSpheres(Transform parent, Vector3 origin, float baseWidth, float baseHeight, bool snowy, int seed)
        {
            const int layers = 6;
            for (int i = 0; i < layers; i++)
            {
                float t = i / (float)(layers - 1);
                float w = Mathf.Lerp(baseWidth, baseWidth * 0.10f, Mathf.Pow(t, 1.3f));
                float d = Mathf.Lerp(baseWidth * 0.88f, baseWidth * 0.10f, Mathf.Pow(t, 1.3f));
                float flatten = Mathf.Lerp(0.55f, 0.95f, t);
                float h = w * flatten;
                float y = Mathf.Lerp(h * 0.5f, baseHeight - h * 0.5f, t);
                float ox = ((seed * (i + 1)) % 7 - 3) * 0.05f * baseWidth * t;
                float oz = ((seed * (i + 2)) % 7 - 3) * 0.05f * baseWidth * t;

                MakePrimitive(parent, $"Layer_{i}", PrimitiveType.Sphere,
                    origin + new Vector3(ox, y, oz),
                    new Vector3(w, h, d), PickPeakMaterial(t, snowy));
            }
        }

        Material PickPeakMaterial(float heightFraction, bool snowy)
        {
            if (snowy && heightFraction >= 0.85f) return _mtnSnow;
            if (heightFraction >= 0.65f) return _mtnTop;
            if (heightFraction >= 0.30f) return _mtnMid;
            return _mtnBase;
        }

        // ------------------------------------------------------------------
        // Woodland — scattered trees in the wasteland between courtyard and mountains.
        // Each tree: brown cylinder trunk + 3 overlapping foliage spheres in varying greens.
        // Trunks have colliders so the player can't walk through them.
        // ------------------------------------------------------------------
        void BuildWoodland(Transform parent, float cMinX, float cMaxX, float cMinZ, float cMaxZ)
        {
            var woodland = new GameObject("Woodland");
            woodland.transform.SetParent(parent, false);

            const int treeCount = 70;
            float innerR = courtyardHalfWidth + floorBuffer + 2.5f;  // just outside the courtyard
            float outerR = mountainInnerRadius - 5f;                 // stop a bit before mountains

            int placed = 0;
            int attempts = 0;
            while (placed < treeCount && attempts < treeCount * 6)
            {
                attempts++;
                int seed = attempts * 131 + 17;
                float a = (seed * 7) % 360 * Mathf.Deg2Rad;
                float r = innerR + ((seed * 13) % 100) * 0.01f * (outerR - innerR);
                float x = Mathf.Cos(a) * r;
                float z = Mathf.Sin(a) * r;
                // Don't drop trees on the courtyard rectangle
                if (x > cMinX - 1f && x < cMaxX + 1f && z > cMinZ - 1f && z < cMaxZ + 1f) continue;

                float scale = 1.0f + ((seed * 17) % 100) * 0.012f;   // 1.0..2.2
                BuildTree(woodland.transform, new Vector3(x, 0f, z), scale, seed);
                placed++;
            }
        }

        void BuildTree(Transform parent, Vector3 basePos, float scale, int seed)
        {
            // Pick a tree species so the woodland feels naturally mixed.
            int kind = seed % 10;
            if (kind < 5)        BuildPineTree(parent, basePos, scale, seed);        // 50% pine (most common)
            else if (kind < 8)   BuildDeciduousTree(parent, basePos, scale, seed);   // 30% round deciduous
            else                 BuildFirTree(parent, basePos, scale * 1.15f, seed); // 20% tall fir
        }

        // Pine — conical evergreen, 5 stacked layers tapering up to a point.
        void BuildPineTree(Transform parent, Vector3 basePos, float scale, int seed)
        {
            var tree = new GameObject("PineTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = basePos;
            tree.transform.localRotation = Quaternion.Euler(0f, (seed * 11) % 360, 0f);

            float trunkR = 0.15f * scale;
            float trunkH = 1.6f * scale;
            MakePrimitive(tree.transform, "Trunk", PrimitiveType.Cylinder,
                new Vector3(0f, trunkH * 0.5f, 0f),
                new Vector3(trunkR * 2f, trunkH * 0.5f, trunkR * 2f),
                _treeTrunk, keepCollider: true);

            Material[] palette = { _treeFoliageDark, _treeFoliageMid, _treeFoliageDark, _treeFoliageOlive };
            Material primary = palette[seed % palette.Length];
            Material accent  = palette[(seed + 2) % palette.Length];

            int layers = 5;
            float foliageStart = trunkH * 0.55f;
            float foliageSpan  = scale * 2.6f;
            float baseRadius   = 1.2f * scale;
            for (int i = 0; i < layers; i++)
            {
                float t = i / (float)(layers - 1);
                float w = Mathf.Lerp(baseRadius, baseRadius * 0.10f, Mathf.Pow(t, 1.1f));
                float h = w * 0.85f;
                float y = foliageStart + t * foliageSpan;
                MakePrimitive(tree.transform, $"PineLayer_{i}", PrimitiveType.Sphere,
                    new Vector3(0f, y, 0f),
                    new Vector3(w, h, w), i % 2 == 0 ? primary : accent);
            }
        }

        // Tall slender fir — narrow trunk + many tiny layered tufts up the trunk.
        void BuildFirTree(Transform parent, Vector3 basePos, float scale, int seed)
        {
            var tree = new GameObject("FirTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = basePos;
            tree.transform.localRotation = Quaternion.Euler(0f, (seed * 13) % 360, 0f);

            float trunkR = 0.10f * scale;
            float trunkH = 2.8f * scale;
            MakePrimitive(tree.transform, "Trunk", PrimitiveType.Cylinder,
                new Vector3(0f, trunkH * 0.5f, 0f),
                new Vector3(trunkR * 2f, trunkH * 0.5f, trunkR * 2f),
                _treeTrunk, keepCollider: true);

            Material primary = _treeFoliageDark;
            Material accent  = _treeFoliageMid;

            int layers = 7;
            float foliageStart = trunkH * 0.35f;
            float foliageSpan  = scale * 3.0f;
            float baseRadius   = 0.95f * scale;
            for (int i = 0; i < layers; i++)
            {
                float t = i / (float)(layers - 1);
                float w = Mathf.Lerp(baseRadius, baseRadius * 0.08f, Mathf.Pow(t, 1.15f));
                float h = w * 0.7f;
                float y = foliageStart + t * foliageSpan;
                MakePrimitive(tree.transform, $"FirLayer_{i}", PrimitiveType.Sphere,
                    new Vector3(0f, y, 0f),
                    new Vector3(w, h, w), i % 2 == 0 ? primary : accent);
            }
        }

        // Deciduous — round bushy crown, three overlapping spheres + top tuft (was the original).
        void BuildDeciduousTree(Transform parent, Vector3 basePos, float scale, int seed)
        {
            var tree = new GameObject("DeciduousTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = basePos;
            tree.transform.localRotation = Quaternion.Euler(0f, (seed * 11) % 360, 0f);

            float trunkR = 0.20f * scale;
            float trunkH = 1.4f * scale;
            MakePrimitive(tree.transform, "Trunk", PrimitiveType.Cylinder,
                new Vector3(0f, trunkH * 0.5f, 0f),
                new Vector3(trunkR * 2f, trunkH * 0.5f, trunkR * 2f),
                _treeTrunk, keepCollider: true);

            Material[] palette = { _treeFoliageMid, _treeFoliageLight, _treeFoliageOlive };
            Material primary = palette[seed % palette.Length];
            Material accent  = palette[(seed + 1) % palette.Length];

            float foliageY = trunkH + 0.4f * scale;
            float bigR = 1.3f * scale;
            MakePrimitive(tree.transform, "Foliage1", PrimitiveType.Sphere,
                new Vector3(0f, foliageY, 0f),
                new Vector3(bigR, bigR * 0.95f, bigR), primary);
            MakePrimitive(tree.transform, "Foliage2", PrimitiveType.Sphere,
                new Vector3(0.38f * scale, foliageY + 0.28f * scale, -0.10f * scale),
                new Vector3(bigR * 0.78f, bigR * 0.72f, bigR * 0.78f), accent);
            MakePrimitive(tree.transform, "Foliage3", PrimitiveType.Sphere,
                new Vector3(-0.32f * scale, foliageY + 0.12f * scale, 0.22f * scale),
                new Vector3(bigR * 0.72f, bigR * 0.68f, bigR * 0.72f), primary);
            MakePrimitive(tree.transform, "FoliageTop", PrimitiveType.Sphere,
                new Vector3(0f, foliageY + 0.58f * scale, 0f),
                new Vector3(bigR * 0.55f, bigR * 0.55f, bigR * 0.55f), accent);
        }

        // Broken pillar stump — like a ruined column with a jagged top
        void BuildBrokenPillar(Transform parent, Vector3 basePos, float height)
        {
            var p = new GameObject("BrokenPillar");
            p.transform.SetParent(parent, false);
            p.transform.localPosition = basePos;
            int seed = Mathf.Abs(Mathf.RoundToInt(basePos.x * 11f + basePos.z * 5f));
            p.transform.localRotation = Quaternion.Euler(0f, (seed * 13) % 360f, 0f);

            // Square base — collidable so player can't walk through ruined pillar.
            MakeCube(p.transform, "Base", new Vector3(0f, 0.25f, 0f), new Vector3(1.25f, 0.5f, 1.25f), _stoneDark, keepCollider: true);
            // Shaft (height varies) — collidable.
            MakeCube(p.transform, "Shaft", new Vector3(0f, 0.5f + height * 0.5f, 0f), new Vector3(0.85f, height, 0.85f), _stone, keepCollider: true);
            // Jagged break at the top — two angled cubes
            var break1 = MakeCube(p.transform, "Break1", new Vector3(0f, 0.5f + height + 0.1f, 0f), new Vector3(0.9f, 0.30f, 0.9f), _stoneDark);
            break1.transform.localRotation = Quaternion.Euler(8f, 0f, -10f);
            var break2 = MakeCube(p.transform, "Break2", new Vector3(0.1f, 0.5f + height + 0.30f, -0.1f), new Vector3(0.5f, 0.18f, 0.5f), _stone);
            break2.transform.localRotation = Quaternion.Euler(-12f, 30f, 6f);
        }

        // Invisible BoxColliders forming a fence around the playable area.
        // No MeshRenderer — they're collision-only.
        void BuildInvisibleWalls(Transform root)
        {
            var walls = new GameObject("InvisibleWalls");
            walls.transform.SetParent(root, false);

            float minX = -courtyardHalfWidth - wallPadding;
            float maxX =  courtyardHalfWidth + wallPadding;
            float minZ = frontRowZ - wallPadding;
            float maxZ = backRowZ  + wallPadding;
            float spanX = maxX - minX;
            float spanZ = maxZ - minZ;
            float midX = (minX + maxX) * 0.5f;
            float midZ = (minZ + maxZ) * 0.5f;
            float yCenter = wallHeight * 0.5f;

            // North wall (back, behind merchants)
            MakeWall(walls.transform, "Wall_North", new Vector3(midX, yCenter, maxZ), new Vector3(spanX + wallThickness * 2f, wallHeight, wallThickness));
            // South wall (front, behind portal)
            MakeWall(walls.transform, "Wall_South", new Vector3(midX, yCenter, minZ), new Vector3(spanX + wallThickness * 2f, wallHeight, wallThickness));
            // West wall (left)
            MakeWall(walls.transform, "Wall_West",  new Vector3(minX, yCenter, midZ), new Vector3(wallThickness, wallHeight, spanZ));
            // East wall (right)
            MakeWall(walls.transform, "Wall_East",  new Vector3(maxX, yCenter, midZ), new Vector3(wallThickness, wallHeight, spanZ));
        }

        static void MakeWall(Transform parent, string name, Vector3 worldPos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = worldPos;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            // No MeshRenderer — invisible.
        }

        // ------------------------------------------------------------------
        // Perimeter pillars with braziers on top + banners + base props.
        // ------------------------------------------------------------------
        void BuildPerimeterPillars(Transform root)
        {
            var pillars = new GameObject("Pillars");
            pillars.transform.SetParent(root, false);

            // Build a row of pillars along each side, from frontRowZ to backRowZ.
            int rowCount = Mathf.Max(2, Mathf.RoundToInt((backRowZ - frontRowZ) / pillarSpacingZ) + 1);
            for (int i = 0; i < rowCount; i++)
            {
                float t = (rowCount == 1) ? 0.5f : i / (float)(rowCount - 1);
                float z = Mathf.Lerp(frontRowZ, backRowZ, t);

                // Left pillar
                BuildPillar(pillars.transform, $"PillarL_{i}", new Vector3(-courtyardHalfWidth, 0f, z),
                    facingInward: 1f, withBanner: (i == 0 || i == rowCount - 1));
                // Right pillar
                BuildPillar(pillars.transform, $"PillarR_{i}", new Vector3(courtyardHalfWidth, 0f, z),
                    facingInward: -1f, withBanner: (i == 0 || i == rowCount - 1));
            }
        }

        void BuildPillar(Transform parent, string name, Vector3 basePos, float facingInward, bool withBanner)
        {
            var p = new GameObject(name);
            p.transform.SetParent(parent, false);
            p.transform.localPosition = basePos;

            // Stone base — collidable so player can't walk through.
            MakeCube(p.transform, "Base",   new Vector3(0f, 0.30f, 0f), new Vector3(1.40f, 0.50f, 1.40f), _stoneDark, keepCollider: true);
            MakeCube(p.transform, "BaseRim",new Vector3(0f, 0.55f, 0f), new Vector3(1.55f, 0.10f, 1.55f), _iron);
            // Column shaft — collidable.
            MakeCube(p.transform, "Shaft",  new Vector3(0f, 2.60f, 0f), new Vector3(0.95f, 4.20f, 0.95f), _stone, keepCollider: true);
            // Mid iron band
            MakeCube(p.transform, "Band",   new Vector3(0f, 2.60f, 0f), new Vector3(1.05f, 0.20f, 1.05f), _iron);
            // Capital (top cap)
            MakeCube(p.transform, "Capital",new Vector3(0f, 4.85f, 0f), new Vector3(1.30f, 0.30f, 1.30f), _stoneLight);
            MakeCube(p.transform, "CapTrim",new Vector3(0f, 5.05f, 0f), new Vector3(1.40f, 0.10f, 1.40f), _iron);

            // Brazier on top
            BuildBrazier(p.transform, new Vector3(0f, 5.30f, 0f));

            // Optional hanging banner draped on the inward face of the pillar
            if (withBanner)
            {
                Color cloth = (name.Contains("L_0") || name.Contains("R_0")) ? _bannerAmber.color : _bannerTeal.color;
                var bannerMat = MakeMat(cloth, 0.05f, 0.45f);
                BuildBanner(p.transform, new Vector3(facingInward * 0.55f, 4.40f, 0f), bannerMat, _bannerTrim);
            }
        }

        // Brazier — stone bowl on a column with a flame and a warm point light.
        void BuildBrazier(Transform parent, Vector3 pos)
        {
            var b = new GameObject("Brazier");
            b.transform.SetParent(parent, false);
            b.transform.localPosition = pos;

            // Bowl base
            MakePrimitive(b.transform, "BowlBase", PrimitiveType.Cylinder, new Vector3(0f, 0.10f, 0f), new Vector3(0.60f, 0.10f, 0.60f), _iron);
            MakePrimitive(b.transform, "Bowl",     PrimitiveType.Cylinder, new Vector3(0f, 0.30f, 0f), new Vector3(0.80f, 0.20f, 0.80f), _iron);
            // Coals (rust-emissive)
            MakePrimitive(b.transform, "Coals",    PrimitiveType.Sphere,   new Vector3(0f, 0.42f, 0f), new Vector3(0.55f, 0.20f, 0.55f), _rust);

            // Flame: use particle VFX prefab if assigned (much more realistic), else procedural spheres.
            bool useVfx = brazierVfxPrefab != null;
            if (!useVfx || !hideProcFlamesWhenVfx)
            {
                MakePrimitive(b.transform, "Flame1",   PrimitiveType.Sphere,   new Vector3(0f,    0.65f, 0f),    new Vector3(0.40f, 0.55f, 0.40f), _flame);
                MakePrimitive(b.transform, "Flame2",   PrimitiveType.Sphere,   new Vector3(-0.10f, 0.55f, 0.05f), new Vector3(0.22f, 0.40f, 0.22f), _flame);
                MakePrimitive(b.transform, "Flame3",   PrimitiveType.Sphere,   new Vector3( 0.08f, 0.58f, -0.06f), new Vector3(0.20f, 0.36f, 0.20f), _flame);
            }
            if (useVfx)
            {
                var vfx = Instantiate(brazierVfxPrefab, b.transform);
                vfx.name = "FlameVFX";
                vfx.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                vfx.transform.localScale = Vector3.one * Mathf.Max(0.01f, brazierVfxScale);
            }

            // Ember sparks rising off the bowl
            if (buildEmberSparks)
            {
                BuildEmberSparks(b.transform, new Vector3(0f, 0.55f, 0f), emissionRate: 6f, radius: 0.18f, lifetime: 1.2f, riseSpeed: 0.8f);
            }

            // Warm point light
            var lightGo = new GameObject("TorchLight");
            lightGo.transform.SetParent(b.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.60f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = torchColor;
            light.intensity = torchIntensity;
            light.range = torchRange;
            light.shadows = LightShadows.None;
            lightGo.AddComponent<FireLightFlicker>();
        }

        // Hanging banner with frayed bottom.
        void BuildBanner(Transform parent, Vector3 topPos, Material cloth, Material trim)
        {
            var go = new GameObject("Banner");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = topPos;

            MakeCube(go.transform, "Rail", new Vector3(0f, 0f, 0f), new Vector3(0.04f, 0.05f, 1.20f), trim);
            MakeCube(go.transform, "Cloth", new Vector3(0f, -1.20f, 0.04f), new Vector3(0.04f, 2.40f, 1.00f), cloth);
            MakeCube(go.transform, "TrimStrip", new Vector3(0f, -0.10f, 0.06f), new Vector3(0.04f, 0.12f, 1.00f), trim);
            // Frayed bottom
            for (int i = -2; i <= 2; i++)
            {
                var frag = MakeCube(go.transform, $"Fray{i}", new Vector3(0.04f, -2.45f, i * 0.22f), new Vector3(0.04f, 0.18f, 0.16f), cloth);
                frag.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            }
        }

        // ------------------------------------------------------------------
        // Scenery — barrels, crates, rope coils around the merchant area.
        // ------------------------------------------------------------------
        void BuildSceneryProps(Transform root)
        {
            var props = new GameObject("Scenery");
            props.transform.SetParent(root, false);

            // Barrels and crates near the merchant area (north side, around z ≈ 3..7).
            BuildBarrel(props.transform, new Vector3(-courtyardHalfWidth + 1.5f, 0f, 2.5f));
            BuildBarrel(props.transform, new Vector3(-courtyardHalfWidth + 1.0f, 0f, 4.0f));
            BuildBarrel(props.transform, new Vector3( courtyardHalfWidth - 1.5f, 0f, 2.5f));
            BuildCrate(props.transform,  new Vector3(-courtyardHalfWidth + 2.5f, 0f, 6.5f), 0.9f, 15f);
            BuildCrate(props.transform,  new Vector3( courtyardHalfWidth - 2.5f, 0f, 6.5f), 1.0f, -10f);
            BuildCrate(props.transform,  new Vector3( courtyardHalfWidth - 1.4f, 0f, 7.6f), 0.7f, 25f);

            // Barrels around the portal end too
            BuildBarrel(props.transform, new Vector3(-courtyardHalfWidth + 1.2f, 0f, -16f));
            BuildBarrel(props.transform, new Vector3( courtyardHalfWidth - 1.2f, 0f, -16f));
            BuildCrate(props.transform,  new Vector3(-courtyardHalfWidth + 2.4f, 0f, -14f), 0.9f, -8f);
            BuildCrate(props.transform,  new Vector3( courtyardHalfWidth - 2.4f, 0f, -14f), 0.8f, 12f);

            // Wagon wheel leaning against a pillar
            BuildWagonWheel(props.transform, new Vector3(-courtyardHalfWidth + 1.0f, 0f, 8.5f));
        }

        void BuildBarrel(Transform parent, Vector3 basePos)
        {
            var b = new GameObject("Barrel");
            b.transform.SetParent(parent, false);
            b.transform.localPosition = basePos;

            MakePrimitive(b.transform, "Body", PrimitiveType.Cylinder, new Vector3(0f, 0.55f, 0f), new Vector3(0.65f, 0.55f, 0.65f), _wood, keepCollider: true);
            MakePrimitive(b.transform, "Top",  PrimitiveType.Cylinder, new Vector3(0f, 1.06f, 0f), new Vector3(0.55f, 0.04f, 0.55f), _woodDark);
            // Iron bands
            for (int i = 0; i < 3; i++)
            {
                float y = 0.20f + i * 0.36f;
                MakePrimitive(b.transform, $"Band_{i}", PrimitiveType.Cylinder, new Vector3(0f, y, 0f), new Vector3(0.68f, 0.04f, 0.68f), _iron);
            }
        }

        void BuildCrate(Transform parent, Vector3 basePos, float scale, float yawDeg)
        {
            var c = new GameObject("Crate");
            c.transform.SetParent(parent, false);
            c.transform.localPosition = basePos;
            c.transform.localRotation = Quaternion.Euler(0f, yawDeg, 0f);

            float s = scale;
            MakeCube(c.transform, "Body", new Vector3(0f, s * 0.45f, 0f), new Vector3(s * 0.9f, s * 0.9f, s * 0.9f), _wood, keepCollider: true);
            // Plank seams (vertical strips of darker wood on each face)
            MakeCube(c.transform, "SeamFront", new Vector3(0f, s * 0.45f, -s * 0.46f), new Vector3(s * 0.04f, s * 0.85f, s * 0.02f), _woodDark);
            MakeCube(c.transform, "SeamBack",  new Vector3(0f, s * 0.45f,  s * 0.46f), new Vector3(s * 0.04f, s * 0.85f, s * 0.02f), _woodDark);
            // Iron corner straps
            foreach (var corner in new[] { new Vector3(-1, 0, -1), new Vector3(1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1) })
            {
                MakeCube(c.transform, "CornerStrap",
                    new Vector3(corner.x * s * 0.45f, s * 0.45f, corner.z * s * 0.45f),
                    new Vector3(s * 0.08f, s * 0.92f, s * 0.08f), _iron);
            }
        }

        void BuildWagonWheel(Transform parent, Vector3 basePos)
        {
            var w = new GameObject("WagonWheel");
            w.transform.SetParent(parent, false);
            w.transform.localPosition = basePos;
            w.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);

            // Outer rim (torus-ish — use a flat cylinder)
            var rim = MakePrimitive(w.transform, "Rim", PrimitiveType.Cylinder, new Vector3(0f, 0.85f, 0f), new Vector3(1.20f, 0.10f, 1.20f), _wood);
            rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            // Hub
            var hub = MakePrimitive(w.transform, "Hub", PrimitiveType.Cylinder, new Vector3(0f, 0.85f, 0f), new Vector3(0.25f, 0.12f, 0.25f), _iron);
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            // Spokes (4)
            for (int i = 0; i < 4; i++)
            {
                var spoke = MakeCube(w.transform, $"Spoke_{i}", new Vector3(0f, 0.85f, 0f), new Vector3(0.06f, 1.0f, 0.06f), _wood);
                spoke.transform.localRotation = Quaternion.Euler(0f, 0f, i * 45f);
            }
        }

        // ------------------------------------------------------------------
        // Central fire pit / gathering — ring of stones with a flame.
        // ------------------------------------------------------------------
        void BuildCentralFirePit(Transform root)
        {
            var f = new GameObject("FirePit");
            f.transform.SetParent(root, false);
            f.transform.localPosition = new Vector3(0f, 0f, -7f);

            // Stone ring (8 stones around) — collidable so player walks around, not through.
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                var s = MakePrimitive(f.transform, $"Stone_{i}", PrimitiveType.Cube,
                    new Vector3(Mathf.Cos(a) * 1.30f, 0.18f, Mathf.Sin(a) * 1.30f),
                    new Vector3(0.55f, 0.32f, 0.45f), _stoneDark, keepCollider: true);
                s.transform.localRotation = Quaternion.Euler(0f, -i * 45f + 90f, ((i * 13) % 5) - 2f);
            }

            // Logs and coals in the center
            var log1 = MakePrimitive(f.transform, "Log1", PrimitiveType.Cylinder, new Vector3(0f, 0.20f, 0f), new Vector3(0.20f, 0.50f, 0.20f), _wood);
            log1.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var log2 = MakePrimitive(f.transform, "Log2", PrimitiveType.Cylinder, new Vector3(0f, 0.20f, 0f), new Vector3(0.20f, 0.50f, 0.20f), _wood);
            log2.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            MakePrimitive(f.transform, "Coals", PrimitiveType.Sphere, new Vector3(0f, 0.30f, 0f), new Vector3(0.55f, 0.20f, 0.55f), _rust);

            // Flame: VFX prefab if assigned (realistic particle fire), else procedural spheres.
            bool useFireVfx = firePitVfxPrefab != null;
            if (!useFireVfx || !hideProcFlamesWhenVfx)
            {
                MakePrimitive(f.transform, "Flame1", PrimitiveType.Sphere, new Vector3(0f, 0.55f, 0f), new Vector3(0.45f, 0.65f, 0.45f), _flame);
                MakePrimitive(f.transform, "Flame2", PrimitiveType.Sphere, new Vector3(-0.12f, 0.45f, 0.05f), new Vector3(0.24f, 0.45f, 0.24f), _flame);
                MakePrimitive(f.transform, "Flame3", PrimitiveType.Sphere, new Vector3( 0.10f, 0.48f, -0.06f), new Vector3(0.22f, 0.40f, 0.22f), _flame);
            }
            if (useFireVfx)
            {
                var vfx = Instantiate(firePitVfxPrefab, f.transform);
                vfx.name = "FireVFX";
                vfx.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                vfx.transform.localScale = Vector3.one * Mathf.Max(0.01f, firePitVfxScale);
            }

            // Bigger, more dramatic ember sparks for the central bonfire
            if (buildEmberSparks)
            {
                BuildEmberSparks(f.transform, new Vector3(0f, 0.45f, 0f), emissionRate: 20f, radius: 0.40f, lifetime: 2.0f, riseSpeed: 1.2f);
            }

            // Strong central point light
            var lightGo = new GameObject("FireLight");
            lightGo.transform.SetParent(f.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = torchColor;
            light.intensity = torchIntensity * 1.3f;
            light.range = torchRange * 1.6f;
            light.shadows = LightShadows.None;
            // Central fire pit gets a slightly more dramatic flicker
            lightGo.AddComponent<FireLightFlicker>().Configure(0.3f, 4.5f);
        }

        // ------------------------------------------------------------------
        // Atmospheric particles — dust motes drifting across the courtyard,
        // ember sparks rising off braziers and the central fire pit.
        // ------------------------------------------------------------------
        void BuildDustMotes(Transform root)
        {
            var go = new GameObject("DustMotes");
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(0f, 1.5f, -4f); // center over playable area
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(12f, 25f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.10f);
            // Tiny motes — pinpoint specks catching light, not visible flecks.
            main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.018f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.92f, 0.75f, 0.20f),
                new Color(1f, 0.85f, 0.65f, 0.30f));
            main.gravityModifier = -0.02f; // slight upward drift
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2f * courtyardHalfWidth, 4f, backRowZ - frontRowZ);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            // All three axes must be the same MinMaxCurve mode — otherwise Unity errors per frame.
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.92f, 0.75f), 0f), new GradientColorKey(new Color(1f, 0.85f, 0.60f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.6f, 0.2f), new GradientAlphaKey(0.6f, 0.8f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = grad;

            ConfigureParticleRenderer(ps, new Color(1f, 0.92f, 0.75f));
        }

        // Fake volumetric god rays — long stretched particle billboards that fall from above
        // and represent shafts of sunlight cutting through the atmosphere. URP has no true
        // volumetric lighting, so this is the closest cheap approximation.
        void BuildGodRays(Transform root)
        {
            var go = new GameObject("GodRays");
            go.transform.SetParent(root, false);
            // Spawn above the courtyard so rays fall onto it.
            go.transform.localPosition = new Vector3(0f, 12f, -4f);
            var ps = go.AddComponent<ParticleSystem>();

            Vector3 dir = godRayDirection.sqrMagnitude < 0.01f ? Vector3.down : godRayDirection.normalized;

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(godRayWidth * 0.8f, godRayWidth * 1.4f);
            main.startColor = godRayColor;
            main.gravityModifier = 0f;
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);

            var emission = ps.emission;
            emission.rateOverTime = godRayEmissionRate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(courtyardHalfWidth * 1.6f, 1f, (backRowZ - frontRowZ) * 0.8f);

            // Constant downward force along sun direction.
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(dir.x * 0.5f, dir.x * 0.8f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(dir.y * 0.5f, dir.y * 0.8f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(dir.z * 0.5f, dir.z * 0.8f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] {
                    new GradientColorKey(godRayColor, 0f),
                    new GradientColorKey(godRayColor, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(godRayColor.a, 0.3f),
                    new GradientAlphaKey(godRayColor.a, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = grad;

            // Stretch the billboard into a long shaft along the velocity direction.
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 12f;
            renderer.velocityScale = 0f;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            // Force the streak to align with the velocity vector (the godRayDirection).
            renderer.alignment = ParticleSystemRenderSpace.Velocity;

            // Reuse the same additive particle material as dust motes / embers.
            ConfigureParticleRenderer(ps, godRayColor);
        }

        void BuildEmberSparks(Transform parent, Vector3 localPos, float emissionRate, float radius, float lifetime, float riseSpeed)
        {
            var go = new GameObject("EmberSparks");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.7f, lifetime * 1.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(riseSpeed * 0.6f, riseSpeed * 1.2f);
            // Much smaller — embers should read as pinpoint specks, not glowing tiles.
            main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.022f);
            main.startColor = new Color(1f, 0.6f, 0.18f, 1f);
            main.gravityModifier = -0.15f; // float up like hot embers
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = emissionRate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = radius;
            shape.position = Vector3.zero;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] {
                    new GradientColorKey(new Color(1f, 0.85f, 0.30f), 0f),
                    new GradientColorKey(new Color(1f, 0.45f, 0.10f), 0.5f),
                    new GradientColorKey(new Color(0.40f, 0.10f, 0.02f), 1f)
                },
                new[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.6f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = grad;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.2f)));

            // Slight horizontal drift so embers swirl as they rise.
            // All three axes must use the same MinMaxCurve mode.
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);

            // Light emission module to add a tiny additive glow per ember (optional, can be heavy)
            // Skipped to keep perf clean.

            ConfigureParticleRenderer(ps, new Color(1f, 0.55f, 0.15f));
        }

        static Material _particleMat;
        static void ConfigureParticleRenderer(ParticleSystem ps, Color tint)
        {
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            if (_particleMat == null)
            {
                // Try URP particles unlit first, then fallback to built-in particle additive.
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
                if (shader == null) shader = Shader.Find("Mobile/Particles/Additive");
                if (shader == null) shader = Shader.Find("Standard");
                _particleMat = new Material(shader) { name = "RuntimeParticleMat" };
                // URP particle unlit: set surface to Transparent, blend to Additive.
                if (_particleMat.HasProperty("_Surface")) _particleMat.SetFloat("_Surface", 1f);
                if (_particleMat.HasProperty("_Blend")) _particleMat.SetFloat("_Blend", 1f); // Additive
                if (_particleMat.HasProperty("_BaseColor")) _particleMat.SetColor("_BaseColor", Color.white);
                if (_particleMat.HasProperty("_Color")) _particleMat.SetColor("_Color", Color.white);
                _particleMat.renderQueue = 3000;
                _particleMat.mainTexture = Texture2D.whiteTexture;
            }
            renderer.material = _particleMat;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        static GameObject MakeCube(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat, bool keepCollider = false)
            => MakePrimitive(parent, name, PrimitiveType.Cube, localPos, localScale, mat, keepCollider);

        static GameObject MakePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Material mat, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (!keepCollider)
            {
                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
            return go;
        }

        static Shader _urpLit;
        static Shader UrpLit
        {
            get
            {
                if (_urpLit == null)
                {
                    _urpLit = Shader.Find("Universal Render Pipeline/Lit");
                    if (_urpLit == null) _urpLit = Shader.Find("Standard");
                }
                return _urpLit;
            }
        }

        static Material MakeMat(Color color, float metallic, float smoothness)
        {
            var m = new Material(UrpLit) { color = color };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            return m;
        }

        static Material MakeMatEmissive(Color color, Color emission, float metallic, float smoothness)
        {
            var m = MakeMat(color, metallic, smoothness);
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission);
            return m;
        }
    }
}
