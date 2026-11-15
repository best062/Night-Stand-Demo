using System.Collections.Generic;
using System.IO;
using NightStand.Core;
using NightStand.Data;
using NightStand.Dialogue;
using NightStand.Equipment;
using NightStand.Items;
using NightStand.Player;
using NightStand.UI;
using NightStand.Vehicle;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NightStand.Editor
{
    /// <summary>
    /// ตัวสร้างฉากทดสอบ Greybox (Unity 6 URP) สำหรับ Night Stand
    /// เมนู: NightStand / Generate Greybox Test Scene
    /// </summary>
    public static class GreyboxSceneGenerator
    {
        private const string PrefabsFolder = "Assets/_Project/Prefabs";
        private const string MaterialsFolder = "Assets/_Project/Materials/Greybox";
        private const string ScriptableObjectsFolder = "Assets/_Project/ScriptableObjects/Greybox";
        private const string ScenesFolder = "Assets/_Project/Scenes";

        private static Material matAsphalt;
        private static Material matGround;
        private static Material matRoadLine;
        private static Material matBoothWall;
        private static Material matGlass;
        private static Material matDesk;
        private static Material matMetal;
        private static Material matGreen;
        private static Material matRed;
        private static Material matWhite;
        private static Material matBlack;
        private static Material matNPC;
        private static Material matWaypoint;
        private static Material matPaper;

        [MenuItem("NightStand/Generate Greybox Test Scene")]
        public static void GenerateGreyboxTestScene()
        {
            EnsureFolders();
            CreateGreyboxMaterials();

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureNightLighting();

            // 1. Environment & Road
            Transform envRoot = new GameObject("--- ENVIRONMENT ---").transform;
            CreateEnvironment(envRoot, out Transform spawnPoint, out Transform gateStopPoint, out Transform exitPoint, out Transform returnPoint);

            // 2. Data Assets (ScriptableObjects)
            CreateSampleDataAssets(
                out DocumentDataSO residentListDoc,
                out QuestDataSO mainQuest,
                out QuestDataSO sideQuest,
                out List<VehicleDataSO> sampleVehicles,
                out ItemDataSO flashlightItemData,
                out ItemDataSO keyItemData,
                out ItemDataSO eliminatorItemData
            );

            // 3. Greybox Vehicle Prefab & Desk Item Prefabs
            GameObject vehiclePrefab = CreateGreyboxVehiclePrefab();
            GameObject docPrefab = CreateDocumentPrefab("GreyboxDocument", residentListDoc, false);
            GameObject passPrefab = CreateDocumentPrefab("GreyboxVehiclePass", null, true);
            GameObject eliminatorPickupPrefab = CreateEliminatorPickupPrefab();

            // 4. Managers
            Transform managersRoot = new GameObject("--- MANAGERS ---").transform;
            CreateManagers(
                managersRoot,
                vehiclePrefab,
                spawnPoint,
                gateStopPoint,
                sampleVehicles,
                out GameManager gameManager,
                out TimeWaveManager timeWaveManager,
                out VehicleSpawner vehicleSpawner,
                out AIPartnerManager aiPartnerManager,
                out JumpscareManager jumpscareManager,
                out EndingManager endingManager
            );

            // 5. Security Booth, Desk, Locker, Gate & Buttons
            Transform boothRoot = new GameObject("--- SECURITY BOOTH & GATE ---").transform;
            CreateSecurityBoothAndGate(
                boothRoot,
                vehicleSpawner,
                exitPoint,
                returnPoint,
                residentListDoc,
                docPrefab,
                passPrefab,
                eliminatorPickupPrefab,
                sideQuest,
                out DeskItemSpawner deskSpawner
            );

            // 6. Player & Right-Hand Equipment
            CreatePlayer(
                flashlightItemData,
                keyItemData,
                eliminatorItemData,
                out FirstPersonController playerController,
                out EquipmentManager equipmentManager
            );

            // 7. Wire Remaining Manager References
            WireManagerReferences(
                timeWaveManager,
                aiPartnerManager,
                deskSpawner,
                equipmentManager,
                mainQuest,
                sideQuest
            );

            // 8. UI Canvas (HUD, DocumentViewer, DialogueUI, ScreenFader, EndingUI)
            CreateUICanvas(
                playerController,
                equipmentManager,
                timeWaveManager,
                aiPartnerManager,
                gameManager,
                endingManager,
                jumpscareManager
            );

            string scenePath = $"{ScenesFolder}/GreyboxTestScene.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GreyboxSceneGenerator] Generated Greybox Test Scene at {scenePath}. Press Play to test!");
        }

        private static void EnsureFolders()
        {
            string[] folders = { PrefabsFolder, MaterialsFolder, ScriptableObjectsFolder, ScenesFolder };
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string[] parts = folder.Split('/');
                    string current = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string next = $"{current}/{parts[i]}";
                        if (!AssetDatabase.IsValidFolder(next))
                        {
                            AssetDatabase.CreateFolder(current, parts[i]);
                        }
                        current = next;
                    }
                }
            }
        }

        private static void CreateGreyboxMaterials()
        {
            matAsphalt = GetOrCreateLitMaterial("M_Asphalt", new Color(0.14f, 0.14f, 0.16f));
            matGround = GetOrCreateLitMaterial("M_NightGround", new Color(0.06f, 0.08f, 0.06f));
            matRoadLine = GetOrCreateLitMaterial("M_RoadLine", new Color(0.85f, 0.75f, 0.25f));
            matBoothWall = GetOrCreateLitMaterial("M_BoothWall", new Color(0.32f, 0.34f, 0.38f));
            matGlass = GetOrCreateTransparentMaterial("M_URPGlass", new Color(0.55f, 0.78f, 0.9f, 0.35f));
            matDesk = GetOrCreateLitMaterial("M_DeskWood", new Color(0.35f, 0.22f, 0.12f));
            matMetal = GetOrCreateLitMaterial("M_DarkMetal", new Color(0.22f, 0.24f, 0.27f));
            matGreen = GetOrCreateLitMaterial("M_ButtonGreen", new Color(0.15f, 0.8f, 0.25f));
            matRed = GetOrCreateLitMaterial("M_ButtonRed", new Color(0.85f, 0.15f, 0.15f));
            matWhite = GetOrCreateLitMaterial("M_White", new Color(0.92f, 0.92f, 0.92f));
            matBlack = GetOrCreateLitMaterial("M_Black", new Color(0.06f, 0.06f, 0.07f));
            matNPC = GetOrCreateLitMaterial("M_NPC", new Color(0.75f, 0.65f, 0.55f));
            matWaypoint = GetOrCreateLitMaterial("M_WaypointMarker", new Color(1f, 0.85f, 0.1f));
            matPaper = GetOrCreateLitMaterial("M_Paper", new Color(0.94f, 0.91f, 0.82f));
        }

        private static Material GetOrCreateLitMaterial(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                return mat;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material GetOrCreateTransparentMaterial(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                return mat;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            mat.color = color;

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void ConfigureNightLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.06f, 0.07f, 0.11f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.02f, 0.03f, 0.05f);
            RenderSettings.fogDensity = 0.022f;

            GameObject moonObj = new GameObject("Moonlight_Directional");
            Light moon = moonObj.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.55f, 0.65f, 0.9f);
            moon.intensity = 0.25f;
            moonObj.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private static void CreateEnvironment(
            Transform parent,
            out Transform spawnPoint,
            out Transform gateStopPoint,
            out Transform exitPoint,
            out Transform returnPoint)
        {
            // Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "NightGround";
            ground.transform.SetParent(parent);
            ground.transform.position = new Vector3(0f, -0.02f, 0f);
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            ground.GetComponent<Renderer>().sharedMaterial = matGround;

            // Main Asphalt Road
            GameObject road = CreateBox("MainRoad_Asphalt", parent, new Vector3(0f, -0.05f, 0f), new Vector3(6.5f, 0.1f, 70f), matAsphalt);

            // Road Center Dashed Lines
            for (int z = -28; z <= 28; z += 4)
            {
                CreateBox($"RoadLine_{z}", road.transform, new Vector3(0f, 0.55f, z / 70f), new Vector3(0.03f, 0.2f, 0.025f), matRoadLine, withCollider: false);
            }

            // Village Entrance Pillars (Z = 18)
            CreateBox("VillageGate_PillarL", parent, new Vector3(-4f, 2.5f, 18f), new Vector3(1.2f, 5f, 1.2f), matBoothWall);
            CreateBox("VillageGate_PillarR", parent, new Vector3(4f, 2.5f, 18f), new Vector3(1.2f, 5f, 1.2f), matBoothWall);
            CreateBox("VillageGate_SignBeam", parent, new Vector3(0f, 4.6f, 18f), new Vector3(9.2f, 0.8f, 0.8f), matMetal);

            // Turnaround Lane
            CreateBox("TurnaroundLane", parent, new Vector3(6.5f, -0.04f, -15f), new Vector3(7f, 0.1f, 8f), matAsphalt);

            // Waypoint Transforms for VehicleSpawner & GateController
            spawnPoint = CreateMarkerTransform("Vehicle_SpawnPoint", parent, new Vector3(0f, 0f, -26f));
            gateStopPoint = CreateMarkerTransform("Vehicle_GateStopPoint", parent, new Vector3(0f, 0f, -1.5f));
            exitPoint = CreateMarkerTransform("Vehicle_VillageExitPoint", parent, new Vector3(0f, 0f, 26f));
            returnPoint = CreateMarkerTransform("Vehicle_TurnaroundReturnPoint", parent, new Vector3(7.5f, 0f, -18f));

            // Street Lamp SpotLight over Gate Inspection Zone
            GameObject lampPost = CreateBox("StreetLamp_Post", parent, new Vector3(3.6f, 3f, -1.5f), new Vector3(0.2f, 6f, 0.2f), matMetal);
            GameObject spotObj = new GameObject("InspectionZone_SpotLight");
            spotObj.transform.SetParent(lampPost.transform);
            spotObj.transform.position = new Vector3(2.5f, 5.8f, -1.5f);
            spotObj.transform.LookAt(gateStopPoint.position);

            Light spot = spotObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1f, 0.82f, 0.58f);
            spot.intensity = 22f;
            spot.range = 16f;
            spot.spotAngle = 72f;
            spot.shadows = LightShadows.Soft;
        }

        private static void CreateSampleDataAssets(
            out DocumentDataSO residentListDoc,
            out QuestDataSO mainQuest,
            out QuestDataSO sideQuest,
            out List<VehicleDataSO> sampleVehicles,
            out ItemDataSO flashlightData,
            out ItemDataSO keyData,
            out ItemDataSO eliminatorData)
        {
            // Resident List Document
            residentListDoc = CreateOrLoadSO<DocumentDataSO>("ResidentList_Doc.asset");
            SerializedObject docSO = new SerializedObject(residentListDoc);
            docSO.FindProperty("documentTitle").stringValue = "Resident List";
            docSO.FindProperty("documentType").enumValueIndex = (int)DocumentType.ResidentList;
            docSO.FindProperty("bodyText").stringValue = "Verify driver name, house number, and license plate before opening the gate:";
            SerializedProperty recordsProp = docSO.FindProperty("residentRecords");
            recordsProp.ClearArray();
            AddResidentRecord(recordsProp, 0, "Somchai Jaidee", "101/4", "AB-1234", "White Sedan");
            AddResidentRecord(recordsProp, 1, "Wipa Raksangob", "102/8", "CD-5678", "Black SUV");
            docSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(residentListDoc);

            // Item Data SOs
            flashlightData = CreateItemSO("Item_Flashlight.asset", "Flashlight", ItemType.Flashlight, "Flashlight for inspecting dark spots inside vehicles.");
            keyData = CreateItemSO("Item_Key.asset", "Cabinet Key", ItemType.Key, "Key for unlocking the booth cabinet or vehicle trunks.");
            eliminatorData = CreateItemSO("Item_Eliminator.asset", "Anomaly Eliminator", ItemType.Eliminator, "Device to eliminate Anomalies. Never use on innocent civilians.");

            // Sample Normal NPC & Vehicle
            NPCDataSO normalNpc = CreateOrLoadSO<NPCDataSO>("NPC_Normal_Somchai.asset");
            SerializedObject nNpcSO = new SerializedObject(normalNpc);
            nNpcSO.FindProperty("npcName").stringValue = "Somchai Jaidee";
            nNpcSO.FindProperty("status").enumValueIndex = (int)NPCStatus.Normal_Valid;
            nNpcSO.FindProperty("houseNumber").stringValue = "101/4";
            nNpcSO.FindProperty("greetingDialogue").stringValue = "Good evening officer. I'm Somchai, heading home.";
            nNpcSO.FindProperty("identityResponse").stringValue = "My name is Somchai Jaidee. I live at house 101/4.";
            nNpcSO.FindProperty("purposeResponse").stringValue = "Just got back from grocery shopping.";
            nNpcSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(normalNpc);

            VehicleDataSO normalVehicle = CreateOrLoadSO<VehicleDataSO>("Vehicle_Normal_Somchai.asset");
            SerializedObject nVehSO = new SerializedObject(normalVehicle);
            nVehSO.FindProperty("frontLicensePlate").stringValue = "AB-1234";
            nVehSO.FindProperty("rearLicensePlate").stringValue = "AB-1234";
            nVehSO.FindProperty("vehicleModel").stringValue = "Sedan";
            nVehSO.FindProperty("vehicleColorName").stringValue = "White";
            nVehSO.FindProperty("vehicleColor").colorValue = new Color(0.88f, 0.88f, 0.9f);
            nVehSO.FindProperty("npcData").objectReferenceValue = normalNpc;
            nVehSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(normalVehicle);

            // Sample Anomaly NPC & Vehicle
            AnomalyDataSO fakePlateAnomaly = CreateOrLoadSO<AnomalyDataSO>("Anomaly_FakePlate.asset");
            SerializedObject anomSO = new SerializedObject(fakePlateAnomaly);
            anomSO.FindProperty("anomalyName").stringValue = "Fake Plate / Intruder";
            anomSO.FindProperty("appearWave").enumValueIndex = (int)GameWave.Wave1;
            anomSO.FindProperty("vehicleClue").stringValue = "Front and rear license plates do not match!";
            anomSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(fakePlateAnomaly);

            NPCDataSO anomalyNpc = CreateOrLoadSO<NPCDataSO>("NPC_Anomaly_FakePlate.asset");
            SerializedObject aNpcSO = new SerializedObject(anomalyNpc);
            aNpcSO.FindProperty("npcName").stringValue = "Somchai (Impostor)";
            aNpcSO.FindProperty("status").enumValueIndex = (int)NPCStatus.Anomaly;
            aNpcSO.FindProperty("anomalyData").objectReferenceValue = fakePlateAnomaly;
            aNpcSO.FindProperty("houseNumber").stringValue = "101/4";
            aNpcSO.FindProperty("greetingDialogue").stringValue = "Hurry up and open the gate...";
            aNpcSO.FindProperty("identityResponse").stringValue = "I'm... Somchai, house 101/4. Stop asking questions.";
            aNpcSO.FindProperty("purposeResponse").stringValue = "Going home... just let me in.";
            SerializedProperty suspProp = aNpcSO.FindProperty("suspiciousResponses");
            suspProp.ClearArray();
            suspProp.InsertArrayElementAtIndex(0);
            suspProp.GetArrayElementAtIndex(0).stringValue = "Maybe the rear plate fell off! Mind your own business!";
            suspProp.InsertArrayElementAtIndex(1);
            suspProp.GetArrayElementAtIndex(1).stringValue = "OPEN THE GATE NOW!!";
            aNpcSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(anomalyNpc);

            VehicleDataSO anomalyVehicle = CreateOrLoadSO<VehicleDataSO>("Vehicle_Anomaly_FakePlate.asset");
            SerializedObject aVehSO = new SerializedObject(anomalyVehicle);
            aVehSO.FindProperty("frontLicensePlate").stringValue = "AB-1234";
            aVehSO.FindProperty("rearLicensePlate").stringValue = "XX-9999";
            aVehSO.FindProperty("vehicleModel").stringValue = "Sedan";
            aVehSO.FindProperty("vehicleColorName").stringValue = "Dark Red";
            aVehSO.FindProperty("vehicleColor").colorValue = new Color(0.45f, 0.1f, 0.1f);
            aVehSO.FindProperty("npcData").objectReferenceValue = anomalyNpc;
            aVehSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(anomalyVehicle);

            sampleVehicles = new List<VehicleDataSO> { normalVehicle, anomalyVehicle };

            // Sample Quests
            mainQuest = CreateOrLoadSO<QuestDataSO>("Quest_Main_SurviveShift.asset");
            SerializedObject mqSO = new SerializedObject(mainQuest);
            mqSO.FindProperty("questId").stringValue = "main_shift";
            mqSO.FindProperty("questTitle").stringValue = "Survive the Night Shift (18:00 - 06:00)";
            mqSO.FindProperty("category").enumValueIndex = (int)QuestCategory.MainQuest;
            mqSO.FindProperty("description").stringValue = "Inspect every vehicle and do not let any Anomaly enter the village.";
            mqSO.FindProperty("partnerIntroMessage").stringValue = "Shift started. Check documents and plates carefully. I am on standby on the radio.";
            mqSO.FindProperty("partnerCompleteMessage").stringValue = "06:00 AM! Shift complete. Great job!";
            mqSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mainQuest);

            sideQuest = CreateOrLoadSO<QuestDataSO>("Quest_Side_UnlockLocker.asset");
            SerializedObject sqSO = new SerializedObject(sideQuest);
            sqSO.FindProperty("questId").stringValue = "sq_locker";
            sqSO.FindProperty("questTitle").stringValue = "Unlock Booth Locker with Key (Slot 2)";
            sqSO.FindProperty("category").enumValueIndex = (int)QuestCategory.SideQuest;
            sqSO.FindProperty("triggerWave").enumValueIndex = (int)GameWave.Wave1;
            sqSO.FindProperty("description").stringValue = "Press 2 to equip Key and Left-Click the locked cabinet inside the booth.";
            sqSO.FindProperty("partnerIntroMessage").stringValue = "Try using your Key (Slot 2) on the booth locker. I left backup gear inside.";
            sqSO.FindProperty("partnerCompleteMessage").stringValue = "Nice! I sent an extra Anomaly Eliminator to your desk.";
            sqSO.FindProperty("rewardDeskSpawnIndex").intValue = 2;
            sqSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sideQuest);
        }

        private static ItemDataSO CreateItemSO(string fileName, string itemName, ItemType type, string desc)
        {
            ItemDataSO asset = CreateOrLoadSO<ItemDataSO>(fileName);
            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("itemName").stringValue = itemName;
            so.FindProperty("itemType").enumValueIndex = (int)type;
            so.FindProperty("description").stringValue = desc;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void AddResidentRecord(SerializedProperty listProp, int index, string name, string house, string plate, string info)
        {
            listProp.InsertArrayElementAtIndex(index);
            SerializedProperty el = listProp.GetArrayElementAtIndex(index);
            el.FindPropertyRelative("residentName").stringValue = name;
            el.FindPropertyRelative("houseNumber").stringValue = house;
            el.FindPropertyRelative("licensePlate").stringValue = plate;
            el.FindPropertyRelative("vehicleInfo").stringValue = info;
        }

        private static GameObject CreateGreyboxVehiclePrefab()
        {
            GameObject root = new GameObject("GreyboxVehicle");
            BoxCollider rootCol = root.AddComponent<BoxCollider>();
            rootCol.center = new Vector3(0f, 0.85f, 0f);
            rootCol.size = new Vector3(2.1f, 1.5f, 4.3f);

            VehicleController vc = root.AddComponent<VehicleController>();

            // Body & Cabin
            GameObject chassis = CreateBox("Chassis", root.transform, new Vector3(0f, 0.55f, 0f), new Vector3(1.9f, 0.65f, 4.2f), matWhite, withCollider: false);
            GameObject cabin = CreateBox("Cabin", root.transform, new Vector3(0f, 1.15f, -0.15f), new Vector3(1.65f, 0.6f, 2.2f), matWhite, withCollider: false);

            // Transparent Windshields
            CreateBox("FrontWindshield", root.transform, new Vector3(0f, 1.15f, 0.96f), new Vector3(1.5f, 0.5f, 0.08f), matGlass, withCollider: false);
            CreateBox("RearWindshield", root.transform, new Vector3(0f, 1.15f, -1.26f), new Vector3(1.5f, 0.5f, 0.08f), matGlass, withCollider: false);

            // 4 Wheels
            Vector3[] wheelPos =
            {
                new Vector3(-0.95f, 0.35f, 1.3f),
                new Vector3(0.95f, 0.35f, 1.3f),
                new Vector3(-0.95f, 0.35f, -1.3f),
                new Vector3(0.95f, 0.35f, -1.3f)
            };
            for (int i = 0; i < wheelPos.Length; i++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Wheel_{i}";
                wheel.transform.SetParent(root.transform);
                wheel.transform.localPosition = wheelPos[i];
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wheel.transform.localScale = new Vector3(0.7f, 0.18f, 0.7f);
                wheel.GetComponent<Renderer>().sharedMaterial = matBlack;
                Object.DestroyImmediate(wheel.GetComponent<Collider>());
            }

            // Driver NPC (Capsule + Sphere)
            GameObject driver = new GameObject("DriverNPC");
            driver.transform.SetParent(root.transform);
            driver.transform.localPosition = new Vector3(-0.4f, 0.9f, 0.25f);
            CreatePrimitiveChild("Body", PrimitiveType.Capsule, driver.transform, Vector3.zero, new Vector3(0.45f, 0.4f, 0.45f), matNPC);
            CreatePrimitiveChild("Head", PrimitiveType.Sphere, driver.transform, new Vector3(0f, 0.48f, 0f), new Vector3(0.32f, 0.32f, 0.32f), matNPC);

            // Stowaway NPC at BackSeat (Hidden by default)
            GameObject stowaway = new GameObject("StowawayNPC_Hidden");
            stowaway.transform.SetParent(root.transform);
            stowaway.transform.localPosition = new Vector3(0.35f, 0.8f, -0.65f);
            CreatePrimitiveChild("Body", PrimitiveType.Capsule, stowaway.transform, Vector3.zero, new Vector3(0.42f, 0.35f, 0.42f), matBlack);
            CreatePrimitiveChild("Head", PrimitiveType.Sphere, stowaway.transform, new Vector3(0f, 0.42f, 0f), new Vector3(0.3f, 0.3f, 0.3f), matBlack);
            stowaway.SetActive(false);

            // License Plates (Front & Rear)
            TMP_Text frontPlate = CreateLicensePlate("FrontPlate", root.transform, new Vector3(0f, 0.45f, 2.12f), Quaternion.Euler(0f, 180f, 0f));
            TMP_Text rearPlate = CreateLicensePlate("RearPlate", root.transform, new Vector3(0f, 0.45f, -2.12f), Quaternion.identity);

            // 4 Waypoints
            CreateWaypoint(root.transform, "WP_Front", new Vector3(0f, 0.9f, 2.25f), VehicleWaypointType.Front, true, GameWave.Wave1);
            CreateWaypoint(root.transform, "WP_DriverWindow", new Vector3(-1.05f, 1.15f, 0.3f), VehicleWaypointType.DriverWindow, true, GameWave.Wave1);
            GameObject wpBack = CreateWaypoint(root.transform, "WP_BackSeat", new Vector3(-1.05f, 1.15f, -0.65f), VehicleWaypointType.BackSeat, false, GameWave.Wave2);
            wpBack.AddComponent<FlashlightJumpscareTarget>();

            GameObject wpTrunk = CreateWaypoint(root.transform, "WP_Trunk", new Vector3(0f, 0.9f, -2.25f), VehicleWaypointType.Trunk, false, GameWave.Wave2);
            wpTrunk.AddComponent<LockedTarget>();

            // Dedicated Driver Dialogue Trigger at Driver Window
            GameObject dialogueZone = new GameObject("DriverDialogueInteractable");
            dialogueZone.transform.SetParent(root.transform);
            dialogueZone.transform.localPosition = new Vector3(-0.98f, 1.25f, 0.25f);
            BoxCollider dCol = dialogueZone.AddComponent<BoxCollider>();
            dCol.size = new Vector3(0.4f, 0.6f, 0.7f);
            dCol.isTrigger = true;
            NPCDialogueInteractable npcDialogue = dialogueZone.AddComponent<NPCDialogueInteractable>();
            SerializedObject dSO = new SerializedObject(npcDialogue);
            dSO.FindProperty("vehicleController").objectReferenceValue = vc;
            dSO.ApplyModifiedPropertiesWithoutUndo();

            // Wire VehicleController references
            SerializedObject vcSO = new SerializedObject(vc);
            SerializedProperty renderersProp = vcSO.FindProperty("bodyRenderers");
            renderersProp.ClearArray();
            renderersProp.InsertArrayElementAtIndex(0);
            renderersProp.GetArrayElementAtIndex(0).objectReferenceValue = chassis.GetComponent<Renderer>();
            renderersProp.InsertArrayElementAtIndex(1);
            renderersProp.GetArrayElementAtIndex(1).objectReferenceValue = cabin.GetComponent<Renderer>();
            vcSO.FindProperty("frontPlateText").objectReferenceValue = frontPlate;
            vcSO.FindProperty("rearPlateText").objectReferenceValue = rearPlate;
            vcSO.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = $"{PrefabsFolder}/GreyboxVehicle.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static TMP_Text CreateLicensePlate(string name, Transform parent, Vector3 localPos, Quaternion localRot)
        {
            GameObject plate = CreateBox(name, parent, localPos, new Vector3(0.65f, 0.22f, 0.04f), matWhite, withCollider: false);
            plate.transform.localRotation = localRot;

            GameObject textObj = new GameObject("PlateText");
            textObj.transform.SetParent(plate.transform);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.6f);
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = new Vector3(1f, 3f, 1f);

            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = "ABC-1234";
            tmp.fontSize = 1.6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            return tmp;
        }

        private static GameObject CreateWaypoint(
            Transform parent,
            string name,
            Vector3 localPos,
            VehicleWaypointType type,
            bool isBasic,
            GameWave unlockWave)
        {
            GameObject wpObj = new GameObject(name);
            wpObj.transform.SetParent(parent);
            wpObj.transform.localPosition = localPos;

            BoxCollider col = wpObj.AddComponent<BoxCollider>();
            col.size = new Vector3(0.6f, 0.6f, 0.6f);
            col.isTrigger = true;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "MarkerVisual";
            marker.transform.SetParent(wpObj.transform);
            marker.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            marker.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
            marker.GetComponent<Renderer>().sharedMaterial = matWaypoint;
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            VehicleWaypoint wp = wpObj.AddComponent<VehicleWaypoint>();
            SerializedObject so = new SerializedObject(wp);
            so.FindProperty("waypointType").enumValueIndex = (int)type;
            so.FindProperty("isBasicWaypoint").boolValue = isBasic;
            so.FindProperty("unlockWave").enumValueIndex = (int)unlockWave;
            so.FindProperty("markerVisual").objectReferenceValue = marker;
            so.ApplyModifiedPropertiesWithoutUndo();

            return wpObj;
        }

        private static GameObject CreateDocumentPrefab(string name, DocumentDataSO docData, bool isVehiclePass)
        {
            GameObject docObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            docObj.name = name;
            docObj.transform.localScale = new Vector3(0.32f, 0.02f, 0.42f);
            docObj.GetComponent<Renderer>().sharedMaterial = matPaper;

            DocumentInteractable interactable = docObj.AddComponent<DocumentInteractable>();
            SerializedObject so = new SerializedObject(interactable);
            so.FindProperty("documentData").objectReferenceValue = docData;
            so.FindProperty("showCurrentVehiclePass").boolValue = isVehiclePass;
            so.ApplyModifiedPropertiesWithoutUndo();

            string path = $"{PrefabsFolder}/{name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(docObj, path);
            Object.DestroyImmediate(docObj);
            return prefab;
        }

        private static GameObject CreateEliminatorPickupPrefab()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "GreyboxEliminatorPickup";
            obj.transform.localScale = new Vector3(0.25f, 0.18f, 0.4f);
            obj.GetComponent<Renderer>().sharedMaterial = matRed;

            EquipmentPickupInteractable pickup = obj.AddComponent<EquipmentPickupInteractable>();
            SerializedObject so = new SerializedObject(pickup);
            so.FindProperty("itemDisplayName").stringValue = "Anomaly Eliminator";
            so.FindProperty("equipmentSlotIndex").intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();

            string path = $"{PrefabsFolder}/GreyboxEliminatorPickup.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static void CreateManagers(
            Transform parent,
            GameObject vehiclePrefab,
            Transform spawnPoint,
            Transform gateStopPoint,
            List<VehicleDataSO> sampleVehicles,
            out GameManager gameManager,
            out TimeWaveManager timeWaveManager,
            out VehicleSpawner vehicleSpawner,
            out AIPartnerManager aiPartnerManager,
            out JumpscareManager jumpscareManager,
            out EndingManager endingManager)
        {
            GameObject twObj = new GameObject("TimeWaveManager");
            twObj.transform.SetParent(parent);
            timeWaveManager = twObj.AddComponent<TimeWaveManager>();

            GameObject gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(parent);
            gameManager = gmObj.AddComponent<GameManager>();
            SerializedObject gmSO = new SerializedObject(gameManager);
            gmSO.FindProperty("timeWaveManager").objectReferenceValue = timeWaveManager;
            gmSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject vsObj = new GameObject("VehicleSpawner");
            vsObj.transform.SetParent(parent);
            vehicleSpawner = vsObj.AddComponent<VehicleSpawner>();
            SerializedObject vsSO = new SerializedObject(vehicleSpawner);
            vsSO.FindProperty("vehiclePrefab").objectReferenceValue = vehiclePrefab.GetComponent<VehicleController>();
            vsSO.FindProperty("spawnPoint").objectReferenceValue = spawnPoint;
            vsSO.FindProperty("gateStopPoint").objectReferenceValue = gateStopPoint;
            vsSO.FindProperty("timeWaveManager").objectReferenceValue = timeWaveManager;
            SerializedProperty poolProp = vsSO.FindProperty("vehiclePool");
            poolProp.ClearArray();
            for (int i = 0; i < sampleVehicles.Count; i++)
            {
                poolProp.InsertArrayElementAtIndex(i);
                poolProp.GetArrayElementAtIndex(i).objectReferenceValue = sampleVehicles[i];
            }
            vsSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject aiObj = new GameObject("AIPartnerManager");
            aiObj.transform.SetParent(parent);
            AudioSource aiAudio = aiObj.AddComponent<AudioSource>();
            aiPartnerManager = aiObj.AddComponent<AIPartnerManager>();
            SerializedObject aiSO = new SerializedObject(aiPartnerManager);
            aiSO.FindProperty("radioAudioSource").objectReferenceValue = aiAudio;
            aiSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject jsObj = new GameObject("JumpscareManager");
            jsObj.transform.SetParent(parent);
            AudioSource jsAudio = jsObj.AddComponent<AudioSource>();
            jumpscareManager = jsObj.AddComponent<JumpscareManager>();
            SerializedObject jsSO = new SerializedObject(jumpscareManager);
            jsSO.FindProperty("audioSource").objectReferenceValue = jsAudio;
            jsSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject endObj = new GameObject("EndingManager");
            endObj.transform.SetParent(parent);
            AudioSource endAudio = endObj.AddComponent<AudioSource>();
            endingManager = endObj.AddComponent<EndingManager>();
            SerializedObject endSO = new SerializedObject(endingManager);
            endSO.FindProperty("audioSource").objectReferenceValue = endAudio;
            endSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSecurityBoothAndGate(
            Transform parent,
            VehicleSpawner vehicleSpawner,
            Transform exitPoint,
            Transform returnPoint,
            DocumentDataSO residentListDoc,
            GameObject docPrefab,
            GameObject passPrefab,
            GameObject eliminatorPickupPrefab,
            QuestDataSO sideQuest,
            out DeskItemSpawner deskSpawner)
        {
            // Booth Dimensions: center at (-4.8, 0, 0), size 3.6 x 3 x 4.2
            Transform booth = new GameObject("SecurityBooth").transform;
            booth.SetParent(parent);

            CreateBox("Booth_Floor", booth, new Vector3(-4.8f, 0.05f, 0f), new Vector3(3.6f, 0.1f, 4.2f), matBoothWall);
            CreateBox("Booth_Roof", booth, new Vector3(-4.8f, 3.05f, 0f), new Vector3(3.8f, 0.15f, 4.4f), matBoothWall);
            CreateBox("Booth_BackWall", booth, new Vector3(-6.55f, 1.55f, 0f), new Vector3(0.15f, 3f, 4.2f), matBoothWall);
            CreateBox("Booth_NorthWall", booth, new Vector3(-4.8f, 1.55f, 2.05f), new Vector3(3.6f, 3f, 0.15f), matBoothWall);

            // South Wall with Open Doorway
            CreateBox("Booth_SouthWall_Left", booth, new Vector3(-6.0f, 1.55f, -2.05f), new Vector3(1.2f, 3f, 0.15f), matBoothWall);
            CreateBox("Booth_SouthWall_Header", booth, new Vector3(-4.2f, 2.65f, -2.05f), new Vector3(2.4f, 0.8f, 0.15f), matBoothWall);

            // Front Wall (facing Road +X) with Transparent Window
            CreateBox("Booth_FrontWall_Bottom", booth, new Vector3(-3.05f, 0.5f, 0f), new Vector3(0.15f, 1f, 4.2f), matBoothWall);
            CreateBox("Booth_FrontWall_Top", booth, new Vector3(-3.05f, 2.65f, 0f), new Vector3(0.15f, 0.8f, 4.2f), matBoothWall);
            CreateBox("Booth_FrontWindow_Glass", booth, new Vector3(-3.05f, 1.62f, 0f), new Vector3(0.05f, 1.25f, 3.8f), matGlass, withCollider: false);

            // Booth Interior Light
            GameObject boothLightObj = new GameObject("Booth_InteriorLight");
            boothLightObj.transform.SetParent(booth);
            boothLightObj.transform.position = new Vector3(-4.6f, 2.75f, 0f);
            Light boothLight = boothLightObj.AddComponent<Light>();
            boothLight.type = LightType.Point;
            boothLight.color = new Color(1f, 0.92f, 0.78f);
            boothLight.intensity = 4.5f;
            boothLight.range = 8f;

            // Desk & Item Spawn Points
            GameObject desk = CreateBox("Booth_Desk", booth, new Vector3(-3.65f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 3.2f), matDesk);
            Transform spResident = CreateMarkerTransform("Spawn_ResidentList", booth, new Vector3(-3.65f, 0.92f, -0.6f));
            Transform spPass = CreateMarkerTransform("Spawn_VehiclePass", booth, new Vector3(-3.65f, 0.92f, 0.0f));
            Transform spReward = CreateMarkerTransform("Spawn_AIReward", booth, new Vector3(-3.65f, 0.92f, 0.6f));

            deskSpawner = desk.AddComponent<DeskItemSpawner>();
            SerializedObject dsSO = new SerializedObject(deskSpawner);
            SerializedProperty entriesProp = dsSO.FindProperty("spawnEntries");
            entriesProp.ClearArray();
            AddDeskEntry(entriesProp, 0, "Resident List", spResident, docPrefab, residentListDoc, true);
            AddDeskEntry(entriesProp, 1, "Vehicle Pass", spPass, passPrefab, null, true);
            AddDeskEntry(entriesProp, 2, "AI Eliminator Reward", spReward, eliminatorPickupPrefab, null, false);
            dsSO.ApplyModifiedPropertiesWithoutUndo();

            // AI Partner Radio on Desk
            GameObject radio = CreateBox("AI_Partner_Radio", booth, new Vector3(-3.55f, 1.0f, 1.15f), new Vector3(0.25f, 0.2f, 0.35f), matMetal);
            CreatePrimitiveChild("Antenna", PrimitiveType.Cylinder, radio.transform, new Vector3(0f, 0.9f, 0f), new Vector3(0.1f, 0.8f, 0.1f), matBlack);

            // Locker (Lockable with Key -> Completes Side Quest)
            GameObject locker = CreateBox("Booth_LockedCabinet", booth, new Vector3(-5.9f, 1.0f, 1.4f), new Vector3(0.75f, 1.9f, 0.85f), matMetal);
            LockedTarget lockedTarget = locker.AddComponent<LockedTarget>();
            QuestObjectiveTrigger questTrigger = locker.AddComponent<QuestObjectiveTrigger>();
            SerializedObject qtSO = new SerializedObject(questTrigger);
            qtSO.FindProperty("targetQuest").objectReferenceValue = sideQuest;
            qtSO.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject ltSO = new SerializedObject(lockedTarget);
            UnityEventTools.AddPersistentListener(lockedTarget.GetType()
                .GetField("onUnlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(lockedTarget) as UnityEngine.Events.UnityEvent, questTrigger.CompleteTargetQuest);
            ltSO.Update();
            EditorUtility.SetDirty(locker);

            // Barrier Gate
            GameObject gateRoot = new GameObject("BarrierGate");
            gateRoot.transform.SetParent(parent);
            gateRoot.transform.position = new Vector3(-2.7f, 0f, 1.8f);
            CreateBox("Gate_Post", gateRoot.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.35f, 1.2f, 0.35f), matMetal);

            GameObject armPivot = new GameObject("BarrierArmPivot");
            armPivot.transform.SetParent(gateRoot.transform);
            armPivot.transform.localPosition = new Vector3(0f, 1.05f, 0f);

            // Barrier Arm segments (White-Red Cylinder)
            for (int i = 0; i < 6; i++)
            {
                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                seg.name = $"ArmSegment_{i}";
                seg.transform.SetParent(armPivot.transform);
                seg.transform.localPosition = new Vector3(0.55f + i * 0.9f, 0f, 0f);
                seg.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                seg.transform.localScale = new Vector3(0.1f, 0.45f, 0.1f);
                seg.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? matRed : matWhite;
                Object.DestroyImmediate(seg.GetComponent<Collider>());
            }

            GateController gateController = gateRoot.AddComponent<GateController>();
            SerializedObject gcSO = new SerializedObject(gateController);
            gcSO.FindProperty("vehicleSpawner").objectReferenceValue = vehicleSpawner;
            gcSO.FindProperty("exitPoint").objectReferenceValue = exitPoint;
            gcSO.FindProperty("returnPoint").objectReferenceValue = returnPoint;
            gcSO.FindProperty("barrierArm").objectReferenceValue = armPivot.transform;
            gcSO.ApplyModifiedPropertiesWithoutUndo();

            // Barrier Control Panel with Allow (Green) & Deny (Red) Buttons
            GameObject panelStand = CreateBox("BarrierControlPanel", booth, new Vector3(-3.35f, 0.55f, 1.55f), new Vector3(0.45f, 1.1f, 0.6f), matMetal);
            GameObject allowBtn = CreateBox("Button_Allow", panelStand.transform, new Vector3(0f, 0.56f, -0.22f), new Vector3(0.3f, 0.15f, 0.25f), matGreen);
            GateButton allowComp = allowBtn.AddComponent<GateButton>();
            SerializedObject abSO = new SerializedObject(allowComp);
            abSO.FindProperty("buttonType").enumValueIndex = (int)GateButtonType.Allow;
            abSO.FindProperty("gateController").objectReferenceValue = gateController;
            abSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject denyBtn = CreateBox("Button_Deny", panelStand.transform, new Vector3(0f, 0.56f, 0.22f), new Vector3(0.3f, 0.15f, 0.25f), matRed);
            GateButton denyComp = denyBtn.AddComponent<GateButton>();
            SerializedObject dbSO = new SerializedObject(denyComp);
            dbSO.FindProperty("buttonType").enumValueIndex = (int)GateButtonType.Deny;
            dbSO.FindProperty("gateController").objectReferenceValue = gateController;
            dbSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddDeskEntry(
            SerializedProperty listProp,
            int index,
            string label,
            Transform spawnPoint,
            GameObject prefab,
            DocumentDataSO docOverride,
            bool spawnOnStart)
        {
            listProp.InsertArrayElementAtIndex(index);
            SerializedProperty el = listProp.GetArrayElementAtIndex(index);
            el.FindPropertyRelative("label").stringValue = label;
            el.FindPropertyRelative("spawnPoint").objectReferenceValue = spawnPoint;
            el.FindPropertyRelative("itemPrefab").objectReferenceValue = prefab;
            el.FindPropertyRelative("overrideDocumentData").objectReferenceValue = docOverride;
            el.FindPropertyRelative("spawnOnStart").boolValue = spawnOnStart;
        }

        private static void CreatePlayer(
            ItemDataSO flashlightData,
            ItemDataSO keyData,
            ItemDataSO eliminatorData,
            out FirstPersonController fps,
            out EquipmentManager equipmentManager)
        {
            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(-4.6f, 1.05f, -0.8f);
            player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;

            fps = player.AddComponent<FirstPersonController>();
            equipmentManager = player.AddComponent<EquipmentManager>();

            // Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(player.transform);
            camObj.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            camObj.transform.localRotation = Quaternion.identity;

            Camera cam = camObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<CameraShake>();

            // Right Hand Anchor
            GameObject handAnchor = new GameObject("RightHandAnchor");
            handAnchor.transform.SetParent(camObj.transform);
            handAnchor.transform.localPosition = new Vector3(0.32f, -0.25f, 0.5f);
            handAnchor.transform.localRotation = Quaternion.identity;

            // Slot 0: Flashlight
            GameObject flashObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            flashObj.name = "Equip_Flashlight";
            flashObj.transform.SetParent(handAnchor.transform);
            flashObj.transform.localPosition = Vector3.zero;
            flashObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            flashObj.transform.localScale = new Vector3(0.06f, 0.14f, 0.06f);
            flashObj.GetComponent<Renderer>().sharedMaterial = matBlack;
            Object.DestroyImmediate(flashObj.GetComponent<Collider>());

            GameObject flashLightChild = new GameObject("SpotLight");
            flashLightChild.transform.SetParent(flashObj.transform);
            flashLightChild.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            flashLightChild.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            Light spot = flashLightChild.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.intensity = 18f;
            spot.range = 18f;
            spot.spotAngle = 48f;
            spot.color = new Color(1f, 0.96f, 0.85f);

            AudioSource flashAudio = flashObj.AddComponent<AudioSource>();
            FlashlightItem flashItem = flashObj.AddComponent<FlashlightItem>();
            WireEquipmentItemData(flashItem, flashlightData);
            SerializedObject flSO = new SerializedObject(flashItem);
            flSO.FindProperty("spotLight").objectReferenceValue = spot;
            flSO.FindProperty("audioSource").objectReferenceValue = flashAudio;
            flSO.ApplyModifiedPropertiesWithoutUndo();

            // Slot 1: Key
            GameObject keyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keyObj.name = "Equip_Key";
            keyObj.transform.SetParent(handAnchor.transform);
            keyObj.transform.localPosition = Vector3.zero;
            keyObj.transform.localScale = new Vector3(0.04f, 0.04f, 0.16f);
            keyObj.GetComponent<Renderer>().sharedMaterial = matRoadLine;
            Object.DestroyImmediate(keyObj.GetComponent<Collider>());
            AudioSource keyAudio = keyObj.AddComponent<AudioSource>();
            KeyItem keyItem = keyObj.AddComponent<KeyItem>();
            WireEquipmentItemData(keyItem, keyData);
            SerializedObject kSO = new SerializedObject(keyItem);
            kSO.FindProperty("audioSource").objectReferenceValue = keyAudio;
            kSO.ApplyModifiedPropertiesWithoutUndo();

            // Slot 2: Eliminator
            GameObject elimObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            elimObj.name = "Equip_Eliminator";
            elimObj.transform.SetParent(handAnchor.transform);
            elimObj.transform.localPosition = Vector3.zero;
            elimObj.transform.localScale = new Vector3(0.09f, 0.11f, 0.28f);
            elimObj.GetComponent<Renderer>().sharedMaterial = matRed;
            Object.DestroyImmediate(elimObj.GetComponent<Collider>());

            GameObject pfxObj = new GameObject("FireParticle");
            pfxObj.transform.SetParent(elimObj.transform);
            pfxObj.transform.localPosition = new Vector3(0f, 0f, 0.6f);
            ParticleSystem ps = pfxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.duration = 0.4f;
            main.startLifetime = 0.35f;
            main.startSpeed = 4f;
            main.startSize = 0.12f;

            AudioSource elimAudio = elimObj.AddComponent<AudioSource>();
            EliminatorItem elimItem = elimObj.AddComponent<EliminatorItem>();
            WireEquipmentItemData(elimItem, eliminatorData);
            SerializedObject elSO = new SerializedObject(elimItem);
            elSO.FindProperty("fireParticle").objectReferenceValue = ps;
            elSO.FindProperty("audioSource").objectReferenceValue = elimAudio;
            elSO.ApplyModifiedPropertiesWithoutUndo();

            // Wire FirstPersonController & EquipmentManager
            SerializedObject fpsSO = new SerializedObject(fps);
            fpsSO.FindProperty("playerCamera").objectReferenceValue = camObj.transform;
            fpsSO.FindProperty("equipmentManager").objectReferenceValue = equipmentManager;
            fpsSO.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject eqSO = new SerializedObject(equipmentManager);
            eqSO.FindProperty("rightHandAnchor").objectReferenceValue = handAnchor.transform;
            eqSO.FindProperty("playerCamera").objectReferenceValue = camObj.transform;
            SerializedProperty slotsProp = eqSO.FindProperty("slots");
            slotsProp.ClearArray();
            AddEquipmentSlot(slotsProp, 0, "1: Flashlight", flashItem, true);
            AddEquipmentSlot(slotsProp, 1, "2: Key", keyItem, true);
            AddEquipmentSlot(slotsProp, 2, "3: Eliminator", elimItem, true);
            eqSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireEquipmentItemData(EquipmentItemBase item, ItemDataSO data)
        {
            SerializedObject so = new SerializedObject(item);
            so.FindProperty("itemData").objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddEquipmentSlot(SerializedProperty slotsProp, int index, string label, EquipmentItemBase item, bool unlocked)
        {
            slotsProp.InsertArrayElementAtIndex(index);
            SerializedProperty el = slotsProp.GetArrayElementAtIndex(index);
            el.FindPropertyRelative("slotLabel").stringValue = label;
            el.FindPropertyRelative("itemInstance").objectReferenceValue = item;
            el.FindPropertyRelative("unlocked").boolValue = unlocked;
        }

        private static void WireManagerReferences(
            TimeWaveManager timeWaveManager,
            AIPartnerManager aiPartnerManager,
            DeskItemSpawner deskSpawner,
            EquipmentManager equipmentManager,
            QuestDataSO mainQuest,
            QuestDataSO sideQuest)
        {
            SerializedObject aiSO = new SerializedObject(aiPartnerManager);
            aiSO.FindProperty("mainQuest").objectReferenceValue = mainQuest;
            SerializedProperty sqList = aiSO.FindProperty("sideQuests");
            sqList.ClearArray();
            sqList.InsertArrayElementAtIndex(0);
            sqList.GetArrayElementAtIndex(0).objectReferenceValue = sideQuest;
            aiSO.FindProperty("timeWaveManager").objectReferenceValue = timeWaveManager;
            aiSO.FindProperty("deskItemSpawner").objectReferenceValue = deskSpawner;
            aiSO.FindProperty("equipmentManager").objectReferenceValue = equipmentManager;
            aiSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateUICanvas(
            FirstPersonController playerController,
            EquipmentManager equipmentManager,
            TimeWaveManager timeWaveManager,
            AIPartnerManager aiPartnerManager,
            GameManager gameManager,
            EndingManager endingManager,
            JumpscareManager jumpscareManager)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();

            GameObject canvasObj = new GameObject("MainHUD_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 1. Crosshair
            GameObject crossObj = new GameObject("CrosshairDot");
            crossObj.transform.SetParent(canvasObj.transform, false);
            Image crossImg = crossObj.AddComponent<Image>();
            crossImg.rectTransform.sizeDelta = new Vector2(8f, 8f);

            TMP_Text promptText = CreateTMP(canvasObj.transform, "InteractionPrompt", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -35f), new Vector2(500f, 40f), 22, TextAlignmentOptions.Center);
            CrosshairUI crosshairUI = canvasObj.AddComponent<CrosshairUI>();
            SerializedObject chSO = new SerializedObject(crosshairUI);
            chSO.FindProperty("playerController").objectReferenceValue = playerController;
            chSO.FindProperty("crosshairImage").objectReferenceValue = crossImg;
            chSO.FindProperty("interactionPromptText").objectReferenceValue = promptText;
            chSO.ApplyModifiedPropertiesWithoutUndo();

            // 2. Digital Clock (Top Right)
            TMP_Text clockText = CreateTMP(canvasObj.transform, "ClockText", "18:00", new Vector2(1f, 1f), new Vector2(-140f, -45f), new Vector2(240f, 50f), 34, TextAlignmentOptions.Right);
            TMP_Text waveText = CreateTMP(canvasObj.transform, "WaveText", "Wave 1", new Vector2(1f, 1f), new Vector2(-140f, -85f), new Vector2(240f, 35f), 22, TextAlignmentOptions.Right);
            DigitalClockUI clockUI = canvasObj.AddComponent<DigitalClockUI>();
            SerializedObject clSO = new SerializedObject(clockUI);
            clSO.FindProperty("timeWaveManager").objectReferenceValue = timeWaveManager;
            clSO.FindProperty("clockText").objectReferenceValue = clockText;
            clSO.FindProperty("waveText").objectReferenceValue = waveText;
            clSO.ApplyModifiedPropertiesWithoutUndo();

            // 3. Equipment HUD (Bottom Center)
            TMP_Text equipText = CreateTMP(canvasObj.transform, "ActiveEquipmentText", "Flashlight", new Vector2(0.5f, 0f), new Vector2(0f, 45f), new Vector2(400f, 40f), 24, TextAlignmentOptions.Center);
            EquipmentSlotUI equipUI = canvasObj.AddComponent<EquipmentSlotUI>();
            SerializedObject eqUiSO = new SerializedObject(equipUI);
            eqUiSO.FindProperty("equipmentManager").objectReferenceValue = equipmentManager;
            eqUiSO.FindProperty("activeItemNameText").objectReferenceValue = equipText;
            eqUiSO.ApplyModifiedPropertiesWithoutUndo();

            // 4. AI Partner Quest HUD (Top Left)
            TMP_Text mainQText = CreateTMP(canvasObj.transform, "MainQuestText", "Main Quest...", new Vector2(0f, 1f), new Vector2(280f, -45f), new Vector2(520f, 60f), 20, TextAlignmentOptions.TopLeft);
            TMP_Text sideQText = CreateTMP(canvasObj.transform, "SideQuestText", "Side Quest...", new Vector2(0f, 1f), new Vector2(280f, -110f), new Vector2(520f, 60f), 18, TextAlignmentOptions.TopLeft);
            GameObject radioBanner = CreatePanel(canvasObj.transform, "RadioBanner", new Vector2(0.5f, 1f), new Vector2(0f, -65f), new Vector2(720f, 70f), new Color(0f, 0f, 0f, 0.75f));
            TMP_Text radioText = CreateTMP(radioBanner.transform, "RadioText", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 60f), 20, TextAlignmentOptions.Center);
            AIPartnerUI aiUI = canvasObj.AddComponent<AIPartnerUI>();
            SerializedObject aiUiSO = new SerializedObject(aiUI);
            aiUiSO.FindProperty("aiPartnerManager").objectReferenceValue = aiPartnerManager;
            aiUiSO.FindProperty("mainQuestText").objectReferenceValue = mainQText;
            aiUiSO.FindProperty("sideQuestText").objectReferenceValue = sideQText;
            aiUiSO.FindProperty("radioBannerRoot").objectReferenceValue = radioBanner;
            aiUiSO.FindProperty("radioMessageText").objectReferenceValue = radioText;
            aiUiSO.ApplyModifiedPropertiesWithoutUndo();

            // 5. Document Viewer UI
            GameObject docPanel = CreatePanel(canvasObj.transform, "DocumentPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 520f), new Color(0.12f, 0.12f, 0.14f, 0.96f));
            TMP_Text docTitle = CreateTMP(docPanel.transform, "DocTitle", "Document", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 50f), 28, TextAlignmentOptions.Center);
            TMP_Text docBody = CreateTMP(docPanel.transform, "DocBody", "", new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(600f, 340f), 20, TextAlignmentOptions.TopLeft);
            Button docClose = CreateButton(docPanel.transform, "CloseDocBtn", "Close (ESC)", new Vector2(0f, -210f), new Vector2(200f, 45f));
            DocumentViewerUI docViewer = canvasObj.AddComponent<DocumentViewerUI>();
            SerializedObject dvSO = new SerializedObject(docViewer);
            dvSO.FindProperty("panelRoot").objectReferenceValue = docPanel;
            dvSO.FindProperty("titleText").objectReferenceValue = docTitle;
            dvSO.FindProperty("bodyText").objectReferenceValue = docBody;
            dvSO.FindProperty("closeButton").objectReferenceValue = docClose;
            dvSO.FindProperty("playerController").objectReferenceValue = playerController;
            dvSO.ApplyModifiedPropertiesWithoutUndo();

            // 6. Dialogue UI
            GameObject dlgPanel = CreatePanel(canvasObj.transform, "DialoguePanel", new Vector2(0.5f, 0f), new Vector2(0f, 180f), new Vector2(820f, 300f), new Color(0.08f, 0.09f, 0.12f, 0.94f));
            TMP_Text npcName = CreateTMP(dlgPanel.transform, "NPCName", "NPC", new Vector2(0f, 1f), new Vector2(220f, -35f), new Vector2(380f, 40f), 26, TextAlignmentOptions.Left);
            TMP_Text dlgBody = CreateTMP(dlgPanel.transform, "DialogueBody", "...", new Vector2(0.5f, 1f), new Vector2(0f, -95f), new Vector2(760f, 90f), 22, TextAlignmentOptions.TopLeft);
            Button btnIdentity = CreateButton(dlgPanel.transform, "Btn_Identity", "Ask Name / House", new Vector2(-270f, -95f), new Vector2(180f, 45f));
            Button btnPurpose = CreateButton(dlgPanel.transform, "Btn_Purpose", "Ask Purpose", new Vector2(-80f, -95f), new Vector2(180f, 45f));
            Button btnSuspicious = CreateButton(dlgPanel.transform, "Btn_Suspicious", "Press Suspicion", new Vector2(110f, -95f), new Vector2(180f, 45f));
            Button btnCloseDlg = CreateButton(dlgPanel.transform, "Btn_CloseDlg", "End Talk", new Vector2(300f, -95f), new Vector2(160f, 45f));
            DialogueUI dlgUI = canvasObj.AddComponent<DialogueUI>();
            SerializedObject dlSO = new SerializedObject(dlgUI);
            dlSO.FindProperty("panelRoot").objectReferenceValue = dlgPanel;
            dlSO.FindProperty("npcNameText").objectReferenceValue = npcName;
            dlSO.FindProperty("dialogueText").objectReferenceValue = dlgBody;
            dlSO.FindProperty("askIdentityButton").objectReferenceValue = btnIdentity;
            dlSO.FindProperty("askPurposeButton").objectReferenceValue = btnPurpose;
            dlSO.FindProperty("askSuspiciousButton").objectReferenceValue = btnSuspicious;
            dlSO.FindProperty("closeButton").objectReferenceValue = btnCloseDlg;
            dlSO.FindProperty("playerController").objectReferenceValue = playerController;
            dlSO.ApplyModifiedPropertiesWithoutUndo();

            // 7. Ending Panel
            GameObject endPanel = CreatePanel(canvasObj.transform, "EndingPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 480f), new Color(0.05f, 0.05f, 0.07f, 0.96f));
            TMP_Text endTitle = CreateTMP(endPanel.transform, "EndingTitle", "SHIFT COMPLETE", new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(700f, 60f), 36, TextAlignmentOptions.Center);
            TMP_Text endDesc = CreateTMP(endPanel.transform, "EndingDesc", "", new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(680f, 120f), 22, TextAlignmentOptions.Center);
            TMP_Text endStats = CreateTMP(endPanel.transform, "EndingStats", "", new Vector2(0.5f, 0.5f), new Vector2(0f, -65f), new Vector2(680f, 140f), 20, TextAlignmentOptions.Center);
            Button restartBtn = CreateButton(endPanel.transform, "RestartBtn", "Restart Shift", new Vector2(0f, -185f), new Vector2(220f, 50f));
            SerializedObject emSO = new SerializedObject(endingManager);
            emSO.FindProperty("gameManager").objectReferenceValue = gameManager;
            emSO.FindProperty("playerController").objectReferenceValue = playerController;
            emSO.FindProperty("endingPanelRoot").objectReferenceValue = endPanel;
            emSO.FindProperty("endingTitleText").objectReferenceValue = endTitle;
            emSO.FindProperty("endingDescriptionText").objectReferenceValue = endDesc;
            emSO.FindProperty("statsSummaryText").objectReferenceValue = endStats;
            emSO.FindProperty("restartButton").objectReferenceValue = restartBtn;
            emSO.ApplyModifiedPropertiesWithoutUndo();

            // 8. Horror Flash & Screen Fader (Last in Canvas Hierarchy)
            GameObject flashObj = CreateFullScreenOverlay(canvasObj.transform, "HorrorFlashOverlay", new Color(0.65f, 0f, 0f, 1f));
            CanvasGroup flashCG = flashObj.AddComponent<CanvasGroup>();
            SerializedObject jsSO = new SerializedObject(jumpscareManager);
            jsSO.FindProperty("horrorFlashCanvasGroup").objectReferenceValue = flashCG;
            jsSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject faderObj = CreateFullScreenOverlay(canvasObj.transform, "ScreenFaderOverlay", Color.black);
            CanvasGroup faderCG = faderObj.AddComponent<CanvasGroup>();
            ScreenFaderUI fader = faderObj.AddComponent<ScreenFaderUI>();
            SerializedObject fdSO = new SerializedObject(fader);
            fdSO.FindProperty("fadeCanvasGroup").objectReferenceValue = faderCG;
            fdSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool withCollider = true)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.localPosition = pos;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = mat;
            if (!withCollider)
            {
                Object.DestroyImmediate(box.GetComponent<Collider>());
            }
            return box;
        }

        private static void CreatePrimitiveChild(string name, PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;
            obj.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(obj.GetComponent<Collider>());
        }

        private static Transform CreateMarkerTransform(string name, Transform parent, Vector3 pos)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = pos;
            return obj.transform;
        }

        private static T CreateOrLoadSO<T>(string fileName) where T : ScriptableObject
        {
            string path = $"{ScriptableObjectsFolder}/{fileName}";
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            T instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image img = panel.AddComponent<Image>();
            img.color = color;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return panel;
        }

        private static GameObject CreateFullScreenOverlay(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return obj;
        }

        private static TMP_Text CreateTMP(Transform parent, string name, string text, Vector2 anchor, Vector2 anchoredPos, Vector2 size, float fontSize, TextAlignmentOptions align)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.24f, 0.28f, 0.34f, 1f);
            RectTransform rt = img.rectTransform;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Button btn = btnObj.AddComponent<Button>();
            CreateTMP(btnObj.transform, "Label", label, new Vector2(0.5f, 0.5f), Vector2.zero, size, 18, TextAlignmentOptions.Center);
            return btn;
        }
    }
}
