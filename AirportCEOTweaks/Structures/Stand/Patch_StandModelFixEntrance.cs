using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;

namespace AirportCEOTweaks
{
	[HarmonyPatch(typeof(StandModel))]
	class Patch_StandModelFixEntrance
    {
        [HarmonyPatch("GetEntryExitPoint")]
        [HarmonyPrefix]
        public static bool Patch_AllowMedNoBaggage(ref StandModel __instance)
        {
            return true;
        }
    }
}
