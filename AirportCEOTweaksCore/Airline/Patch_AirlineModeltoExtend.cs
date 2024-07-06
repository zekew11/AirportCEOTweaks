using System;
using System.Collections;
using UnityEngine;
using HarmonyLib;
using BepInEx;

namespace AirportCEOFlightLimitTweak
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
			__instance.Extend(ref __instance);
			//if (__instance as AirlineModelExtended != null) { ((AirlineModelExtended)__instance).Refresh(); return false; }
			//else { Debug.LogError("AirlineModelExtended turned null before could refresh"); return true; }
			//return false;
		}

		[HarmonyPatch("GenerateFlight")]
		[HarmonyPostfix]
		public static void Patch_GenerateFlight(ref AirlineModel __instance)
		{
			if (__instance as AirlineModelExtended != null)
			{
				((AirlineModelExtended)__instance).Refresh();
				return;// true;
			}
			Debug.Log("Patch to extend " + __instance.businessName + " is triggered (Generate Flight)");
			__instance.Extend(ref __instance);
			//if (__instance as AirlineModelExtended != null) { ((AirlineModelExtended)__instance).Refresh(); return false; }
			//else { Debug.LogError("AirlineModelExtended turned null before could refresh"); return true; }
			//return false;
		}
	}
}