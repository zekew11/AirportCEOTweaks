using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(CommercialFlightModel))]
    static class Patch_CommercialFlightModel
    {
        [HarmonyPatch("FinalizeFlightDetails")]
        [HarmonyPostfix]
        public static void PostfixFinalizeFlightDetails(ref CommercialFlightModel __instance)
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
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
                //Singleton<ModsController>.Instance.commercialFlightExtensionDictionary.Remove(__instance);
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
            if (!AirportCEOTweaksConfig.flightTypes)
            { return; }
            try
            {
                Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                ecfm.CompleteFlight();
                //ecm = null;
                //Singleton<ModsController>.Instance.commercialFlightExtensionDictionary.Remove(__instance);
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
                ecfm.RefreshSeriesLen();
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | Error: Failed to refresh commercial flight model extension on flight allocate!");
            }
        }
        [HarmonyPatch("ActivateFlight")]
        [HarmonyPrefix]
        public static void Patch_RefreshOnActivate(CommercialFlightModel __instance)
        {
            Singleton<ModsController>.Instance.GetExtensions(__instance, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);

            ecfm.RefreshServices();
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


        public string referenceID;
        public Reference reference;

        public CommercialFlightModel parent;
        public Extend_AirlineModel parentAirlineExtension;
        public AircraftController aircraft;
        public AircraftModel aircraftModel;
        private TimeSpan turnaroundTime;
        private int routeNbr;
        private int satisfaction = 0;
        private int demerits = 0;
        public float turnaroundBias;
        public float turnaroundPlayerBias = 1f;

        public FlightTypes.FlightType inboundFlightType;
        public FlightTypes.FlightType outboundFlightType;
        public FlightTypes.TurnaroundType turnaroundType;

        public Dictionary<TurnaroundServices,TurnaroundService> turnaroundServices = new Dictionary<TurnaroundServices, TurnaroundService>();


        public Extend_CommercialFlightModel(CommercialFlightModel parent, Extend_AirlineModel eam)
        {
            this.referenceID = Guid.NewGuid().ToString();
            this.reference = new Reference(this.referenceID);

            this.parent = parent;
            if (parent == null) { Debug.LogError("ACEO Tweaks | Error: Tried adding commercial flight model extension to null comercial flight model!"); }

            Singleton<ModsController>.Instance.RegisterThisECFM(this, parent);


            RefreshFlightTypes(eam);
            AfterLoadRefresh(0f);
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
            //RefreshFlightTypes();
            Extend_FlightModel.IfNoPAX(parent);
            //EvaluateServices();

            parent.Aircraft.am.CurrentWasteStored = parent.currentTotalNbrOfArrivingPassengers.ClampMin(2) + ((parent.Aircraft.am.MaxPax-parent.currentTotalNbrOfArrivingPassengers)*0.33f).RoundToIntLikeANormalPerson();

            AfterLoadRefresh(0.5f);
        }
        public void CompleteFlight()// ------------- RENEWAL CODE WITHIN ------------------------------
        {
            float maxDelay = GetMaxDelay();

            if (parent.delayCounter<maxDelay) //might renew if no delay
            {
                Singleton<ModsController>.Instance.GetExtensions(parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                if (ecfm != this)
                {
                    Debug.LogError("ACEO Tweaks | WARN: Get extensions called from an ecfm returned a differnt ecfm.");
                }
                
                if (parent.numberOfFlightsInSerie <= 7 && parent.numberOfFlightsInSerie >= 4 && Utils.ChanceOccured(parent.Airline.Rating.ClampMax(0.9f)))
                {
                    int num = 1;

                    if (parent.delayCounter < maxDelay/2 && Utils.ChanceOccured(parent.Airline.Rating.ClampMax(0.7f)))
                    {
                        num++;
                    }


                    //find the info for the last flight in the series

                    HashSet<CommercialFlightModel> mySeries = Singleton<ModsController>.Instance.FutureFlightsByFlightNumber(Airline, DepartureFlightNumber, CurrentTime);
                    DateTime lastArrivalTime = parent.arrivalTimeDT;
                    DateTime lastDepartureTime = parent.departureTimeDT;
                    StandModel lastStand = parent.Stand;
                    float lastTurnaroundPlayerBias = turnaroundPlayerBias;

                    foreach (CommercialFlightModel commercialFlightModel in mySeries)
                    {
                        Singleton<ModsController>.Instance.GetExtensions(commercialFlightModel, out Extend_CommercialFlightModel other_ecfm, out Extend_AirlineModel other_eam);

                        // Catch any strange future-casting
                        if (commercialFlightModel.arrivalTimeDT > Singleton<TimeController>.Instance.GetCurrentContinuousTime().AddDays(8))
                        {
                            Debug.LogWarning("ACEO Tweaks | WARN: A flight is future-cast more than 7 days!");
                            commercialFlightModel.CancelFlight();
                            continue;
                        }

                        lastStand = lastArrivalTime > commercialFlightModel.arrivalTimeDT ? lastStand : commercialFlightModel.Stand;
                        lastTurnaroundPlayerBias = lastArrivalTime > commercialFlightModel.arrivalTimeDT ? lastTurnaroundPlayerBias : other_ecfm.turnaroundPlayerBias;
                        
                        //Do anything else before changeing the time vars so they can be used in comparisons.

                        lastArrivalTime = lastArrivalTime > commercialFlightModel.arrivalTimeDT ? lastArrivalTime : commercialFlightModel.arrivalTimeDT;
                        lastDepartureTime = lastDepartureTime > commercialFlightModel.departureTimeDT ? lastDepartureTime : commercialFlightModel.departureTimeDT;
                    }

                    //catch too soon/too late scheduling
                    if (lastArrivalTime < Singleton<TimeController>.Instance.GetCurrentContinuousTime().AddDays(3) || lastArrivalTime > Singleton<TimeController>.Instance.GetCurrentContinuousTime().AddDays(8))
                    {
                        Debug.LogError("ACEO Tweaks | ERROR: Tried to renew flight for too early/late!");
                        return;
                    }


                    // Instantiate and allocate the new flight(s)

                    HashSet<CommercialFlightModel> cfmSet = eam.InstantiateFlightSeries(parent.aircraftTypeString, num, parent.arrivalRoute, parent.departureRoute);
                    foreach (CommercialFlightModel cfm in cfmSet)
                    {
                        cfm.AllocateFlight(lastArrivalTime.AddDays(num), lastDepartureTime.AddDays(num), parent.Stand);  // eg 3 renewals Adddays 3,2,1
                        RefreshSeriesLen(false, true);
                        num--;
                    }

                }

               
                
            }

            if (parent.delayCounter>maxDelay && parent.Airline.rating<0.25 && Utils.ChanceOccured(0.35f - parent.Airline.rating))
            {
                HashSet<CommercialFlightModel> mySeries = Singleton<ModsController>.Instance.FlightsByFlightNumber(Airline, DepartureFlightNumber);
                foreach (CommercialFlightModel commercialFlightModel in mySeries)
                {
                    if (commercialFlightModel.arrivalTimeDT.AddDays(-2) < CurrentTime)
                    {
                        commercialFlightModel.CancelFlight(false);
                    }
                }
            }

               
        }
        public void CancelFlight()
        {
            RefreshSeriesLen(false, true);
        }
        public void RefreshSeriesLen(bool self = true, bool others = false)
        {
            if (self)
            {
                //Debug.LogError("selfrefresh");
                parent.numberOfFlightsInSerie = Singleton<ModsController>.Instance.FutureFlightsByFlightNumber(Airline, DepartureFlightNumber, parent.arrivalTimeDT).Count;
            }
            if (others)
            {
                HashSet<CommercialFlightModel> mySeries = new HashSet<CommercialFlightModel>();
                HashSet<Extend_CommercialFlightModel> myExtensions = new HashSet<Extend_CommercialFlightModel>();
                try
                {
                    mySeries = Singleton<ModsController>.Instance.FlightsByFlightNumber(Airline, DepartureFlightNumber);
                    //Debug.LogError("ACEO Tweaks | Debug: myseries.count = " + mySeries.Count);
                }
                catch
                {
                    Debug.LogError("ACEO Tweaks | ERROR: Failed in flightsbyflightnumber");
                    return;
                }
                foreach (CommercialFlightModel cfm in mySeries)
                {
                    Singleton<ModsController>.Instance.GetExtensions(cfm, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                    myExtensions.Add(ecfm);
                }
                //Debug.LogError("ACEO Tweaks | Debug: myextensions.count = " + myExtensions.Count);
                foreach (Extend_CommercialFlightModel e_cfm in myExtensions)
                {
                    e_cfm.RefreshSeriesLen(true, false);
                }
            }
        }
        public void RefreshFlightTypes()
        {
             Singleton<ModsController>.Instance.GetExtensions(parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
             RefreshFlightTypes(eam);
        }
        public void RefreshFlightTypes(Extend_AirlineModel myAirlineExtension)
        {
            if (!AirportCEOTweaksConfig.flightTypes)
            {
                inboundFlightType = FlightTypes.FlightType.Vanilla;
                outboundFlightType = inboundFlightType;
                turnaroundType = FlightTypes.TurnaroundType.Vanilla;
            }
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
        public void AfterLoadRefresh(float wait = 0.5f)
        {
            GameObject attachto = UnityEngine.GameObject.Find("CoreGameControllers");
            /*if (!attachto.TryGetComponent<Refresher>(out Refresher myRefresher))
            {
                myRefresher = attachto.AddComponent<Refresher>();
            }
            */
            Refresher myRefresher = attachto.AddComponent<Refresher>();
            myRefresher.Me = this;
            myRefresher.maxWait = wait;
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
        public int[] RefreshServices(bool evaluate = false)
        {


            int[] tempint = new int[5] { 0, 0, 0, 0, 0 };

            if (turnaroundServices.Count < tempint.Length)
            {
                if (parent == null)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: ecfm parent == null in refresh services");
                }
                if (this == null)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: ecfm this == null in refresh services");
                }
                if (ParentAirlineExtension == null)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: ecfm ParentAirlineExtension == null in refresh services");
                }
                
                turnaroundServices.Add(TurnaroundServices.Catering,    new TurnaroundService(TurnaroundServices.Catering, parent, this, ParentAirlineExtension));
                turnaroundServices.Add(TurnaroundServices.Cleaning,    new TurnaroundService(TurnaroundServices.Cleaning, parent, this, ParentAirlineExtension));
                turnaroundServices.Add(TurnaroundServices.Fueling,     new TurnaroundService(TurnaroundServices.Fueling, parent, this, ParentAirlineExtension));
                turnaroundServices.Add(TurnaroundServices.Baggage,     new TurnaroundService(TurnaroundServices.Baggage, parent, this, ParentAirlineExtension));
                turnaroundServices.Add(TurnaroundServices.RampService, new TurnaroundService(TurnaroundServices.RampService, parent, this, ParentAirlineExtension));
            }


            for (var i = 0; i<tempint.Length; i++)
            {
                TurnaroundService service;
                if (turnaroundServices.TryGetValue((TurnaroundServices)i, out service))
                {
                    tempint[i] = (int)service.MyDesire;

                    if (evaluate)
                    {
                        //Debug.LogError("ACEO Tweaks | DEBUG: About to do the thing!");
                        service.ServiceRequestSetter(true);
                        //Debug.LogError("ACEO Tweaks | DEBUG: Survived the thing!");
                    }
                }
                else
                {
                    Debug.LogError("ACEO Tweaks | ERROR: failed to find turnaround service index ==" + i.ToString());
                }
            }

            return tempint;
        }
        public float GetPaymentPercentAndReportRating(bool rate = true)
        {
            satisfaction = 0;
            demerits = 0;
            float maxDelay = GetMaxDelay();

            int[] services = RefreshServices(true);

            if (parent.delayCounter > maxDelay)
            {
                SatisfactionAdder = -1;

                if (parent.delayCounter > maxDelay*2 || turnaroundType == FlightTypes.TurnaroundType.Reduced)
                {
                    SatisfactionAdder = -1;
                }
            }
            else if (parent.delayCounter < maxDelay*.5)
            {
                SatisfactionAdder = 1;

                if (parent.delayCounter == 0 || outboundFlightType == FlightTypes.FlightType.Economy)
                {
                    SatisfactionAdder = 1;
                }
            }

            if (rate)
            {
                ParentAirlineExtension.ReportRating(satisfaction.Clamp(-3, 3), demerits.Clamp(0, 3), parent.weightClass);
            }

            if (demerits >= 3)
            {
                return -1f;
            }
            if (demerits == 2)
            {
                return 0f;
            }
            else if (demerits == 1)
            {
                return 0.5f;
            }
            else
            {
                return 1f;
            }
            
        }
        public float GetMaxDelay()
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

            return maxDelay;
        }

        private DateTime CurrentTime
        {
            get
            {
                return Singleton<TimeController>.Instance.GetCurrentContinuousTime() ;
            }
        }
        public Extend_AirlineModel ParentAirlineExtension
        {
            get
            {
                Singleton<ModsController>.Instance.GetExtensions(parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                return eam;
            }
        }
        private int SatisfactionAdder
        {
            get
            {
                return satisfaction;
            }
            set
            {
                if (value < 0)
                { demerits -= value; }
                satisfaction += value;
            }
        }
        public string AircraftManufactuer
        {
            get
            {
                return this.AircraftManufactuer;
            }
            set
            {
                this.AircraftManufactuer = value;
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
                if (AirportCEOTweaksConfig.forceNormalTurnaroundTime)
                {
                    bool remote = parent.StandIsAssigned ? parent.StandIsRemote : false;
                    return AirTrafficController.GetTurnaroundTime(parent.weightClass, parent.isEmergency, remote);
                }
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

        private class Refresher : MonoBehaviour
        {
            public Extend_CommercialFlightModel Me;
            public float maxWait = 0.5f;
            void Start()
            {
                StartCoroutine(RefreshCoRoutine());
            }
            IEnumerator RefreshCoRoutine()
            {
                while (!SaveLoadGameDataController.loadComplete)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, maxWait));

                try
                {
                    Singleton<ModsController>.Instance.GetExtensions(Me.parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                    Me.RefreshFlightTypes(eam);
                    
                    
                    Me.RefreshServices();
                }
                catch
                {
                    Debug.LogError("ACEO Tweaks | ERROR: Extended Commercial Flight Model Refresh Failed!");
                }
            }
            
        }

        public class TurnaroundService
        {
            string nameString;
            private FlightModel flightModel;
            private Extend_CommercialFlightModel ecfm;
            private Extend_AirlineModel eam;
            private bool failed = false;
            private bool succeeded = false;
            private bool capable = true;
            private TurnaroundServices service;
            private readonly int[] primes = {13,17,19,23,29,31,37,41,43,47};
            private HashSet<TurnaroundService> children = new HashSet<TurnaroundService>();

            public TurnaroundService(TurnaroundServices service, FlightModel flightModel, Extend_CommercialFlightModel ecfm, Extend_AirlineModel eam)
            {
                this.flightModel = flightModel;
                this.ecfm = ecfm;
                this.eam = eam;
                this.service = service;

                switch (service)
                {
                    case TurnaroundServices.Catering: nameString = "cateringService"; break;
                    case TurnaroundServices.Cleaning: nameString = "cabinCleaningService"; break;
                    case TurnaroundServices.Fueling: nameString = "refueling"; break;
                    case TurnaroundServices.RampService: nameString = "rampAgentService"; break;
                    case TurnaroundServices.Baggage: 
                        nameString = "cargoLoading"; 
                        children.Add(new TurnaroundService(service, "cargoUnloading", flightModel, ecfm, eam));
                        children.Add(new TurnaroundService(service, "cargoTransferAssistance", flightModel, ecfm, eam));
                        break;
                }
            }
            private TurnaroundService(TurnaroundServices service, string nameString, FlightModel flightModel, Extend_CommercialFlightModel ecfm, Extend_AirlineModel eam)
            {
                this.nameString = nameString;
                this.service = service;
                this.flightModel = flightModel;
                this.ecfm = ecfm;
                this.eam = eam;
            }
            public enum Desire
            {
                Refused = -1,
                Indiffernt,
                Desired,
                Demanded
            }
            public Desire MyDesire
            {
                get 
                {
                    switch (service)
                    {
                        case TurnaroundServices.Catering:    return (Desire)EvaluateCatering(); 
                        case TurnaroundServices.Cleaning:    return (Desire)EvaluateCleaning(); 
                        case TurnaroundServices.Fueling:     return (Desire)EvaluateFueling(); 
                        case TurnaroundServices.RampService: return (Desire)EvaluateRampService(); 
                        case TurnaroundServices.Baggage:     return (Desire)EvaluateBaggage();
                        default:                             return Desire.Refused;
                    }
                }
            }
            public bool Succeeded
            {
                get
                {
                    if (Requested && Completed && !Failed)
                    {
                        Succeeded = true;
                    }

                    return succeeded;

                }
                set
                {
                    failed = !value;
                    succeeded = value;

                    Completed = true;
                }
            }
            public bool Failed
            {
                get
                {
                    return failed;
                }
                set
                {
                    succeeded = !value;
                    failed = value;

                    Completed = true;
                }
            }
            public bool Completed
            {
                get
                {
                    if (succeeded && failed)
                    {
                        failed = false;
                        Completed = true;
                        Debug.LogError("ACEO Tweaks | WARN: Turnaround service had both succeeded and failed. Reverted to (valid) succeeded (and not failed) state.");
                        return true;
                    }

                    string stringy = nameString + "Completed";
                    bool value = (bool)flightModel.GetType().GetField(stringy).GetValue(flightModel);

                    if (value == false)
                    {
                        failed = false;
                        succeeded = false;
                    }
                    else if (value == true && !Requested)
                    {
                        failed = true;
                        succeeded = false;
                    }

                    return value;
                }
                set
                {
                    if (!AirportCEOTweaksConfig.flightTypes)
                    { return; }
                    string stringy = nameString + "Completed";
                    flightModel.GetType().GetField(stringy).SetValue(flightModel,value);

                    if (value == false)
                    {
                        failed = false;
                        succeeded = false;
                        Requested = true;
                    }
                }
            }
            public bool Requested
            {
                get
                {
                    string stringy = nameString + "Requested";
                    bool value = (bool)flightModel.GetType().GetField(stringy).GetValue(flightModel);

                    return value;
                }
                set
                {
                    if (!AirportCEOTweaksConfig.flightTypes)
                    { return; }
                    string stringy = nameString + "Requested";
                    flightModel.GetType().GetField(stringy).SetValue(flightModel, value);

                    if (value == false)
                    {
                        failed = true;
                        succeeded = false;
                        Completed = true;
                    }
                }
            }
            private int EvaluateCatering()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available

                int result = 0;

                capable = Singleton<AirportController>.Instance.AirportData.cateringServiceEnabled;

                switch (ecfm.outboundFlightType)
                {
                    case FlightTypes.FlightType.Vanilla: return 1;

                    case FlightTypes.FlightType.Economy: result = -1; break;
                    case FlightTypes.FlightType.Commuter: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson() - 1, 2); break;
                    case FlightTypes.FlightType.Mainline: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson(), 2); break;
                    case FlightTypes.FlightType.Flagship: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson() + 1, 2); break;
                    case FlightTypes.FlightType.VIP: result = 2; break;

                    default: result = -10; break;
                }
                switch (ecfm.turnaroundType)
                {
                    case FlightTypes.TurnaroundType.Reduced: result -= 1; break;
                    case FlightTypes.TurnaroundType.FuelOnly: result -= 10; break;
                }
                if (ecfm.parent.departureRoute.routeDistance / ecfm.aircraftModel.flyingSpeed > 8) // longhaul
                {
                    result += 1;
                }
                else if (ecfm.parent.departureRoute.routeDistance / ecfm.aircraftModel.flyingSpeed < 4) //short
                {
                    result -= 1;
                }
                if (ecfm.parent.weightClass == Enums.ThreeStepScale.Small)
                {
                    result = -10;
                }

                result = result.Clamp(-1, 2);
                return result;
            }
            private int EvaluateCleaning()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available
                int result = 0;
                capable = Singleton<AirportController>.Instance.AirportData.aircraftCabinCleaningServiceEnabled;
                switch (ecfm.inboundFlightType)
                {
                    case FlightTypes.FlightType.Vanilla: return 1;

                    case FlightTypes.FlightType.Economy: result = 0; break;
                    case FlightTypes.FlightType.Commuter: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson(), 2); break;
                    case FlightTypes.FlightType.Mainline: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson(), 2); break;
                    case FlightTypes.FlightType.Flagship: result = Math.Min(eam.economyTier.RoundToIntLikeANormalPerson() + 1, 2); break;
                    case FlightTypes.FlightType.VIP: result = 2; break;

                    case FlightTypes.FlightType.Divert: result = 0; break;
                    case FlightTypes.FlightType.Cargo: result = 0; break;
                    case FlightTypes.FlightType.SpecialCargo: result = 0; break;

                    default: result = -10; break;
                }
                switch (ecfm.turnaroundType)
                {
                    case FlightTypes.TurnaroundType.Reduced: result -= 1; break;
                    case FlightTypes.TurnaroundType.FuelOnly: result -= 10; break;
                }
                if (ecfm.parent.arrivalRoute.routeDistance / ecfm.aircraftModel.flyingSpeed > 8) // longhaul
                {
                    result += 1;
                }
                else if (ecfm.parent.arrivalRoute.routeDistance / ecfm.aircraftModel.flyingSpeed < 4) //short
                {
                    result -= 1;
                }
                if (ecfm.parent.weightClass == Enums.ThreeStepScale.Small)
                {
                    result = -10;
                }
                result = result.Clamp(-1, 2);
                return result;
            }
            private int EvaluateFueling()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available

                int result = 0;
                capable = ecfm.aircraftModel.FuelType == Enums.FuelType.JetA1 ? Singleton<AirportController>.Instance.AirportData.jetA1RefuelingServiceEnabled : Singleton<AirportController>.Instance.AirportData.avgas100LLRefuelingServiceEnabled;
                switch (ecfm.inboundFlightType)
                {
                    case FlightTypes.FlightType.Vanilla: return 1;

                    case FlightTypes.FlightType.Positioning: result = 2; break;
                    case FlightTypes.FlightType.Divert: result = 0; break;
                    case FlightTypes.FlightType.Cargo: result = 2; break;
                    case FlightTypes.FlightType.SpecialCargo: result = 2; break;

                    default: result = 1; break;
                }

                if (flightModel.departureRoute.routeDistance / ecfm.aircraftModel.rangeKM > 0.5)
                {
                    result += 3;
                }
                if (ecfm.turnaroundType == FlightTypes.TurnaroundType.FuelOnly)
                {
                    result += 3;
                }

                result = result.Clamp(-1, 2);
                return result;
            }
            private int EvaluateRampService()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available
                int result = 0;
                capable = Singleton<AirportController>.Instance.AirportData.rampAgentServiceRoundEnabled;

                switch (eam.economyTier.RoundToIntLikeANormalPerson())
                {
                    case 0: result = -1; break;
                    case 1: result = 2; break;
                    case 2: result = 2; break;
                    case 3: result = 3; break;
                    case 4: result = 2; break;

                    default: result = 1; break;
                }

                if (flightModel.arrivalRoute.routeDistance / ecfm.aircraftModel.flyingSpeed > 8) // longhaul over 8 hrs
                {
                    result += 1;
                }

                result = result.Clamp(-1, 2);
                return result;
            }
            private int EvaluateBaggage()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available
                int result = 0;
                capable = Singleton<AirportController>.Instance.AirportData.baggageHandlingSystemEnabled;

                switch (ecfm.turnaroundType)
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
                switch (ecfm.inboundFlightType)
                {
                    case FlightTypes.FlightType.Cargo: result -= 10; break;
                    case FlightTypes.FlightType.SpecialCargo: result -= 10; break;
                }
                switch (ecfm.outboundFlightType)
                {
                    case FlightTypes.FlightType.Cargo: result -= 10; break;
                    case FlightTypes.FlightType.SpecialCargo: result -= 10; break;
                }

                if (ecfm.aircraftModel.weightClass == Enums.ThreeStepScale.Small && AirportCEOTweaksConfig.smallPlaneBaggageOff)
                {
                    result = result.Clamp(-1, 0);
                    capable = false;
                }
                try
                {
                    if (!flightModel.Stand.HasConnectedBaggageBay && AirportCEOTweaksConfig.disconnectedBaggageOff)
                    {
                        capable = false;
                    }
                }
                catch { } //disconnectedbaggagebaybaggagething

                result = result.Clamp(-1, 2);
                if ((result == 0 || result == 1) && this.flightModel.isActivated)
                {
                    if (Singleton<AirportController>.Instance.AirportData.baggageHandlingSystemEnabled && flightModel.Stand.HasConnectedBaggageBay)
                    {
                        result = 2;
                    }
                    else
                    {
                        result = 0;
                    }
                }

                if (nameString == "cargoTransferAssistance")
                {
                    bool cta = false;
                    if (flightModel.Aircraft == null)
                    {
                        
                    }
                    else
                    {
                        cta = flightModel.Aircraft.requiresCargoTransferAssistance;
                    }

                    capable = capable == false ? false : cta;
                }
                
                return result;
            }
            private void ServiceResultEvaluator(Desire desire, bool complete)
            {
                if (desire == Desire.Desired || desire == Desire.Demanded)
                {
                    if (complete == true)
                    {
                        ecfm.SatisfactionAdder = 1;
                    }
                    else
                    {
                        ecfm.SatisfactionAdder = (desire == Desire.Demanded) ? -1 : 0;
                    }
                }
            }
            public void ServiceRequestSetter(bool evaluate = false, bool impact = true)
            {
                foreach (TurnaroundService child in children)
                {
                    child.ServiceRequestSetter(evaluate, false);
                }
                
                int hours = ((int)MyDesire - 1).Clamp(-1, 1);
                int mod = primes[(int)service];

                try
                {
                    ecfm.CurrentTime.AddHours(hours);
                }
                catch
                {
                    hours = 0;
                } //dumb block for flight less than 1hr into world
                bool departing = flightModel.isAllocated ? (flightModel.departureTimeDT.AddHours(hours) < ecfm.CurrentTime) : false;


                //set request
                switch (MyDesire)
                {
                    case Desire.Refused: Requested = false; break;
                    case Desire.Indiffernt: Requested = (flightModel.departureRoute.routeNbr % mod <= mod / 2) ? false : true; break;
                    default: Requested = true; break;
                }

                if (departing)
                {
                    if (!Completed)
                    {
                        Failed = true; //also sets completed to true
                    }
                }

                switch (Requested)
                {
                    case false:
                        break;
                    case true:

                        if (!flightModel.isAllocated || flightModel.arrivalTimeDT > ecfm.CurrentTime)
                        {
                            Completed = false;
                            return;
                        }

                        if (evaluate)
                        {
                            departing = true;
                        }

                        if (departing)
                        {
                            if (evaluate && impact) 
                            { 
                                if (Completed && !Failed)
                                {
                                    Succeeded = true;
                                }
                                ServiceResultEvaluator(MyDesire, Succeeded);
                            }
                        }

                        break;
                }
            }
        }

        public enum TurnaroundServices
        {
            Catering,
            Cleaning,
            Fueling,
            RampService,
            Baggage
        }
    }
}