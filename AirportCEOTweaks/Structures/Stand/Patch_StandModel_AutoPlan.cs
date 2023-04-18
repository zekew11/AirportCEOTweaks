using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(StandModel))]
    static class Patch_StandModel_AutoPlan
    {

        [HarmonyPatch("ChangeToBuilt")]
        public static void Postfix(StandModel __instance)
        {
            __instance.autoPlan = false;
        }

    }
   
}