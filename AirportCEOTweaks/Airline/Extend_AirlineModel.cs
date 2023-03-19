using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Random = UnityEngine.Random;

namespace AirportCEOTweaks
{
    public class Extend_AirlineModel
    {
        // Constructors ----------------------------------------------------------------------------------------------------

        public Extend_AirlineModel(AirlineModel airline)
        {
            if (airline == null) { return; }
            if (Singleton<ModsController>.Instance==null) { return; }

            parent = airline;

            Singleton<ModsController>.Instance.RegisterThisEAM(this, airline); //This also triggers refresh of AirlineBuisinessData

            //Basic

            starRank = parent?.businessClass ?? Enums.BusinessClass.Small;
            economyTier = GetEconomyTiers(airline.businessName, airline.businessDescription, airline.businessClass);

            //Load AirlineBusinessData
            
            if (Singleton<ModsController>.Instance.airlineBusinessDataDic.TryGetValue(parent.businessName,out AirlineBusinessData data))
            {
                airlineBusinessData = data;
            }
            else
            {
                airlineBusinessData = default(AirlineBusinessData);
            }

            //Nationality

            countryCode = Singleton<BusinessController>.Instance?.GetAirline(airline.businessName)?.countryCode ?? "";

            if (!AirportCEOTweaksConfig.airlineNationality)
            {
                countries = null;
                goto describer;
            }

            try
            {
                HashSet<string> codeList = new HashSet<string>();
                List<Country> countryList = new List<Country>();
                Country country;

                if (airlineBusinessData.arrayHomeCountryCodes != null)
                {
                    codeList.UnionWith(airlineBusinessData.arrayHomeCountryCodes);
                }
                codeList.Add(countryCode);


                foreach (string code in codeList)
                {
                    try
                    {
                        country = TravelController.GetCountryByCode(code);
                        if (country != null && !countryList.Contains(country))
                        {
                            countryList.Add(country);
                        }
                    }
                    catch
                    {
                        if (code == "")
                        {
                            if (AirportCEOTweaksConfig.liveryLogs)
                            {
                                Debug.LogWarning("ACEO Tweaks | Warn: In airline " + parent.businessName + " country code is empty string!");
                            }
                        }
                        else
                        {
                            Debug.LogError("ACEO Tweaks | ERROR: In airline " + parent.businessName + " could not get country for counrty code [" + countryCode + "]!");
                        }
                    }
                }
                                
                countries = countryList.ToArray();
                if (countryList.Count == 0)
                {
                    countries = null;
                }
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | ERROR: In airline " + parent.businessName + "; problem parsing country code(s)");
                countries = null;
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
                    //ecfm.RefreshFlightTypes(this);
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
        public Country[] countries;
        private SortedDictionary<int, TypeModel> typeModelDictionary;
        public AirlineBusinessData airlineBusinessData;

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
                if (airlineBusinessData.domesticOnly)
                {
                    return true;
                }
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
                    if(countries == null)
                    {
                        if (AirportCEOTweaksConfig.liveryLogs)
                        {
                            Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged NOT domestic by country == null");
                        }
                        return false;
                    } //can't enforce nationality on the null pirates!

                    if (!Singleton<ModsController>.Instance.IsDomestic(countries))
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
                if(airlineBusinessData.domesticOnly)
                {
                    return false;
                }
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
                if (countries == null)
                {
                    if (AirportCEOTweaksConfig.liveryLogs)
                    {
                        Debug.Log("ACEO Tweaks | Debug: airline " + parent.businessName + " is flagged international by country == null");
                    }
                    return true;
                } //can't enforce nationality on the null pirates!
                if (!Singleton<ModsController>.Instance.IsDomestic(countries))
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

            //Replace Fleet with TweaksFleet
            bool tweaksFleet = false;

            if (airlineBusinessData.tweaksFleet==null)
            {
                goto CreateFleetCount;
            }

            if (airlineBusinessData.tweaksFleet.Length > 0)
            {
                parent.aircraftFleetModels = airlineBusinessData.tweaksFleet;
                tweaksFleet = true;
            }
            else
            {
                airlineBusinessData.tweaksFleet = parent.aircraftFleetModels;
            }

            if(airlineBusinessData.tweaksFleetCount == null)
            {
                goto CreateFleetCount;
            }
            if (airlineBusinessData.tweaksFleetCount.Length > 0 && tweaksFleet)
            {
                parent.fleetCount = airlineBusinessData.tweaksFleetCount;
            }
            else
            {
                //patch tweaks fleet count after we make sure fleet count exists
            }

            
            // Create a fleet counts if none exists ........................................................................................
            CreateFleetCount:

            if (FleetCount == null || FleetCount.Length != FleetModels.Length)
            {
                FleetCount = new int[FleetModels.Length];
                for (int i = 0; i < parent.fleetCount.Length; i++)
                {
                    FleetCount[i] = 2 * ((int)parent.businessClass);
                    //Debug.LogError("Airline " + parent.businessName + " has " + FleetCount[i] + " aircraft of type "+ FleetModels[i]);
                }

                airlineBusinessData.tweaksFleetCount = parent.fleetCount;
            }
            for (int i = 0; i<FleetModels.Length ;i++)
            {
                typeModelDictionary.Add(i, new TypeModel(FleetModels[i], FleetCount[i]));
            }
            // Make sure we're in the business list

            if (FleetModels.Length>0)
            {
                Singleton<BusinessController>.Instance.AddToBusinessList(parent);
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
            try { Country[] countries = this.countries; } catch { countries = null; }
            bool forceDomestic=Utils.ChanceOccured(0f) || (IsDomestic&&!ForceInternational);
            bool forceOrigin;
            
            if (countries == null)
            {
                forceOrigin = false;
                if (AirportCEOTweaksConfig.liveryLogs && AirportCEOTweaksConfig.airlineNationality)
                {
                    Debug.LogWarning("ACEO Tweaks | Warn: Generate flight for " + parent.businessName + "encountered (airline) country == null");
                }
            }
            else
            {
                forceOrigin = forceDomestic ? true : !Singleton<ModsController>.Instance.IsDomestic(countries);
            }
            if (forceOrigin && forceDomestic && !Singleton<ModsController>.Instance.IsDomestic(countries))
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

            RouteGenerationController routeGC = UnityEngine.GameObject.Find("CoreGameControllers").GetComponent<RouteGenerationController>();

            SortedSet<RouteContainer> routeContainers = new SortedSet<RouteContainer>();
            routeContainers = routeGC.SelectRouteContainers((desiredRange*1.1f).ClampMax(maxRange), (desiredRange / 1.5f).ClampMin(minRange), forceDomestic, forceOrigin, countries);
            SortedSet<RouteContainer> routeContainer = new SortedSet<RouteContainer>();
            routeContainer = routeGC.NewSelectRoutesByChance(routeContainers);

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

            // Determine the flight data ...........................................................................................

            FlightDataBuilder.GetSpecificFlightDatas(this, selectedAircraft, route, route2, out FlightTypeData inboundFlightData, out FlightTypeData outboundFlightData);

            // Instantiate the flights ..............................................................................................
            int seriesLength = GetSeriesLength(inboundFlightData);
            InstantiateFlightSeries(selectedAircraftType.id, seriesLength, route, route2, inboundFlightData, outboundFlightData);

            return true;
        }
        public int GetSeriesLength(FlightTypeData flightData)
        {
            //assuming inbound and outbound are same

            int value = Random.Range(flightData.minOfferRepetition[0], flightData.maxOfferRepetition[0] + 1); // UnityEngine.Random.Range<int>(inclusiveLower,exclusiveUpper)

            return value.Clamp(1, 20);
        }
        public HashSet<CommercialFlightModel> InstantiateFlightSeries(string AircraftType, int seriesLength, Route arrivalRoute, Route departureRoute, FlightTypeData inboundFlightData, FlightTypeData outboundFlightData)
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
                int exitLimit = seatsin;

                ref int operand = ref seatsin;
                FlightTypeData currentFlightData = inboundFlightData;

                if (TryGetAircraftData(AircraftType, out AircraftTypeData aircraftTypeData, out int typeDataIndex))
                {
                    exitLimit = aircraftTypeData.exitLimit_PAX[typeDataIndex];
                }

                for (var i = 0; i < 2; i++)
                {
                    operand = (operand * currentFlightData.paxMod[0]).RoundToIntLikeANormalPerson().ClampMax(exitLimit);
                    currentFlightData = outboundFlightData;
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
        public int GetServiceLevel(string aircraftType)
        {
            int index = -1;

            if(airlineBusinessData.tweaksFleet == null)
            {
                //Debug.LogWarning("ACEO Tweaks | WARN: tweaksfleet == null for " + parent.businessName);
                goto DefaultReturn;
            }

            for (int i = 0; i<airlineBusinessData.tweaksFleet.Length;i++)
            {
                if (airlineBusinessData.tweaksFleet[i] == aircraftType)
                {
                    index = i;
                    break;
                }
            }

            if (index==-1 || airlineBusinessData.overrideServiceLevelByAircraftType == null)
            {
                Debug.LogWarning("ACEO Tweaks | WARN: index == -1 ("+index+") catch for " + parent.businessName);
                goto DefaultReturn;
            }

            if (index<airlineBusinessData.overrideServiceLevelByAircraftType.Length)
            {
                try
                {
                    return airlineBusinessData.overrideServiceLevelByAircraftType[index];
                }
                catch
                {
                    Debug.LogWarning("ACEO Tweaks | WARN: couldn't return overrideServiceLevelByAircraftType[index] for " + parent.businessName);
                }
            }
            else
            {
                try
                {
                    return airlineBusinessData.overrideServiceLevelByAircraftType[0];
                }
                catch
                {
                    Debug.LogWarning("ACEO Tweaks | WARN: couldn't return overrideServiceLevelByAircraftType[0] for " + parent.businessName);
                }
            }

            DefaultReturn:
            return (int)parent.businessClass;
        }
        public bool GetCargo(string aircraftType)
        {
            if (airlineBusinessData.cargo)
            {
                return true;
            }

            if (AirportCEOTweaks.aircraftTypeDataDict.TryGetValue(aircraftType, out AircraftTypeData aircraftTypeData))
            {
                for (int i = 0; i< aircraftTypeData.id.Length; i++)
                {
                    int index = 0;
                    if (aircraftTypeData.id[i] == aircraftType && i<aircraftTypeData.cargo.Length)
                    {
                        index = i;
                        break;
                    }

                    return aircraftTypeData.cargo[i];
                }
            }

            return false;
        }
        public bool TryGetAircraftData(string aircraftType, out AircraftTypeData aircraftTypeData, out int index)
        {
            index = 0;
            if (AirportCEOTweaks.aircraftTypeDataDict.TryGetValue(aircraftType, out aircraftTypeData))
            {
                for (int i = 0; i < aircraftTypeData.id.Length; i++)
                {
                    index = 0;
                    if (aircraftTypeData.id[i] == aircraftType && i < aircraftTypeData.cargo.Length)
                    {
                        index = i;
                        break;
                    }
                }
                return true;
            }
            else
            {
                aircraftTypeData = default;
                return false;
            }
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
            basePay *= ((extend_CommercialFlightModel.flightDatas[0].payMod[0] + extend_CommercialFlightModel.flightDatas[1].payMod[0]) / 2);

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