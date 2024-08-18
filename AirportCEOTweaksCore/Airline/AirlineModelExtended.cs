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
using System.Reflection;
using Random = UnityEngine.Random;

namespace AirportCEOTweaksCore
{
    public class AirlineModelExtended : AirlineModel
    {
        AirlineBusinessData airlineBusinessData;
        public AirlineModelExtended(Airline airline, ref AirlineModel airlineModel) : base(airline)
        {
            if (airline == null) { Debug.LogError("ERROR: Airline Model Extended ctor encountered airline == null!");  return; }
            if (Singleton<ModsController>.Instance == null) { Debug.LogError("ERROR: Airline Model Extended ctor encountered ModsController == null!"); return; }

            //Debug.Log("AirlineModelExtended for " + businessName + " is ctor-ing");

            Singleton<BusinessController>.Instance.RemoveFromBusinessList(this);

            ConsumeBaseAirlineModel(airlineModel);

            if (Singleton<ModsController>.Instance.airlineBusinessDataByBusinessName.TryGetValue(businessName, out AirlineBusinessData data))
            {
                airlineBusinessData = data;
            }
            else
            {
                Debug.LogWarning("ACEO Tweaks WARN: No airlinebusinessdata path for "+businessName);
            }

            //Singleton<BusinessController>.Instance.RemoveFromBusinessList(airlineModel);
            airlineModel = this;
            Singleton<BusinessController>.Instance.RemoveFromBusinessList(this);
            Singleton<BusinessController>.Instance.AddToBusinessList(this);


            MakeUpdateFleet();

        }

        public Dictionary<string, (float Available, float Allocated)> AircraftTypeAllocation = new Dictionary<string, (float Available, float Allocated)>();

        public void Refresh()
        {
            MakeUpdateFleet();
        }
        private void ConsumeBaseAirlineModel(AirlineModel airlineModel)
        {
            foreach (var field in typeof(AirlineModel).GetFields(HarmonyLib.AccessTools.all))
            {
                field.SetValue(this, field.GetValue(airlineModel));
            }
        }
        private void MakeUpdateFleet()
        {
            //Replace Fleet with TweaksFleet
            if (airlineBusinessData.tweaksFleet == null || airlineBusinessData.tweaksFleet.Length <= 0)
            {
                Debug.Log("ACEO Tweaks | Debug - Airline " + businessName + " tweaksFleet is null or 0");
                
                if (airlineBusinessData.fleet != null && airlineBusinessData.fleet.Length > 0)
                {
                    List<string> FleetList = airlineBusinessData.fleet.ToList();

                    List<string> AllTypesList = new List<string>();
                    foreach (AircraftModel aircraftModel in Singleton<AirTrafficController>.Instance.aircraftModels)
                    {
                        AllTypesList.Add(aircraftModel.aircraftType);
                    }
                        //Singleton<AirTrafficController>.Instance.aircraftModels.ToList()//(typeof(AirTrafficController).GetField("aircraftModels", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Singleton<AirTrafficController>.Instance)).ToList();

                    for (int i = 0; i < FleetList.Count;)
                    {
                        if (AllTypesList.Contains(FleetList[i]))
                        {
                            i++;
                        }
                        else
                        {
                            FleetList.RemoveAt(i);
                            if (i >= FleetList.Count)
                            {
                                break;
                            }
                        }
                    }

                    aircraftFleetModels = airlineBusinessData.fleet;
                    //Debug.Log("updated a non-tweaksFleet fleet");
                }
            }
            else
            {
                List<string> FleetList = airlineBusinessData.tweaksFleet.ToList();
                List<AircraftModel> AllTypesList = ((AircraftModel[])typeof(AirTrafficController).GetField("aircraftModels", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Singleton<AirTrafficController>.Instance)).ToList();

                int i = 0;
                OuterTweaksLoop:
                for (;i < FleetList.Count;)
                {
                    
                    foreach (AircraftModel aircraftModel in AllTypesList)
                    {
                        if (aircraftModel.aircraftType == FleetList[i] && AirTrafficController.OwnsDLCAircraft(FleetList[i]))
                        {
                            i++;
                            goto OuterTweaksLoop;
                        }
                    }

                    //if we get here it means we processed all types and didn't get a match
                    FleetList.RemoveAt(i);
                    if (i >= FleetList.Count)
                    {
                        break;
                    }

                }
                
                aircraftFleetModels = FleetList.ToArray();

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

        public string GetAndAllocateRandomAircraft(bool allocate = true)
        {
            string aircraft;
            SortedDictionary<float, string> lotto = new SortedDictionary<float, string>();
            float counter = 0;
            foreach (string key in AircraftTypeAllocation.Keys)
            {
                if (Singleton<ModsController>.Instance.CanServeAircraftType(key))
                {
                    float numToAdd = AircraftTypeAllocation[key].Available + counter;
                    lotto.Add(numToAdd, key);
                    counter += AircraftTypeAllocation[key].Available;
                }
            }

            float pick = Random.Range(0, counter);
            
            foreach (float number in lotto.Keys)
            {
                if (number >= pick)
                {
                    aircraft = lotto[number];
                    if (allocate) { AircraftTypeAllocation[aircraft] = (AircraftTypeAllocation[aircraft].Available, AircraftTypeAllocation[aircraft].Allocated + 1); }
                    return aircraft;
                }
            }
            return "ERROR NO AIRCRAFT AVAILABLE";
        }
        private void MakeUpdateFleetAllocations()
        {
            for (int i = 0; i < aircraftFleetModels.Length ; i++)
            {
                string aircraft = aircraftFleetModels[i];
                float count = (i < fleetCount.Length) ? fleetCount[i] : 0f ;
                if (! AircraftTypeAllocation.ContainsKey(aircraft))
                {
                    AircraftTypeAllocation.Add(aircraft, (0, 0));
                }
                AircraftTypeData aTD = AirportCEOTweaksCore.aircraftTypeDataDict[aircraft];
                AircraftTypeAllocation[aircraft] = (aTD.GetAllocationCount() * count, 0);
            }
            foreach (string aircraft in AircraftTypeAllocation.Keys)
            {
                if (! aircraftFleetModels.Contains(aircraft))
                {
                    AircraftTypeAllocation.Remove(aircraft);
                }
            }
            foreach (CommercialFlightModel cFM in flightListObjects)
            {
                if (cFM.arrivalTimeDT - Singleton<TimeController>.Instance.GetCurrentContinuousTime() < new TimeSpan(24*AircraftTypeDataUtilities.AllocationBaselineDays, 0, 0)) 
                {
                    AircraftTypeAllocation[cFM.aircraftTypeString] = (AircraftTypeAllocation[cFM.aircraftTypeString].Available, AircraftTypeAllocation[cFM.aircraftTypeString].Allocated + 1f);
                }
            }
        }
    }
}
