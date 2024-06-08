using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AirportCEOAircraft
{
    [HarmonyPatch(typeof(LiveryController))]
    static class Patch_LiveryControllerToSupportNewAircraft
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddLivery")]
        private static bool EnsureTypeExists(string aircraftType, LiveryController __instance)
        {
            foreach (AirlineLivery airlineLivery in __instance.allLiveriesList)
            {
                if (airlineLivery.aircraftType == aircraftType)
                {
                    return true;  //if we have a list for that type then no action needed
                }
            }

            //if we got here then we don't have a livery list for that aircraft. Make one.

            AirlineLivery newAirlineLivery = new AirlineLivery();
            newAirlineLivery.aircraftType = aircraftType;
            newAirlineLivery.liveries = new List<UnityEngine.GameObject>();
            newAirlineLivery.SetDefaultCount();

            __instance.allLiveriesList.Add(newAirlineLivery);

            return true;
        }
    }
}
