using UnityEngine;
using BepInEx;
using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using AirportCEOTweaksCore;

namespace AirportCEOAircraft
{

    [BepInPlugin("org.airportceoaircraft.zeke", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("org.airportceomodloader.humoresque")]
    [BepInDependency("org.airportceotweakscore.zeke")]
    [BepInIncompatibility("org.airportceotweaks.zeke")]
    public class AirportCEOAircraft : BaseUnityPlugin
    {
        public static List<string> aircraftPaths = new List<string>();
        public static List<string> airlinePaths = new List<string>();
        public static Dictionary<GameObject, GameObject> aircraftPrefabOverwrites = new Dictionary<GameObject, GameObject>();

        public static AirportCEOAircraft Instance { get; private set; }
        internal static Harmony Harmony { get; private set; }
        internal static ManualLogSource TweaksLogger { get; private set; }
        internal static ConfigFile ConfigReference { get; private set; }

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            Harmony.PatchAll();

            Instance = this;
            TweaksLogger = Logger;
            ConfigReference = Config;

            // Config
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is setting up config.");
            AirportCEOAircraftConfig.SetUpConfig();
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} finished setting up config.");

            GameObject child = Instantiate(new GameObject());
            child.transform.SetParent(null);
            child.name = "ACEOTweaksAircraftActive";
        }

        private void Start()
        {
            ModLoaderInteractionHandler.SetUpInteractions();
            LogInfo("Tweaks Aircraft finished start");
        }

        // This is code for BepInEx logging, which Tweaks doesn't really use. Here if nessesary
        internal static void Log(string message) => LogInfo(message);
        internal static void LogInfo(string message) => TweaksLogger.LogInfo(message);
        internal static void LogError(string message) => TweaksLogger.LogError(message);
        internal static void LogWarning(string message) => TweaksLogger.LogWarning(message);
        internal static void LogDebug(string message) => TweaksLogger.LogDebug(message);
    }
}