using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(CheckInDeskController))]
    static class Patch_CheckInDeskController
    {
        [HarmonyPatch("CanAcceptBag")]
        [HarmonyPostfix]
        public static void Patch_NoBagsFlights(CommercialFlightModel flight, CheckInDeskController __instance, ref bool __result)
        {
            if (!AirportCEOTweaksConfig.flightTypes)
            {
                return;
            }
            if (flight.cargoLoadingRequested == false)
            {
                __result = false;
            }
            if (flight.StandIsAssigned && flight.Stand.connectedCargoBayReferenceID == "")
            {
                __result = false;
            }
        }
    }   
}
