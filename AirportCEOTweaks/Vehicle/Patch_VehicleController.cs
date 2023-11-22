using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;



namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(VehicleController))]
    static class Patch_VehicleController
    {
        [HarmonyPatch("Launch")]
        [HarmonyPostfix]
        public static void Postfix(VehicleController __instance)
        {
            float scale = 1f;
            switch (__instance.VehicleType)
            {
                case Enums.VehicleType.FuelTruck:
                case Enums.VehicleType.FuelTruckAvgas100LL:
                case Enums.VehicleType.FuelTruckJetA1:
                case Enums.VehicleType.FluidSupplyTruck: scale = .9f; break;

                case Enums.VehicleType.Ambulance:
                case Enums.VehicleType.AirportPoliceCar:
                case Enums.VehicleType.StairTruck:
                case Enums.VehicleType.ServiceCar:
                case Enums.VehicleType.MiniBus:
                case Enums.VehicleType.DeicingTruck:
                case Enums.VehicleType.PersonCar: scale = .85f; break;

                case Enums.VehicleType.BeltLoaderTruck:
                case Enums.VehicleType.PushbackTruck:
                case Enums.VehicleType.AircraftCabinCleaningTruck:
                case Enums.VehicleType.CateringTruck:
                case Enums.VehicleType.LargePushbackTruck:
                case Enums.VehicleType.LargeBeltLoaderTruck:
                case Enums.VehicleType.ServiceTruck: scale = .8f; break;
            }

            if (scale == 1f)
            {
                return;
            }

            Transform[] children = __instance.Transform.GetComponentsInChildren<Transform>();

            foreach (Transform child in children)
            {
                if (child.parent ==__instance.Transform)
                {
                    child.localScale = new Vector3(scale, scale, scale);
                }
            }
        }
    }
}
