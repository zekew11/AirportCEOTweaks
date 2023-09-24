using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace AirportCEOTweaks
{
    public static class Patch_StandDevelopmentTools
    {
        [HarmonyPatch(typeof(DevelopmentConsoleController), "ProcessConsoleInput")]
        public static void Prefix(DevelopmentConsoleController __instance, string consoleInput)
        {
            //TBI
        }
    }
}
