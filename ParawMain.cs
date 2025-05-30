using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
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

        public void Awake()
        {
            //CONFIG FILE
            saveCleanerConfig = Config.Bind("A) Various Settings", "Save Cleaner", false, "Removes the saves dependency on this mod. Only use if you want to remove the mod from an ongoing save! Change to true (with the game closed), open the game → load the save → save → close the game → remove the mod → done. A save backup is recommended.");
            
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

            //make sure the Paraw is dropped in the right place in the Kicia Shipyard
            MethodInfo originalShipyardPos = AccessTools.Method(typeof(Shipyard), "DischargeShip");
            MethodInfo patchShipyardPos = AccessTools.Method(typeof(ParawPatches), "ShipyardPosPatch");
            harmony.Patch(originalShipyardPos, new HarmonyMethod(patchShipyardPos));

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
        private void FindBelow()
        {   //debugging method to find the object below the player
            Transform player = Refs.ovrController.transform;
            Debug.LogWarning("ovrController:" + player.name);
            Debug.LogWarning("ovrController.parent:" + player.parent.name);

            RaycastHit raycastHit;
            if (Physics.Raycast(player.position, Vector3.down, out raycastHit, 1.25f))
            {
                Debug.LogWarning("hit: " + raycastHit.collider.name);
            }
        }
        public void ShowWalkCols()
        {   //debug method to show the walk cols

            Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("WalkCols");
        }
    }
}