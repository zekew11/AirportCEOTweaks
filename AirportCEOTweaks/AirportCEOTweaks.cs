using UnityEngine;
using UModFramework.API;
using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;

namespace AirportCEOTweaks
{
    class AirportCEOTweaks
    {

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
        }

		[UMFHarmony(41)] //Set this to the number of harmony patches in your mod.
        public static void Start()
		{
			Log("AirportCEOTweaks v" + UMFMod.GetModVersion().ToString(), true);
        }
	}
}