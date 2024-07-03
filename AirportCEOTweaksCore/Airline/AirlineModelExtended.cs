using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using Unity;
using HarmonyLib.Tools;

namespace AirportCEOTweaksCore
{
    public class AirlineModelExtended : AirlineModel
    {
        AirlineModel asBaseModel;
        AirlineBusinessData airlineBusinessData;
        public AirlineModelExtended(Airline airline, ref AirlineModel airlineModel) : base(airline)
        {
            if (airline == null) { Debug.LogError("ERROR: Airline Model Extended ctor encountered airline == null!");  return; }
            if (Singleton<ModsController>.Instance == null) { Debug.LogError("ERROR: Airline Model Extended ctor encountered ModsController == null!"); return; }

            Debug.Log("AirlineModelExtended for " + businessName + " is ctor-ing");

            Singleton<BusinessController>.Instance.RemoveFromBusinessList(this);

            ConsumeBaseAirlineModel(airlineModel);

            if (Singleton<ModsController>.Instance.airlineBusinessDataByBusinessName.TryGetValue(businessName, out AirlineBusinessData data))
            {
                airlineBusinessData = data;
            } 

            Singleton<BusinessController>.Instance.RemoveFromBusinessList(airlineModel);
            airlineModel = this;
            Singleton<BusinessController>.Instance.RemoveFromBusinessList(this);
            Singleton<BusinessController>.Instance.AddToBusinessList(this);


            MakeUpdateTypeModelDictionary();

        }

        public void Refresh()
        {
            MakeUpdateTypeModelDictionary();
        }
        private void ConsumeBaseAirlineModel(AirlineModel airlineModel)
        {
            foreach (var field in typeof(AirlineModel).GetFields(HarmonyLib.AccessTools.all))
            {
                field.SetValue(this, field.GetValue(airlineModel));
            }
        }
        private void MakeUpdateTypeModelDictionary()
        {
            //Replace Fleet with TweaksFleet
            if (airlineBusinessData.tweaksFleet == null || airlineBusinessData.tweaksFleet.Length <= 0)
            {
                Debug.Log("ACEO Tweaks | Debug - Airline " + businessName + " tweaksFleet is null or 0");
                
                if (airlineBusinessData.fleet != null && airlineBusinessData.fleet.Length > 0)
                {
                    aircraftFleetModels = airlineBusinessData.fleet;
                    Debug.Log("updated a non-tweaksFleet fleet");
                }
            }
            else
            {
                aircraftFleetModels = airlineBusinessData.tweaksFleet;
                //Debug.Log("ACEO Tweaks | Debug - Airline " + businessName + " tweaksFleet length > 0");

                if (airlineBusinessData.tweaksFleetCount != null && airlineBusinessData.tweaksFleetCount.Length == aircraftFleetModels.Length)
                {
                    fleetCount = airlineBusinessData.tweaksFleetCount;
                }
            }

            // Create a fleet counts if none exists ........................................................................................

            if (airlineBusinessData.tweaksFleetCount != null)
            {
                if (fleetCount == null || fleetCount.Length != aircraftFleetModels.Length)
                {
                    fleetCount = new int[aircraftFleetModels.Length];
                    for (int i = 0; i < fleetCount.Length; i++)
                    {
                        fleetCount[i] = 2 * ((int)businessClass);
                    }

                    airlineBusinessData.tweaksFleetCount = fleetCount;
                }
            }
        }
    }
}
