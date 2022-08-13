using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(CommercialFlightModel))]
    static class Patch_CommercialFlightModel
    {
        [HarmonyPatch("FinalizeFlightDetails")]
        [HarmonyPostfix]
        public static void PostfixFinalizeFlightDetails(ref CommercialFlightModel __instance)
        {
            SingletonNonDestroy<ModsController>.Instance.GetExtensions(__instance as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            __instance.SetFlightPassengerTrafficValues(1);
            ecfm.FinalizeFlightDetails();
        }
        
        [HarmonyPatch("SetFlightPassengerTrafficValues")]
        public static void Postfix(CommercialFlightModel __instance, float __0)
        {
            if (AirportCEOTweaksConfig.fixes == false && AirportCEOTweaksConfig.cargoSystem == false) { return; }

            if (__instance.isAllocated)
            {
                int inhr =  __instance.arrivalTimeDT.Hour;
                int outhr = __instance.departureTimeDT.Hour;

                long seed = __instance.arrivalTimeDT.ToBinary() + __instance.departureRoute.routeNbr;
                UnityEngine.Random.InitState((int)seed);

                float mult = UnityEngine.Random.Range(.3f, 1.0f) + UnityEngine.Random.Range(.6f, 1.33f); //(0.66 +/- 0.33) + ( 1.0 +/- 0.33) /2
                float mult2 = UnityEngine.Random.Range(.3f, 1.0f) + UnityEngine.Random.Range(.6f, 1.33f);

                mult  += (float)(0.70 - 0.01986964 * inhr  + 0.009644035 * Math.Pow((double)inhr, 2d)  - 0.0003787513 * Math.Pow((double)inhr, 3d));
                mult2 += (float)(0.70 - 0.01986964 * outhr + 0.009644035 * Math.Pow((double)outhr, 2d) - 0.0003787513 * Math.Pow((double)outhr, 3d));

                mult  /= 3;
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
        public static void Patch_AddExtensionsOnLoad( ref CommercialFlightModel __instance )
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            __instance.SetFlightPassengerTrafficValues(1);
            /*
                  FlightTypes.FlightType outBound;
                  FlightTypes.FlightType inBound;

                  try
                  {
                      inBound = eam.GetFlightType(__instance);
                      outBound = inBound;
                  }
                  catch
                  {
                      inBound = FlightTypes.FlightType.Vanilla;
                      outBound = FlightTypes.FlightType.Vanilla;
                      //Debug.LogError("ACEO Tweak | ERROR: could not recall flight type for " + __instance.departureFlightNbr);
                  }


                  Extend_CommercialFlightModel ecm = new Extend_CommercialFlightModel(__instance,inBound,outBound);
                  ecm.Initialize(); */
            
        }

        [HarmonyPatch("CancelFlight")]
        [HarmonyPostfix]
        public static void Patch_RemoveExtensionCan(CommercialFlightModel __instance)
        {
            try
            {
                Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                ecfm.CancelFlight();
                //ecm = null;
                //SingletonNonDestroy<ModsController>.Instance.commercialFlightExtensionDictionary.Remove(__instance);
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
                //ecm = null;
                //SingletonNonDestroy<ModsController>.Instance.commercialFlightExtensionDictionary.Remove(__instance);
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
                ecfm.RefreshSeries();
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | Error: Failed to refresh commercial flight model extension on flight allocate!");
            }
        }
    }

   // [Serializable]
    public class Extend_CommercialFlightModel : IComparable<Extend_CommercialFlightModel>
    {
        public int CompareTo(Extend_CommercialFlightModel other)
        {
            if (other.referenceID.Equals(this.referenceID))
            {
                return 0;
            }
            return 1;
        }
        public Extend_CommercialFlightModel(CommercialFlightModel parent, FlightTypes.FlightType inboundFlightType, FlightTypes.FlightType outboundFlightType)
        {
            this.referenceID = Guid.NewGuid().ToString();
            this.reference = new Reference(this.referenceID);

            this.parent = parent;
            if (parent == null) { Debug.LogError("ACEO Tweaks | Error: Tried adding commercial flight model extension to null comercial flight model!"); }


            Singleton<ModsController>.Instance.RegisterThisECFM(this, parent);


            this.inboundFlightType = inboundFlightType;
            this.outboundFlightType = outboundFlightType;
            this.turnaroundType = SingletonNonDestroy<FlightTypesController>.Instance.GetTurnaroundType(parent,inboundFlightType,outboundFlightType);
        }
        public Extend_CommercialFlightModel(CommercialFlightModel parent, Extend_AirlineModel eam)
        {
            this.referenceID = Guid.NewGuid().ToString();
            this.reference = new Reference(this.referenceID);

            this.parent = parent;
            if (parent == null) { Debug.LogError("ACEO Tweaks | Error: Tried adding commercial flight model extension to null comercial flight model!"); }

            Singleton<ModsController>.Instance.RegisterThisECFM(this, parent);

            RefreshTypes(eam);
        }
        public void Initialize()
        {
            try
            {
                turnaroundTime = parent.turnaroundTime;
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | WARN: Failed to get parent turnaround time in ECM Init");
                turnaroundTime = new TimeSpan(4, 0, 0);
            }
            try
            {
                routeNbr = parent.arrivalRoute.routeNbr;
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | WARN: Failed to get parent route number in ECM Init");
                routeNbr = 0;
            }
            try
            {

                if (inboundFlightType == FlightTypes.FlightType.Cargo || inboundFlightType == FlightTypes.FlightType.SpecialCargo || inboundFlightType == FlightTypes.FlightType.Positioning)
                {
                    parent.ResetArrivingPassengers();
                }
                if (outboundFlightType == FlightTypes.FlightType.Cargo || outboundFlightType == FlightTypes.FlightType.SpecialCargo || outboundFlightType == FlightTypes.FlightType.Positioning)
                {
                    parent.ResetDeparingPassengers();
                }
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | WARN: Failed to set turnaround time and PAX levels in ECM Init");
            }
            try
            {
                aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(parent.aircraftTypeString);   //parent.aircraftTypeString
            }
            catch
            {

            }
            InitializeTurnaroundTime();
        }
        private void InitializeTurnaroundTime()
        {
            int num = routeNbr % 12;
            float[] modifiers = { 0.85f, 0.9f, 1f, 1.1f, 1.15f };


            turnaroundBias = num < 2 ? modifiers[0] : num < 4 ? modifiers[1] : num < 9 ? modifiers[2] : num < 11 ? modifiers[3] : modifiers[4];


            TimeSpan tempTurnaroundTime = SingletonNonDestroy<FlightTypesController>.Instance.GetTurnaroundTime(this, turnaroundType, 1f);
            if (parent.isAllocated)
            {
                turnaroundPlayerBias = ((float)(parent.turnaroundTime.TotalMinutes / tempTurnaroundTime.TotalMinutes));
            }

            TurnaroundTime = SingletonNonDestroy<FlightTypesController>.Instance.GetTurnaroundTime(this, turnaroundType, turnaroundPlayerBias);
        }
        public void ResetTurnaroundTime(bool baseline = false)
        {
            float b = baseline ? 1f : turnaroundPlayerBias;
            TurnaroundTime = SingletonNonDestroy<FlightTypesController>.Instance.GetTurnaroundTime(this, turnaroundType, b);
        }
        public void FinalizeFlightDetails()
        {
            RefreshTypes();
            Extend_FlightModel.IfNoPAX(parent);
            EvaluateServices();
        }
        public void CompleteFlight()// ------------- RENEWAL CODE WITHIN ------------------------------
        {
            float maxDelay = 30;
            switch (Airline.businessClass)
            {
                case Enums.BusinessClass.Cheap: maxDelay = 90f; break;
                case Enums.BusinessClass.Small: maxDelay = 75f; break;
                case Enums.BusinessClass.Medium: maxDelay = 60f; break;
                case Enums.BusinessClass.Large: maxDelay = 45f; break;
                case Enums.BusinessClass.Exclusive: maxDelay = 30f; break;
            }
            switch (outboundFlightType)
            {
                case FlightTypes.FlightType.Economy:
                case FlightTypes.FlightType.Commuter: maxDelay *= 0.7f; break;

                case FlightTypes.FlightType.VIP:
                case FlightTypes.FlightType.SpecialCargo: maxDelay *= 0.5f; break;

                case FlightTypes.FlightType.Mainline: break;

                case FlightTypes.FlightType.Flagship: maxDelay *= 1.25f; break;

                case FlightTypes.FlightType.Positioning:
                case FlightTypes.FlightType.Cargo: maxDelay *= 2f; break;
            }
            if (parent.delayCounter<maxDelay)
            {
                SingletonNonDestroy<ModsController>.Instance.GetExtensions(parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                if (ecfm != this)
                {
                    Debug.LogError("ACEO Tweaks | WARN: Get extensions called from an ecfm returned a differnt ecfm.");
                }
                
                    if (Utils.ChanceOccured(parent.Airline.Rating.ClampMax(0.8f)))
                    {
                        if (parent.numberOfFlightsInSerie <= 14 && parent.numberOfFlightsInSerie > 3)
                        {
                            int num = 1;

                            if (parent.delayCounter < maxDelay/2)
                            {
                                num++;
                                if (parent.Airline.rating > 0.8)
                                {
                                    num++;
                                }
                            }


                            //find the info for the last flight in the series

                            HashSet<CommercialFlightModel> mySeries = Singleton<ModsController>.Instance.FutureFlightsByFlightNumber(Airline, DepartureFlightNumber, Singleton<TimeController>.Instance.GetCurrentContinuousTime());
                            DateTime lastArrivalTime = parent.arrivalTimeDT;
                            DateTime lastDepartureTime = parent.departureTimeDT;
                            StandModel lastStand = parent.Stand;
                            float lastTurnaroundPlayerBias = turnaroundPlayerBias;
                            foreach (CommercialFlightModel cfms in mySeries)
                            {
                                Singleton<ModsController>.Instance.GetExtensions(cfms, out Extend_CommercialFlightModel other_ecfm, out Extend_AirlineModel other_eam);

                                lastStand = lastArrivalTime > cfms.arrivalTimeDT ? lastStand : cfms.Stand;
                                lastTurnaroundPlayerBias = lastArrivalTime > cfms.arrivalTimeDT ? lastTurnaroundPlayerBias : other_ecfm.turnaroundPlayerBias;
                                
                                //Do anything else before changeing the time vars so they can be used in comparisons.
                                lastArrivalTime = lastArrivalTime > cfms.arrivalTimeDT ? lastArrivalTime : cfms.arrivalTimeDT;
                                lastDepartureTime = lastDepartureTime > cfms.departureTimeDT ? lastDepartureTime : cfms.departureTimeDT;
                            }


                            // Instantiate and allocate the new flight(s)

                            HashSet<CommercialFlightModel> cfmSet = eam.InstantiateFlightSeries(parent.aircraftTypeString, num, parent.arrivalRoute, parent.departureRoute);
                            foreach (CommercialFlightModel cfm in cfmSet)
                            {
                                num--;
                                cfm.AllocateFlight(lastArrivalTime.AddDays(num+1), lastDepartureTime.AddDays(num+1), parent.Stand);
                                RefreshSeries(false, true);
                            }

                        }

                    }
                
            }

            if (parent.delayCounter>maxDelay && parent.Airline.rating<0.25 && Utils.ChanceOccured(0.35f - parent.Airline.rating))
            {
                HashSet<CommercialFlightModel> mySeries = SingletonNonDestroy<ModsController>.Instance.FlightsByFlightNumber(Airline, DepartureFlightNumber);
                foreach (CommercialFlightModel cfm in mySeries)
                {
                    if (cfm.arrivalTimeDT.AddDays(-2) < Singleton<TimeController>.Instance.GetCurrentContinuousTime())
                    {
                        cfm.CancelFlight(false);
                    }
                }
            }

               
        }
        public void CancelFlight()
        {
            RefreshSeries(false, true);
        }
        public void RefreshSeries(bool self = true, bool others = false)
        {
            if (self)
            {
                //Debug.LogError("selfrefresh");
                parent.numberOfFlightsInSerie = SingletonNonDestroy<ModsController>.Instance.FutureFlightsByFlightNumber(Airline, DepartureFlightNumber, parent.arrivalTimeDT).Count;
            }
            if (others)
            {
                HashSet<CommercialFlightModel> mySeries = new HashSet<CommercialFlightModel>();
                HashSet<Extend_CommercialFlightModel> myExtensions = new HashSet<Extend_CommercialFlightModel>();
                try
                {
                    mySeries = SingletonNonDestroy<ModsController>.Instance.FlightsByFlightNumber(Airline, DepartureFlightNumber);
                    //Debug.LogError("ACEO Tweaks | Debug: myseries.count = " + mySeries.Count);
                }
                catch
                {
                    Debug.LogError("ACEO Tweaks | ERROR: Failed in flightsbyflightnumber");
                    return;
                }
                foreach (CommercialFlightModel cfm in mySeries)
                {
                    SingletonNonDestroy<ModsController>.Instance.GetExtensions(cfm, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                    myExtensions.Add(ecfm);
                }
                //Debug.LogError("ACEO Tweaks | Debug: myextensions.count = " + myExtensions.Count);
                foreach (Extend_CommercialFlightModel e_cfm in myExtensions)
                {
                    e_cfm.RefreshSeries(true, false);
                }
            }
        }
        public void RefreshTypes()
        {
             SingletonNonDestroy<ModsController>.Instance.GetExtensions(parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
             RefreshTypes(eam);
        }
        public void RefreshTypes(Extend_AirlineModel myAirlineExtension)
        {
            try
            {
                if (parent == null)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: In RefreshTypes(), parent (cfm) == null");
                }
                if (myAirlineExtension == null)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: In RefreshTypes(), airline extension == null");
                }

                inboundFlightType = myAirlineExtension.GetFlightType(parent);
                outboundFlightType = inboundFlightType;
                this.turnaroundType = SingletonNonDestroy<FlightTypesController>.Instance.GetTurnaroundType(parent,inboundFlightType, outboundFlightType);
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | ERROR: Failed to Set Flight Types in RefreshTypes()");
            }
        }
        public string GetDescription(bool flight_type, bool turnaround_type, bool international, bool duration)
        {
            string stringy = "";
            string post = "";
            string timeword= "";

            if (parent.isAmbulance)
            {
                return parent.departureFlightNbr + " is an air-ambulance!";
            }
            if (parent.isEmergency)
            {
                return parent.departureFlightNbr + " has decalared an emergency!";
            }

            if (duration && false)
            {
                TimeSpan flightTime;
                
                Extend_FlightModel.TakeoffTime(parent, out flightTime, 2f, 12f);
                if (flightTime.TotalMinutes < 240)
                {
                    timeword = "Short";
                    if (flightTime.TotalMinutes < 90)
                    {
                        timeword = "Very short";
                    }
                }
                else if (flightTime.TotalMinutes > 420)
                {
                    timeword = "Long-haul";
                    if (flightTime.TotalMinutes > 660)
                    {
                        //timeword = "Extreme-long-haul";
                    }
                }
                else
                {
                    timeword = "";
                }
            }

            if (international && !duration)
            {
                if (parent.Stand.onlyAcceptInternational)
                {
                    stringy += "International ";
                    
                }
                else
                {
                    stringy += "";
                    
                }
            }
            if (international && duration)
            {
                if (parent.Stand.onlyAcceptInternational)
                {
                    stringy +="International "+timeword ;
                    
                }
                else
                {
                    stringy +=""+ timeword;
                    
                }
            }

            if (flight_type)
            {
                if (turnaround_type)
                {
                    post = "";
                }
                else
                {
                    post = " Flight";
                }
                FlightTypes.FlightType ftype = parent.currentTravelDirection == Enums.TravelDirection.Arrival ? inboundFlightType : outboundFlightType;
                if (!international)
                {
                    stringy += "";
                }
                else
                {
                    stringy += "";
                }
                switch (ftype)
                {
                    case FlightTypes.FlightType.Vanilla: stringy += "Vanilla"; break;

                    case FlightTypes.FlightType.Economy:  stringy += "Economy"; break;
                    case FlightTypes.FlightType.Commuter: stringy += "Commuter"; break;
                    case FlightTypes.FlightType.Mainline: stringy += "Main-Line"; break;
                    case FlightTypes.FlightType.Flagship: stringy += "Flagship"; break;
                    case FlightTypes.FlightType.VIP:      stringy += "VIP"; break;

                    case FlightTypes.FlightType.Positioning: stringy += "Positioning"; break;
                    case FlightTypes.FlightType.Divert: stringy = parent.departureFlightNbr + "Diverting!"; return stringy;

                    case FlightTypes.FlightType.Cargo: stringy += "Cargo"; break;
                    case FlightTypes.FlightType.SpecialCargo: stringy += "Specialty Cargo"; break;
                }
            }

            stringy += post;

            if (turnaround_type)
            {
                stringy += "; Requests ";
                switch (turnaroundType)
                {
                    case FlightTypes.TurnaroundType.Vanilla: stringy += "Vanilla Turnaround"; break;

                    case FlightTypes.TurnaroundType.FuelOnly: stringy += "Refueling Only!"; break;
                    case FlightTypes.TurnaroundType.Reduced: stringy += "Fast Turnaround"; break;
                    case FlightTypes.TurnaroundType.Normal: stringy += "Normal Turnaround"; break;
                    case FlightTypes.TurnaroundType.Full: stringy += "Full Services"; break;
                    case FlightTypes.TurnaroundType.Exended: stringy += "Extended Turnaround"; break;

                    case FlightTypes.TurnaroundType.Maintenance: stringy += "Hanger Services!"; break;

                    case FlightTypes.TurnaroundType.Cargo: stringy += "Cargo Services"; break;
                    case FlightTypes.TurnaroundType.SpecialCargo: stringy += "Cargo Services"; break;
                }
            }


            return stringy;
        }
        private Extend_AirlineModel E_Airline()
        {
            Singleton<ModsController>.Instance.GetExtensions(parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            if (ecfm !=this)
            {
                Debug.LogError("ACEO Tweaks | WARN: ECFM's E_Airline() gave other ECFM");
            }
            return eam;
        }
        public int[] EvaluateServices()
        {
            int[] tempint = new int[5] { 0, 0, 0, 0, 0 };
            tempint[0] = EvaluateCatering();
            tempint[1] = EvaluateCleaning();
            tempint[2] = EvaluateFueling();
            tempint[3] = EvaluateRampService();
            tempint[4] = EvaluateBaggage();
            return tempint;
        }
        public int EvaluateCatering()
        {
            // -1 = refuse
            //  0 = indifferent; might do it
            //  1 = will take if available, will be pleased
            //  2 = demanded, will be displeased if not available
            
            int result = 0;
            Extend_AirlineModel eam = E_Airline();
            switch (outboundFlightType)
            {
                case FlightTypes.FlightType.Vanilla: return 1;

                case FlightTypes.FlightType.Economy: result = -1; break;
                case FlightTypes.FlightType.Commuter: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson()-1,2); break;
                case FlightTypes.FlightType.Mainline: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson(),2); break;
                case FlightTypes.FlightType.Flagship: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson()+1,2); break;
                case FlightTypes.FlightType.VIP: result = 2; break;

                default: result = -10; break;
            }
            switch (turnaroundType)
            {
                case FlightTypes.TurnaroundType.Reduced: result -= 1; break;
                case FlightTypes.TurnaroundType.FuelOnly: result -= 10; break;
            }
            if (parent.departureRoute.routeDistance/aircraftModel.flyingSpeed > 8) // longhaul
            {
                result += 1;
            }
            else if(parent.departureRoute.routeDistance/aircraftModel.flyingSpeed < 4) //short
            {
                result -= 1;
            }

            result = result.Clamp(-1, 2);

            //if we're making the request in/before flight
            
            if (parent.isAllocated==false || parent.arrivalTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {
                ServiceRequestSetter(result, 11, ref parent.cateringServiceRequested);
                parent.cateringServiceCompleted = false;
            }
            //on the ground
            else if (parent.departureTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {
                if (Singleton<AirportController>.Instance.AirportData.cateringServiceEnabled)
                {
                    ServiceRequestSetter(result, 11, ref parent.cateringServiceRequested);
                }
                else
                {
                    parent.cateringServiceRequested = false;
                    parent.cateringServiceCompleted = false;
                }
            }
            //after departure time
            else
            {
                ServiceResultEvaluator(result, parent.cateringServiceCompleted, ref satisfaction);
            }


            return result;
        }
        public int EvaluateCleaning()
        {
            // -1 = refuse
            //  0 = indifferent; might do it
            //  1 = will take if available, will be pleased
            //  2 = demanded, will be displeased if not available
            int result = 0;
            Extend_AirlineModel eam = E_Airline();
            switch (inboundFlightType)
            {
                case FlightTypes.FlightType.Vanilla: return 1;

                case FlightTypes.FlightType.Economy: result = 0 ; break;
                case FlightTypes.FlightType.Commuter: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson(), 2); break;
                case FlightTypes.FlightType.Mainline: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson(), 2); break;
                case FlightTypes.FlightType.Flagship: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson() + 1, 2); break;
                case FlightTypes.FlightType.VIP: result = 2; break;

                case FlightTypes.FlightType.Divert: result = 0; break;
                case FlightTypes.FlightType.Cargo: result = 0; break;
                case FlightTypes.FlightType.SpecialCargo: result = 0; break;

                default: result = -10; break;
            }
            switch (turnaroundType)
            {
                case FlightTypes.TurnaroundType.Reduced: result -= 1; break;
                case FlightTypes.TurnaroundType.FuelOnly: result -= 10; break;
            }
            if (parent.arrivalRoute.routeDistance / aircraftModel.flyingSpeed > 8) // longhaul
            {
                result += 1;
            }
            else if (parent.arrivalRoute.routeDistance / aircraftModel.flyingSpeed < 4) //short
            {
                result -= 1;
            }

            result = result.Clamp(-1, 2);

            //if we're making the request in/before flight
            if (parent.isAllocated == false || parent.arrivalTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {
                ServiceRequestSetter(result, 5, ref parent.cabinCleaningServiceRequested);
                parent.cabinCleaningServiceCompleted = false;
            }
            //on the ground
            else if (parent.departureTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {
                if (Singleton<AirportController>.Instance.AirportData.aircraftCabinCleaningServiceEnabled)
                {
                    ServiceRequestSetter(result, 5, ref parent.cabinCleaningServiceRequested);
                }
                else
                {
                    parent.cabinCleaningServiceRequested = false;
                    parent.cabinCleaningServiceCompleted = false;
                }
            }
            //after departure time
            else
            {
                ServiceResultEvaluator(result, parent.cabinCleaningServiceCompleted, ref satisfaction);
            }

            return result;
        }
        public int EvaluateFueling()
        {
            // -1 = refuse
            //  0 = indifferent; might do it
            //  1 = will take if available, will be pleased
            //  2 = demanded, will be displeased if not available
            int result = 0;
            Extend_AirlineModel eam = E_Airline();
            switch (inboundFlightType)
            {
                case FlightTypes.FlightType.Vanilla: return 1;

                case FlightTypes.FlightType.Positioning: result = 2; break;
                case FlightTypes.FlightType.Divert: result = 0; break;
                case FlightTypes.FlightType.Cargo: result = 2; break;
                case FlightTypes.FlightType.SpecialCargo: result = 2; break;

                default: result = 1; break;
            }

            if (parent.departureRoute.routeDistance / aircraftModel.rangeKM > 0.5)
            {
                result += 3;
            }
            if (turnaroundType == FlightTypes.TurnaroundType.FuelOnly)
            {
                result += 3;
            }

            result = result.Clamp(-1, 2);

            //if we're making the request in/before flight
            if (parent.isAllocated == false || parent.arrivalTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {
                ServiceRequestSetter(result, 13, ref parent.refuelingRequested);
                parent.refuelingCompleted = false;
            }
            //on the ground
            else if (parent.departureTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {

                ServiceRequestSetter(result, 13, ref parent.refuelingRequested);

            }
            //after departure time
            else
            {
                ServiceResultEvaluator(result, parent.refuelingCompleted, ref satisfaction);
            }

            return result;
        }
        public int EvaluateRampService()
        {
            // -1 = refuse
            //  0 = indifferent; might do it
            //  1 = will take if available, will be pleased
            //  2 = demanded, will be displeased if not available
            int result = 0;
            Extend_AirlineModel eam = E_Airline();
            
            switch (eam.economyTier.RoundToIntLikeANormalPerson())
            {
                case 0: result = -1; break;
                case 1: result =  2; break;
                case 2: result =  2; break;
                case 3: result =  3; break;
                case 4: result =  2; break;

                default: result = 1; break;
            }

            if (parent.arrivalRoute.routeDistance / aircraftModel.flyingSpeed > 8) // longhaul
            {
                result += 1;
            }

            result = result.Clamp(-1, 2);

            //if we're making the request in/before flight
            if (parent.isAllocated == false || parent.arrivalTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {
                ServiceRequestSetter(result, 23, ref parent.rampAgentServiceRequested);
                parent.rampAgentServiceCompleted = false;
            }
            //on the ground
            else if (parent.departureTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {
                if (Singleton<AirportController>.Instance.AirportData.rampAgentServiceRoundEnabled)
                {
                    ServiceRequestSetter(result, 23, ref parent.rampAgentServiceRequested);
                }
                else
                {
                    parent.rampAgentServiceRequested = false;
                    parent.rampAgentServiceCompleted = false;
                }
            }
            //after departure time
            else
            {
                ServiceResultEvaluator(result, parent.rampAgentServiceCompleted, ref satisfaction);
            }

            return result;
        }
        public int EvaluateBaggage()
        {
            // -1 = refuse
            //  0 = indifferent; might do it
            //  1 = will take if available, will be pleased
            //  2 = demanded, will be displeased if not available
            int result = 0;
            Extend_AirlineModel eam = E_Airline();
            switch (turnaroundType)
            {
                case FlightTypes.TurnaroundType.FuelOnly: result = -20; break;
                case FlightTypes.TurnaroundType.Reduced: result = -1; break;
                case FlightTypes.TurnaroundType.Normal: result = 1; break;
                case FlightTypes.TurnaroundType.Exended: result = 2; break;

                default: result = -10; break;
            }
            switch (eam.economyTier)
            {
                case 0: result -= 10; break;
                case 1: break;
                case 2: break;
                case 3: result = Math.Max(1, result++); break;
                case 4: result = Math.Max(1, result++); break;
            }
            switch (inboundFlightType)
            {
                case FlightTypes.FlightType.Cargo: result -=10; break;
                case FlightTypes.FlightType.SpecialCargo: result -= 10; break;
            }
            switch (outboundFlightType)
            {
                case FlightTypes.FlightType.Cargo: result -= 10; break;
                case FlightTypes.FlightType.SpecialCargo: result -= 10; break;
            }
            if (aircraftModel.weightClass == Enums.ThreeStepScale.Small && AirportCEOTweaksConfig.smallPlaneBaggageOff)
            {
                result = -10;
            }
            try
            {
                if (!parent.Stand.HasConnectedBaggageBay && AirportCEOTweaksConfig.disconnectedBaggageOff)
                {
                    result = -10;
                }
            }catch{ } //disconnectedbaggagebaybaggagething

            result = result.Clamp(-1, 2);

            //on the ground
            if (parent.isAllocated == false || parent.departureTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime())
            {
                if (Singleton<AirportController>.Instance.AirportData.baggageHandlingSystemEnabled)
                {
                    ServiceRequestSetter(result, 29, ref parent.cargoLoadingRequested);
                    ServiceRequestSetter(result, 29, ref parent.cargoUnloadingRequested);
                }
                else
                {
                    parent.cargoUnloadingRequested = false;
                    parent.cargoUnloadingCompleted = false;
                    parent.cargoTransferAssistanceRequested = false;
                    parent.cargoTransferAssistanceCompleted = false;
                    parent.cargoLoadingCompleted = false;
                    parent.cargoLoadingRequested = false;
                }
            }
            //after departure time
            else
            {
                int tempint = 0;

                ServiceResultEvaluator(result, parent.cargoLoadingCompleted, ref tempint);
                ServiceResultEvaluator(result, parent.cargoUnloadingCompleted, ref tempint);

                satisfaction = (tempint ==  2) ? satisfaction++ : satisfaction;
                satisfaction = (tempint == -2) ? satisfaction-- : satisfaction;
            }
            return result;
        }
        private void ServiceResultEvaluator(int desire, bool complete, ref int operand)
        {
            if (desire >= 1)
            {
                if ( complete == true)
                {
                    operand++;
                }
                else
                {
                    operand = (desire == 2) ? operand-- : operand;
                }
            }
        }
        private void ServiceRequestSetter(int desire, int mod, ref bool operand)
        {
            switch (desire)
            {
                case -1: operand = false; break;
                case 0: operand = (parent.departureRoute.routeNbr % mod <= mod/2) ? false : true; break;
                default: operand = true; break;
            }
        }

        public bool master = false;
        public bool last = false;
        public string referenceID;
        public Reference reference;

        public CommercialFlightModel parent;
        public Extend_AirlineModel parentAirlineExtension;
        public AircraftController aircraft;
        public AircraftModel aircraftModel;
        private TimeSpan turnaroundTime;
        private int routeNbr;
        private int satisfaction = 0;
        public float turnaroundBias;
        public float turnaroundPlayerBias = 1f;

        FlightTypes.FlightType inboundFlightType;
        FlightTypes.FlightType outboundFlightType;
        FlightTypes.TurnaroundType turnaroundType;

        public string aircraftManufactuer
        {
            get
            {
                return this.aircraftManufactuer;
            }
            set
            {
                this.aircraftManufactuer = value;
                parent.aircraftManufacturer = value;
            }
        }
        public string aircraftModelNbr
        {
            get
            {
                return this.aircraftModelNbr;
            }
            set
            {
                this.aircraftModelNbr = value;
                parent.aircraftModelNbr = value;
            }
        }
        public string aircraftTypeString
        {
            get
            {
                return this.aircraftTypeString;
            }
            set
            {
                this.aircraftTypeString = value;
                parent.aircraftTypeString = value;
            }
        }
        public bool IsRemote
        {
            get
            {
                try
                {
                    return parent.Stand.isRemote;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool IsEmergency
        {
            get
            {
                try
                {
                    return parent.isEmergency;
                }
                catch
                {
                    return false;
                }
            }
        }
        public AirlineModel Airline
        {
            get
            {
                return parent.Airline;
            }
        }
        public Enums.ThreeStepScale WeightClass
        {
            get
            {
                try
                {
                    return parent.weightClass;
                }
                catch
                {
                    Debug.LogError("ACEO Tweaks | ERROR: A commercial flight failed to return its weight class; reverted to default = medium");
                    return Enums.ThreeStepScale.Medium;
                }
            }
        }
        public TimeSpan TurnaroundTime
        {
            get
            {
                switch (IsRemote)
                {
                    case true:
                        if (outboundFlightType == FlightTypes.FlightType.Vanilla )
                        {
                            switch (WeightClass)
                            {
                                case Enums.ThreeStepScale.Large: return turnaroundTime.Add(TimeSpan.FromMinutes(45));
                                case Enums.ThreeStepScale.Medium: return turnaroundTime.Add(TimeSpan.FromMinutes(45));
                                case Enums.ThreeStepScale.Small: return turnaroundTime.Add(TimeSpan.FromMinutes(45));
                                default: return turnaroundTime;
                            }
                        }
                        else
                        {
                            return turnaroundTime;
                        }
                    case false: return turnaroundTime;
                    default: return turnaroundTime;
                }
            }
            set
            {
                turnaroundTime = TimeSpan.FromMinutes(Utils.RoundToNearest(((float)value.TotalMinutes), 5f));
                parent.turnaroundTime = turnaroundTime;
            }
        }
        public string DepartureFlightNumber
        {
            get
            {
                return parent.departureFlightNbr;
            }
        }

    }
}