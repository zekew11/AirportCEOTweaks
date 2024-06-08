using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;




namespace AirportCEOAircraft
{
    [HarmonyPatch(typeof(VehicleController))]
    static class Patch_VehicleController_ForNewScale
    {
        [HarmonyPatch("Launch")]
        [HarmonyPostfix]
        public static void Postfix(VehicleController __instance)
        {
            float scale = 1f;
            switch (__instance.VehicleType)
            {
                case Enums.VehicleType.Bus:
                case Enums.VehicleType.ContractorShuttle: scale = 0.95f; break;
                
                case Enums.VehicleType.FuelTruck:
                case Enums.VehicleType.FuelTruckAvgas100LL:
                case Enums.VehicleType.FuelTruckJetA1:
                case Enums.VehicleType.StairTruck:
                case Enums.VehicleType.AirsideShuttleBus:
                case Enums.VehicleType.FluidSupplyTruck: scale = .9f; break;

                case Enums.VehicleType.Ambulance:
                case Enums.VehicleType.AirportPoliceCar:
                case Enums.VehicleType.ServiceCar:
                case Enums.VehicleType.MiniBus:
                case Enums.VehicleType.DeicingTruck:
                case Enums.VehicleType.AircraftCabinCleaningTruck:
                case Enums.VehicleType.LargePushbackTruck:
                case Enums.VehicleType.LargeBeltLoaderTruck:
                case Enums.VehicleType.PersonCar: scale = .85f; break;

                case Enums.VehicleType.BeltLoaderTruck:
                case Enums.VehicleType.PushbackTruck:
                case Enums.VehicleType.CateringTruck:
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
                    child.localPosition = new Vector3(child.localPosition.x + (1.5f/scale)-1.5f, child.localPosition.y, child.localPosition.z);
                }
            }


        }
    }
}
