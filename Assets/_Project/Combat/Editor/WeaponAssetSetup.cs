using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DungeonBlade.Combat;
using DungeonBlade.Inventory;
using UnityEditor;
using UnityEngine;

namespace DungeonBlade.EditorTools
{
    /// One-shot importer for the per-rank weapon FBX bundles that live in the
    /// repo's top-level `Weapons/` folder (sibling of `Assets/`). Unity can't
    /// see assets outside the project, so we copy each weapon's FBX + textures
    /// into Assets/_Project/Combat/Weapons/Models/<Rank>/<WeaponName>/, build
    /// a URP/Lit material that wires up base/metal/normal/roughness, and
    /// stamp out a WeaponItem ScriptableObject pre-tagged with the rank.
    /// Idempotent: re-running skips weapons already present.
    public static class WeaponAssetSetup
    {
        // The external bundle folder lives next to Assets/ — Application.dataPath
        // resolves to <project>/Assets, so the parent directory is the project root.
        const string ExternalRoot   = "Weapons";
        const string AssetsModelsRoot = "Assets/_Project/Combat/Weapons/Models";
        const string AssetsItemsRoot  = "Assets/_Project/Combat/Weapons/Items";

        // Folder name → (clean weapon name, rank, weapon kind). Hand-curated
        // because the upstream folder names are messy and we want stable IDs.
        // Kind drives which combat behaviour script (Sword/Gun) the weapon
        // prefab gets when you eventually wire one up; for the import itself
        // it only affects the WeaponItem.weaponKind field.
        struct WeaponSpec
        {
            public string sourceFolder;
            public string cleanName;
            public ItemRank rank;
            public WeaponKind kind;
        }

        static readonly WeaponSpec[] Specs =
        {
            new WeaponSpec { sourceFolder = "longsword+3d+model-common",        cleanName = "Longsword",        rank = ItemRank.Common, kind = WeaponKind.Melee  },
            new WeaponSpec { sourceFolder = "assault+rifle+3d+model-common",    cleanName = "AssaultRifle",     rank = ItemRank.Common, kind = WeaponKind.Ranged },
            new WeaponSpec { sourceFolder = "semi-automatic+pistol-common",     cleanName = "SemiAutoPistol",   rank = ItemRank.Common, kind = WeaponKind.Ranged },
            new WeaponSpec { sourceFolder = "dual+demon+sword+3d+model-rare",   cleanName = "DualDemonSword",   rank = ItemRank.Rare,   kind = WeaponKind.Melee  },
            new WeaponSpec { sourceFolder = "futuristic+rifle+3d+model-rare",   cleanName = "FuturisticRifle",  rank = ItemRank.Rare,   kind = WeaponKind.Ranged },
            new WeaponSpec { sourceFolder = "pistol-blue-rare",                 cleanName = "PistolBlue",       rank = ItemRank.Rare,   kind = WeaponKind.Ranged },
            new WeaponSpec { sourceFolder = "ice+katana+3d+model-legend",       cleanName = "IceKatana",        rank = ItemRank.Legendary, kind = WeaponKind.Melee  },
            new WeaponSpec { sourceFolder = "fantasy+gun+3d+model-legend",      cleanName = "FantasyGun",       rank = ItemRank.Legendary, kind = WeaponKind.Ranged },
            new WeaponSpec { sourceFolder = "fantasy+pistol+3d+model-legend",   cleanName = "FantasyPistol",    rank = ItemRank.Legendary, kind = WeaponKind.Ranged },
        };

        [MenuItem("Tools/Dungeon Blade/Weapons/1. Import Weapons by Rank")]
        public static void ImportWeaponsByRank()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string externalAbs = Path.Combine(projectRoot, ExternalRoot);
            if (!Directory.Exists(externalAbs))
            {
                Debug.LogError($"[Dungeon Blade] External weapons folder not found at {externalAbs}. Expected sibling of Assets/.");
                return;
            }

            Directory.CreateDirectory(AssetsModelsRoot);
            Directory.CreateDirectory(AssetsItemsRoot);
            // Pre-make rank subfolders so manual organisation in the Project
            // window doesn't require fighting Unity's missing-folder warnings.
            foreach (ItemRank rank in Enum.GetValues(typeof(ItemRank)))
            {
                Directory.CreateDirectory($"{AssetsModelsRoot}/{rank}");
                Directory.CreateDirectory($"{AssetsItemsRoot}/{rank}");
            }
            AssetDatabase.Refresh();

            int imported = 0, skipped = 0, missing = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var spec in Specs)
                {
                    string sourceDir = Path.Combine(externalAbs, spec.sourceFolder);
                    if (!Directory.Exists(sourceDir))
                    {
                        Debug.LogWarning($"[Dungeon Blade]   missing  {spec.sourceFolder} — skipped.");
                        missing++;
                        continue;
                    }

                    string targetDir = $"{AssetsModelsRoot}/{spec.rank}/{spec.cleanName}";
                    if (Directory.Exists(targetDir) && Directory.GetFiles(targetDir, "*.fbx").Length > 0)
                    {
                        Debug.Log($"[Dungeon Blade]   skip     {spec.cleanName} ({spec.rank}) — already imported.");
                        skipped++;
                        continue;
                    }
                    Directory.CreateDirectory(targetDir);

                    if (!CopyWeaponSourceFiles(sourceDir, targetDir, spec.cleanName))
                    {
                        Debug.LogWarning($"[Dungeon Blade]   no FBX in {sourceDir} — skipped.");
                        missing++;
                        continue;
                    }

                    imported++;
                    Debug.Log($"[Dungeon Blade]   import   {spec.cleanName} ({spec.rank}) → {targetDir}");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            // Material + WeaponItem creation has to happen AFTER the asset
            // database has actually picked up the copied files — that's why
            // it runs in a second pass outside StartAssetEditing.
            int materialsCreated = 0, itemsCreated = 0;
            foreach (var spec in Specs)
            {
                string targetDir = $"{AssetsModelsRoot}/{spec.rank}/{spec.cleanName}";
                string fbxPath   = $"{targetDir}/{spec.cleanName}.fbx";
                if (!File.Exists(fbxPath)) continue;

                Material mat = CreateOrUpdateWeaponMaterial(spec, targetDir);
                if (mat != null) materialsCreated++;

                if (mat != null) RemapFbxMaterial(fbxPath, mat);

                WeaponItem item = CreateOrUpdateWeaponItem(spec);
                if (item != null) itemsCreated++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Dungeon Blade] Weapons import done. {imported} imported, {skipped} skipped, {missing} missing. Created/updated {materialsCreated} material(s) and {itemsCreated} WeaponItem asset(s). Materials and Items grouped under Common / Rare / Legend.");
        }

        static bool CopyWeaponSourceFiles(string sourceDir, string targetDir, string cleanName)
        {
            // Each upstream folder has one FBX and one .fbm/ subfolder with
            // basecolor / metallic / normal / roughness JPEGs. We copy the FBX
            // to <Clean>.fbx and the textures to <Clean>_basecolor.jpeg etc.
            // Renaming standardises the asset names so the rest of the editor
            // pipeline can reason about them by suffix instead of GUID.
            var fbxFiles = Directory.GetFiles(sourceDir, "*.fbx", SearchOption.TopDirectoryOnly);
            if (fbxFiles.Length == 0) return false;
            File.Copy(fbxFiles[0], Path.Combine(targetDir, $"{cleanName}.fbx"), overwrite: true);

            var fbmDirs = Directory.GetDirectories(sourceDir, "*.fbm", SearchOption.TopDirectoryOnly);
            foreach (var fbm in fbmDirs)
            {
                foreach (var tex in Directory.GetFiles(fbm))
                {
                    string baseName = Path.GetFileNameWithoutExtension(tex);
                    string ext      = Path.GetExtension(tex); // includes leading dot
                    string suffix   = SuffixFromTextureName(baseName);
                    string outName  = $"{cleanName}_{suffix}{ext}";
                    File.Copy(tex, Path.Combine(targetDir, outName), overwrite: true);
                }
            }
            return true;
        }

        // Texture file names look like "longsword3dmodel_basecolor". Strip the
        // upstream prefix and keep just the suffix that tells us which channel
        // it is (basecolor / metallic / normal / roughness).
        static string SuffixFromTextureName(string baseName)
        {
            int underscore = baseName.LastIndexOf('_');
            if (underscore < 0 || underscore == baseName.Length - 1) return baseName;
            return baseName.Substring(underscore + 1).ToLowerInvariant();
        }

        static Material CreateOrUpdateWeaponMaterial(WeaponSpec spec, string targetDir)
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("[Dungeon Blade] URP/Lit shader not found. Is URP installed?");
                return null;
            }

            string matPath = $"{targetDir}/{spec.cleanName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(urpLit) { name = spec.cleanName };
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else if (mat.shader != urpLit)
            {
                mat.shader = urpLit;
            }

            // Texture lookup is by suffix — the importer renamed them earlier.
            Texture2D baseMap   = LoadTextureFromDir(targetDir, spec.cleanName, "basecolor");
            Texture2D metallic  = LoadTextureFromDir(targetDir, spec.cleanName, "metallic");
            Texture2D normalMap = LoadTextureFromDir(targetDir, spec.cleanName, "normal");
            Texture2D roughness = LoadTextureFromDir(targetDir, spec.cleanName, "roughness");

            // Normal map texture type defaults to Default for newly-imported
            // JPEGs; flip it to NormalMap so URP samples correctly.
            EnsureTextureType(normalMap, TextureImporterType.NormalMap);

            if (baseMap   != null && mat.HasProperty("_BaseMap"))   mat.SetTexture("_BaseMap",   baseMap);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Metallic"))  mat.SetFloat("_Metallic", metallic != null ? 1f : 0f);
            // Roughness → smoothness inversion (no per-pixel control without a
            // packed mask channel — the high-level tint matches roughness map
            // intensity well enough for prototype play).
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", roughness != null ? 0.55f : 0.4f);
            if (normalMap != null && mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normalMap);
            if (metallic  != null && mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", metallic);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Texture2D LoadTextureFromDir(string dir, string cleanName, string suffix)
        {
            // Try common JPEG/PNG variants — Tripo emits .JPEG (uppercase) but
            // some hand-edited folders use .jpg / .png.
            string[] candidates =
            {
                $"{dir}/{cleanName}_{suffix}.JPEG",
                $"{dir}/{cleanName}_{suffix}.jpeg",
                $"{dir}/{cleanName}_{suffix}.jpg",
                $"{dir}/{cleanName}_{suffix}.PNG",
                $"{dir}/{cleanName}_{suffix}.png",
            };
            foreach (var p in candidates)
            {
                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                if (t != null) return t;
            }
            return null;
        }

        static void EnsureTextureType(Texture2D tex, TextureImporterType type)
        {
            if (tex == null) return;
            string p = AssetDatabase.GetAssetPath(tex);
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp == null || imp.textureType == type) return;
            imp.textureType = type;
            imp.SaveAndReimport();
        }

        // Bind the FBX's auto-generated material slot to our hand-built .mat
        // via the importer's external-objects map. Same trick the player
        // characters use — keeps a single source of truth for the material.
        static void RemapFbxMaterial(string fbxPath, Material mat)
        {
            var imp = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (imp == null) return;

            // Find the embedded material's name so we know what key to remap.
            string embeddedName = null;
            foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
            {
                if (sub is Material m) { embeddedName = m.name; break; }
            }
            if (string.IsNullOrEmpty(embeddedName)) return;

            var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), embeddedName);
            imp.AddRemap(key, mat);
            imp.SaveAndReimport();
        }

        static WeaponItem CreateOrUpdateWeaponItem(WeaponSpec spec)
        {
            string itemPath = $"{AssetsItemsRoot}/{spec.rank}/Item_{spec.cleanName}.asset";
            WeaponItem item = AssetDatabase.LoadAssetAtPath<WeaponItem>(itemPath);
            bool isNew = item == null;
            if (isNew)
            {
                item = ScriptableObject.CreateInstance<WeaponItem>();
                AssetDatabase.CreateAsset(item, itemPath);
            }

            // Reflection-poke the private fields — keeps the public API clean
            // (no editor-only setters on WeaponItem) and survives someone
            // hand-editing the asset later.
            var so = new SerializedObject(item);
            SetIfPresent(so, "itemId",      spec.cleanName.ToLowerInvariant());
            SetIfPresent(so, "displayName", PrettifyName(spec.cleanName));
            SetIfPresent(so, "type",        (int)ItemType.Weapon);
            SetIfPresent(so, "equipSlot",   (int)EquipmentSlot.MainHand);
            SetIfPresent(so, "rank",        (int)spec.rank);
            SetIfPresent(so, "stackable",   false);
            SetIfPresent(so, "weaponKind",  (int)spec.kind);
            // Rank-driven economy: legendaries are worth a lot, commons are pocket change.
            int sell = spec.rank == ItemRank.Legendary ? 500 : spec.rank == ItemRank.Rare ? 120 : 25;
            SetIfPresent(so, "sellValue",   sell);
            SetIfPresent(so, "buyValue",    sell * 4);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        static void SetIfPresent(SerializedObject so, string name, object value)
        {
            var prop = so.FindProperty(name);
            if (prop == null) return;
            switch (value)
            {
                case int i:   prop.intValue       = i; break;
                case bool b:  prop.boolValue      = b; break;
                case string s: prop.stringValue   = s; break;
                case float f: prop.floatValue     = f; break;
                case UnityEngine.Object o: prop.objectReferenceValue = o; break;
            }
        }

        // "DualDemonSword" → "Dual Demon Sword" for the display label.
        static string PrettifyName(string camelCase)
        {
            var chars = new List<char>(camelCase.Length + 4);
            for (int i = 0; i < camelCase.Length; i++)
            {
                char c = camelCase[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(camelCase[i - 1])) chars.Add(' ');
                chars.Add(c);
            }
            return new string(chars.ToArray());
        }
    }
}
