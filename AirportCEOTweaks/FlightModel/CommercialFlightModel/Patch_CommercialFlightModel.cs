using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(CommercialFlightModel))]
    static class Patch_CommercialFlightModel
    {
        [HarmonyPatch("FinalizeFlightDetails")]
        [HarmonyPostfix]
        public static void PostfixFinalizeFlightDetails(ref CommercialFlightModel __instance)
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            __instance.SetFlightPassengerTrafficValues(1);
            ecfm.FinalizeFlightDetails();
        }

        [HarmonyPatch("SetFlightPassengerTrafficValues")]
        public static void Postfix(CommercialFlightModel __instance, float __0)
        {
            if (__instance.isAllocated)
            {
                int inhr = __instance.arrivalTimeDT.Hour;
                int outhr = __instance.departureTimeDT.Hour;

                long seed = __instance.arrivalTimeDT.ToBinary() + __instance.departureRoute.routeNbr;
                UnityEngine.Random.InitState((int)seed);

                float mult = UnityEngine.Random.Range(.3f, 1.0f) + UnityEngine.Random.Range(.6f, 1.33f); //(0.66 +/- 0.33) + ( 1.0 +/- 0.33) /2
                float mult2 = UnityEngine.Random.Range(.3f, 1.0f) + UnityEngine.Random.Range(.6f, 1.33f);

                mult += (float)(0.70 - 0.01986964 * inhr + 0.009644035 * Math.Pow((double)inhr, 2d) - 0.0003787513 * Math.Pow((double)inhr, 3d));
                mult2 += (float)(0.70 - 0.01986964 * outhr + 0.009644035 * Math.Pow((double)outhr, 2d) - 0.0003787513 * Math.Pow((double)outhr, 3d));

                mult /= 3;
                mult2 /= 3;

                mult = mult.Clamp(0f, 1f);
                mult2 = mult2.Clamp(0f, 1f);

                __instance.currentTotalNbrOfArrivingPassengers = ((float)__instance.totalNbrOfArrivingPassengers * mult * __0).RoundToIntLikeANormalPerson();
                __instance.currentTotalNbrOfDepartingPassengers = ((float)__instance.totalNbrOfDepartingPassengers * mult2 * __0).RoundToIntLikeANormalPerson();
            }
            else
            {
                __instance.currentTotalNbrOfArrivingPassengers = __instance.totalNbrOfArrivingPassengers;
                __instance.currentTotalNbrOfDepartingPassengers = __instance.totalNbrOfDepartingPassengers;
            }
        }

        [HarmonyPatch("SetFromSerializer")]
        [HarmonyPostfix]
        public static void Patch_AddExtensionsOnLoad(ref CommercialFlightModel __instance)
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            __instance.SetFlightPassengerTrafficValues(1);
        }

        [HarmonyPatch("CancelFlight")]
        [HarmonyPostfix]
        public static void Patch_RemoveExtensionCan(CommercialFlightModel __instance)
        {
            try
            {
                Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                ecfm.CancelFlight();
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | Error: Failed to remove commercial flight model extension from the dictionary and scope on flight cancel!");
            }
        }

        [HarmonyPatch("CompleteFlight")]
        [HarmonyPostfix]
        public static void Patch_RemoveExtensionCom(CommercialFlightModel __instance)
        {
            try
            {
                Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                ecfm.CompleteFlight();
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | Error: Failed to remove commercial flight model extension from the dictionary and scope on flight complete!");
            }

        }

        [HarmonyPatch("AllocateFlight")]
        [HarmonyPostfix]
        public static void Patch_RefreshSeriesOnAllocate(ref CommercialFlightModel __instance)
        {
            __instance.SetFlightPassengerTrafficValues(1);
            try
            {
                Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                if (ecfm == default(Extend_CommercialFlightModel)) { Debug.LogError("nullecm"); }
                ecfm.RefreshSeriesLen();
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | Error: Failed to refresh commercial flight model extension on flight allocate!");
            }
        }
        [HarmonyPatch("ActivateFlight")]
        [HarmonyPrefix]
        public static void Patch_RefreshOnActivate(CommercialFlightModel __instance)
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);

            ecfm.RefreshServices();
        }
        //CompleteAllTurnaroundActivities()
        [HarmonyPatch("CompleteAllTurnaroundActivities")]
        [HarmonyPrefix]
        public static void Patch_FailIncomplete(CommercialFlightModel __instance)
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            ecfm.RefreshServices(true);
        }
    }


    [HarmonyPatch(typeof(FlightModel))]
    static class Patch_FlightModel
    {
        [HarmonyPrefix]
        [HarmonyPatch("ShouldActivateFlight")] //Disable auro flight actvation until 2 hours prior to arrival to give room for new flight activation rules
        public static bool Prefix_ShouldActivateFlight(DateTime currentTime, ref bool __result, FlightModel __instance)
        {
            __result = __instance.arrivalTimeDT - currentTime < new TimeSpan(2, 0, 0);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("TimeUntilFlightActivation")] //calculate based on takeoff time
        public static bool Prefix_TimeUntilFlightActivation(DateTime currentTime, ref TimeSpan __result, FlightModel __instance)
        {
            try
            {
                __result = FlightModelExtensionMethods.TakeoffDateTime(__instance, out TimeSpan t, 0f, 99f) - currentTime.AddMinutes(-10);
                return false;
            }
            catch
            { return true; }
        }
        [HarmonyPostfix]
        [HarmonyPatch("SetFromSerializer")]
        public static void Postfix_SetFromSerializer(FlightModel __instance)
        {
            
            if (__instance.isAllocated == false)
            {
                __instance.CancelFlight(true);
            }

            
        }

    }
}