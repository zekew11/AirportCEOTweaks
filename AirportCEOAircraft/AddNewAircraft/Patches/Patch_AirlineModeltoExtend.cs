using System;
using System.Collections;
using UnityEngine;
using HarmonyLib;
using BepInEx;

namespace AirportCEOAircraft
{
	[HarmonyPatch(typeof(AirlineModel))]
	static class Patch_AirlineModeltoExtend
	{
		[HarmonyPatch(typeof(AirlineModel), MethodType.Constructor, new Type[] { typeof(Airline) })]
		[HarmonyPrefix]
		public static bool Patch_Ctor(AirlineModel __instance)
		{
			if (__instance is AirlineModelExtended)
            {
				//((AirlineModelExtended)__instance).Refresh();
				return true;
            }
			Debug.Log("Patch to extend " + __instance.businessName + " is triggered (ctor)");
			AirlineModelExtended ex = __instance.Extend(ref __instance);
			if (ex != null) { ex.Refresh(); return false; }
			else { Debug.LogError("AirlineModelExtended turned null before could refresh"); return true; }
		}
	}
}