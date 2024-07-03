using UnityEngine;
using BepInEx;
using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace AirportCEOTweaksCore
{

    [BepInPlugin("org.airportceotweakscore.zeke","TweaksCore", "1.0")]
    [BepInDependency("org.airportceomodloader.humoresque")]
    [BepInIncompatibility("org.airportceotweaks.zeke")]
    public class AirportCEOTweaksCore : BaseUnityPlugin
    {
        public static List<string> aircraftPaths = new List<string>();
        public static List<string> airlinePaths = new List<string>();
        public static Dictionary<GameObject, GameObject> aircraftPrefabOverwrites = new Dictionary<GameObject, GameObject>();
        public static Dictionary<string, AircraftTypeData> aircraftTypeDataDict = new Dictionary<string, AircraftTypeData>();

        public static AirportCEOTweaksCore Instance { get; private set; }
        internal static Harmony Harmony { get; private set; }
        internal static ManualLogSource TweaksLogger { get; private set; }
        internal static ConfigFile ConfigReference { get; private set; }

        private void Awake()
        {
            Logger.LogInfo($"Plugin {"org.airportceotweakscore.zeke"} is loaded!");
            Harmony = new Harmony("org.airportceotweakscore.zeke");
            Harmony.PatchAll();

            Instance = this;
            TweaksLogger = Logger;
            ConfigReference = Config;

            // Config
            Logger.LogInfo($"{"org.airportceotweakscore.zeke"} is setting up config.");
            AirportCEOTweaksCoreConfig.SetUpConfig();
            Logger.LogInfo($"{"org.airportceotweakscore.zeke"} finished setting up config.");

            GameObject child = Instantiate(new GameObject());
            child.transform.SetParent(null);
            child.name = "ACEOTweaksActive";
        }

        private void Start()
        {
            ModLoaderInteractionHandler.SetUpInteractions();
            LogInfo("Tweaks finished start");
        }

        // This is code for BepInEx logging, which Tweaks doesn't really use. Here if nessesary
        internal static void Log(string message) => LogInfo(message);
        internal static void LogInfo(string message) => TweaksLogger.LogInfo(message);
        internal static void LogError(string message) => TweaksLogger.LogError(message);
        internal static void LogWarning(string message) => TweaksLogger.LogWarning(message);
        internal static void LogDebug(string message) => TweaksLogger.LogDebug(message);
    }
}