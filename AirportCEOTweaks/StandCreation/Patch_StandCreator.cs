using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(SaveLoadGameDataController))]
    public static class Patch_StandCreator
    {
        [HarmonyPatch("StartNewGame")]
        public static void Prefix(SaveLoadGameDataController __instance)
        {
            return;
            //AirportCEOTweaks.Log("initial test");
            //StandCreator.CreateStandMedium();
        }

    }

    class StandCreator
    {
        public static void CreateStandMedium()
        {
            if (!StandCreationUtilities.GetStandTemplateCopy(Enums.ThreeStepScale.Medium, out GameObject stand))
            {
                return;
            }

            StandUICreator.CreateStandUI(stand);
        }

    }
}
