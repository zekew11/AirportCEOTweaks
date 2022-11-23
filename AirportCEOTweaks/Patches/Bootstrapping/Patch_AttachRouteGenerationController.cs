using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(TravelController))]
    static class Patch_TravelController
    {
         [HarmonyPostfix]    
         [HarmonyPatch("GenerateDomesticAirports")]
         public static void Attach(Airport[] ___airports, City[] ___cities, Country[] ___countries, Continent[] ___continents)
         {
             Debug.LogError("ACEOTweaks | Debug Patch_AttachRouteGenerationController is running...");
             GameObject attachto = UnityEngine.GameObject.Find("CoreGameControllers");
             RouteGenerationController routeController = attachto.AddComponent<RouteGenerationController>();
             routeController.Init(___airports, ___cities, ___countries, ___continents);
             if (routeController == null) { Debug.LogError("ACEOTweaks | ERROR: " + "Null routeController in Patch_AttachRouteGenerationController"); }
             else { Debug.LogError("ACEOTweaks | Debug: Route Generation Controller object is named " + routeController.name); }
         }
     }
}
