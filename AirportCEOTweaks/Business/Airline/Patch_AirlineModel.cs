using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(AirlineModel))]
    static class Patch_AirlineModel
    {
        [HarmonyPatch(typeof(AirlineModel), MethodType.Constructor, new Type[] { typeof(Airline) })]
        [HarmonyPostfix]
        public static void Postfix(Airline airline, AirlineModel __instance)
        {
            if (__instance == null)
            {
                return;
            }
            Singleton<ModsController>.Instance.GetExtensions(__instance, out _);
        }
        [HarmonyPatch("SetFromSerializer")]
        [HarmonyPostfix]
        public static void PostfixSetExtensionFromSave(AirlineModel __instance)
        {
            if (__instance==null)
            {
                return;
            }
            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_AirlineModel eam);

            __instance.aircraftFleetModels = eam.FleetModels;
        }

        [HarmonyPatch("GenerateFlight")]
        [HarmonyPrefix]
        public static bool Prefix(ref bool isEmergency, ref bool isAmbulance, AirlineModel __instance)
        {

            if (isAmbulance || isEmergency)
            {
                return true;
            }

            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_AirlineModel eam);
            if (eam == null)
            {
                return false;
            }

            for (int i = 0; i<AirportCEOTweaksConfig.FlightGenerationMultiplyer.Value;i++)
            {
                eam.GenerateFlight(__instance, isEmergency, isAmbulance);
            }

            return false;
        }


        [HarmonyPatch("CountAllFlights")]
        [HarmonyPrefix]
        public static bool HackFlightCount(ref AirlineModel __instance)
        {
            HashSet<string> unAllocatedNbrs = new HashSet<string>();
            __instance.UnAllocatedCount = 0;
            __instance.AllocatedCount = 0;
            __instance.ActiveCount = 0;
            foreach (CommercialFlightModel commercialFlightModel in __instance.flightListObjects)
            {
                if (commercialFlightModel.isCanceled || commercialFlightModel.isCompleted)
                {
                    continue;
                }
                if (!commercialFlightModel.isAllocated) //not allocated
                {
                    if(unAllocatedNbrs.Add(commercialFlightModel.departureFlightNbr))
                    {
                        __instance.UnAllocatedCount = __instance.UnAllocatedCount + 1;
                    }
                }
                if (commercialFlightModel.isAllocated) //allocated
                {
                    if (commercialFlightModel.arrivalTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime().AddDays(3)) //3+ days away
                    {
                        continue;
                    }
                    else
                    {
                        __instance.AllocatedCount = __instance.AllocatedCount + 1;
                    }
                }
                if (commercialFlightModel.isActivated) //activated
                {
                    __instance.ActiveCount = __instance.ActiveCount + 1;
                }
            }

            return false;
        }
    }
}