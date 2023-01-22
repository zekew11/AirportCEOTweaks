using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(AircraftController))]
    static class Patch_AircraftController
    {

        [HarmonyPostfix]
        [HarmonyPatch("LaunchAircraft")]
        public static void Patch_EvaluateNightLightHide(AircraftController __instance)
        {
            if (AirportCEOTweaksConfig.cargoSystem == false) { return; }

            if (__instance.FlightModel == null)
            {
                return;
            }

            if (__instance.FlightModel is CommercialFlightModel)
            {
                CommercialFlightModel f = __instance.FlightModel as CommercialFlightModel;
                //Debug.Log("ACEO Tweaks | Debug: Launch Aircraft is Commercial");
                if (f.currentTotalNbrOfArrivingPassengers==0 && f.currentTotalNbrOfDepartingPassengers==0)
                {
                    NightLightHide n;
                    if (__instance.gameObject.TryGetComponent<NightLightHide>(out n))
                    {
                        Debug.LogError("ACEO Tweaks: ERROR | Tried adding anouther nightlight hide in Aircraft Controller Patch 'Launch Aircraft'");
                    }
                    else
                    {
                        __instance.gameObject.AddComponent<NightLightHide>();
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AircraftController), "SetLivery", new Type[] { })]
        public static void Patch_LiveryAddActive(AircraftController __instance)
        {
            //if (AirportCEOTweaksConfig.liveryExtensions == false) { return; }
            try
            {
                Transform liveryTransform = __instance.Transform.Find("Sprite").Find("Livery").GetChild(0);
                GameObject liveryGameObject = liveryTransform.gameObject;
                Livery livery = liveryGameObject.GetComponent<Livery>();
                List<GameObject> componentGameObjects = new List<GameObject>();
                
                for (int i = 0;   i<liveryTransform.childCount; i++)
                {
                    componentGameObjects.Add(liveryTransform.GetChild(i).gameObject);
                }

                LiveryActiveComponent lac = __instance.gameObject.GetComponent<LiveryActiveComponent>();

                if (lac == null)
                {
                    lac = __instance.gameObject.AddComponent<LiveryActiveComponent>();
                }

                foreach (GameObject obj in componentGameObjects)
                {
                    lac.DoLiveryComponentActions(obj);
                }
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | Error: failed to postfix SetLivery in Aircraft Controller");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ChargeFlightIncome")]
        public static bool Patch_FlightIncome(ref AircraftController __instance)
        {
            if (!AirportCEOTweaksConfig.flightTypes)
            {
                return true;
            }
            if (__instance.FlightModel == null)
            {
                return false;
            }
            CommercialFlightModel cfm;

            if ((cfm = (__instance.FlightModel as CommercialFlightModel)) != null && cfm.Airline != null)
            {
                Singleton<ModsController>.Instance.GetExtensions(cfm, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);

                float val = eam.PaymentPerFlight(ecfm,cfm.Airline.GetPaymentPerFlight(cfm.weightClass)) * ecfm.GetPaymentPercentAndReportRating();

                if (!__instance.IsTotaled)
                {
                    Singleton<EconomyController>.Instance.CollectFlightCompletionFee<AircraftController>(val, __instance);
                }
            }    
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("ResetLivery")]
        public static bool Patch_LetParentScaleLiv(GameObject livery, Transform ___liveryTransform)
        {
            if (livery != null)
            {
                livery.transform.SetParent(___liveryTransform, false);
                livery.transform.localPosition = Vector3.zero;
                livery.transform.localEulerAngles = Vector3.zero;
            }
            return false;
        }
    }
}
   

