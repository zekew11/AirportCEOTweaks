using UnityEngine;
using HarmonyLib;
using System;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(RunwayModel))]
    static class Patch_StructureRepairRunway
    {
        [HarmonyPatch("UseObject")]
        public static void Prefix(RunwayModel __instance)
        {
            __instance.repairAt = AirportCEOTweaksConfig.structureRepairLevel;
        }
    }

    [HarmonyPatch(typeof(StandModel))]
    static class Patch_StructureRepairStand
    {
        [HarmonyPatch("RepairAt", MethodType.Getter)]
        public static bool Prefix(StandModel __instance, ref float __result)
        {
            __result = AirportCEOTweaksConfig.structureRepairLevel;
            return false;
        }
    }
}