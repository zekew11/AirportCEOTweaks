using System;
using System.Collections;
using UnityEngine;
using HarmonyLib;
using BepInEx;

namespace AirportCEOTweaksCore
{
	[HarmonyPatch(typeof(AirlineModel))]
	static class Patch_AirlineModeltoExtend
	{
		[HarmonyPatch(typeof(AirlineModel), MethodType.Constructor, new Type[] { typeof(Airline) })]
		[HarmonyPostfix]
		public static void Patch_Ctor(ref AirlineModel __instance)
		{
			if (__instance as AirlineModelExtended != null)
            {
				//((AirlineModelExtended)__instance).Refresh();
				return;// true;
            }
			Debug.Log("Patch to extend " + __instance.businessName + " is triggered (ctor)");
			__instance.ExtendAirlineModel(ref __instance);
			//if (__instance as AirlineModelExtended != null) { ((AirlineModelExtended)__instance).Refresh(); return false; }
			//else { Debug.LogError("AirlineModelExtended turned null before could refresh"); return true; }
			//return false;
		}

		[HarmonyPatch("GenerateFlight")]
		[HarmonyPostfix]
		public static void Patch_GenerateFlightToExtend(ref AirlineModel __instance)
		{
			if (__instance as AirlineModelExtended != null)
			{
				((AirlineModelExtended)__instance).Refresh();
				return;// true;
			}
			Debug.Log("Patch to extend " + __instance.businessName + " is triggered (Generate Flight)");
			__instance.ExtendAirlineModel(ref __instance);
			//if (__instance as AirlineModelExtended != null) { ((AirlineModelExtended)__instance).Refresh(); return false; }
			//else { Debug.LogError("AirlineModelExtended turned null before could refresh"); return true; }
			//return false;
		}

		[HarmonyPatch("GenerateFlight")]
		[HarmonyPrefix]
		public static bool Patch_GenerateFlight(AirlineModel __instance, bool isEmergency, bool isAmbulance)
		{
			if (Singleton<ModsController>.Instance.flightGenerator.OverrideHarmonyPrefix)
            {
				return true;
            }
			Debug.Log("Generate Flight Prefix for "+ __instance.businessName);
			if (Singleton<ModsController>.Instance.flightGenerator.GenerateFlight(__instance, isEmergency, isAmbulance))
            {
				Debug.Log("Generate Flight was True");
				return false;
            }
			else
            {
				Debug.Log("Generate Flight was False");
				return true;
            }
			
		}
	}
}