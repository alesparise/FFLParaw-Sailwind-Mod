using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace ShipyardLib
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ShipyardLibPatches : BaseUnityPlugin
    {
        public const string pluginGuid = "pr0skynesis.shipyardLib";
        public const string pluginName = "Shipyard Lib";
        public const string pluginVersion = "1.0.0";
        public const string shortName = "pr0.shipyardLib";
        public void Awake()
        {
            Harmony harmony = new Harmony(pluginGuid);
            //initial setup
            MethodInfo setupOg = AccessTools.Method(typeof(FloatingOriginManager), "Start");
            MethodInfo setupP = AccessTools.Method(typeof(ShipyardLibPatches), "Setup");
            harmony.Patch(setupOg, new HarmonyMethod(setupP));

            //enable modded menu
            MethodInfo shipCheckOG = AccessTools.Method(typeof(Shipyard), "ToggleMenu");
            MethodInfo shipCheckP = AccessTools.Method(typeof(ShipyardLibPatches), "EnableCustomShipyard");
            harmony.Patch(shipCheckOG, new HarmonyMethod(shipCheckP));
        }
        public static void Setup() => ShipyardHelpers.Setup();
        public static void EnableCustomShipyard(bool state, Shipyard __instance)
        {
            if (__instance.GetCurrentBoat()?.GetComponent<CustomShipyard>() == null) return;

            ShipyardHelpers.EnableUI();
        }
    }
}
