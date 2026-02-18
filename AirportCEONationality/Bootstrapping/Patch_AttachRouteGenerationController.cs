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
    [HarmonyPatch(typeof(GameController))]
    static class Patch_AttachRouteGenerationController 
    {
        [HarmonyPatch("Awake")]
        public static void Postfix()
        {
			GameObject attachto = UnityEngine.GameObject.Find("CoreGameControllers");
			RouteGenerationController routeGenerationController = attachto.AddComponent<RouteGenerationController>();
			if (routeGenerationController == null) { Debug.LogError("ACEOTweaks | ERROR: " + "Null routeGenerationControllerController in Patch_AttachRouteGenerationController"); }
			else { Debug.Log("ACEOTweaks | Debug: RouteGenerationController controller object is named " + routeGenerationController.name); }
		}
    }
}
