using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using AirportCEOTweaksCore;
using UnityEngine;

namespace AirportCEORunways
{
    [HarmonyPatch(typeof(RunwayUI))]
    class Patch_RunwwayUI
    {
        [HarmonyPostfix]
        [HarmonyPatch("LoadButtonsAndMessages")]
        public static void Patch_ExtendUI(RunwayModel runway, ref RunwayUI __instance)
        {
            
            RunwayModelExtended runwayext = (RunwayModelExtended)runway.ExtendMono<RunwayModel,RunwayModelExtended>(ref runway);
            RunwayUIExtended uIExtended = (RunwayUIExtended)__instance.ExtendMono<RunwayUI,RunwayUIExtended>(ref __instance);
            
            if (uIExtended == null)
            {
                Debug.LogError("ACEORunways | Failed to extend a runway ui - ui == null");
            }
            if (runwayext == null)
            {
                Debug.LogError("ACEORunways | Failed to extend a runway ui - runway==null");
            }

            uIExtended.UpdateText(runwayext);

            return;
        }
    }
}
