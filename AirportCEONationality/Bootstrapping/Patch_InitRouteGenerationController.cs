using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using HarmonyLib;

namespace AirportCEONationality
{
    [HarmonyPatch(typeof(TravelController))]
    static class Patch_InitRouteGenerationController 
    {
        [HarmonyPatch("GenerateTravelData")]
        public static void Postfix()
        {
			GameObject attachedto = UnityEngine.GameObject.Find("CoreGameControllers");
			RouteGenerationController routeGenerationController = attachedto.GetComponent<RouteGenerationController>();
			if (routeGenerationController == null) { Debug.LogError("ACEOTweaks | ERROR: " + "Null routeGenerationControllerController in Patch_InitRouteGenerationController"); }

            routeGenerationController.Init(TravelController.airports, TravelController.cities, TravelController.countries, TravelController.continents);
		}
    }
}
