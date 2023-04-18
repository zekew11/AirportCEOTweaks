using System;
using System.Collections;
using UnityEngine;
using HarmonyLib;

namespace AirportCEOTweaks
{
	[HarmonyPatch(typeof(AirTrafficController))]
	static class Patch_AirTrafficControllerToAddNewPrefabs
	{
		[HarmonyPostfix]
		[HarmonyPatch("Awake")]
		public static void Patch_AddPrefabs(AirTrafficController __instance)
		{
			AircraftAdder adder = __instance.gameObject.AddComponent<AircraftAdder>();
			adder.Initilize();
			//Debug.Log("ACEO Tweaks | Log : added aircraft adder");
		}
	}
}