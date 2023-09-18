using UnityEngine;
using UModFramework.API;
using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;

namespace AirportCEOTweaks
{
    class AirportCEOTweaks : MonoBehaviour
    {
        public static List<string> aircraftPaths = new List<string>();
        public static List<string> airlinePaths = new List<string>();
        public static Dictionary<GameObject,GameObject> aircraftPrefabOverwrites = new Dictionary<GameObject,GameObject>();
        public static Dictionary<string, AircraftTypeData> aircraftTypeDataDict = new Dictionary<string, AircraftTypeData>();
        
        internal static void Log(string text, bool clean = false)
        {
            using (UMFLog log = new UMFLog()) log.Log(text, clean);
        }

        [UMFConfig]
        public static void LoadConfig()
        {
            AirportCEOTweaksConfig.Load();
        }
        static void Awake()
        {
            Log("ACEO Tweaks v" + UMFMod.GetModVersion().ToString(), true);

            GameObject child = Instantiate(new GameObject());
            child.transform.SetParent(null);
            child.name = "ACEOTweaksActive";
        }

        [UMFHarmony(63)] //Set this to the number of harmony patches in your mod.
        public static void Start()
		{
			Log("AirportCEOTweaks v" + UMFMod.GetModVersion().ToString(), true);
        }
	}
}