using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeakClimber.Editor
{
    public static class PeakWorldBuilder
    {
        private const string SpriteDir = "Assets/Sprites/";

        [MenuItem("Tools/PEAK/Build Complete Game World", false, 1)]
        public static void BuildCompleteWorld()
        {
            EnsureSpriteDirectory();
            GenerateAllSpriteAssets();

            // Clear existing non-camera objects or old game objects in scene
            ClearOldSceneObjects();

            // Build Game Components
            BuildManagers();
            BuildLighting();
            Transform mountainRoot = BuildMountainEnvironment();
            BuildCollectibles(mountainRoot);
            BuildSummitHelicopter(mountainRoot);
            ScoutClimber scout = BuildScoutPlayer();
            BuildCamera(scout);
            BuildMobileUI(scout);

            // Mark scene dirty and save
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("<color=green><b>[PEAK 2D]</b> Complete mobile game world successfully built and saved!</color>");
        }

        private static void EnsureSpriteDirectory()
        {
            if (!Directory.Exists(SpriteDir))
            {
                Directory.CreateDirectory(SpriteDir);
            }
        }

        private static void GenerateAllSpriteAssets()
        {
            SaveSpritePng("Scout_Idle.png", ProceduralSpriteGenerator.CreateScoutSprite(false).texture);
            SaveSpritePng("Scout_Climb.png", ProceduralSpriteGenerator.CreateScoutSprite(true).texture);

            // Rocks
            SaveSpritePng("Rock_Grass.png", ProceduralSpriteGenerator.CreateRockSprite(64, 32, new Color(0.45f, 0.42f, 0.38f), new Color(0.35f, 0.32f, 0.28f), new Color(0.28f, 0.72f, 0.25f), true).texture);
            SaveSpritePng("Rock_Wall.png", ProceduralSpriteGenerator.CreateRockSprite(32, 64, new Color(0.48f, 0.45f, 0.4f), new Color(0.32f, 0.3f, 0.26f), Color.clear, false).texture);
            SaveSpritePng("Rock_Canyon.png", ProceduralSpriteGenerator.CreateRockSprite(64, 48, new Color(0.85f, 0.48f, 0.24f), new Color(0.65f, 0.32f, 0.15f), new Color(0.95f, 0.65f, 0.35f), false).texture);
            SaveSpritePng("Rock_Ice.png", ProceduralSpriteGenerator.CreateRockSprite(64, 48, new Color(0.55f, 0.85f, 0.95f), new Color(0.35f, 0.65f, 0.85f), new Color(0.92f, 0.98f, 1f), true).texture);
            SaveSpritePng("Rock_Crumble.png", ProceduralSpriteGenerator.CreateRockSprite(48, 24, new Color(0.58f, 0.52f, 0.45f), new Color(0.35f, 0.3f, 0.25f), Color.clear, false).texture);

            // Objects
            SaveSpritePng("Mushroom.png", ProceduralSpriteGenerator.CreateMushroomSprite().texture);
            SaveSpritePng("Campfire_Off.png", ProceduralSpriteGenerator.CreateCampfireSprite(false).texture);
            SaveSpritePng("Campfire_Lit.png", ProceduralSpriteGenerator.CreateCampfireSprite(true).texture);
            SaveSpritePng("Piton.png", ProceduralSpriteGenerator.CreatePitonSprite().texture);
            SaveSpritePng("Badge.png", ProceduralSpriteGenerator.CreateBadgeSprite().texture);
            SaveSpritePng("Berry.png", ProceduralSpriteGenerator.CreateEnergyBerrySprite().texture);
            SaveSpritePng("Spikes.png", ProceduralSpriteGenerator.CreateHazardSpikesSprite(4).texture);

            // Helicopter
            SaveSpritePng("Helicopter.png", ProceduralSpriteGenerator.CreateHelicopterSprite().texture);
            SaveSpritePng("Rotor.png", ProceduralSpriteGenerator.CreateRotorBladeSprite().texture);

            // UI
            SaveSpritePng("UI_Circle.png", ProceduralSpriteGenerator.CreateCircleSprite(64, Color.white).texture);
            SaveSpritePng("UI_Ring.png", ProceduralSpriteGenerator.CreateRingSprite(64, 8, Color.white).texture);

            AssetDatabase.Refresh();
        }

        private static void SaveSpritePng(string fileName, Texture2D tex)
        {
            string path = Path.Combine(SpriteDir, fileName);
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 50f;
                importer.filterMode = FilterMode.Point;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                if (fileName.Contains("Rock") || fileName.Contains("Campfire") || fileName.Contains("UI"))
                {
                    importer.spriteBorder = new Vector4(6, 6, 6, 6);
                }
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static Sprite LoadSprite(string fileName)
        {
            string path = SpriteDir + fileName;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void ClearOldSceneObjects()
        {
            string[] toRemove = { "--- MANAGERS ---", "--- ENVIRONMENT ---", "--- PLAYER ---", "--- UI_MOBILE ---", "GameObject", "RescueFlare" };
            foreach (var name in toRemove)
            {
                var go = GameObject.Find(name);
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static void BuildManagers()
        {
            GameObject mgrRoot = new GameObject("--- MANAGERS ---");

            GameObject gm = new GameObject("GameManager");
            gm.transform.SetParent(mgrRoot.transform);
            gm.AddComponent<GameManager>();

            GameObject sm = new GameObject("SoundManager");
            sm.transform.SetParent(mgrRoot.transform);
            sm.AddComponent<SoundManager>();
        }

        private static void BuildLighting()
        {
            var light = GameObject.Find("Global Light 2D");
            if (light == null)
            {
                light = new GameObject("Global Light 2D");
            }
            light.transform.position = new Vector3(0, 0, 0);
        }

        private static Transform BuildMountainEnvironment()
        {
            GameObject envRoot = new GameObject("--- ENVIRONMENT ---");

            // Background Parallax
            BuildBackgroundVista(envRoot.transform);

            // Mountain Zones Root
            GameObject mountainRoot = new GameObject("Mountain");
            mountainRoot.transform.SetParent(envRoot.transform);

            // Zone 1: Shore & The Roots (Y: -2 to 85)
            BuildZone1_Roots(mountainRoot.transform);

            // Zone 2: Canyon & Mesa Crags (Y: 85 to 190)
            BuildZone2_Canyon(mountainRoot.transform);

            // Zone 3: Alpine Frost & Frozen Spires (Y: 190 to 290)
            BuildZone3_Alpine(mountainRoot.transform);

            // Zone 4: The Summit (Y: 290 to 320)
            BuildZone4_Summit(mountainRoot.transform);

            return envRoot.transform;
        }

        private static void BuildBackgroundVista(Transform parent)
        {
            GameObject bgRoot = new GameObject("ParallaxBackground");
            bgRoot.transform.SetParent(parent);
            var parallax = bgRoot.AddComponent<ParallaxBackground>();
            parallax.parallaxFactorX = 0.1f;
            parallax.parallaxFactorY = 0.35f;

            // Sky Backdrop Gradient Panels
            Color skyBottom = new Color(0.45f, 0.72f, 0.95f);
            Color skyMid = new Color(0.25f, 0.45f, 0.8f);
            Color skyTop = new Color(0.08f, 0.15f, 0.4f);

            CreateBgPanel(bgRoot.transform, "Sky_Bottom", new Vector3(0, 50, 20), new Vector2(120, 120), skyBottom);
            CreateBgPanel(bgRoot.transform, "Sky_Mid", new Vector3(0, 160, 20), new Vector2(120, 120), skyMid);
            CreateBgPanel(bgRoot.transform, "Sky_Top", new Vector3(0, 270, 20), new Vector2(120, 120), skyTop);

            // Distant Mountains Silhouettes
            Color distantPeakColor = new Color(0.2f, 0.32f, 0.55f, 0.65f);
            for (int i = 0; i < 12; i++)
            {
                float yPos = i * 28f + 10f;
                float xOffset = (i % 2 == 0 ? -12f : 12f) + Mathf.Sin(i) * 6f;
                CreateDistantMountainPeak(bgRoot.transform, $"DistantPeak_{i}", new Vector3(xOffset, yPos, 15), distantPeakColor);
            }
        }

        private static void CreateBgPanel(Transform parent, string name, Vector3 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteGenerator.CreateBoxSprite(32, 32, color, color);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.sortingOrder = -20;
        }

        private static void CreateDistantMountainPeak(Transform parent, string name, Vector3 pos, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteGenerator.CreateRockSprite(48, 64, color, color * 0.8f, Color.clear);
            sr.sortingOrder = -15;
            go.transform.localScale = new Vector3(1.2f, 1.4f, 1f);
        }

        private static void BuildZone1_Roots(Transform parent)
        {
            GameObject z1 = new GameObject("Zone1_Roots");
            z1.transform.SetParent(parent);

            Sprite rockGrass = LoadSprite("Rock_Grass.png");
            Sprite rockWall = LoadSprite("Rock_Wall.png");
            Sprite rockCrumble = LoadSprite("Rock_Crumble.png");
            Sprite mushroomSpr = LoadSprite("Mushroom.png");

            // Base ground
            CreatePlatform(z1.transform, "BaseCampGround", new Vector3(0, -1, 0), new Vector2(36, 3), rockGrass, SurfaceType.NormalRock);

            // Base Campfire
            CreateCampfire(z1.transform, "BaseCamp_Campfire", new Vector3(1, 0.6f, 0), 0, "Base Camp");

            // Introductory Cliffs
            CreateRockWall(z1.transform, "Cliff_Left_1", new Vector3(-12, 18, 0), new Vector2(4, 34), rockWall, SurfaceType.NormalRock);
            CreateRockWall(z1.transform, "Cliff_Right_1", new Vector3(12, 22, 0), new Vector2(4, 40), rockWall, SurfaceType.NormalRock);

            // Stepped Practice Ledges
            CreatePlatform(z1.transform, "Ledge_Practice_1", new Vector3(-4, 5, 0), new Vector2(6, 2), rockGrass, SurfaceType.NormalRock);
            CreatePlatform(z1.transform, "Ledge_Practice_2", new Vector3(4, 11, 0), new Vector2(6, 2), rockGrass, SurfaceType.NormalRock);

            // Bouncy Mushroom
            CreateMushroom(z1.transform, "BounceMushroom_1", new Vector3(-3, 6.2f, 0), mushroomSpr, 16f);

            // Mid Ledges & Pillars
            CreatePlatform(z1.transform, "Pillar_Center_1", new Vector3(-2, 22, 0), new Vector2(4, 8), rockGrass, SurfaceType.NormalRock);
            CreatePlatform(z1.transform, "Ledge_Mid_1", new Vector3(5, 30, 0), new Vector2(5, 2), rockGrass, SurfaceType.NormalRock);

            // Crumbling Fragile Holds Section
            CreatePlatform(z1.transform, "Crumble_Hold_1", new Vector3(-6, 38, 0), new Vector2(3, 1.5f), rockCrumble, SurfaceType.Crumbling);
            CreatePlatform(z1.transform, "Crumble_Hold_2", new Vector3(-1, 44, 0), new Vector2(3, 1.5f), rockCrumble, SurfaceType.Crumbling);
            CreatePlatform(z1.transform, "Crumble_Hold_3", new Vector3(4, 50, 0), new Vector2(3, 1.5f), rockCrumble, SurfaceType.Crumbling);

            // Rest Shelf
            CreatePlatform(z1.transform, "RestShelf_1", new Vector3(0, 58, 0), new Vector2(8, 2.5f), rockGrass, SurfaceType.NormalRock);

            // Upper Approach to Mesa
            CreateRockWall(z1.transform, "Cliff_Approach_Left", new Vector3(-10, 72, 0), new Vector2(4, 26), rockWall, SurfaceType.NormalRock);
            CreateRockWall(z1.transform, "Cliff_Approach_Right", new Vector3(10, 75, 0), new Vector2(4, 28), rockWall, SurfaceType.NormalRock);
            CreatePlatform(z1.transform, "Step_Ledge_A", new Vector3(-4, 68, 0), new Vector2(4, 2), rockGrass, SurfaceType.NormalRock);
            CreatePlatform(z1.transform, "Step_Ledge_B", new Vector3(4, 76, 0), new Vector2(4, 2), rockGrass, SurfaceType.NormalRock);
        }

        private static void BuildZone2_Canyon(Transform parent)
        {
            GameObject z2 = new GameObject("Zone2_Canyon");
            z2.transform.SetParent(parent);

            Sprite rockCanyon = LoadSprite("Rock_Canyon.png");
            Sprite rockCrumble = LoadSprite("Rock_Crumble.png");
            Sprite spikesSpr = LoadSprite("Spikes.png");

            // Mesa Camp Plateau
            CreatePlatform(z2.transform, "MesaPlateau", new Vector3(-2, 85, 0), new Vector2(16, 3), rockCanyon, SurfaceType.NormalRock);
            CreateCampfire(z2.transform, "Mesa_Campfire", new Vector3(-2, 86.8f, 0), 1, "Mesa Canyon Camp");

            // Hazard Pit below plateau
            CreateHazardSpikes(z2.transform, "CanyonSpikes_1", new Vector3(9, 84, 0), spikesSpr, 4);

            // Canyon Chimney (Two vertical walls close together for wall-jumping & stamina traversal)
            CreateRockWall(z2.transform, "Chimney_Wall_Left", new Vector3(-6, 115, 0), new Vector2(3, 50), rockCanyon, SurfaceType.NormalRock);
            CreateRockWall(z2.transform, "Chimney_Wall_Right", new Vector3(0, 115, 0), new Vector2(3, 50), rockCanyon, SurfaceType.NormalRock);

            // Crumbling stepping stones inside chimney
            CreatePlatform(z2.transform, "Chimney_Crumble_1", new Vector3(-3, 102, 0), new Vector2(2.5f, 1.2f), rockCrumble, SurfaceType.Crumbling);
            CreatePlatform(z2.transform, "Chimney_Crumble_2", new Vector3(-3, 120, 0), new Vector2(2.5f, 1.2f), rockCrumble, SurfaceType.Crumbling);
            CreatePlatform(z2.transform, "Chimney_Crumble_3", new Vector3(-3, 136, 0), new Vector2(2.5f, 1.2f), rockCrumble, SurfaceType.Crumbling);

            // Canyon Exit Shelf
            CreatePlatform(z2.transform, "Canyon_Exit_Ledge", new Vector3(-3, 145, 0), new Vector2(7, 2.5f), rockCanyon, SurfaceType.NormalRock);

            // Extreme Mesa Overhang (Vertical face with gaps requiring Pitons)
            CreateRockWall(z2.transform, "Overhang_Wall", new Vector3(7, 168, 0), new Vector2(4, 40), rockCanyon, SurfaceType.NormalRock);
            CreatePlatform(z2.transform, "Overhang_Hold_1", new Vector3(4, 154, 0), new Vector2(3, 1.5f), rockCanyon, SurfaceType.NormalRock);
            CreatePlatform(z2.transform, "Overhang_Hold_2", new Vector3(4, 166, 0), new Vector2(3, 1.5f), rockCrumble, SurfaceType.Crumbling);
            CreatePlatform(z2.transform, "Overhang_Hold_3", new Vector3(4, 178, 0), new Vector2(3, 1.5f), rockCanyon, SurfaceType.NormalRock);

            // Approach to Alpine Ridge
            CreatePlatform(z2.transform, "Alpine_Approach_Ledge", new Vector3(0, 186, 0), new Vector2(8, 2.5f), rockCanyon, SurfaceType.NormalRock);
        }

        private static void BuildZone3_Alpine(Transform parent)
        {
            GameObject z3 = new GameObject("Zone3_Alpine");
            z3.transform.SetParent(parent);

            Sprite rockIce = LoadSprite("Rock_Ice.png");
            Sprite rockCrumble = LoadSprite("Rock_Crumble.png");
            Sprite mushroomSpr = LoadSprite("Mushroom.png");

            // Alpine Camp Plateau
            CreatePlatform(z3.transform, "AlpinePlateau", new Vector3(0, 195, 0), new Vector2(16, 3), rockIce, SurfaceType.NormalRock);
            CreateCampfire(z3.transform, "Alpine_Campfire", new Vector3(0, 196.8f, 0), 2, "Alpine Snow Camp");

            // Frozen High Altitude Spires (Icy surfaces!)
            CreateRockWall(z3.transform, "IceSpire_Left", new Vector3(-8, 230, 0), new Vector2(3.5f, 60), rockIce, SurfaceType.Icy);
            CreateRockWall(z3.transform, "IceSpire_Center", new Vector3(0, 245, 0), new Vector2(3.5f, 65), rockIce, SurfaceType.Icy);
            CreateRockWall(z3.transform, "IceSpire_Right", new Vector3(8, 235, 0), new Vector2(3.5f, 60), rockIce, SurfaceType.Icy);

            // Bouncy Snow Mushroom to skip lower ice section
            CreateMushroom(z3.transform, "Alpine_Mushroom", new Vector3(-4, 196.5f, 0), mushroomSpr, 18f);

            // Icy Crumbling Bridging Holds
            CreatePlatform(z3.transform, "Ice_Bridge_1", new Vector3(-4, 215, 0), new Vector2(2.5f, 1.2f), rockCrumble, SurfaceType.Crumbling);
            CreatePlatform(z3.transform, "Ice_Bridge_2", new Vector3(4, 225, 0), new Vector2(2.5f, 1.2f), rockCrumble, SurfaceType.Crumbling);
            CreatePlatform(z3.transform, "Ice_Bridge_3", new Vector3(-4, 250, 0), new Vector2(2.5f, 1.2f), rockIce, SurfaceType.Icy);
            CreatePlatform(z3.transform, "Ice_Bridge_4", new Vector3(4, 265, 0), new Vector2(2.5f, 1.2f), rockIce, SurfaceType.Icy);

            // Final Pre-Summit Rest Ledge
            CreatePlatform(z3.transform, "PreSummit_Ledge", new Vector3(0, 280, 0), new Vector2(6, 2.5f), rockIce, SurfaceType.NormalRock);
        }

        private static void BuildZone4_Summit(Transform parent)
        {
            GameObject z4 = new GameObject("Zone4_Summit");
            z4.transform.SetParent(parent);

            Sprite rockIce = LoadSprite("Rock_Ice.png");

            // The Peak Summit Plateau
            CreatePlatform(z4.transform, "SummitPeakPlateau", new Vector3(0, 295, 0), new Vector2(24, 3.5f), rockIce, SurfaceType.NormalRock);

            // Summit Flag
            GameObject flag = new GameObject("SummitFlag");
            flag.transform.SetParent(z4.transform);
            flag.transform.position = new Vector3(-5, 297.2f, 0);
            var sr = flag.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteGenerator.CreateBoxSprite(6, 40, new Color(0.9f, 0.2f, 0.2f), Color.black, 1);
            sr.sortingOrder = 5;
        }

        private static void BuildCollectibles(Transform parent)
        {
            GameObject colRoot = new GameObject("Collectibles");
            colRoot.transform.SetParent(parent);

            // 5 Scout Badges placed across the mountain
            CreateBadge(colRoot.transform, "Badge1_ShoreScout", new Vector3(6, 12.5f, 0), 1, "Shore Scout");
            CreateBadge(colRoot.transform, "Badge2_RootsClimber", new Vector3(-7, 40f, 0), 2, "Roots Master");
            CreateBadge(colRoot.transform, "Badge3_CanyonDaredevil", new Vector3(-3, 125f, 0), 3, "Chimney Daredevil");
            CreateBadge(colRoot.transform, "Badge4_IceWalker", new Vector3(-8, 255f, 0), 4, "Ice Walker");
            CreateBadge(colRoot.transform, "Badge5_PeakConqueror", new Vector3(6, 297f, 0), 5, "Summit Conqueror");

            // Energy Berries along tricky routes
            CreateBerry(colRoot.transform, "Berry_1", new Vector3(-4, 23.5f, 0));
            CreateBerry(colRoot.transform, "Berry_2", new Vector3(5, 31.5f, 0));
            CreateBerry(colRoot.transform, "Berry_3", new Vector3(0, 59.5f, 0));
            CreateBerry(colRoot.transform, "Berry_4", new Vector3(4, 155.5f, 0));
            CreateBerry(colRoot.transform, "Berry_5", new Vector3(4, 179.5f, 0));
            CreateBerry(colRoot.transform, "Berry_6", new Vector3(0, 281.5f, 0));
        }

        private static void BuildSummitHelicopter(Transform parent)
        {
            GameObject heliRoot = new GameObject("SummitRescueHelicopter");
            heliRoot.transform.SetParent(parent);
            heliRoot.transform.position = new Vector3(0, 301, 0);

            // Helicopter Body
            GameObject bodyGo = new GameObject("HeliBody");
            bodyGo.transform.SetParent(heliRoot.transform);
            bodyGo.transform.localPosition = Vector3.zero;
            var bodySr = bodyGo.AddComponent<SpriteRenderer>();
            bodySr.sprite = LoadSprite("Helicopter.png");
            bodySr.sortingOrder = 12;

            // Rotor
            GameObject rotorGo = new GameObject("Rotor");
            rotorGo.transform.SetParent(bodyGo.transform);
            rotorGo.transform.localPosition = new Vector3(-0.1f, 1.1f, 0);
            var rotorSr = rotorGo.AddComponent<SpriteRenderer>();
            rotorSr.sprite = LoadSprite("Rotor.png");
            rotorSr.sortingOrder = 13;

            // Rescue Script
            var summit = heliRoot.AddComponent<SummitRescue>();
            summit.helicopterBody = bodyGo.transform;
            summit.rotorTransform = rotorGo.transform;

            // Trigger Zone on the summit platform
            var col = heliRoot.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(10f, 6f);
            col.offset = new Vector2(0f, -4f);
        }

        private static ScoutClimber BuildScoutPlayer()
        {
            GameObject scoutGo = new GameObject("ScoutPlayer");
            scoutGo.transform.position = new Vector3(-5f, 1.2f, 0f);

            var rb = scoutGo.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 2.5f;
            rb.freezeRotation = true;

            var col = scoutGo.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.7f, 1.2f);
            col.offset = new Vector2(0f, 0.6f);

            // Visual child
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(scoutGo.transform);
            visual.transform.localPosition = Vector3.zero;
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Scout_Idle.png");
            sr.sortingOrder = 8;

            // Scout Climber script
            var scout = scoutGo.AddComponent<ScoutClimber>();
            scout.visualRoot = visual.transform;
            scout.bodyRenderer = sr;
            scout.idleSprite = LoadSprite("Scout_Idle.png");
            scout.climbSprite = LoadSprite("Scout_Climb.png");

            // Setup Ground Check
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(scoutGo.transform);
            gc.transform.localPosition = new Vector3(0, 0, 0);
            scout.groundCheck = gc.transform;

            // Piton Prefab
            GameObject pitonPf = new GameObject("PitonPrefab");
            pitonPf.AddComponent<Piton>();
            pitonPf.SetActive(false);
            pitonPf.transform.SetParent(scoutGo.transform);
            scout.pitonPrefab = pitonPf;

            // Set layers
            scout.groundLayer = LayerMask.GetMask("Default");
            scout.climbableLayer = LayerMask.GetMask("Default");

            return scout;
        }

        private static void BuildCamera(ScoutClimber scout)
        {
            var camGo = GameObject.FindWithTag("MainCamera");
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.AddComponent<Camera>();
            }

            camGo.transform.position = new Vector3(scout.transform.position.x, scout.transform.position.y + 2f, -10f);
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.backgroundColor = new Color(0.4f, 0.68f, 0.92f);
            cam.clearFlags = CameraClearFlags.SolidColor;

            var follow = camGo.GetComponent<CameraFollow2D>();
            if (follow == null) follow = camGo.AddComponent<CameraFollow2D>();
            follow.target = scout.transform;
        }

        private static void BuildMobileUI(ScoutClimber scout)
        {
            GameObject canvasGo = new GameObject("Canvas_MobileUI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Event System
            EnsureEventSystem();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var mobileController = canvasGo.AddComponent<MobileTouchController>();
            mobileController.scout = scout;

            Sprite circleSpr = LoadSprite("UI_Circle.png");
            Sprite ringSpr = LoadSprite("UI_Ring.png");

            // --- 1. HUD Panel (Top) ---
            GameObject hudPanel = CreateUIPanel(canvasGo.transform, "HUD_Top", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(1800, 80), new Color(0.1f, 0.15f, 0.22f, 0.75f));

            // Altitude Text
            GameObject altTextGo = CreateUIText(hudPanel.transform, "AltitudeText", "ALT: 0m / 350m", font, 32, TextAnchor.MiddleLeft, Color.white);
            RectTransform altRt = altTextGo.GetComponent<RectTransform>();
            altRt.anchorMin = new Vector2(0, 0.5f);
            altRt.anchorMax = new Vector2(0, 0.5f);
            altRt.anchoredPosition = new Vector2(180, 0);
            altRt.sizeDelta = new Vector2(320, 60);
            mobileController.altitudeText = altTextGo.GetComponent<Text>();

            // Mountain Progress Slider
            GameObject sliderGo = new GameObject("MountainSlider");
            sliderGo.transform.SetParent(hudPanel.transform);
            RectTransform sRt = sliderGo.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.5f);
            sRt.anchorMax = new Vector2(0.5f, 0.5f);
            sRt.anchoredPosition = new Vector2(0, 0);
            sRt.sizeDelta = new Vector2(400, 24);

            Slider slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            // Slider Background
            GameObject sBg = CreateUIPanel(sliderGo.transform, "BG", new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, new Color(0.1f, 0.15f, 0.2f, 0.8f));
            // Fill Area & Fill
            GameObject sFillArea = new GameObject("FillArea");
            sFillArea.transform.SetParent(sliderGo.transform);
            RectTransform faRt = sFillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            GameObject sFill = CreateUIPanel(sFillArea.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.2f, 0.85f, 0.45f, 0.95f));
            slider.fillRect = sFill.GetComponent<RectTransform>();
            slider.targetGraphic = sFill.GetComponent<Image>();
            mobileController.mountainProgressSlider = slider;

            // Badges Text
            GameObject badgeTextGo = CreateUIText(hudPanel.transform, "BadgeText", "Badges: 0 / 5", font, 30, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.2f));
            RectTransform bRt = badgeTextGo.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1, 0.5f);
            bRt.anchorMax = new Vector2(1, 0.5f);
            bRt.anchoredPosition = new Vector2(-280, 0);
            bRt.sizeDelta = new Vector2(240, 60);
            mobileController.badgeCounterText = badgeTextGo.GetComponent<Text>();

            // Pitons Count Text
            GameObject pitonTextGo = CreateUIText(hudPanel.transform, "PitonText", "x3", font, 30, TextAnchor.MiddleRight, new Color(0.3f, 0.8f, 1f));
            RectTransform pRt = pitonTextGo.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(1, 0.5f);
            pRt.anchorMax = new Vector2(1, 0.5f);
            pRt.anchoredPosition = new Vector2(-120, 0);
            pRt.sizeDelta = new Vector2(120, 60);
            mobileController.pitonCountText = pitonTextGo.GetComponent<Text>();

            // Mute Button
            GameObject muteBtnGo = CreateUIButton(hudPanel.transform, "MuteBtn", "🔊", font, 28, new Vector2(1, 0.5f), new Vector2(-40, 0), new Vector2(60, 60), circleSpr);
            mobileController.soundButton = muteBtnGo.GetComponent<Button>();
            mobileController.soundButtonText = muteBtnGo.GetComponentInChildren<Text>();

            // --- 2. Mobile Touch Controls (Bottom Left Joystick) ---
            GameObject joyBg = CreateUIPanel(canvasGo.transform, "JoystickBG", new Vector2(0, 0), new Vector2(0, 0), new Vector2(220, 220), new Vector2(220, 220), new Color(0.15f, 0.2f, 0.25f, 0.65f), ringSpr);
            GameObject joyHandle = CreateUIPanel(joyBg.transform, "JoystickHandle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90, 90), new Color(1f, 1f, 1f, 0.75f), circleSpr);
            mobileController.joystickBackground = joyBg.GetComponent<RectTransform>();
            mobileController.joystickHandle = joyHandle.GetComponent<RectTransform>();

            // --- 3. Mobile Action Buttons (Bottom Right) ---
            // JUMP Button (A)
            GameObject jumpBtnGo = CreateUIButton(canvasGo.transform, "Btn_Jump", "JUMP", font, 28, new Vector2(1, 0), new Vector2(-160, 180), new Vector2(140, 140), circleSpr, new Color(0.18f, 0.75f, 0.35f, 0.85f));
            mobileController.jumpButton = jumpBtnGo.GetComponent<Button>();

            // CLIMB Button (Hold / Toggle)
            GameObject climbBtnGo = CreateUIButton(canvasGo.transform, "Btn_Climb", "CLIMB", font, 24, new Vector2(1, 0), new Vector2(-330, 140), new Vector2(120, 120), circleSpr, new Color(0.95f, 0.55f, 0.15f, 0.85f));
            mobileController.climbButton = climbBtnGo.GetComponent<Button>();
            mobileController.climbButtonBg = climbBtnGo.GetComponent<Image>();

            // PITON Button
            GameObject pitonBtnGo = CreateUIButton(canvasGo.transform, "Btn_Piton", "PITON", font, 22, new Vector2(1, 0), new Vector2(-230, 330), new Vector2(110, 110), circleSpr, new Color(0.2f, 0.65f, 0.95f, 0.85f));
            mobileController.pitonButton = pitonBtnGo.GetComponent<Button>();

            // --- 4. Victory Panel (Hidden by default) ---
            GameObject vicPanel = CreateUIPanel(canvasGo.transform, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(650, 480), new Color(0.08f, 0.12f, 0.18f, 0.95f));
            vicPanel.SetActive(false);
            mobileController.victoryPanel = vicPanel;

            GameObject vicTitle = CreateUIText(vicPanel.transform, "VicTitle", "🏆 PEAK CONQUERED! 🚁", font, 40, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f));
            RectTransform vtRt = vicTitle.GetComponent<RectTransform>();
            vtRt.anchorMin = new Vector2(0.5f, 1f);
            vtRt.anchorMax = new Vector2(0.5f, 1f);
            vtRt.anchoredPosition = new Vector2(0, -60);
            vtRt.sizeDelta = new Vector2(600, 60);

            GameObject vicStats = CreateUIText(vicPanel.transform, "VicStats", "Loading stats...", font, 26, TextAnchor.MiddleCenter, Color.white);
            RectTransform vsRt = vicStats.GetComponent<RectTransform>();
            vsRt.anchorMin = new Vector2(0.5f, 0.5f);
            vsRt.anchorMax = new Vector2(0.5f, 0.5f);
            vsRt.anchoredPosition = new Vector2(0, 10);
            vsRt.sizeDelta = new Vector2(580, 200);
            mobileController.victoryStatsText = vicStats.GetComponent<Text>();

            GameObject playAgainBtn = CreateUIButton(vicPanel.transform, "PlayAgainBtn", "CLIMB AGAIN", font, 28, new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(260, 70), null, new Color(0.2f, 0.8f, 0.35f, 1f));
            mobileController.playAgainButton = playAgainBtn.GetComponent<Button>();
        }

        private static void EnsureEventSystem()
        {
            var es = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
                var inputType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputType != null)
                {
                    esGo.AddComponent(inputType);
                }
                else
                {
                    esGo.AddComponent<StandaloneInputModule>();
                }
            }
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color, Sprite spr = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = color;
            if (spr != null) img.sprite = spr;
            return go;
        }

        private static GameObject CreateUIText(Transform parent, string name, string text, Font font, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.AddComponent<RectTransform>();
            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.color = color;
            return go;
        }

        private static GameObject CreateUIButton(Transform parent, string name, string label, Font font, int fontSize, Vector2 anchor, Vector2 pos, Vector2 size, Sprite spr, Color? bgColor = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = bgColor ?? Color.white;
            if (spr != null) img.sprite = spr;

            go.AddComponent<Button>();

            GameObject textGo = CreateUIText(go.transform, "Label", label, font, fontSize, TextAnchor.MiddleCenter, Color.white);
            RectTransform tRt = textGo.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.sizeDelta = Vector2.zero;

            return go;
        }

        // Helper Object Builders
        private static void CreatePlatform(Transform parent, string name, Vector3 pos, Vector2 size, Sprite spr, SurfaceType type)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.sortingOrder = 2;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            var surf = go.AddComponent<ClimbableSurface>();
            surf.surfaceType = type;
        }

        private static void CreateRockWall(Transform parent, string name, Vector3 pos, Vector2 size, Sprite spr, SurfaceType type)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            sr.sortingOrder = 1;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            var surf = go.AddComponent<ClimbableSurface>();
            surf.surfaceType = type;
        }

        private static void CreateMushroom(Transform parent, string name, Vector3 pos, Sprite spr, float bounceForce)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.8f, 1.2f);
            col.offset = new Vector2(0f, 0.6f);

            var surf = go.AddComponent<ClimbableSurface>();
            surf.surfaceType = SurfaceType.Bouncy;
            surf.bounceForce = bounceForce;
        }

        private static void CreateCampfire(Transform parent, string name, Vector3 pos, int index, string campName)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(index == 0 ? "Campfire_Lit.png" : "Campfire_Off.png");
            sr.sortingOrder = 4;

            var cp = go.AddComponent<CampfireCheckpoint>();
            cp.isLit = (index == 0);
            cp.checkpointIndex = index;
            cp.campName = campName;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.2f;
        }

        private static void CreateHazardSpikes(Transform parent, string name, Vector3 pos, Sprite spr, int count)
        {
            GameObject go = new GameObject(name);
            EnsureTag("Hazard");
            try { go.tag = "Hazard"; } catch {}
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(count * 1f, 0.6f);
            col.offset = new Vector2(0f, 0.3f);
        }

        private static void CreateBadge(Transform parent, string name, Vector3 pos, int id, string badgeName)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Badge.png");
            sr.sortingOrder = 6;

            var b = go.AddComponent<ScoutBadge>();
            b.badgeId = id;
            b.badgeName = badgeName;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }

        private static void CreateBerry(Transform parent, string name, Vector3 pos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Berry.png");
            sr.sortingOrder = 6;

            go.AddComponent<EnergyBerry>();

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
        }

        private static void EnsureTag(string tag)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;
            SerializedObject tagManager = new SerializedObject(assets[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            if (tagsProp == null) return;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
            }
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
    }
}
