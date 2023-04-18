using System;
using UnityEngine;
using HarmonyLib;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(StandModel))]
    static class Patch_CargoPrematurePushback
    {
        [HarmonyPostfix]
        [HarmonyPatch("ShouldRequestPushback")]
        public static void Patch_Pushback(StandModel __instance, ref bool __result)
        {

            if (__instance.CurrentFlight is CommercialFlightModel && __instance.CurrentCommercialFlight.currentTotalNbrOfDepartingPassengers==0)
            {
                TimeSpan timeSpan = __instance.CurrentFlight.departureTimeDT - Singleton<TimeController>.Instance.GetCurrentContinuousTime();
                if (timeSpan.TotalMinutes > 20)
                {
                    __result = false;
                }
            }
        }
    }
}


