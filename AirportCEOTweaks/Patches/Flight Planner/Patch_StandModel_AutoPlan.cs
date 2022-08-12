using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace AirportCEOTweaks
{
    [HarmonyPatch]
    static class Patch_StandModel_AutoPlan
    {

        [HarmonyPatch(typeof(StandModel), "ChangeToBuilt")]
        public static void Postfix(StandModel __instance)
        {
           if (AirportCEOTweaksConfig.airlineChanges == false && AirportCEOTweaksConfig.fixes == false) { return; }

            __instance.autoPlan = false;
        }

    }
   
}