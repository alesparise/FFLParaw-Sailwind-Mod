using BepInEx;
using BepInEx.Configuration;
//debug
using Crest;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using UnityEngine;
//poorly written by pr0skynesis (discord username)

namespace FFLParaw
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ParawMain : BaseUnityPlugin
    {
        // Necessary plugin info
        public const string pluginGuid = "pr0skynesis.paraw";
        public const string pluginName = "FFL Paraw";
        public const string pluginVersion = "1.0.0";
        public const string shortName = "pr0.paraw";

        //config file info
        public static ConfigEntry<bool> saveCleanerConfig;
        public static ConfigEntry<bool> invertedTillerConfig;

        public void Awake()
        {
            //CONFIG FILE
            saveCleanerConfig = Config.Bind("A) Various Settings", "Save Cleaner", false, "Removes the saves dependency on this mod. Only use if you want to remove the mod from an ongoing save! Change to true (with the game closed), open the game → load the save → save → close the game → remove the mod → done. A save backup is recommended.");
            invertedTillerConfig = Config.Bind("A) Various Settings", "Invert Tiller", false, "Invert the tiller controls (e.g. press left to go right)");

            //PATCHING
            Harmony harmony = new Harmony(pluginGuid);

            //patch to manage indexes
            MethodInfo original4 = AccessTools.Method(typeof(SaveLoadManager), "LoadGame");
            MethodInfo patch4 = AccessTools.Method(typeof(IndexManager), "Manager");
            harmony.Patch(original4, new HarmonyMethod(patch4));

            //save modded indexes
            MethodInfo original5 = AccessTools.Method(typeof(SaveLoadManager), "LoadModData");
            MethodInfo patch5 = AccessTools.Method(typeof(IndexManager), "SaveIndex");
            harmony.Patch(original5, new HarmonyMethod(patch5));

            //Save mod data on new game
            MethodInfo original6 = AccessTools.Method(typeof(StartMenu), "StartNewGame");
            MethodInfo patch6 = AccessTools.Method(typeof(IndexManager), "StartNewGamePatch");
            harmony.Patch(original6, new HarmonyMethod(patch6));

            //make sure the WakeAdjuster component initializes correctly
            MethodInfo originalWake = AccessTools.Method(typeof(WakeAdjuster), "Awake");
            MethodInfo patchWake = AccessTools.Method(typeof(ParawPatches), "WakeObjectPatch");
            harmony.Patch(originalWake, new HarmonyMethod(patchWake));

            //make sure the Paraw is dropped in the right place in the Kicia Shipyard, add sails to the shipyard
            MethodInfo originalShipyardPos = AccessTools.Method(typeof(Shipyard), "Awake");
            MethodInfo patchShipyardPos = AccessTools.Method(typeof(ParawPatches), "ShipyardPatch");
            harmony.Patch(originalShipyardPos, new HarmonyMethod(patchShipyardPos));

            //add modded sails to the Directory
            MethodInfo originalModSail = AccessTools.Method(typeof(PrefabsDirectory), "Start");
            MethodInfo patchModSail = AccessTools.Method(typeof(ParawPatches), "AddModdedSail");
            harmony.Patch(originalModSail, null, new HarmonyMethod(patchModSail));

            //CONDITIONAL PATCHES
            if (!saveCleanerConfig.Value)
            {
                //load the paraw and all assets
                MethodInfo originalFloating = AccessTools.Method(typeof(FloatingOriginManager), "Start");
                MethodInfo patchFloating = AccessTools.Method(typeof(ParawPatches), "StartPatch");
                harmony.Patch(originalFloating, new HarmonyMethod(patchFloating));

                //patch to attach initial sails
                MethodInfo originalSail = AccessTools.Method(typeof(Mast), "Start");
                MethodInfo patchSail = AccessTools.Method(typeof(ParawPatches), "SailSetupPatch");
                harmony.Patch(originalSail, new HarmonyMethod(patchSail));

                //Patch to move sails to position
                MethodInfo originalSail2 = AccessTools.Method(typeof(Mast), "AttachInitialSail");
                MethodInfo patchSail2 = AccessTools.Method(typeof(ParawPatches), "SailSetupPatch2");
                harmony.Patch(originalSail2, null, new HarmonyMethod(patchSail2));
            }
            else
            {   //clean the save
                MethodInfo originalLoad = AccessTools.Method(typeof(SaveLoadManager), "LoadGame");
                MethodInfo patchLoad = AccessTools.Method(typeof(SaveCleaner), "CleanSave");
                harmony.Patch(originalLoad, new HarmonyMethod(patchLoad));
            }
        }

        //DEBUG:
        public void Update()
        {   //debugging methods
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                StartCoroutine(QuickStart());
            }
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                ShowWalkCols();
            }
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                FindBelow();
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                ControlEngine(1);
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                ControlEngine(-1);
            }
            if (Input.GetKeyDown(KeyCode.KeypadMultiply))
            {
                ControlEngine(0);
            }
        }
        private void ControlEngine(int val)
        {
            BoatProbes probes = GameState.currentBoat?.GetComponentInParent<BoatProbes>();

            FieldInfo fieldInfo = AccessTools.Field(typeof(BoatProbes), "_enginePower");
            float power = (float)fieldInfo.GetValue(probes);
            if (val == 0)
            {
                fieldInfo.SetValue(probes, 0f);
            }
            else
            {
                power += val * 0.1f;
                fieldInfo.SetValue(probes, power);
            }
        }
        private void FindBelow()
        {   //debugging method to find the object below the player
            Transform player = Refs.ovrController.transform;
            Debug.LogWarning("ovrController:" + player.name);
            Debug.LogWarning("ovrController.parent:" + player.parent.name);

            if (Physics.Raycast(player.position, Vector3.down, out RaycastHit raycastHit, 1.25f))
            {
                Debug.LogWarning("hit: " + raycastHit.collider.name);
            }
        }
        private void ShowWalkCols()
        {   //debug method to show the walk cols

            Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("WalkCols");
        }
        private IEnumerator QuickStart()
        {   //debug method to quickly get to Kicia or only enable god mode if the boat is already bought instead

            PlayerNeeds.food = 100;
            PlayerNeeds.water = 100;
            PlayerNeeds.sleep = 100;
            PlayerNeeds.instance.godMode = true;
            Sun.sun.globalTime = 8f;
            PurchasableBoat[] boats = FindObjectsOfType<PurchasableBoat>();
            foreach (PurchasableBoat boat in boats)
            {
                if (boat.name == "FFL Paraw" && boat.isPurchased()) yield break;
            }
            PlayerGold.currency[0] = 1000000;
            PlayerGold.currency[1] = 1000000;
            PlayerGold.currency[2] = 1000000;
            PlayerGold.currency[3] = 1000000;

            GameState.recovering = true;
            Port.ports[22].teleportPlayer = true;
            yield return new WaitForSeconds(1f);
            GameState.recovering = false;
        }
    }
}