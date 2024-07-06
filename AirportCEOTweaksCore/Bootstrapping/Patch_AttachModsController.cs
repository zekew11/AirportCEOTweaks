using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using HarmonyLib;

namespace AirportCEOFlightLimitTweak
{
    [HarmonyPatch(typeof(GameController))]
    static class Patch_AttachModsController 
    {
        [HarmonyPatch("Awake")]
        public static void Postfix()
        {
			GameObject attachto = UnityEngine.GameObject.Find("CoreGameControllers");
			ModsController modsController = attachto.AddComponent<ModsController>();
			if (modsController == null) { Debug.LogError("ACEOTweaks | ERROR: " + "Null modsController in Patch_QueMods"); }
			else { Debug.LogError("ACEOTweaks | Debug: Mods controller object is named " + modsController.name); }
		}
    }
}
