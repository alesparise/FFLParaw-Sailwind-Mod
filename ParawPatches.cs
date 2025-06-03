using Crest;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

using Object = UnityEngine.Object;

namespace FFLParaw
{
    public class ParawPatches
    {
        // variables 
        public static string modFolder;
        public static readonly string[] assemblies = {
            "FFLParawScripts.dll",
            /*"ShipyardLib.dll"*/ };

        public static AssetBundle bundle;

        public static GameObject paraw;
        public static GameObject parawEmbark;
        public static GameObject tanjaM;
        public static GameObject tanjaS;

        // PATCHES
        public static void StartPatch(FloatingOriginManager __instance)
        {   //this patches the FloatingOriginManager Start() method and does all the setup part

            SetupThings();

            IndexManager.AssignAvailableIndex(paraw);

            Transform shiftingWorld = __instance.transform;
            SetMooring(shiftingWorld);

            GameObject parawInstance = Object.Instantiate(paraw, shiftingWorld);
            parawInstance.name = paraw.name;

            //SET UP WALK COL
            parawEmbark = parawInstance.transform.Find("WALK paraw").gameObject;
            parawEmbark.transform.parent = GameObject.Find("walk cols").transform;

            //SET INITIAL POSITION  (temporarily in the Kicia Bay shipyard)
            Vector3 startPos = new Vector3(26025f, 0f, -71009f);
            SetRotationAndPosition(parawInstance, -70f, startPos);
        }
        public static bool WakeObjectPatch(WakeAdjuster __instance, ref Rigidbody ___boat, ref SphereWaterInteraction ___interaction, ref Vector3 ___initialScale, ref float ___initialWeight)
        {
            if (__instance.gameObject.name.StartsWith("WaterSphere"))
            { //hacky way to find my own WaterSpheres
                ___boat = __instance.GetComponentInParent<Rigidbody>();
                ___initialScale = __instance.transform.localScale;
                ___interaction = __instance.GetComponent<SphereWaterInteraction>();
                ___initialWeight = ___interaction._weight;

                return false;
            }
            return true;
        }
        public static void ShipyardPatch(Shipyard __instance)
        {   //since the outriggers are pretty large, we need to adjust the release position of the shipyard, otherwise the boat gets stuck
            if (__instance.name == "shipyard Lagoon")
            {
                __instance.shipReleasePosition.localPosition = new Vector3(12.8f, -1.4f, -20.2f);
                Array.Resize(ref __instance.sailPrefabs, __instance.sailPrefabs.Length + 2);
                __instance.sailPrefabs[__instance.sailPrefabs.Length - 2] = PrefabsDirectory.instance.sails[500]; //add the tanja medium sail to the shipyard
                __instance.sailPrefabs[__instance.sailPrefabs.Length - 1] = PrefabsDirectory.instance.sails[501]; //add the tanja small sail to the shipyard
            }
        }
        public static void AddModdedSail()
        {   //add modded sails to the directory

            if (PrefabsDirectory.instance.sails.Length < 512) Array.Resize(ref PrefabsDirectory.instance.sails, 512);   //if the sails array has not been resized yet (by other mods), resize it to 512

            PrefabsDirectory.instance.sails[500] = tanjaM;
            PrefabsDirectory.instance.sails[501] = tanjaS;
        }
        public static void SailSetupPatch(Mast __instance)
        {   // set the inital sail to attach them

            if (__instance.shipRigidbody.name.StartsWith("FFL Paraw"))
            {   //if this is a mast on the paraw
                if (__instance.name == "mast_main_0")
                {
                    __instance.startSailPrefab = PrefabsDirectory.instance.sails[500];
                }
                if (__instance.name == "mast_mizzen_0")
                {
                    __instance.startSailPrefab = PrefabsDirectory.instance.sails[501];
                }
            }
        }
        public static void SailSetupPatch2(Mast __instance)
        {   //fixes the default rig install height
            if (__instance.shipRigidbody.name.StartsWith("FFL Paraw"))
            {
                if (__instance.name == "mast_main_0")
                {
                    Sail sail = __instance.GetComponentInChildren<Sail>();
                    sail.ChangeInstallHeight(-2.3f);    //starts at 12,2f
                    sail.UpdateInstallPosition();       //target is 9,9f
                }
                if (__instance.name == "mast_mizzen_0")
                {
                    Sail sail = __instance.GetComponentInChildren<Sail>();
                    sail.ChangeInstallHeight(-5.1f);    //starts at 9,6f
                    sail.UpdateInstallPosition();       //target is 4.5f
                }
            }
        }

        // HELPER METHODS
        public static void SetupThings()
        {   // loads all the mods stuff (assemblies and assets)

            //Get the folder of the mod's assembly
            modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //Load the scripts assembly where the script components are stored
            foreach (string assembly in assemblies)
            {
                string assemblyPath = Path.Combine(modFolder, assembly);
                if (File.Exists(assemblyPath)) Assembly.LoadFrom(assemblyPath);
                else Debug.LogError("FFLParaw: Couldn't load " + assembly + "!");
            }

            //Load bundle
            string bundlePath = Path.Combine(modFolder, "paraw");
            bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Debug.LogError("FFLParaw: Could not load the bundle!");
                return;
            }

            //Load boat prefab
            string parawPath = "Assets/Paraw/FFL Paraw.prefab";
            paraw = bundle.LoadAsset<GameObject>(parawPath);

            //load sail prefab
            //NOTE: additional sail setup is done in AddModdedSail() and SailSetupPatch() methods
            //      You can't setup sails here, because the PrefabsDirectory is not initialized yet
            string sailsPath = "Assets/Paraw/Sails";
            string tanjaMediumPath = Path.Combine(sailsPath, "sail_tanja_medium.prefab");
            string tanjaSmallPath = Path.Combine(sailsPath, "sail_tanja_small.prefab");
            tanjaM = bundle.LoadAsset<GameObject>(tanjaMediumPath);
            tanjaS = bundle.LoadAsset<GameObject>(tanjaSmallPath);

            //ADD CUSTOM COMPONENTS
            //mostly added directly in unity or using bridge components right now
            //but this is a good place to add them if needed

            //Set the region
            paraw.GetComponent<PurchasableBoat>().region = GameObject.Find("Region Emerald Lagoon").GetComponent<Region>();

            //Fix materials
            FixMaterials();
        }
        public static void SetRotationAndPosition(GameObject boat, float yRot, Vector3 position)
        {   //set the initial rotation of the boat
            Transform t = boat.transform;
            t.eulerAngles = new Vector3(0f, yRot, 0f);
            t.position = position;
        }
        public static void SetMooring(Transform shiftingWorld)
        {   //attach the initial mooring lines to the correct cleats in Kicia Bay for now
            Transform fort = shiftingWorld.Find("island 27 Lagoon Shipyard");
            BoatMooringRopes mr = paraw.GetComponent<BoatMooringRopes>();
            mr.mooringFront = fort.Find("dock_mooring E (11)").transform;
            mr.mooringBack = fort.Find("dock_mooring E (13)").transform;
        }
        private static void FixMaterials()
        {   //fix the materials of the paraw prefab
            Transform parawTransform = paraw.transform;
            Transform parawModel = paraw.transform.Find("paraw");
            Transform outrigger = parawModel.Find("outrigger");
            Transform outriggerLeft = outrigger.Find("outrigger_left");
            Transform outriggerRight = outrigger.Find("outrigger_right");

            MatLib.RegisterMaterials();
            parawTransform.Find("WaterFoam").GetComponent<MeshRenderer>().sharedMaterial = MatLib.foam;
            outriggerLeft.Find("WaterFoam").GetComponent<MeshRenderer>().sharedMaterial = MatLib.foam;
            outriggerRight.Find("WaterFoam").GetComponent<MeshRenderer>().sharedMaterial = MatLib.foam;
            parawTransform.Find("WaterObjectInteractionSphereBack").GetComponent<MeshRenderer>().sharedMaterial = MatLib.objectInteraction;
            parawTransform.Find("WaterObjectInteractionSphereFront").GetComponent<MeshRenderer>().sharedMaterial = MatLib.objectInteraction;
            outriggerLeft.Find("WaterSphereLeftBack").GetComponent<MeshRenderer>().sharedMaterial = MatLib.objectInteraction;
            outriggerLeft.Find("WaterSphereLeftFront").GetComponent<MeshRenderer>().sharedMaterial = MatLib.objectInteraction;
            outriggerRight.Find("WaterSphereRightBack").GetComponent<MeshRenderer>().sharedMaterial = MatLib.objectInteraction;
            outriggerRight.Find("WaterSphereRightFront").GetComponent<MeshRenderer>().sharedMaterial = MatLib.objectInteraction;
            parawTransform.Find("overflow_left").GetComponent<Renderer>().sharedMaterial = MatLib.overflow;
            parawTransform.Find("overflow_right").GetComponent<Renderer>().sharedMaterial = MatLib.overflow;
            parawModel.Find("water_mask_reg").GetComponent<MeshRenderer>().sharedMaterial = MatLib.convexHull;
            parawModel.Find("water_mask_ext").GetComponent<MeshRenderer>().sharedMaterial = MatLib.convexHull;
            parawModel.Find("water_damage").GetComponent<MeshRenderer>().sharedMaterial = MatLib.water4;
            parawModel.Find("splash_mask_reg").GetComponent<MeshRenderer>().sharedMaterial = MatLib.mask;
            parawModel.Find("splash_mask_ext").GetComponent<MeshRenderer>().sharedMaterial = MatLib.mask;
        }
    }
}
