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
        //[HarmonyPostfix]
        public static void Postfix(Airline airline, AirlineModel __instance)
        {
            //Debug.LogError("AirlineModelConstructor.");
            //__instance = new ModAirlineModel(airline);
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
            Singleton<ModsController>.Instance.GetExtensions(__instance, out _);
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

            if (isAmbulance || isEmergency || !AirportCEOTweaksConfig.flightTypes)
            {
                return true;
            }

            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_AirlineModel eam);
            if (eam == null)
            {
                return false;
            }

            for (int i = 0; i<AirportCEOTweaksConfig.flightGenerationMultiplyer;i++)
            {
                eam.GenerateFlight(__instance, isEmergency, isAmbulance);
            }

            return false;
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
    }

    public class Extend_AirlineModel
    {
        // Constructors ----------------------------------------------------------------------------------------------------

        public Extend_AirlineModel(AirlineModel airline)
        {
            if (airline == null) { return; }
            if (Singleton<ModsController>.Instance==null) { return; }

            parent = airline;

            Singleton<ModsController>.Instance.RegisterThisEAM(this, airline);

            //Basic

            starRank = parent?.businessClass ?? Enums.BusinessClass.Small;
            economyTier = GetEconomyTiers(airline.businessName, airline.businessDescription, airline.businessClass);

            //Nationality

            countryCode = Singleton<BusinessController>.Instance?.GetAirline(airline.businessName)?.countryCode ?? "";

            if (!AirportCEOTweaksConfig.airlineNationality)
            {
                country = null;
                goto describer;
            }

            try
            {
                country = TravelController.GetCountryByCode(countryCode);
            }
            catch
            {
                if (countryCode == "" && AirportCEOTweaksConfig.liveryLogs)
                {
                    Debug.LogWarning("ACEO Tweaks | Warn: In airline " + parent.businessName + " country code is empty string!");
                }
                else
                {
                    Debug.LogError("ACEO Tweaks | ERROR: In airline " + parent.businessName + " could not get country for counrty code [" + countryCode + "]!");
                }
                country = null;
            }

            //Describer

            describer:

            Airline_Descriptions describer = new Airline_Descriptions();
            parent.businessDescription = describer?.Replace_Description(parent) ?? "";

            //maxrange
            if (FleetModels.Length > 0)
            {
                foreach (string aircraftModelString in FleetModels)
                {
                    try
                    {
                        int range = Singleton<AirTrafficController>.Instance?.GetAircraftGameObject(aircraftModelString)?.GetComponent<AircraftController>()?.am?.rangeKM ?? 0;
                        maxRange = Math.Max(range, maxRange);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            //list flights

            myFlights = new HashSet<Extend_CommercialFlightModel>();
            if (parent.flightListObjects.Count > 0)
            {
                foreach (CommercialFlightModel cfm in parent.flightListObjects)
                {
                    Singleton<ModsController>.Instance.GetExtensions(cfm, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                    if (eam == null) { return; }
                    if (eam != this)
                    {
                        Debug.LogWarning("ACEO Tweaks | WARN: extend airline model constructor assigned other eam to own flights");
                    }
                    
                    if (ecfm == null)
                    {
                        return;
                    }
                    myFlights.Add(ecfm);
                    ecfm.RefreshFlightTypes(this);
                }
            }

            MakeTypeModelDictionary();
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
        private string countryCode;
        public Country country;
        private SortedDictionary<int, TypeModel> typeModelDictionary;

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
        public bool IsDomestic
        {
            get
            {
                foreach(string flag in AirportCEOTweaksConfig.noInternationalFlags)
                {
                    if (parent.businessName.ToLower().Contains(flag.ToLower()))
                    { 
                        if (AirportCEOTweaksConfig.liveryLogs)
                        {
                            Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged domestic by keywords");
                        }
                        return true; 
                    }
                }
                if ((int)starRank+1 < AirportCEOTweaksConfig.minimumStarsForInternational && cargoProportion < 1f)
                {
                    if(country == null)
                    {
                        if (AirportCEOTweaksConfig.liveryLogs)
                        {
                            Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged NOT domestic by country == null");
                        }
                        return false;
                    } //can't enforce nationality on the null pirates!
                    if (!TravelController.IsDomesticAirport(GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport, country))
                    {
                        if (AirportCEOTweaksConfig.liveryLogs)
                        {
                            Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged NOT domestic becasue it would be unable to fly to player airport");
                        }
                        return false;
                    } //we don't want to be making forien airlines domestic only
                    if (AirportCEOTweaksConfig.liveryLogs)
                    {
                        Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged domestic due to low star rank && not cargo");
                    }
                    return true;
                }
                if (AirportCEOTweaksConfig.liveryLogs)
                {
                    Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged NOT domestic by default");
                }
                return false;
            }
        }
        public bool ForceInternational
        {
            get
            {
                foreach (string flag in AirportCEOTweaksConfig.yesInternationalFlags)
                {
                    if (parent.businessName.ToLower().Contains(flag.ToLower()))
                    {
                        if (AirportCEOTweaksConfig.liveryLogs)
                        {
                            Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged international by keywords");
                        }
                        return true; 
                    }
                }
                if (country == null)
                {
                    if (AirportCEOTweaksConfig.liveryLogs)
                    {
                        Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged international by country == null");
                    }
                    return true;
                } //can't enforce nationality on the null pirates!
                if (!TravelController.IsDomesticAirport(GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport, country))
                {
                    if (AirportCEOTweaksConfig.liveryLogs)
                    {
                        Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged international because it is foreign to the player airport. Zeke has a hunch this is involved...");
                    }
                    return true;
                } //we don't want to be making forien airlines domestic only
                if (AirportCEOTweaksConfig.liveryLogs)
                {
                    Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged NOT international by default");
                }
                return false;
            }
        }
        // Methods ---------------------------------------------------------------------------------------------------------

        private void MakeTypeModelDictionary()
        {
            typeModelDictionary = new SortedDictionary<int, TypeModel>();
            // Create a fleet counts if none exists ........................................................................................
            if (FleetCount == null || FleetCount.Length != FleetModels.Length)
            {
                FleetCount = new int[FleetModels.Length];
                for (int i = 0; i < parent.fleetCount.Length; i++)
                {
                    FleetCount[i] = 2 * ((int)parent.businessClass);
                    //Debug.LogError("Airline " + parent.businessName + " has " + FleetCount[i] + " aircraft of type "+ FleetModels[i]);
                }
            }
            for (int i = 0; i<FleetModels.Length ;i++)
            {
                typeModelDictionary.Add(i, new TypeModel(FleetModels[i], FleetCount[i]));
            }
        }
        public bool GenerateFlight(AirlineModel airlineModel, bool isEmergency = false, bool isAmbulance = false)
        {
            if (!AirportCEOTweaksConfig.flightTypes) { return false; }
            if (FleetCount.Length == 0)
            {
                Debug.LogWarning("ACEO Tweaks | WARN: Generate flight for " + parent.businessName + " failed due to FleetCount.Length==0");
                return true;
                
            } //error catch

            // Maybe don't generate a flight if there are lots already...............................................................

            //later

            // Preselect route number ...............................................................................................

            int maxflightnumber = (((int)starRank + 3) ^ 2) * 50 + Utils.RandomRangeI(100f, 200f);
            int flightnumber = Utils.RandomRangeI(1f, maxflightnumber);

            for (int i = 0; ; i++)
            {
                if (Singleton<ModsController>.Instance.FlightsByFlightNumber(parent, parent.airlineFlightNbr + flightnumber).Count > 0)
                {
                    flightnumber = Utils.RandomRangeI(1f, maxflightnumber);
                    if (i > 200) 
                    {
                        Debug.LogWarning("ACEO Tweaks | WARN: Generate flight for " + parent.businessName + " failed due to no available flight number");
                        return false; 
                    }
                }
                else
                {
                    break;
                }
            } //no duplicate flight #s

            // Parameterize for route gen ...........................................................................................

            float maxRange=0;
            float minRange=float.MaxValue;
            float desiredRange;
            try { Country country = this.country; } catch { country = null; }
            bool forceDomestic=Utils.ChanceOccured(0f) || (IsDomestic&&!ForceInternational);
            bool forceOrigin;
            
            if (country == null)
            {
                forceOrigin = false;
                if (AirportCEOTweaksConfig.liveryLogs && AirportCEOTweaksConfig.airlineNationality)
                {
                    Debug.LogWarning("ACEO Tweaks | Warn: Generate flight for " + parent.businessName + "encountered (airline) country == null");
                }
            }
            else
            {
                forceOrigin = forceDomestic ? true : !TravelController.IsDomesticAirport(GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport, country);
            }
            if (forceOrigin && forceDomestic && !TravelController.IsDomesticAirport(GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport, country))
            {
                Debug.LogWarning("ACEO Tweaks | WARN: Generate flight for " + parent.businessName + " failed due to forceDomestic on internationally based operator");
                //return true;
                Debug.LogWarning("ACEO Tweaks | WARN: above failure suppressed, proceeding anyways...");
            }

            HashSet<TypeModel> typeModels = new HashSet<TypeModel>();
            
            bool CalcMinMaxRange() //true iff success
            {
                for (int i = 0; i < FleetModels.Length; i++)
                {
                    typeModelDictionary.TryGetValue(i, out TypeModel workingModel);
                    if (workingModel == null) { return false; }
                    if (workingModel.CanOperateFromPlayerAirportStands(.1f) && workingModel.AvailableByDLC() && workingModel.CanDispatchAdditionalAircraft())
                    {
                        typeModels.Add(workingModel);
                        maxRange = workingModel.rangeKM > maxRange ? workingModel.rangeKM : maxRange;
                        minRange = workingModel.rangeKM < minRange ? workingModel.rangeKM : minRange;
                    }
                }
                minRange /= 4;
                minRange = minRange.Clamp(20f, 100f);
                if (minRange > maxRange) { Debug.LogError("ACEO Tweaks | ERROR: min range > max range"); return false; }

                
                return true;
            }
            if (!CalcMinMaxRange()) { Debug.LogWarning("ACEO Tweaks | WARN: Generate flight for " + parent.businessName + " failed due to failure to caluclate min/max airline range"); return true; }
            desiredRange = Math.Abs(Random.Range(-maxRange, maxRange)+Random.Range(-maxRange, maxRange)+Random.Range(-maxRange, maxRange)) /3;
            

            // Route gen ............................................................................................................

            SortedSet<RouteContainer> routeContainers = new SortedSet<RouteContainer>();
            routeContainers = UnityEngine.GameObject.Find("CoreGameControllers").GetComponent<RouteGenerationController>().SelectRouteContainers((desiredRange*1.1f).ClampMax(maxRange), (desiredRange / 1.5f).ClampMin(minRange), forceDomestic, forceOrigin, country);
            SortedSet<RouteContainer> routeContainer = new SortedSet<RouteContainer>();
            routeContainer = UnityEngine.GameObject.Find("CoreGameControllers").GetComponent<RouteGenerationController>().NewSelectRoutesByChance(routeContainers);

            if (routeContainer.Count == 0)
            {
                return true;
            }

            RouteContainer container = routeContainer.ElementAt(0);
            Route route = new Route(container.route);
            Route route2;

            route2 = new Route(route);

            if (route == null || route2 == null)
            {
                Debug.LogWarning("ACEO Tweaks | WARN: Could not generate route for " + parent.businessName);
                return false;
            }

            route2.ReverseRoute();

            route.routeNbr = flightnumber;
            route2.routeNbr = flightnumber;

            // Select Aircraft ......................................................................................................

            AircraftModel selectedAircraft = null;
            AircraftType selectedAircraftType = default;

            bool SelectAircaft()
            {
                
                var filteredTypeModels = typeModels.Where(model => model.CanServeRoute(container, 0f ,AirportCEOTweaksConfig.liveryLogs));
                SortedDictionary<float, TypeModel> orderedTypeDictionay = new SortedDictionary<float, TypeModel>();
                float totalSutability = 0f;

                if (filteredTypeModels.Count() == 0)
                {

                    if (AirportCEOTweaksConfig.liveryLogs) { Debug.LogWarning("ACEO Tweaks | WARN: filteredTypeModels.Count == 0"); }
                    return false;
                }

                foreach(TypeModel type in filteredTypeModels)
                {
                    if (type == null) { break; }
                    float suit = type.SuitabilityForRoute(container);
                    if (float.IsNaN(suit)) { Debug.LogError("ACEO Tweaks | ERROR: Suitability is NaN!"); break; }

                    try 
                    {
                        orderedTypeDictionay.Add(suit, type);
                        
                    }
                    catch
                    {
                        continue;
                    }
                    totalSutability += suit;
                }

                float selectedSutability = Random.Range(0f, totalSutability);
                foreach(KeyValuePair<float,TypeModel> kvp in orderedTypeDictionay)
                {
                    selectedSutability -= kvp.Key;
                    if(selectedSutability<=0.01f)
                    {
                        selectedAircraft = kvp.Value.aircraftModel;
                        selectedAircraftType = kvp.Value.aircraftType;
                        break;
                    }
                }

                if (selectedAircraft == null) { return false; }
                return true;
            }
            if (!SelectAircaft()) { if (AirportCEOTweaksConfig.liveryLogs) { Debug.LogWarning("ACEO Tweaks | WARN: Generate flight for " + parent.businessName + " failed due to failure to select an aircraft"); } return true; }

            // Determine the flight types ...........................................................................................

            FlightTypes.FlightType inBound = GetFlightType(selectedAircraft, flightnumber);
            FlightTypes.FlightType outBound = inBound;

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

        private class TypeModel
        {
            public string aircraftString;
            public int fleetCount;
            public AircraftType aircraftType;
            public AircraftModel aircraftModel;
            //AircraftTypeData aircraftTypeData;

            public int rangeKM;
            public Enums.GenericSize aircraftSize;
            int capacityPAX;

            public TypeModel(string aircraftString, int fleetCount)
            {
                this.aircraftString = aircraftString;
                this.fleetCount = fleetCount;
                CustomEnums.TryGetAircraftType(aircraftString, out this.aircraftType);
                this.aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(aircraftType.id);

                rangeKM = aircraftModel.rangeKM;
                aircraftSize = aircraftType.size;
                capacityPAX = aircraftModel.MaxPax;
            }

            public bool AvailableByDLC()
            {
                return (!AirTrafficController.IsSupersonic(aircraftString) || DLCManager.OwnsSupersonicDLC) && (!AirTrafficController.IsVintage(aircraftString) || DLCManager.OwnsVintageDLC) && (!AirTrafficController.IsEastern(aircraftString) || DLCManager.OwnsBeastsOfTheEastDLC);
            }
            public bool CanOperateFromOtherAirportSize(Enums.GenericSize airportSize)
            {
                if ((int)aircraftSize > (int)airportSize+3 || (int)aircraftSize < (int)airportSize - 1)
                {
                    return false;
                }
                return true;
            }
            public bool CanFlyDistance(int distance) //if player does not have fuel service the available route distance is cut in half
            {

                switch (aircraftModel.fuelType)
                {
                    case Enums.FuelType.JetA1: if (!Singleton<AirportController>.Instance.hasJetA1FuelDepotWithContent) { distance /= 2; } break;
                    case Enums.FuelType.Gasoline:
                    case Enums.FuelType.Diesel:
                    case Enums.FuelType.Unspecified:
                    case Enums.FuelType.Avgas100LL: if (!Singleton<AirportController>.Instance.hasAvgasFuelDepotWithContent) { distance /= 2; } break;
                    default: break;
                }

                return (distance < rangeKM);
            }
            public bool CanDispatchAdditionalAircraft()
            {
                return true;
            } //for counting fleet aircarft in future
            public bool CanOperateFromPlayerAirportStands(float chanceToOfferRegaurdless)
            {
                switch(aircraftModel.weightClass)
                {
                    case Enums.ThreeStepScale.Small: return true;
                    case Enums.ThreeStepScale.Medium: if (Singleton<AirportController>.Instance.hasMediumStand || Singleton<AirportController>.Instance.hasLargeStand) { return true; } break;
                    case Enums.ThreeStepScale.Large: 
                        if (Singleton<AirportController>.Instance.hasLargeStand)
                        { return true; } 
                        else // dont offer anyways if no medium stand
                        { 
                            if (!Singleton<AirportController>.Instance.hasMediumStand) 
                            { return false; } 
                        } 
                        break;
                }
                if (Random.Range(0f,1f)<chanceToOfferRegaurdless)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public bool CanServeRoute(RouteContainer route, float chanceToOfferRegaurdless = 0, bool debug = false)
            {
                if (debug)
                {
                    if (!AvailableByDLC()) { Debug.Log("ACEO Tweaks | Info: " + aircraftString + " not available by DLC"); }
                    if (!CanOperateFromOtherAirportSize(route.Airport.paxSize)) { Debug.Log("ACEO Tweaks | Info: " + aircraftString + " cannot operate from " + route.Airport.airportName + " ["+ route.Airport.airportIATACode + "]"); }
                    if (!CanFlyDistance(route.Distance.RoundToIntLikeANormalPerson())) { Debug.Log("ACEO Tweaks | Info: " + aircraftString + " cannot fly distance " + route.Distance + "km (do you have refueling available?)"); }
                    if (!CanDispatchAdditionalAircraft()) { }
                    if (!CanOperateFromPlayerAirportStands(0)) { Debug.Log("ACEO Tweaks | Info: " + aircraftString + " cannot operate from player stand sizes"); }
                }
                
                return
                    AvailableByDLC() &&
                    CanOperateFromOtherAirportSize(route.Airport.paxSize) &&
                    CanFlyDistance(route.Distance.RoundToIntLikeANormalPerson()) &&
                    CanDispatchAdditionalAircraft() &&
                    CanOperateFromPlayerAirportStands(chanceToOfferRegaurdless);
            }
            public float SuitabilityForRoute(RouteContainer routeThatIsPossible)
            {
                float rangecap = 1f;

                switch(aircraftModel.seatRows)
                {
                    case 1:
                    case 2:
                    case 3: rangecap = .2f; break;
                    case 4:
                    case 5:
                    case 6: rangecap = .4f; break;
                    case 7: rangecap = .6f; break;
                    case 8: rangecap = .8f; break;
                    default: rangecap = .9f; break;
                }
                
                int sizeMismatch = (Math.Abs((int)routeThatIsPossible.Airport.paxSize - (int)aircraftSize)); // 0,1,2,3,4...
                sizeMismatch = sizeMismatch == 0 ? 1 : sizeMismatch;                                         // 1,1,2,3,4...

                float rangeUtilization = (routeThatIsPossible.Distance/rangeKM).Clamp(0f,rangecap); //utilizing range is good, where possible shorter range aircraft should be used for shorter routes.

                float suitability = (rangeUtilization*100) / sizeMismatch;
                suitability = (float)(suitability * fleetCount);

                // Post-processing special conditions

                if (AirTrafficController.IsSupersonic(aircraftString))
                {
                    if (routeThatIsPossible.Etops)
                    {
                        suitability *= 2;
                    }
                    else
                    {
                        suitability /= 2;
                    }
                }                             //more likely for ocean crossing
                if (AirTrafficController.IsEastern(aircraftString) || aircraftType.id == "TU144")
                {
                    bool ussr = false;
                    string[] codes = new string[] {"AM","AZ","BY","EE","GE","KZ","KG","LV","LT","MD","RU","TJ","TM","UA","UZ"};
                    foreach(string code in codes)
                    {
                        ussr = code == routeThatIsPossible.country.countryCode ? true : ussr;
                        ussr = code == GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country.countryCode ? true : ussr;
                        if (ussr) { break; }
                    }

                    if (ussr)
                    {
                        suitability *= 2;
                    }
                    else
                    {
                        suitability /= 2;
                    }
                }  //more likely from former USSR
                if (AirTrafficController.IsVintage(aircraftString))
                {
                    suitability /= 3;
                }                                //less likely

                if (suitability == float.NaN)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: Route Suitibility is NaN! Info: aircraft = " + aircraftString + ", range utilization = " + rangeUtilization + "sizeMismatch = " + sizeMismatch);
                    return 0f;
                }
                return (suitability+Random.Range(-0.2f*suitability,0.2f*suitability));
            }

        }
    }
}