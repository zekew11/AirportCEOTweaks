using UnityEngine;
using BepInEx;
using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace AirportCEOFlightLimitTweak
{

    [BepInPlugin(GUID,PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("org.airportceomodloader.humoresque")]
    [BepInIncompatibility("org.airportceotweaks.zeke")]
    public class AirportCEOFlightLimitTweak : BaseUnityPlugin
    {
        public const string GUID = "org.airportceoflightlimittweak.zeke";

        public static AirportCEOFlightLimitTweak Instance { get; private set; }
        internal static Harmony Harmony { get; private set; }
        internal static ManualLogSource TweaksLogger { get; private set; }
        internal static ConfigFile ConfigReference { get; private set; }

        private void Awake()
        {
            Logger.LogInfo($"Plugin {GUID} is loaded!");
            Harmony = new Harmony(GUID);
            Harmony.PatchAll();

            Instance = this;
            TweaksLogger = Logger;
            ConfigReference = Config;

            // Config
            Logger.LogInfo($"{GUID} is setting up config.");
            AirportCEOFlightLimitTweakConfig.SetUpConfig();
            Logger.LogInfo($"{GUID} finished setting up config.");

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