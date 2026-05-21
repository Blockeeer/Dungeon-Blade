using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DungeonBlade.Bank
{
    /// <summary>
    /// Builds a stylized stall (Bank vault or Shop counter) from primitives at runtime.
    /// Attach to the Banker/Shopkeeper GameObject; the existing capsule mesh is hidden
    /// and replaced with a counter + signage + small merchant figure.
    /// </summary>
    [DisallowMultipleComponent]
    public class MerchantStallDecor : MonoBehaviour
    {
        public enum Kind { Bank, Shop }

        [Header("Stall")]
        [SerializeField] Kind kind = Kind.Bank;
        [SerializeField] string overrideSignText = "";
        [Tooltip("Hide the host GameObject's own MeshRenderer so the stall replaces the capsule.")]
        [SerializeField] bool hideHostMesh = true;

        [Header("Merchant model (optional)")]
        [Tooltip("Drag in a character FBX or prefab. If set, the cube humanoid is skipped and your model is instantiated as the merchant.")]
        [SerializeField] GameObject merchantModel;
        [Tooltip("Local position of the merchant model relative to the stall base. Default centers them behind the counter.")]
        [SerializeField] Vector3 merchantLocalPosition = new Vector3(0f, 0f, 0.25f);
        [Tooltip("Local Euler rotation. Mixamo characters face +Z natively, so Y=180 turns them to face the player (-Z).")]
        [SerializeField] Vector3 merchantLocalRotation = new Vector3(0f, 180f, 0f);
        [Tooltip("Uniform scale applied to the spawned model. 1 = native FBX size. Bump up if the model imports tiny (Mixamo files often need 1.5-2x for human size).")]
        [SerializeField] float merchantScale = 1.7f;
        [Tooltip("Optional idle animation clip. Drag in a Mixamo idle clip (e.g. Breathing Idle.fbx) so the merchant breathes instead of T-posing. The clip is played via PlayableGraph — no Animator Controller asset needed and loops automatically.")]
        [SerializeField] AnimationClip merchantIdleClip;
        [Tooltip("Tint applied to the merchant model's materials. White = use the FBX's native color. Use this to color a plain gray mannequin into a unique merchant.")]
        [ColorUsage(false, false)]
        [SerializeField] Color merchantTint = Color.white;

        [Header("Placement")]
        [Tooltip("If true, raycasts straight down at Awake to find the floor and drops the stall so its base sits on it.")]
        [SerializeField] bool autoGround = true;
        [Tooltip("Layer mask used by the ground raycast. Default: everything except triggers.")]
        [SerializeField] LayerMask groundMask = ~0;
        [Tooltip("Extra vertical nudge applied after auto-grounding. When autoGround is off, this is the absolute local Y for the stall base (typical value: -1 if the host pivot is 1m above the floor).")]
        [SerializeField] float groundYOffset = 0f;
        [Tooltip("Yaw rotation in degrees so the stall faces the player. 180 makes the merchant face the opposite direction.")]
        [SerializeField] float yawDegrees = 0f;

        const string BuiltRootName = "_StallDecor";

        void Awake()
        {
            if (hideHostMesh)
            {
                var mr = GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }

            // Rebuild safely (handles domain reloads / duplicate Awake).
            var existing = transform.Find(BuiltRootName);
            if (existing != null) Destroy(existing.gameObject);

            var root = new GameObject(BuiltRootName);
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, ComputeGroundOffset(), 0f);
            root.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);

            if (kind == Kind.Bank) BuildBank(root.transform);
            else BuildShop(root.transform);
        }

        float ComputeGroundOffset()
        {
            if (!autoGround) return groundYOffset;

            // Raycast down from a little above the host pivot to find the actual floor.
            // We temporarily disable our own colliders so the ray doesn't hit them.
            var ownCols = GetComponentsInChildren<Collider>();
            var savedEnabled = new bool[ownCols.Length];
            for (int i = 0; i < ownCols.Length; i++) { savedEnabled[i] = ownCols[i].enabled; ownCols[i].enabled = false; }

            try
            {
                Vector3 origin = transform.position + Vector3.up * 0.5f;
                if (Physics.Raycast(origin, Vector3.down, out var hit, 50f, groundMask, QueryTriggerInteraction.Ignore))
                {
                    // We want the stall's localY=0 (the floor of the prop) to land at the world floor.
                    // root.localY = floor_y - host_y, then add the user nudge.
                    return (hit.point.y - transform.position.y) + groundYOffset;
                }
            }
            finally
            {
                for (int i = 0; i < ownCols.Length; i++) ownCols[i].enabled = savedEnabled[i];
            }

            return groundYOffset;
        }

        // ---------- Bank: iron-bound strongroom — riveted plates, padlocked chest, lanterns ----------
        void BuildBank(Transform root)
        {
            var stone        = MakeMat(new Color(0.30f, 0.28f, 0.26f), 0.05f, 0.35f);
            var stoneDark    = MakeMat(new Color(0.18f, 0.17f, 0.16f), 0.05f, 0.35f);
            var weatheredWood= MakeMat(new Color(0.20f, 0.13f, 0.07f), 0.05f, 0.30f);
            var ironDark     = MakeMat(new Color(0.16f, 0.14f, 0.13f), 0.85f, 0.40f);
            var ironRust     = MakeMatEmissive(new Color(0.40f, 0.22f, 0.10f), new Color(0.05f, 0.02f, 0f), 0.55f, 0.25f);
            var brassDull    = MakeMat(new Color(0.55f, 0.42f, 0.18f), 0.75f, 0.30f);
            var pennantAmber = MakeMat(new Color(0.52f, 0.24f, 0.08f), 0.05f, 0.45f);
            var pennantTrim  = MakeMat(new Color(0.18f, 0.12f, 0.06f), 0.20f, 0.40f);

            // Plinth (stone)
            MakeCube(root, "Plinth", new Vector3(0f, 0.10f, 0f), new Vector3(3.0f, 0.20f, 1.8f), stoneDark, keepCollider: true);
            MakeCube(root, "PlinthTrim", new Vector3(0f, 0.22f, 0f), new Vector3(3.1f, 0.05f, 1.9f), ironDark);

            // Counter — stone front with iron straps + heavy wood top
            MakeCube(root, "CounterFront", new Vector3(0f, 0.60f, -0.35f), new Vector3(2.6f, 0.95f, 0.25f), stone, keepCollider: true);
            MakeCube(root, "CounterStrapL", new Vector3(-0.90f, 0.60f, -0.48f), new Vector3(0.10f, 0.95f, 0.04f), ironDark);
            MakeCube(root, "CounterStrapR", new Vector3( 0.90f, 0.60f, -0.48f), new Vector3(0.10f, 0.95f, 0.04f), ironDark);
            AddRivetColumn(root, new Vector3(-0.90f, 0.60f, -0.50f), 4, 0.20f, ironRust);
            AddRivetColumn(root, new Vector3( 0.90f, 0.60f, -0.50f), 4, 0.20f, ironRust);
            MakeCube(root, "CounterTop",   new Vector3(0f, 1.10f, -0.25f), new Vector3(2.7f, 0.12f, 0.90f), weatheredWood, keepCollider: true);
            MakeCube(root, "CounterTopRim",new Vector3(0f, 1.17f, -0.68f), new Vector3(2.7f, 0.04f, 0.05f), ironDark);

            // Side posts — wood with iron caps and corner straps
            MakeCube(root, "PostL", new Vector3(-1.45f, 1.20f, 0.10f), new Vector3(0.26f, 2.40f, 0.26f), weatheredWood, keepCollider: true);
            MakeCube(root, "PostR", new Vector3( 1.45f, 1.20f, 0.10f), new Vector3(0.26f, 2.40f, 0.26f), weatheredWood, keepCollider: true);
            MakeCube(root, "PostCapL", new Vector3(-1.45f, 2.42f, 0.10f), new Vector3(0.34f, 0.10f, 0.34f), ironDark);
            MakeCube(root, "PostCapR", new Vector3( 1.45f, 2.42f, 0.10f), new Vector3(0.34f, 0.10f, 0.34f), ironDark);
            MakeCube(root, "PostFootL", new Vector3(-1.45f, 0.30f, 0.10f), new Vector3(0.34f, 0.20f, 0.34f), ironDark);
            MakeCube(root, "PostFootR", new Vector3( 1.45f, 0.30f, 0.10f), new Vector3(0.34f, 0.20f, 0.34f), ironDark);

            // Back wall — riveted iron plate
            MakeCube(root, "BackPlate", new Vector3(0f, 1.40f, 0.70f), new Vector3(2.7f, 2.40f, 0.10f), ironDark, keepCollider: true);
            // Plate seam
            MakeCube(root, "BackSeam", new Vector3(0f, 1.40f, 0.64f), new Vector3(2.7f, 0.04f, 0.02f), ironRust);
            // Rivet borders on the plate
            for (int i = 0; i < 6; i++)
            {
                float x = Mathf.Lerp(-1.20f, 1.20f, i / 5f);
                MakePrimitive(root, $"BPRivetT{i}", PrimitiveType.Sphere, new Vector3(x, 2.45f, 0.64f), new Vector3(0.08f, 0.08f, 0.06f), ironRust);
                MakePrimitive(root, $"BPRivetB{i}", PrimitiveType.Sphere, new Vector3(x, 0.40f, 0.64f), new Vector3(0.08f, 0.08f, 0.06f), ironRust);
            }

            // Strongbox chest on counter — iron-bound with padlock
            BuildIronChest(root, new Vector3(0f, 1.18f, -0.10f));

            // Stacks of dull coins beside the chest
            MakeCoinStack(root, "CoinsL", new Vector3(-0.95f, 1.18f, -0.10f), brassDull, 3);
            MakeCoinStack(root, "CoinsR", new Vector3( 0.95f, 1.18f, -0.10f), brassDull, 5);

            // Tattered pennants flanking the back plate
            BuildPennant(root, new Vector3(-0.80f, 2.10f, 0.62f), pennantAmber, pennantTrim);
            BuildPennant(root, new Vector3( 0.80f, 2.10f, 0.62f), pennantAmber, pennantTrim);

            // Oil lanterns hanging from the post caps
            BuildLantern(root, new Vector3(-1.45f, 2.10f, -0.10f));
            BuildLantern(root, new Vector3( 1.45f, 2.10f, -0.10f));

            // Merchant — assigned character model if any, else stylized cube bank guard.
            if (!TrySpawnMerchantModel(root))
            {
                BuildMerchantFigure(root, new Vector3(0f, 0f, 0.15f),
                    bodyColor: new Color(0.22f, 0.16f, 0.10f),       // brown leather coat
                    headColor: new Color(0.82f, 0.66f, 0.54f),
                    accentColor: new Color(0.55f, 0.22f, 0.08f),     // rust sash / cape
                    shoulderMat: ironDark,
                    hooded: false);
            }

            // Sign — weathered plank with iron brackets
            BuildIronSign(root,
                string.IsNullOrEmpty(overrideSignText) ? "BANK" : overrideSignText,
                new Vector3(0f, 2.85f, 0.10f),
                new Color(0.92f, 0.78f, 0.42f),
                weatheredWood, ironDark, ironRust);
        }

        // ---------- Shop: mercenary supply tent — tattered canvas, lantern, weapon rack ----------
        void BuildShop(Transform root)
        {
            var weatheredWood = MakeMat(new Color(0.24f, 0.15f, 0.08f), 0.05f, 0.30f);
            var woodDark      = MakeMat(new Color(0.14f, 0.09f, 0.05f), 0.05f, 0.35f);
            var ironDark      = MakeMat(new Color(0.16f, 0.14f, 0.13f), 0.85f, 0.40f);
            var ironRust      = MakeMatEmissive(new Color(0.40f, 0.22f, 0.10f), new Color(0.05f, 0.02f, 0f), 0.55f, 0.25f);
            var canvasTan     = MakeMat(new Color(0.42f, 0.34f, 0.22f), 0f, 0.45f);
            var canvasDirty   = MakeMat(new Color(0.32f, 0.25f, 0.16f), 0f, 0.50f);
            var pennantTeal   = MakeMat(new Color(0.10f, 0.30f, 0.32f), 0.05f, 0.45f);
            var pennantTrim   = MakeMat(new Color(0.05f, 0.12f, 0.13f), 0.20f, 0.40f);
            var potionEmerald = MakeMatEmissive(new Color(0.18f, 0.55f, 0.28f), new Color(0.04f, 0.18f, 0.08f), 0.30f, 0.35f);
            var potionCrimson = MakeMatEmissive(new Color(0.62f, 0.12f, 0.14f), new Color(0.20f, 0.04f, 0.04f), 0.30f, 0.35f);
            var potionAmber   = MakeMatEmissive(new Color(0.78f, 0.45f, 0.10f), new Color(0.22f, 0.10f, 0.02f), 0.30f, 0.35f);
            var bladeSteel    = MakeMat(new Color(0.60f, 0.62f, 0.66f), 0.90f, 0.55f);

            // Counter — rough planks with iron-strapped corners
            MakeCube(root, "CounterFront", new Vector3(0f, 0.60f, -0.35f), new Vector3(2.8f, 1.10f, 0.25f), weatheredWood, keepCollider: true);
            // Vertical plank seams
            for (int i = -2; i <= 2; i++)
            {
                MakeCube(root, $"PlankSeam{i}", new Vector3(i * 0.55f, 0.60f, -0.48f), new Vector3(0.02f, 1.05f, 0.02f), woodDark);
            }
            // Iron corner straps + rivets
            MakeCube(root, "CornerStrapL", new Vector3(-1.32f, 0.60f, -0.48f), new Vector3(0.14f, 1.05f, 0.05f), ironDark);
            MakeCube(root, "CornerStrapR", new Vector3( 1.32f, 0.60f, -0.48f), new Vector3(0.14f, 1.05f, 0.05f), ironDark);
            AddRivetColumn(root, new Vector3(-1.32f, 0.60f, -0.51f), 4, 0.22f, ironRust);
            AddRivetColumn(root, new Vector3( 1.32f, 0.60f, -0.51f), 4, 0.22f, ironRust);
            // Top
            MakeCube(root, "CounterTop", new Vector3(0f, 1.18f, -0.20f), new Vector3(2.9f, 0.10f, 0.95f), weatheredWood, keepCollider: true);
            MakeCube(root, "CounterTopRim", new Vector3(0f, 1.24f, -0.65f), new Vector3(2.9f, 0.05f, 0.05f), ironDark);

            // Support poles — rough wood, iron strapping
            MakeCube(root, "PoleL", new Vector3(-1.50f, 1.65f, 0.20f), new Vector3(0.14f, 3.30f, 0.14f), woodDark, keepCollider: true);
            MakeCube(root, "PoleR", new Vector3( 1.50f, 1.65f, 0.20f), new Vector3(0.14f, 3.30f, 0.14f), woodDark, keepCollider: true);
            MakeCube(root, "PoleStrapL", new Vector3(-1.50f, 1.30f, 0.20f), new Vector3(0.18f, 0.10f, 0.18f), ironDark);
            MakeCube(root, "PoleStrapR", new Vector3( 1.50f, 1.30f, 0.20f), new Vector3(0.18f, 0.10f, 0.18f), ironDark);

            // Back wall — rough planks with iron seam
            MakeCube(root, "BackWall", new Vector3(0f, 1.50f, 0.58f), new Vector3(2.8f, 1.80f, 0.10f), weatheredWood, keepCollider: true);
            MakeCube(root, "BackSeam", new Vector3(0f, 1.50f, 0.52f), new Vector3(2.8f, 0.05f, 0.02f), ironDark);

            // Tattered canvas tarp — overlapping slumped panels (no bright stripes)
            BuildTatteredCanopy(root, canvasTan, canvasDirty, woodDark);

            // Weapon rack — two leaning blades behind the counter
            BuildLeaningBlade(root, new Vector3(-1.05f, 0f, 0.45f), -12f, weatheredWood, ironDark, bladeSteel);
            BuildLeaningBlade(root, new Vector3( 1.05f, 0f, 0.45f),  12f, weatheredWood, ironDark, bladeSteel);

            // Tattered war pennant draped over front of counter
            BuildPennant(root, new Vector3(0f, 0.95f, -0.49f), pennantTeal, pennantTrim);

            // Dark glass potions with gem-tone liquid
            BuildPotion(root, new Vector3(-1.05f, 1.23f, -0.10f), potionEmerald);
            BuildPotion(root, new Vector3(-0.70f, 1.23f, -0.10f), potionCrimson);
            BuildPotion(root, new Vector3( 0.70f, 1.23f, -0.10f), potionAmber);
            BuildPotion(root, new Vector3( 1.05f, 1.23f, -0.10f), potionEmerald);

            // Pouches / coin sack on counter — small dark cubes
            MakeCube(root, "Pouch1", new Vector3(-0.20f, 1.27f, -0.05f), new Vector3(0.22f, 0.20f, 0.22f), woodDark);
            MakeCube(root, "Pouch2", new Vector3( 0.20f, 1.25f, -0.08f), new Vector3(0.18f, 0.18f, 0.20f), woodDark);

            // Hanging oil lantern from the peak
            BuildLantern(root, new Vector3(0f, 2.80f, -0.10f));

            // Merchant — assigned character model if any, else stylized hooded cube mercenary.
            if (!TrySpawnMerchantModel(root))
            {
                BuildMerchantFigure(root, new Vector3(0f, 0f, 0.15f),
                    bodyColor: new Color(0.16f, 0.14f, 0.13f),      // dark coat
                    headColor: new Color(0.82f, 0.68f, 0.58f),
                    accentColor: new Color(0.55f, 0.18f, 0.10f),    // rust scarf
                    shoulderMat: ironDark,
                    hooded: true);
            }

            // Sign — weathered plank with iron brackets
            BuildIronSign(root,
                string.IsNullOrEmpty(overrideSignText) ? "SHOP" : overrideSignText,
                new Vector3(0f, 3.60f, -0.20f),
                new Color(0.92f, 0.80f, 0.45f),
                weatheredWood, ironDark, ironRust);
        }

        PlayableGraph _idleGraph;
        AnimationClipPlayable _idleClipPlayable;

        // Instantiates the assigned character model as the merchant if one is set.
        // Strips colliders, applies tint color, and optionally drives a Humanoid idle clip
        // via PlayableGraph so the model breathes (looped) instead of standing in T-pose.
        bool TrySpawnMerchantModel(Transform root)
        {
            if (merchantModel == null) return false;
            var instance = Instantiate(merchantModel, root);
            instance.name = "Merchant_Model";
            instance.transform.localPosition = merchantLocalPosition;
            instance.transform.localRotation = Quaternion.Euler(merchantLocalRotation);
            instance.transform.localScale = Vector3.one * Mathf.Max(0.01f, merchantScale);

            foreach (var c in instance.GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }

            // Tint all materials on the spawned model (skins, body parts, accessories).
            // Uses material instances so we don't bleed color back into the shared asset.
            if (merchantTint != Color.white)
            {
                foreach (var r in instance.GetComponentsInChildren<Renderer>())
                {
                    var mats = r.materials; // creates instances
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] == null) continue;
                        if (mats[i].HasProperty("_BaseColor")) mats[i].SetColor("_BaseColor", merchantTint);
                        else if (mats[i].HasProperty("_Color")) mats[i].SetColor("_Color", merchantTint);
                        mats[i].color = merchantTint;
                    }
                    r.materials = mats;
                }
            }

            // Drive idle animation via PlayableGraph (works with Humanoid Mixamo clips,
            // no AnimatorController asset required). Looping handled in Update.
            if (merchantIdleClip != null)
            {
                var animator = instance.GetComponentInChildren<Animator>();
                if (animator == null) animator = instance.AddComponent<Animator>();
                animator.applyRootMotion = false;

                _idleGraph = PlayableGraph.Create($"MerchantIdle_{instance.GetInstanceID()}");
                _idleGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
                var output = AnimationPlayableOutput.Create(_idleGraph, "Animation", animator);
                _idleClipPlayable = AnimationClipPlayable.Create(_idleGraph, merchantIdleClip);
                _idleClipPlayable.SetApplyFootIK(false);
                output.SetSourcePlayable(_idleClipPlayable);
                _idleGraph.Play();
            }

            return true;
        }

        void Update()
        {
            // Manually wrap the clip time to make the idle loop (AnimationClipPlayable
            // doesn't auto-loop, it plays once and stops at the end).
            if (_idleClipPlayable.IsValid() && merchantIdleClip != null && merchantIdleClip.length > 0f)
            {
                double t = _idleClipPlayable.GetTime();
                if (t >= merchantIdleClip.length)
                {
                    _idleClipPlayable.SetTime(t % merchantIdleClip.length);
                }
            }
        }

        void OnDestroy()
        {
            if (_idleGraph.IsValid()) _idleGraph.Destroy();
        }

        // ---------- shared builders ----------
        // Stylized humanoid built from capsules and spheres — closer to the Mixamo character
        // silhouette than the previous cube version. Stands facing -Z (toward the player),
        // with a slight contrapposto pose (head turned, weight on one leg) so it doesn't look stiff.
        void BuildMerchantFigure(Transform root, Vector3 basePos, Color bodyColor, Color headColor, Color accentColor, Material shoulderMat = null, bool hooded = false)
        {
            var coat       = MakeMat(bodyColor, 0.05f, 0.30f);
            var coatDark   = MakeMat(bodyColor * 0.65f, 0.05f, 0.30f);
            var skin       = MakeMat(headColor, 0.05f, 0.35f);
            var hair       = MakeMat(headColor * 0.30f + new Color(0.05f, 0.04f, 0.03f, 0f), 0.05f, 0.30f);
            var accent     = MakeMat(accentColor, 0.10f, 0.40f);
            var leatherDark= MakeMat(new Color(0.08f, 0.05f, 0.03f), 0.10f, 0.30f);
            var bootLeather= MakeMat(new Color(0.10f, 0.07f, 0.04f), 0.05f, 0.30f);
            var eyeMat     = MakeMat(new Color(0.04f, 0.03f, 0.02f), 0f, 0.1f);

            // Parent for the whole figure.
            var fig = new GameObject("Merchant");
            fig.transform.SetParent(root, false);
            fig.transform.localPosition = basePos;
            // Slight contrapposto — tilt the whole figure a tiny bit so it doesn't look rigid.
            fig.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            // ----- legs -----
            // Right leg slightly forward; left leg straight (weight on left).
            // Boots
            MakeCube(fig.transform, "BootL", new Vector3(-0.11f, 0.06f, 0.02f), new Vector3(0.20f, 0.12f, 0.32f), bootLeather);
            MakeCube(fig.transform, "BootR", new Vector3( 0.11f, 0.06f, 0.06f), new Vector3(0.20f, 0.12f, 0.32f), bootLeather);
            // Shins (capsules)
            MakePrimitive(fig.transform, "ShinL", PrimitiveType.Capsule, new Vector3(-0.11f, 0.34f, 0.00f), new Vector3(0.16f, 0.30f, 0.16f), coatDark);
            MakePrimitive(fig.transform, "ShinR", PrimitiveType.Capsule, new Vector3( 0.11f, 0.34f, 0.04f), new Vector3(0.16f, 0.30f, 0.16f), coatDark);
            // Knees
            MakePrimitive(fig.transform, "KneeL", PrimitiveType.Sphere, new Vector3(-0.11f, 0.52f, 0f), new Vector3(0.15f, 0.15f, 0.16f), coatDark);
            MakePrimitive(fig.transform, "KneeR", PrimitiveType.Sphere, new Vector3( 0.11f, 0.52f, 0.04f), new Vector3(0.15f, 0.15f, 0.16f), coatDark);
            // Thighs
            MakePrimitive(fig.transform, "ThighL", PrimitiveType.Capsule, new Vector3(-0.11f, 0.72f, 0f), new Vector3(0.20f, 0.36f, 0.22f), coat);
            MakePrimitive(fig.transform, "ThighR", PrimitiveType.Capsule, new Vector3( 0.11f, 0.72f, 0.02f), new Vector3(0.20f, 0.36f, 0.22f), coat);

            // ----- hips + waist + belt -----
            MakePrimitive(fig.transform, "Pelvis", PrimitiveType.Capsule, new Vector3(0f, 0.95f, 0f), new Vector3(0.38f, 0.22f, 0.28f), coat);
            // Belt strap + buckle
            MakeCube(fig.transform, "Belt",   new Vector3(0f, 1.02f, 0f), new Vector3(0.48f, 0.07f, 0.32f), leatherDark);
            MakeCube(fig.transform, "Buckle", new Vector3(0f, 1.02f, -0.17f), new Vector3(0.10f, 0.09f, 0.04f), accent);
            // Diagonal sash across the chest
            var sash = MakeCube(fig.transform, "Sash", new Vector3(0f, 1.28f, -0.15f), new Vector3(0.62f, 0.08f, 0.04f), accent);
            sash.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);

            // ----- torso -----
            // Tapered chest using a capsule on its side for soft shoulders + a capsule torso vertical.
            MakePrimitive(fig.transform, "Torso", PrimitiveType.Capsule, new Vector3(0f, 1.32f, 0f), new Vector3(0.42f, 0.50f, 0.28f), coat);
            // Chest detail strip (suggests a coat front / breastplate seam)
            MakeCube(fig.transform, "ChestSeam", new Vector3(0f, 1.32f, -0.15f), new Vector3(0.03f, 0.40f, 0.02f), coatDark);
            // Collar
            MakeCube(fig.transform, "Collar", new Vector3(0f, 1.55f, -0.05f), new Vector3(0.30f, 0.06f, 0.18f), coatDark);

            // ----- shoulders / arms -----
            // Shoulder balls
            MakePrimitive(fig.transform, "ShoulderL", PrimitiveType.Sphere, new Vector3(-0.28f, 1.50f, 0f), new Vector3(0.22f, 0.22f, 0.22f), coat);
            MakePrimitive(fig.transform, "ShoulderR", PrimitiveType.Sphere, new Vector3( 0.28f, 1.50f, 0f), new Vector3(0.22f, 0.22f, 0.22f), coat);
            // Upper arms (capsules) — slight outward angle
            var uArmL = MakePrimitive(fig.transform, "UpperArmL", PrimitiveType.Capsule, new Vector3(-0.30f, 1.30f, 0.02f), new Vector3(0.14f, 0.32f, 0.14f), coat);
            uArmL.transform.localRotation = Quaternion.Euler(0f, 0f, 6f);
            var uArmR = MakePrimitive(fig.transform, "UpperArmR", PrimitiveType.Capsule, new Vector3( 0.30f, 1.30f, 0.02f), new Vector3(0.14f, 0.32f, 0.14f), coat);
            uArmR.transform.localRotation = Quaternion.Euler(0f, 0f, -6f);
            // Elbows
            MakePrimitive(fig.transform, "ElbowL", PrimitiveType.Sphere, new Vector3(-0.32f, 1.10f, 0.04f), new Vector3(0.13f, 0.13f, 0.13f), coatDark);
            MakePrimitive(fig.transform, "ElbowR", PrimitiveType.Sphere, new Vector3( 0.32f, 1.10f, 0.04f), new Vector3(0.13f, 0.13f, 0.13f), coatDark);
            // Forearms — angle slightly forward, suggesting hands resting on the counter / hip
            var fArmL = MakePrimitive(fig.transform, "ForearmL", PrimitiveType.Capsule, new Vector3(-0.32f, 0.92f, -0.04f), new Vector3(0.12f, 0.28f, 0.12f), coatDark);
            fArmL.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            var fArmR = MakePrimitive(fig.transform, "ForearmR", PrimitiveType.Capsule, new Vector3( 0.32f, 0.92f, -0.04f), new Vector3(0.12f, 0.28f, 0.12f), coatDark);
            fArmR.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            // Gloves / hands
            MakePrimitive(fig.transform, "HandL", PrimitiveType.Sphere, new Vector3(-0.32f, 0.78f, -0.18f), new Vector3(0.14f, 0.12f, 0.16f), leatherDark);
            MakePrimitive(fig.transform, "HandR", PrimitiveType.Sphere, new Vector3( 0.32f, 0.78f, -0.18f), new Vector3(0.14f, 0.12f, 0.16f), leatherDark);

            // Optional iron pauldrons — now hemispheres on top of the shoulders
            if (shoulderMat != null)
            {
                MakePrimitive(fig.transform, "PauldronL", PrimitiveType.Sphere, new Vector3(-0.30f, 1.54f, 0f), new Vector3(0.28f, 0.20f, 0.28f), shoulderMat);
                MakePrimitive(fig.transform, "PauldronR", PrimitiveType.Sphere, new Vector3( 0.30f, 1.54f, 0f), new Vector3(0.28f, 0.20f, 0.28f), shoulderMat);
            }

            // ----- neck + head -----
            MakePrimitive(fig.transform, "Neck", PrimitiveType.Cylinder, new Vector3(0f, 1.62f, 0f), new Vector3(0.10f, 0.06f, 0.10f), skin);
            MakePrimitive(fig.transform, "Head", PrimitiveType.Sphere,   new Vector3(0f, 1.78f, 0f), new Vector3(0.26f, 0.30f, 0.26f), skin);
            // Subtle chin/jaw using a flattened sphere
            MakePrimitive(fig.transform, "Jaw", PrimitiveType.Sphere, new Vector3(0f, 1.71f, -0.04f), new Vector3(0.20f, 0.10f, 0.18f), skin);
            // Slight head turn for personality
            var headTurn = fig.transform.Find("Head");
            if (headTurn != null) headTurn.localRotation = Quaternion.Euler(0f, hooded ? 10f : -8f, 0f);

            // ----- face: eyes + nose -----
            MakePrimitive(fig.transform, "EyeL", PrimitiveType.Sphere, new Vector3(-0.06f, 1.79f, -0.14f), new Vector3(0.035f, 0.04f, 0.02f), eyeMat);
            MakePrimitive(fig.transform, "EyeR", PrimitiveType.Sphere, new Vector3( 0.06f, 1.79f, -0.14f), new Vector3(0.035f, 0.04f, 0.02f), eyeMat);
            MakePrimitive(fig.transform, "Nose", PrimitiveType.Sphere, new Vector3( 0f,    1.76f, -0.16f), new Vector3(0.045f, 0.05f, 0.04f), skin);

            // ----- hair OR hood OR hat -----
            if (hooded)
            {
                // Hood — a larger sphere over the head with the front face hollowed visually
                // by a darker cube "shadow" sitting just over the forehead.
                MakePrimitive(fig.transform, "Hood",      PrimitiveType.Sphere, new Vector3(0f, 1.84f, 0.04f), new Vector3(0.40f, 0.40f, 0.42f), coatDark);
                MakeCube(fig.transform,      "HoodShade", new Vector3(0f, 1.82f, -0.13f), new Vector3(0.30f, 0.18f, 0.02f), new Material(coatDark) { color = coatDark.color * 0.5f });
                // Drape down the back of the head/neck
                MakeCube(fig.transform, "HoodDrape", new Vector3(0f, 1.60f, 0.12f), new Vector3(0.34f, 0.20f, 0.08f), coatDark);
            }
            else
            {
                // Bank guard: short-cropped hair + a low iron-banded cap
                MakePrimitive(fig.transform, "Hair", PrimitiveType.Sphere, new Vector3(0f, 1.86f, 0.02f), new Vector3(0.28f, 0.18f, 0.28f), hair);
                MakeCube(fig.transform, "Cap",     new Vector3(0f, 1.96f, 0f), new Vector3(0.30f, 0.06f, 0.30f), coatDark);
                MakeCube(fig.transform, "CapBand", new Vector3(0f, 1.93f, 0f), new Vector3(0.32f, 0.03f, 0.32f), shoulderMat != null ? shoulderMat : accent);
            }

            // ----- cape (only for the bank guard — gives a knightly silhouette) -----
            if (!hooded)
            {
                var cape = MakeCube(fig.transform, "Cape", new Vector3(0f, 1.05f, 0.18f), new Vector3(0.50f, 0.90f, 0.04f), accent);
                cape.transform.localRotation = Quaternion.Euler(-4f, 0f, 0f);
                // Cape clasp
                MakePrimitive(fig.transform, "Clasp", PrimitiveType.Sphere, new Vector3(0f, 1.52f, 0.14f), new Vector3(0.10f, 0.06f, 0.06f), shoulderMat != null ? shoulderMat : accent);
            }
            else
            {
                // Shop merchant: short waist-length coat tails + scarf trailing forward
                var coatTailL = MakeCube(fig.transform, "CoatTailL", new Vector3(-0.10f, 0.78f, 0.04f), new Vector3(0.22f, 0.45f, 0.20f), coatDark);
                var coatTailR = MakeCube(fig.transform, "CoatTailR", new Vector3( 0.10f, 0.78f, 0.04f), new Vector3(0.22f, 0.45f, 0.20f), coatDark);
                _ = coatTailL; _ = coatTailR;
                var scarf = MakeCube(fig.transform, "Scarf", new Vector3(0f, 1.50f, -0.18f), new Vector3(0.26f, 0.10f, 0.08f), accent);
                _ = scarf;
                // Scarf trailing end
                MakeCube(fig.transform, "ScarfEnd", new Vector3(-0.10f, 1.30f, -0.20f), new Vector3(0.08f, 0.30f, 0.04f), accent);
            }
        }

        // Iron-bound chest — rectangular base + arched lid with padlock
        void BuildIronChest(Transform root, Vector3 pos)
        {
            var wood = MakeMat(new Color(0.18f, 0.11f, 0.05f), 0.05f, 0.30f);
            var iron = MakeMat(new Color(0.14f, 0.12f, 0.11f), 0.85f, 0.40f);
            var rust = MakeMatEmissive(new Color(0.42f, 0.22f, 0.10f), new Color(0.06f, 0.02f, 0f), 0.55f, 0.25f);
            var brass = MakeMat(new Color(0.55f, 0.42f, 0.18f), 0.75f, 0.30f);

            MakeCube(root, "Chest_Body", pos, new Vector3(0.95f, 0.45f, 0.55f), wood);
            // Iron bands
            MakeCube(root, "Chest_BandT", pos + new Vector3(0f, 0.20f, 0f), new Vector3(0.97f, 0.06f, 0.57f), iron);
            MakeCube(root, "Chest_BandB", pos + new Vector3(0f, -0.20f, 0f), new Vector3(0.97f, 0.06f, 0.57f), iron);
            MakeCube(root, "Chest_BandV", pos + new Vector3(0f, 0f, 0f), new Vector3(0.10f, 0.45f, 0.57f), iron);
            // Corner rivets
            float rx = 0.43f, ry = 0.20f, rz = 0.28f;
            foreach (var s in new[] { new Vector3(-1f, 1f, -1f), new Vector3(1f, 1f, -1f), new Vector3(-1f, -1f, -1f), new Vector3(1f, -1f, -1f) })
                MakePrimitive(root, "Chest_Rivet", PrimitiveType.Sphere, pos + new Vector3(s.x * rx, s.y * ry, s.z * rz), new Vector3(0.08f, 0.08f, 0.06f), rust);
            // Padlock on the front
            MakeCube(root, "Chest_Lock", pos + new Vector3(0f, 0f, -0.30f), new Vector3(0.16f, 0.20f, 0.06f), brass);
            var shackle = MakePrimitive(root, "Chest_Shackle", PrimitiveType.Cylinder, pos + new Vector3(0f, 0.12f, -0.30f), new Vector3(0.12f, 0.04f, 0.12f), brass);
            shackle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        // Tattered hanging pennant — long banner with frayed bottom
        void BuildPennant(Transform root, Vector3 topPos, Material cloth, Material trim)
        {
            var go = new GameObject("Pennant");
            go.transform.SetParent(root, false);
            go.transform.localPosition = topPos;
            go.transform.localRotation = Quaternion.identity;

            // Top rail
            MakeCube(go.transform, "Rail", new Vector3(0f, 0f, 0f), new Vector3(0.55f, 0.04f, 0.04f), trim);
            // Main body
            MakeCube(go.transform, "Body", new Vector3(0f, -0.55f, 0.01f), new Vector3(0.48f, 1.10f, 0.02f), cloth);
            // Trim strip
            MakeCube(go.transform, "Trim", new Vector3(0f, -0.05f, 0.02f), new Vector3(0.48f, 0.06f, 0.02f), trim);
            // Frayed bottom triangles (cubes rotated)
            for (int i = -2; i <= 2; i++)
            {
                var frag = MakeCube(go.transform, $"Fray{i}", new Vector3(i * 0.10f, -1.18f, 0.02f), new Vector3(0.08f, 0.12f, 0.02f), cloth);
                frag.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
        }

        // Hanging oil lantern with warm point light
        void BuildLantern(Transform root, Vector3 anchorPos)
        {
            var iron = MakeMat(new Color(0.10f, 0.09f, 0.08f), 0.85f, 0.40f);
            var flame = MakeMatEmissive(new Color(1f, 0.55f, 0.18f), new Color(3.5f, 1.6f, 0.4f), 0f, 0.20f);
            var glass = MakeMat(new Color(0.95f, 0.85f, 0.55f, 0.35f), 0f, 0.10f);

            var go = new GameObject("Lantern");
            go.transform.SetParent(root, false);
            go.transform.localPosition = anchorPos;

            // Chain (thin cylinder)
            var chain = MakePrimitive(go.transform, "Chain", PrimitiveType.Cylinder, new Vector3(0f, 0.25f, 0f), new Vector3(0.03f, 0.30f, 0.03f), iron);
            _ = chain;
            // Cap
            MakeCube(go.transform, "Cap", new Vector3(0f, 0f, 0f), new Vector3(0.20f, 0.06f, 0.20f), iron);
            // Cage bars (4 vertical iron cylinders)
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                var bar = MakePrimitive(go.transform, $"Bar{i}", PrimitiveType.Cylinder,
                    new Vector3(Mathf.Cos(a) * 0.09f, -0.15f, Mathf.Sin(a) * 0.09f),
                    new Vector3(0.025f, 0.18f, 0.025f), iron);
                _ = bar;
            }
            // Glass housing (semi-transparent sphere)
            MakePrimitive(go.transform, "Glass", PrimitiveType.Sphere, new Vector3(0f, -0.15f, 0f), new Vector3(0.18f, 0.20f, 0.18f), glass);
            // Flame core (small bright sphere)
            MakePrimitive(go.transform, "Flame", PrimitiveType.Sphere, new Vector3(0f, -0.15f, 0f), new Vector3(0.08f, 0.12f, 0.08f), flame);
            // Bottom cap
            MakeCube(go.transform, "Base", new Vector3(0f, -0.28f, 0f), new Vector3(0.18f, 0.04f, 0.18f), iron);

            // Point light
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(go.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.62f, 0.28f);
            light.intensity = 2.8f;
            light.range = 6f;
            light.shadows = LightShadows.None;
        }

        // Tattered overlapping canvas canopy (replaces bright stripes)
        void BuildTatteredCanopy(Transform root, Material canvas, Material canvasDirty, Material trim)
        {
            var canopyParent = new GameObject("Canopy");
            canopyParent.transform.SetParent(root, false);
            canopyParent.transform.localPosition = new Vector3(0f, 3.20f, -0.10f);
            canopyParent.transform.localRotation = Quaternion.Euler(-22f, 0f, 0f);

            // Three overlapping panels of slightly different sizes and tilts for a slumped look
            var p1 = MakeCube(canopyParent.transform, "Panel1", new Vector3(-0.95f, 0f, 0f), new Vector3(1.35f, 0.04f, 1.30f), canvas);
            p1.transform.localRotation = Quaternion.Euler(0f, 0f, 3f);
            var p2 = MakeCube(canopyParent.transform, "Panel2", new Vector3(0.05f, 0.02f, -0.05f), new Vector3(1.40f, 0.04f, 1.30f), canvasDirty);
            p2.transform.localRotation = Quaternion.Euler(0f, 0f, -2f);
            var p3 = MakeCube(canopyParent.transform, "Panel3", new Vector3(1.00f, 0.01f, 0.02f), new Vector3(1.30f, 0.04f, 1.30f), canvas);
            p3.transform.localRotation = Quaternion.Euler(0f, 0f, 4f);

            // Front trim beam
            MakeCube(canopyParent.transform, "FrontBeam", new Vector3(0f, -0.04f, -0.60f), new Vector3(3.10f, 0.08f, 0.08f), trim);
            // Frayed tears along front
            for (int i = -2; i <= 2; i++)
            {
                var tear = MakeCube(canopyParent.transform, $"Tear{i}", new Vector3(i * 0.55f, -0.06f, -0.62f), new Vector3(0.08f, 0.06f, 0.18f), canvas);
                tear.transform.localRotation = Quaternion.Euler(0f, 0f, (i % 2 == 0) ? 25f : -25f);
            }
        }

        // A blade leaning against the back wall — wooden hilt, iron crossguard, steel blade
        void BuildLeaningBlade(Transform root, Vector3 footPos, float tiltDeg, Material wood, Material iron, Material steel)
        {
            var go = new GameObject("Blade");
            go.transform.SetParent(root, false);
            go.transform.localPosition = footPos;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, tiltDeg);

            // Blade
            MakeCube(go.transform, "BladeMain", new Vector3(0f, 1.20f, 0f), new Vector3(0.10f, 1.80f, 0.04f), steel);
            // Tip (sphere)
            MakePrimitive(go.transform, "BladeTip", PrimitiveType.Sphere, new Vector3(0f, 2.10f, 0f), new Vector3(0.10f, 0.10f, 0.04f), steel);
            // Crossguard
            MakeCube(go.transform, "Guard", new Vector3(0f, 0.25f, 0f), new Vector3(0.34f, 0.06f, 0.10f), iron);
            // Grip
            MakeCube(go.transform, "Grip", new Vector3(0f, 0.10f, 0f), new Vector3(0.08f, 0.25f, 0.08f), wood);
            // Pommel
            MakePrimitive(go.transform, "Pommel", PrimitiveType.Sphere, new Vector3(0f, -0.05f, 0f), new Vector3(0.12f, 0.10f, 0.10f), iron);
        }

        // Iron-bracketed wooden sign
        void BuildIronSign(Transform root, string text, Vector3 pos, Color textColor, Material plankMat, Material ironMat, Material rivetMat)
        {
            var go = new GameObject("Sign");
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;

            // Iron bracket arms reaching up to the chain attach
            MakeCube(go.transform, "BracketL", new Vector3(-0.95f, 0.30f, 0f), new Vector3(0.08f, 0.60f, 0.08f), ironMat);
            MakeCube(go.transform, "BracketR", new Vector3( 0.95f, 0.30f, 0f), new Vector3(0.08f, 0.60f, 0.08f), ironMat);
            // Plank
            MakeCube(go.transform, "Plank", new Vector3(0f, 0f, 0f), new Vector3(2.0f, 0.55f, 0.10f), plankMat);
            // Iron edge bands
            MakeCube(go.transform, "EdgeL", new Vector3(-0.97f, 0f, -0.02f), new Vector3(0.08f, 0.55f, 0.08f), ironMat);
            MakeCube(go.transform, "EdgeR", new Vector3( 0.97f, 0f, -0.02f), new Vector3(0.08f, 0.55f, 0.08f), ironMat);
            // Corner rivets
            foreach (var c in new[] {
                new Vector3(-0.97f, 0.22f, -0.06f), new Vector3(-0.97f, -0.22f, -0.06f),
                new Vector3( 0.97f, 0.22f, -0.06f), new Vector3( 0.97f, -0.22f, -0.06f)
            })
            {
                MakePrimitive(go.transform, "SignRivet", PrimitiveType.Sphere, c, new Vector3(0.07f, 0.07f, 0.05f), rivetMat);
            }

            // 3D text
            var tgo = new GameObject("Text");
            tgo.transform.SetParent(go.transform, false);
            tgo.transform.localPosition = new Vector3(0f, 0f, -0.07f);
            var tmp = tgo.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 4.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.characterSpacing = 12f;
            var rt = tmp.rectTransform;
            rt.sizeDelta = new Vector2(2.0f, 0.55f);
            rt.localRotation = Quaternion.identity;
        }

        // Column of rivets running vertically
        void AddRivetColumn(Transform root, Vector3 centerPos, int count, float spacing, Material mat)
        {
            float startY = -(count - 1) * 0.5f * spacing;
            for (int i = 0; i < count; i++)
            {
                MakePrimitive(root, "Rivet", PrimitiveType.Sphere,
                    centerPos + new Vector3(0f, startY + i * spacing, 0f),
                    new Vector3(0.07f, 0.07f, 0.05f), mat);
            }
        }

        void BuildPotion(Transform root, Vector3 pos, Material liquid)
        {
            var glass = MakeMat(new Color(0.85f, 0.92f, 0.95f, 0.6f), 0.05f, 0.1f);
            MakePrimitive(root, "Potion", PrimitiveType.Cylinder, pos,
                new Vector3(0.12f, 0.12f, 0.12f), liquid);
            MakePrimitive(root, "PotionNeck", PrimitiveType.Cylinder, pos + new Vector3(0f, 0.12f, 0f),
                new Vector3(0.05f, 0.04f, 0.05f), glass);
        }

        void MakeCoinStack(Transform root, string name, Vector3 pos, Material gold, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var coin = MakePrimitive(root, $"{name}_{i}", PrimitiveType.Cylinder,
                    pos + new Vector3(0f, i * 0.025f, 0f),
                    new Vector3(0.18f, 0.012f, 0.18f), gold);
                _ = coin;
            }
        }

        // ---------- primitive helpers ----------
        GameObject MakeCube(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat, bool keepCollider = false)
            => MakePrimitive(parent, name, PrimitiveType.Cube, localPos, localScale, mat, keepCollider);

        GameObject MakePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPos, Vector3 localScale, Material mat, bool keepCollider = false)
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

        Material MakeMat(Color color, float metallic, float smoothness)
        {
            var m = new Material(UrpLit) { color = color };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (color.a < 0.99f)
            {
                if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f); // URP: 1 = transparent
                m.renderQueue = 3000;
            }
            return m;
        }

        Material MakeMatEmissive(Color color, Color emission, float metallic, float smoothness)
        {
            var m = MakeMat(color, metallic, smoothness);
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emission);
            return m;
        }
    }
}
