using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(AirlineModel))]
    static class Patch_AirlineModel
    {
        [HarmonyPatch(typeof(AirlineModel), MethodType.Constructor, new Type[] { typeof(Airline) })]
        //[HarmonyPostfix]
        public static void Postfix(Airline airline, AirlineModel __instance)
        {
            //Debug.LogError("AirlineModelConstructor.");
            //__instance = new ModAirlineModel(airline);

            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_AirlineModel eam);
        }
        [HarmonyPatch("SetFromSerializer")]
        [HarmonyPostfix]
        public static void PostfixSetExtensionFromSave(AirlineModel __instance)
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_AirlineModel eam);
        }


        [HarmonyPatch("GetPaymentPerFlight")]
        public static void Postfix(AirlineModel __instance, ref float __result)
        {
            if (AirportCEOTweaksConfig.cargoSystem == true)
            {
                foreach (string flag in AirportCEOTweaksConfig.cargoAirlineFlags)
                {
                    if (__instance.businessName.ToLower().Contains(flag.ToLower()))
                    {
                        __result *= AirportCEOTweaksConfig.cargoPayMod;
                        break;
                    }

                }
            }
        }
        [HarmonyPatch("GenerateFlight")]
        [HarmonyPrefix]
        public static bool Prefix(ref bool isEmergency, ref bool isAmbulance, AirlineModel __instance)
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_AirlineModel eam);

            if (isAmbulance || isEmergency)
            {
                return true;
            }

            return !eam.OldGenerateFlight(__instance, isEmergency, isAmbulance);
        }


        [HarmonyPatch("CountAllFlights")]
        [HarmonyPrefix]
        public static bool HackFlightCount(ref AirlineModel __instance)
        {
            if (!AirportCEOTweaksConfig.flightTypes)
            {
                return true;
            }
            
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

        /*        [HarmonyPatch("GenerateFlight")]  //PAX By Airline
                public static void Postfix(ref AirlineModel __instance, bool __result)
                {
                    if (__result == false)
                    { return; }

                    FlightTypes.FlightType inBound = FlightTypes.FlightType.Mainline;
                    FlightTypes.FlightType outBound = FlightTypes.FlightType.Mainline;

                    if (AirportCEOTweaksConfig.cargoSystem == true)
                    {
                        foreach (string flag in AirportCEOTweaksConfig.cargoAirlineFlags)
                        {
                            if (__instance.businessName.Contains(flag) && AirportCEOTweaksConfig.cargoSystem == true)
                            {
                                inBound = FlightTypes.FlightType.Cargo;
                                outBound = FlightTypes.FlightType.Cargo;
                            }
                        }
                    }

                    int var1 = __instance.flightListObjects.Count;
                    int var2 = __instance.flightListObjects[(var1 - 1)].numberOfFlightsInSerie;
                    int var3 = var1 - var2;

                    Extend_CommercialFlightModel ecm;

                    for (int ii = var3; ii < var1; ii++)
                    {
                        ecm = new Extend_CommercialFlightModel(__instance.flightListObjects[ii], inBound, outBound);
                        ecm.Initialize();
                    }
                } */
    }

    public class Extend_AirlineModel
    {
        // Constructors ----------------------------------------------------------------------------------------------------

        public Extend_AirlineModel(AirlineModel airline)
        {
            parent = airline;
            Singleton<ModsController>.Instance.RegisterThisEAM(this, airline);
            starRank = parent.businessClass;
            economyTier = GetEconomyTiers(airline.businessName, airline.businessDescription, airline.businessClass);
            //Debug.LogError("ACEO Tweaks | INFO: Constructed airline extension for " + parent.businessName);
            Airline_Descriptions describer = new Airline_Descriptions();
            parent.businessDescription = describer.Generate_Description(this) + "\n \n" + describer.Replace_Description(parent);
            describer = null;


            foreach (string aircraftModelString in FleetModels)
            {
                int range = Singleton<AirTrafficController>.Instance.GetAircraftGameObject(aircraftModelString).GetComponent<AircraftController>().am.rangeKM;
                maxRange = Math.Max(range, maxRange);
            }


            myFlights = new HashSet<Extend_CommercialFlightModel>();
            foreach (CommercialFlightModel cfm in parent.flightListObjects)
            {
                Singleton<ModsController>.Instance.GetExtensions(cfm, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                if (eam != this)
                {
                    Debug.LogError("ACEO Tweaks | WARN: extend airline model constructor assigned other eam to own flights");
                }
                myFlights.Add(ecfm);
                ecfm.RefreshFlightTypes(this);
            }


        }


        // Fields ----------------------------------------------------------------------------------------------------------

        public AirlineModel parent;
        public Enums.BusinessClass starRank;
        public float economyTier = 2;
        public HashSet<Extend_CommercialFlightModel> myFlights;
        private int maxFlights = 30;
        private int maxSeries = 3;
        public float cargoProportion = 0f;
        public float maxRange = 0f;

        // Properties ------------------------------------------------------------------------------------------------------

        public string[] FleetModels
        {
            get
            {
                return parent.aircraftFleetModels;
            }
            set
            {
                parent.aircraftFleetModels = value;
            }
        }

        public int[] FleetCount
        {
            get
            {
                return parent.fleetCount;
            }
            set
            {
                parent.fleetCount = value;
            }
        }

        // Methods ---------------------------------------------------------------------------------------------------------


        public bool OldGenerateFlight(AirlineModel airlineModel, bool isEmergency = false, bool isAmbulance = false)
        {

            //if (Utils.ChanceOccured(0f)) //chance to do vanilla gen{return false;}
            if (!AirportCEOTweaksConfig.flightTypes){return false;}

            //Debug.LogError("ACEO Tweaks | INFO: GenerateFlight called in extension by airline " + parent.businessName);

            Dictionary<AircraftModel, float> AircraftModels = new Dictionary<AircraftModel, float>();
            float bigNum = 0f;
            AircraftModel selectedAircraft;


            // Create a fleet counts if none exists ........................................................................................
            if (FleetCount == null || FleetCount.Length != FleetModels.Length)
            {
                FleetCount = new int[FleetModels.Length];
                for (int i = 0; i < airlineModel.fleetCount.Length; i++)
                {
                    FleetCount[i] = 2 * ((int)airlineModel.businessClass);
                    //Debug.LogError("Airline " + parent.businessName + " has " + FleetCount[i] + " aircraft of type "+ FleetModels[i]);
                }
            }

            // Add relevant aircrafts to pool .......................................................................................
            for (int j = 0; j < FleetModels.Length; j++)
            {
                string aircraftString = FleetModels[j];
                float num = (float)FleetCount[j] + 1;


                if ((!AirTrafficController.IsSupersonic(aircraftString) || DLCManager.OwnsSupersonicDLC) && (!AirTrafficController.IsVintage(aircraftString) || DLCManager.OwnsVintageDLC) && (!AirTrafficController.IsEastern(aircraftString) || DLCManager.OwnsBeastsOfTheEastDLC) && CustomEnums.TryGetAircraftType(aircraftString, out AircraftType aircraftType))
                {
                    if (string.IsNullOrEmpty(aircraftType.id))
                    {
                        continue;
                    }

                    AircraftModel aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(aircraftType.id);

                    if (aircraftModel == null)
                    {
                        continue;
                    }

                    num *= (aircraftModel.flyingSpeed / aircraftModel.rangeKM);
                    num *= 10;
                    num = AirTrafficController.IsVintage(aircraftString) ? num / 1.5f : num;
                    num = AirTrafficController.IsSupersonic(aircraftString) ? num / 1.5f : num;
                    num = aircraftModel.weightClass == Enums.ThreeStepScale.Large ? num / 1.25f : num;
                    num = aircraftModel.aircraftEngineType == Enums.AircraftEngineType.Prop ? num / 1.25f : num;

                    if (aircraftType.Size == Enums.ThreeStepScale.Small && (Singleton<AirportController>.Instance.hasSmallCommercialStand || Singleton<AirportController>.Instance.hasMediumStand))
                    {
                        if (AircraftModels.TryAdd(aircraftModel, num))
                        {
                            bigNum += num;
                        }
                        else
                        {
                            Debug.LogError("ACEO Tweaks | ERROR: Failed to add small aircraft" + aircraftString + "to probablility pool for airline " + parent.businessName);
                        }
                    }
                    else if (aircraftType.Size == Enums.ThreeStepScale.Medium && (Singleton<AirportController>.Instance.hasMediumStand || Singleton<AirportController>.Instance.hasLargeStand))
                    {
                        if (AircraftModels.TryAdd(aircraftModel, num))
                        {
                            bigNum += num;
                        }
                        else
                        {
                            Debug.LogError("ACEO Tweaks | ERROR: Failed to add medium aircraft" + aircraftString + "to probablility pool for airline " + parent.businessName);
                        }
                    }
                    else if (aircraftType.Size == Enums.ThreeStepScale.Large && Singleton<AirportController>.Instance.hasLargeStand)
                    {
                        if (AircraftModels.TryAdd(aircraftModel, num))
                        {
                            bigNum += num;
                        }
                        else
                        {
                            Debug.LogError("ACEO Tweaks | ERROR: Failed to add large aircraft " + aircraftString + "to probablility pool for airline " + parent.businessName);
                        }
                    }

                }
            }

            if (AircraftModels.Count == 0)
            {
                return false;
            }

            // Select aircraft at weighted random ............................................................................................
            for (; ; )
            {
                int start = Utils.RandomRangeI(0, AircraftModels.Count);
                float randf = Utils.RandomRangeF(0f, bigNum);
                for (; ; )
                {
                    foreach (KeyValuePair<AircraftModel, float> kvp in AircraftModels)
                    {
                        if (start > 0)
                        {
                            start--;
                            continue;
                        }

                        if (kvp.Value > randf)
                        {
                            selectedAircraft = kvp.Key;
                            goto LoopEnd;
                        }
                    }
                    start--;
                    if (start % 3 == 0)
                    {
                        randf = Utils.RandomRangeF(0f, bigNum);
                    }
                    if (start < -1000)
                    {
                        Debug.LogError("ACEO Tweaks | ERROR: Generate flight could not find a probable aircraft! Airline = " + parent.businessName);
                        return false;
                    }
                }
            }
            LoopEnd:

            // Preselect route number ...............................................................................................

            int maxflightnumber = (((int)starRank + 3) ^ 2) * 50 + Utils.RandomRangeI(100f, 200f);
            int flightnumber = Utils.RandomRangeI(1f, maxflightnumber);

            for (; ; )
            {
                if (Singleton<ModsController>.Instance.FlightsByFlightNumber(parent, parent.airlineFlightNbr + flightnumber).Count > 0)
                {
                    flightnumber = Utils.RandomRangeI(1f, maxflightnumber);
                }
                else
                {
                    break;
                }
            }

            // Determine the flight types ...........................................................................................

            FlightTypes.FlightType inBound = GetFlightType(selectedAircraft, flightnumber);
            FlightTypes.FlightType outBound = inBound;

            // Generate the routes ..................................................................................................

            if (!CustomEnums.TryGetAircraftType(selectedAircraft.aircraftType, out AircraftType selectedAircraftType))
            {
                Debug.LogError("ACEO Tweaks | ERROR: In generate flight selected aircraft model nbr " + selectedAircraft.aircraftType + " did not return an aircraft type structure!");
                return false;
            } //error catch

            Route route = TravelController.GenerateRoute((float)selectedAircraft.rangeKM, selectedAircraftType.size, selectedAircraft.weightClass, RangeByFlightType(inBound, true), RangeByFlightType(inBound, false));
            Route route2;

            if (inBound == FlightTypes.FlightType.Economy || inBound == FlightTypes.FlightType.Cargo || inBound == FlightTypes.FlightType.Positioning || inBound == FlightTypes.FlightType.Mainline || inBound == FlightTypes.FlightType.Flagship || inBound == FlightTypes.FlightType.Divert)
            {
                if (inBound == FlightTypes.FlightType.Positioning || inBound == FlightTypes.FlightType.Divert)
                {
                    route2 = TravelController.GenerateRoute((float)selectedAircraft.rangeKM, selectedAircraftType.size, selectedAircraft.weightClass, RangeByFlightType(outBound, true), RangeByFlightType(outBound, false));
                    goto Route2Exists;
                }
                else if (flightnumber % 9 < 2)
                {
                    route2 = TravelController.GenerateRoute((float)selectedAircraft.rangeKM, selectedAircraftType.size, selectedAircraft.weightClass, RangeByFlightType(outBound, true), RangeByFlightType(outBound, false));
                    goto Route2Exists;
                }
                else if ((flightnumber % 3 < 2) && (inBound == FlightTypes.FlightType.Mainline))
                {
                    route2 = TravelController.GenerateRoute((float)selectedAircraft.rangeKM, selectedAircraftType.size, selectedAircraft.weightClass, RangeByFlightType(outBound, true), RangeByFlightType(outBound, false));
                    goto Route2Exists;
                }
            }

            route2 = new Route(route);
            Route2Exists:

            if (route == null || route2 == null)
            {
                Debug.Log("ACEO Tweaks | Could not generate route for " + parent.businessName);
                return false;
            }

            route2.ReverseRoute();

            route.routeNbr = flightnumber;
            route2.routeNbr = flightnumber;

            // Instantiate the flights ..............................................................................................
            int seriesLength = GetSeriesLength(inBound, outBound);
            InstantiateFlightSeries(selectedAircraftType.id, seriesLength, route, route2, inBound, outBound);

            return true;
        }
        public int GetSeriesLength(FlightTypes.FlightType inbound, FlightTypes.FlightType outbound)
        {
            //assuming inbound and outbound are same

            float value = parent.Rating.ClampMin(.33f);
            switch (starRank)
            {
                case Enums.BusinessClass.Cheap: value *= 5; break;

                case Enums.BusinessClass.Small: value *= 7; break;

                case Enums.BusinessClass.Medium: value *= 10; break;

                case Enums.BusinessClass.Large: value *= 14; break;

                case Enums.BusinessClass.Exclusive: value *= 21; break;

                default: value = 7f; break;
            }
            switch (inbound)
            {
                case FlightTypes.FlightType.Vanilla: value = Utils.RandomRangeI(2, 5); break;

                case FlightTypes.FlightType.SpecialCargo:
                case FlightTypes.FlightType.VIP: value *= Utils.RandomRangeF(.1f, .5f); break;

                case FlightTypes.FlightType.Mainline: value *= Utils.RandomRangeF(.9f, 1.45f); break;

                case FlightTypes.FlightType.Cargo: value *= Utils.RandomRangeF(.5f, 2.5f); break;

                default: value *= Utils.RandomRangeF(.8f, 1.2f); break;
            }

            return value.RoundToIntLikeANormalPerson().Clamp(1, 20);
        }
        public HashSet<CommercialFlightModel> InstantiateFlightSeries(string AircraftType, int seriesLength, Route arrivalRoute, Route departureRoute, FlightTypes.FlightType arrivalType = FlightTypes.FlightType.Vanilla, FlightTypes.FlightType departureType = FlightTypes.FlightType.Vanilla)
        {
            HashSet<CommercialFlightModel> set = new HashSet<CommercialFlightModel>();
            AircraftModel aircraftModel;
            int seatsin;
            int seatsout;


            PAX();

            for (int l = 0; l < seriesLength; l++)
            {
                //Vanilla Commercial Flight Model

                CommercialFlightModel commercialFlightModel = new CommercialFlightModel(parent.referenceID, true, AircraftType, arrivalRoute, departureRoute)
                {
                    numberOfFlightsInSerie = seriesLength
                };

                commercialFlightModel.totalNbrOfArrivingPassengers = seatsin;
                commercialFlightModel.totalNbrOfDepartingPassengers = seatsout;

                set.Add(commercialFlightModel);

                //Extended Commercial Flight Model

                Extend_CommercialFlightModel ecm = new Extend_CommercialFlightModel(commercialFlightModel, this)
                {
                    //last = (l == seriesLength - 1) //unused, I think...
                };
                ecm.Initialize();

                //Register to vote today!!!

                Singleton<AirTrafficController>.Instance.AddToFlightList(commercialFlightModel);
                parent.flightList.Add(commercialFlightModel.referenceID);
                parent.flightListObjects.Add(commercialFlightModel);
                myFlights.Add(ecm);

            }

            return set;

            void PAX()
            {
                aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(AircraftType);
                seatsin = aircraftModel.maxPax;
                seatsout = seatsin;

                ref int operand = ref seatsin;
                FlightTypes.FlightType flightType = arrivalType;

                for (var i = 0; i < 2; i++)
                {
                    switch (flightType)
                    {
                        case FlightTypes.FlightType.Economy: operand = (operand * 1.15f).RoundToIntLikeANormalPerson(); break;
                        case FlightTypes.FlightType.VIP: operand = (operand * 0.5f).RoundToIntLikeANormalPerson(); break;

                        case FlightTypes.FlightType.Cargo:
                        case FlightTypes.FlightType.SpecialCargo:
                        case FlightTypes.FlightType.Positioning: operand = 0; break;
                    }
                    flightType = departureType;
                    operand = ref seatsout;
                }
            }
        }
        public float GetEconomyTiers(string name, string desc, Enums.BusinessClass stars)
        {
            switch (name)
            {
                case "Ambulance Air": return 5f;

                case "Goose Wings":
                case "Strada Regional":
                case "Havana":
                case "Wildcat Air":
                case "Maple Express":
                case "Zoom":
                case "Arrow by CLM":
                case "Yuri Air": return 1f;

                case "Jumper":
                case "Edwards Bay":
                case "Stripe Air":
                case "Olympus Organization":
                case "Air Strada":
                case "Stripe Air Regional":
                case "Fly Penguin":
                case "Nordic":
                case "SkyFly Airlines":
                case "Swiftly":
                case "SkyLink":
                case "CLM (mainline)":
                case "CLM Express":
                case "Tulip Airlines":
                case "Tulip Airlines Vintage":
                case "Nordic Vintage":
                case "Siberian Airlines": return 2f;

                case "Forrest Air":
                case "Trinity Aviation":
                case "OK Air":
                case "Maple":
                case "Atom Air":
                case "Swiftly Vintage":
                case "Maple Vintage": return 3f;

                case "Coast 2 Coast":
                case "Allure":
                case "OK Air Vintage":
                case "Crown Airlines": return 4f;

                default: //Debug.LogError("ACEO Tweaks | WARN: Airline Name " + name + " not recognised as vanilla airline!");
                    return GetModEconomyTiers(name, desc, stars);
            }
        }
        public float GetModEconomyTiers(string name, string desc, Enums.BusinessClass stars)
        {
            if (AirportCEOTweaksConfig.cargoSystem == true)
            {
                foreach (string flag in AirportCEOTweaksConfig.cargoAirlineFlags)
                {
                    if (name.ToLower().Contains(flag.ToLower()) && AirportCEOTweaksConfig.cargoSystem == true)
                    {
                        cargoProportion = 1f;

                        if (stars == Enums.BusinessClass.Exclusive)
                        {
                            return 7f;
                        }
                        else
                        {
                            return 6f;
                        }
                    }
                }
            }
            switch (stars)
            {
                case Enums.BusinessClass.Cheap:
                case Enums.BusinessClass.Small: return 1f;
                case Enums.BusinessClass.Medium:
                case Enums.BusinessClass.Large: return 2f;
                case Enums.BusinessClass.Exclusive: return 3f;
                default: return 2f;
            }
        }
        public FlightTypes.FlightType GetFlightType(AircraftModel aircraft, int flightNumber)
        {
            if (AirportCEOTweaksConfig.cargoSystem == true)
            {
                foreach (string flag in AirportCEOTweaksConfig.cargoAirlineFlags)
                {
                    if (parent.businessName.ToLower().Contains(flag.ToLower()) && AirportCEOTweaksConfig.cargoSystem == true)
                    {
                        if (starRank == Enums.BusinessClass.Exclusive)
                        {
                            return FlightTypes.FlightType.SpecialCargo;
                        }
                        else
                        {
                            return FlightTypes.FlightType.Cargo;
                        }
                    }
                }
            }
            if (!AirportCEOTweaksConfig.flightTypes)
            {
                return FlightTypes.FlightType.Vanilla;
            }

            int intEconomyTier = (economyTier * 73f).RoundToIntLikeANormalPerson();
            bool rounddown = (flightNumber % 73 > intEconomyTier % 73);

            intEconomyTier = rounddown ? intEconomyTier - intEconomyTier % 73 : intEconomyTier + (73 - (intEconomyTier % 73));

            if (intEconomyTier % 73 != 0)
            {
                Debug.LogError("shucks, zeke can't do math!");
            }
            else
            {
                //Debug.LogError("zeke can do math. " + flightNumber.ToString() + " economy tier = " + (intEconomyTier/73).ToString());
            }

            intEconomyTier /= 73;
            intEconomyTier = AirTrafficController.IsSupersonic(aircraft.aircraftType) ? intEconomyTier += 1 : intEconomyTier;
            intEconomyTier = Utils.Clamp(intEconomyTier, 0, 4);



            switch (intEconomyTier)
            {
                case 0: return FlightTypes.FlightType.Economy;
                case 1: //-------------------------------------------------------------Economy
                    if ((aircraft.rangeKM < 3500 + ((flightNumber % 10) * 100) && flightNumber % 3 == 2))
                    {
                        return FlightTypes.FlightType.Commuter;
                    }
                    else
                    {
                        return FlightTypes.FlightType.Economy;
                    }
                case 2: //--------------------------------------------------------------Mainline
                    if ((aircraft.rangeKM > 10000 + ((flightNumber % 30) * 100)) && flightNumber % 6 == 2)
                    {
                        return FlightTypes.FlightType.Flagship;
                    }
                    else if (aircraft.rangeKM < 2500 + ((flightNumber % 10) * 100))
                    {
                        return FlightTypes.FlightType.Commuter;
                    }
                    else
                    {
                        return FlightTypes.FlightType.Mainline;
                    }
                case 3: //--------------------------------------------------------------Full Service
                    if (aircraft.rangeKM > 10000)
                    {
                        return FlightTypes.FlightType.Flagship;
                    }
                    else if (flightNumber % 3 == 2)
                    {
                        return FlightTypes.FlightType.Mainline;
                    }
                    else
                    {
                        return FlightTypes.FlightType.Flagship;
                    }
                case 4: //---------------------------------------------------------------VIP
                    if (flightNumber % 3 == 1 && aircraft.rangeKM < 5000)
                    {
                        return FlightTypes.FlightType.Flagship;
                    }
                    else
                    {
                        return FlightTypes.FlightType.VIP;
                    }
                default: Debug.LogError("ACEO Tweaks | ERROR: Economy tier fell out of range while setting flight type!"); return FlightTypes.FlightType.Vanilla;
            }

        }
        public FlightTypes.FlightType GetFlightType(AircraftModel aircraft, string flightNumber)
        {
            string flightNumeric = "";
            int flightint = 0;

            foreach (char c in flightNumber)
            {
                if (c >= '0' && c <= '9')
                {
                    flightNumeric = string.Concat(flightNumeric, c);
                }
                else
                {
                    continue;
                }
            }

            flightint = System.Convert.ToInt32(flightNumeric);

            return GetFlightType(aircraft, flightint);
        }
        public FlightTypes.FlightType GetFlightType(CommercialFlightModel cfm)
        {
            AircraftModel acm = Singleton<AirTrafficController>.Instance.GetAircraftModel(cfm.aircraftTypeString);

            return GetFlightType(acm, cfm.arrivalRoute.routeNbr);
        }
        private float RangeByFlightType(FlightTypes.FlightType flightType, bool min = true)
        {
            float value;
            switch (flightType)
            {
                case FlightTypes.FlightType.Cargo: value = min ? 0.1f : .5f; break;
                case FlightTypes.FlightType.SpecialCargo: value = min ? 0.05f : .8f; break;

                case FlightTypes.FlightType.Divert: value = min ? 0.1f : .9f; break;
                case FlightTypes.FlightType.Positioning: value = min ? 0.05f : 1.25f; break;

                case FlightTypes.FlightType.Vanilla: value = min ? 0.05f : 1f; break;

                case FlightTypes.FlightType.Economy: value = min ? 0.5f : 1f; break;
                case FlightTypes.FlightType.Commuter: value = min ? 0.25f : .85f; break;
                case FlightTypes.FlightType.Mainline: value = min ? 0.65f : 1f; break;
                case FlightTypes.FlightType.Flagship: value = min ? 0.75f : 1f; break;
                case FlightTypes.FlightType.VIP: value = min ? 0.05f : 1.25f; break;

                default: value = min ? 0.05f : 1f; break;
            }
            return value;
        }
        public void ReportRating(int satisfaction, int demerits, Enums.ThreeStepScale weight)
        {
            switch (starRank)
            {
                case Enums.BusinessClass.Cheap:     demerits = demerits.Clamp(0, 1);             break;
                case Enums.BusinessClass.Small:     demerits = demerits.Clamp(0, 1);             break;
                case Enums.BusinessClass.Medium:    demerits = demerits.Clamp(0, 2);             break;
                case Enums.BusinessClass.Large:     demerits = (demerits > 0) ? demerits +1 : 0; break;
                case Enums.BusinessClass.Exclusive: demerits = (demerits > 0) ? demerits *2 : 0; break;
            }

            int planesizemod = (int)weight + 1;

            if (parent.AircraftFleetBySize(Enums.ThreeStepScale.Large).Count == 0)
            {
                planesizemod++;

                if (parent.AircraftFleetBySize(Enums.ThreeStepScale.Medium).Count == 0)
                {
                    planesizemod++;
                }
            }

            satisfaction -= demerits; // any demerits hurt twice (they were assigned for satisfaction penalty already)
            satisfaction *= (planesizemod)*2; // (*=6 >> *=8)

            satisfaction = satisfaction.Clamp(-24, 24); //should be maxxed at (-lots,+24) already
            //Debug.LogError("ACEO Tweaks | Debug: satisfaction = " + satisfaction.ToString());

            float percentChange = Math.Abs((float)satisfaction / 100f); //

            if (satisfaction > 0)
            {
                parent.Rating += (1 - parent.Rating.Clamp(0.2f, 0.8f)) * percentChange;
                //Debug.LogError("ACEO Tweaks | Debug: " + parent.businessName + " rating increase = " + (1 - parent.Rating.Clamp(0.2f, 0.8f)).ToString() +" * "+ percentChange.ToString());
            }
            else if (satisfaction < 0)
            {
                parent.Rating -= parent.Rating.Clamp(0.2f, 0.8f) * percentChange;
                //Debug.LogError("ACEO Tweaks | Debug: rating decrease =" + ((1 - parent.Rating.Clamp(0.2f, 0.8f)) * percentChange).ToString());
            }

        }
        public float PaymentPerFlight(Extend_CommercialFlightModel extend_CommercialFlightModel, float basePay)
        {
            int services = 0;

            foreach (var i in extend_CommercialFlightModel.RefreshServices())
            {
                services += i;
            }

            services.Clamp(0, 5);
            services *= ((int)extend_CommercialFlightModel.parent.weightClass + 1);
            basePay -= 500* ((int)extend_CommercialFlightModel.parent.weightClass + 1);
            basePay += 125 * services;

            switch (economyTier.RoundToIntLikeANormalPerson())
            {
                case 0: basePay *= .33f; break;
                case 1: basePay *= .66f; break;
                case 2: basePay *= 1f; break;
                case 3: basePay *= 1.25f; break;
                case 4: basePay *= 2f; break;
            }
            switch (extend_CommercialFlightModel.turnaroundType)
            {
                case FlightTypes.TurnaroundType.FuelOnly: basePay *= .4f; break;
                case FlightTypes.TurnaroundType.Reduced: basePay *= 1.15f; break;
                case FlightTypes.TurnaroundType.Exended: basePay *= 1.15f; break;
                case FlightTypes.TurnaroundType.Cargo: basePay *= AirportCEOTweaksConfig.cargoPayMod; break;
                case FlightTypes.TurnaroundType.SpecialCargo: basePay *= (AirportCEOTweaksConfig.cargoPayMod*1.5f); break;
            }
            switch (starRank)
            {
                case Enums.BusinessClass.Cheap: basePay *= 1f; break;
                case Enums.BusinessClass.Small: basePay *= 1f; break;
                case Enums.BusinessClass.Medium: basePay *= 1.1f; break;
                case Enums.BusinessClass.Large: basePay *= 1.2f; break;
                case Enums.BusinessClass.Exclusive: basePay *= 1.25f; break;
                default: basePay = 1f; break;
            }
            if (AirTrafficController.IsInternational(extend_CommercialFlightModel.parent))
            {
                basePay *= 1.33f;
            }
            return basePay.RoundToNearest(250f);

        }
    }
}