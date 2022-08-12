using System;
using UnityEngine;
using HarmonyLib;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(StandModel))]
    static class Patch_StandModelFixes
    {

        [HarmonyPostfix]
        [HarmonyPatch("ShouldRequestPushback")]
        public static void Patch_Pushback(StandModel __instance, ref bool __result)
        {
            if (AirportCEOTweaksConfig.fixes == false && AirportCEOTweaksConfig.cargoSystem == false) { return; }

            if (__instance.CurrentFlight is CommercialFlightModel)
            {
                TimeSpan timeSpan = __instance.CurrentFlight.departureTimeDT - Singleton<TimeController>.Instance.GetCurrentContinuousTime();
                if (timeSpan.TotalMinutes > 25)
                {
                    __result = false;
                }
            }
        }
    }
}


