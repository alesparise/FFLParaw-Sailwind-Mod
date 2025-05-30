//using ShipyardExpansion;
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
        //public const string bridge = "DinghiesBridge.dll";      //the name of the .dll file containing the bridge components
        public const string scripts = "FFLParawScripts.dll";    //the name of the .dll file containing the scripts components

        public static AssetBundle bundle;

        public static GameObject paraw;
        public static GameObject parawEmbark;
        public static GameObject tanjaM;

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
        public static void ShipyardPosPatch(Shipyard __instance)
        {   //since the outriggers are pretty large, we need to adjust the release position of the shipyard, otherwise the boat gets stuck
            if (__instance.name == "shipyard Lagoon")
            {
                __instance.shipReleasePosition.localPosition = new Vector3(12.8f, -1.4f , -20.2f);
            }
        }
        public static void SailSetupPatch(Mast __instance)
        {   // set the inital sail to attach them
            if (__instance.shipRigidbody.name.StartsWith("FFL Paraw"))
            {   //if this is a mast on the paraw

                GameObject[] sails = PrefabsDirectory.instance.sails;

                if (sails.Length < 512)
                {   //if the sails array has not been resized yet (by other mods), resize it to 512
                    Array.Resize(ref sails, 512);
                }

                sails[500] = tanjaM;
                PrefabsDirectory.instance.sails = sails;

                Debug.LogWarning("PARAW: sails length: " + sails.Length + " prefabdir: " + PrefabsDirectory.instance.sails.Length);

                if (__instance.name == "mast_main_0")
                {
                    __instance.startSailPrefab = sails[500];
                }
            }
        }
        public static void SailSetupPatch2(Mast __instance)
        {   //fixes the default rig's scale and install height
            if (__instance.shipRigidbody.name.StartsWith("FFL Paraw"))
            {   
                if (__instance.name == "mast_main_0")
                {
                    Sail sail = __instance.GetComponentInChildren<Sail>();
                    sail.ChangeInstallHeight(-2.5f);    //starts at 12,2f
                    sail.UpdateInstallPosition();       //target is 9,7f
                }
            }
        }
        
        // HELPER METHODS
        public static void SetupThings()
        {   // loads all the mods stuff (assemblies and assets)

            //Get the folder of the mod's assembly
            modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //Load the scripts assembly where the script components are stored
            string scriptsPath = Path.Combine(modFolder, scripts);
            if (File.Exists(scriptsPath)) Assembly.LoadFrom(scriptsPath);
            else Debug.LogError("FFLParaw: Couldn't load " + scripts + "!");

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
            string tanjaMediumPath = "Assets/Paraw/Sails/sail_tanja_medium.prefab";
            tanjaM = bundle.LoadAsset<GameObject>(tanjaMediumPath);
            tanjaM.GetComponentInChildren<ReefEffectAnimUniversal>();

            //ADD COMPONENTS
            AddComponents();
            
            //Set the region
            paraw.GetComponent<PurchasableBoat>().region = GameObject.Find("Region Emerald Lagoon").GetComponent<Region>();

            //Fix materials
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
            
            //DEBUG:
            //ShowWalkCols(); //this shows the WalkCols layer
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
        private static void AddComponents()
        {   //adds the components to the paraw prefab
            Transform parawModel = paraw.GetComponentInChildren<BoatHorizon>().transform;
            
            //add outrigger component to the outriggers
            Transform outrigger = parawModel.Find("outrigger");
            Transform outriggerLeft = outrigger.Find("outrigger_left");
            Transform outriggerRight = outrigger.Find("outrigger_right");

            outriggerLeft.gameObject.AddComponent<Outrigger>();
            outriggerRight.gameObject.AddComponent<Outrigger>();
            outriggerLeft.gameObject.AddComponent<OutriggerFoam>();
            outriggerRight.gameObject.AddComponent<OutriggerFoam>();
        }
    }
}
