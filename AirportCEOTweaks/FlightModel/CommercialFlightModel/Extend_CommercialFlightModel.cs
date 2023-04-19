using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;

namespace AirportCEOTweaks
{
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
        //public short turnaroundAirlineBiasPercent = 100;          //floats are bad because round numbers to us are not round in float world
        private int turnaroundPlayerBiasMins = 0;
        public FlightTypeData[] flightDatas; 

        public Dictionary<TurnaroundServices,TurnaroundService> turnaroundServices = new Dictionary<TurnaroundServices, TurnaroundService>();
        public Extend_CommercialFlightModel(CommercialFlightModel parent, Extend_AirlineModel eam)
        {
            this.referenceID = Guid.NewGuid().ToString();
            this.reference = new Reference(this.referenceID);

            this.parent = parent;
            if (parent == null) { Debug.LogError("ACEO Tweaks | Error: Tried adding commercial flight model extension to null comercial flight model!"); }

            Singleton<ModsController>.Instance.RegisterThisECFM(this, parent);

            CreateRefresherObj(0f);
        }
        public Extend_CommercialFlightModel(CommercialFlightSaveData data)
        {
            this.referenceID = data.referenceID;
            this.reference = new Reference(this.referenceID);

            //Airline airline = Singleton<BusinessController>.Instance.GetAirline(data.airlineString);

            parent = Singleton<AirTrafficController>.Instance.GetFlightByReferenceID<CommercialFlightModel>(data.parentFlightReferenceID);
            if (parent == null) { Debug.LogError("ACEO Tweaks | Error: Tried adding commercial flight model extension to null comercial flight model!"); }
            
            Singleton<ModsController>.Instance.RegisterThisECFM(this, parent);

            satisfaction = data.satisfaction;
            demerits = data.demerits;

            flightDatas = data.flightDatas;

            foreach (TurnaroundService.TurnaroundServiceData turnaroundServiceData in data.turnaroundServiceDatas)
            {
                turnaroundServices.Add(turnaroundServiceData.service, new TurnaroundService(turnaroundServiceData, parent, this, ParentAirlineExtension));
            }

            CreateRefresherObj(0f);
        }
        public void Initialize()
        {
            flightDatas = FlightDataBuilder.GetSpecificFlightDatasArray(parent);
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
                if (flightDatas[0].paxMod[0] == 0)
                {
                    parent.ResetArrivingPassengers();
                }
                if (flightDatas[1].paxMod[0] == 0)
                {
                    parent.ResetDeparingPassengers();
                }
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | WARN: Failed to set PAX levels in ECM Init");
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
            turnaroundTime = parent.TurnaroundTime;

            if (parent.isAllocated)
            {
                turnaroundPlayerBiasMins = 0;
                TimeSpan savedTime = parent.TurnaroundTime;
                DetermineUnmodifiedTurnaroundTime(out TimeSpan rawTurnaroundTime);
                TurnaroundPlayerBiasMins = ((float)(savedTime.TotalMinutes - rawTurnaroundTime.TotalMinutes)).RoundToIntLikeANormalPerson();
            }
            else
            {
                TurnaroundPlayerBiasMins = 0;
            }
        }
        public void DetermineTurnaroundTime()
        {
            DetermineUnmodifiedTurnaroundTime(out _);
        }
        public void DetermineUnmodifiedTurnaroundTime(out TimeSpan rawTurnaroundTime, bool resetBias = false)
        {
            if (parent.isEmergency)
            {
                turnaroundPlayerBiasMins = 0;
                rawTurnaroundTime = AirTrafficController.GetTurnaroundTime(parent.weightClass, parent.isEmergency, parent.StandIsRemote);
                TurnaroundTimeSetRawGetTrue = rawTurnaroundTime;
                return;
            }
            if (resetBias)
            {
                turnaroundPlayerBiasMins = 0;
            }

            float hours = 0;

            switch (GetDynamicFlightSize())
            {
                case Tweaks8SizeScale.Jumbo : hours = 5.5f; break;
                case Tweaks8SizeScale.VeryLarge: hours = 5.25f; break;
                case Tweaks8SizeScale.Large: hours = 4.75f; break;
                case Tweaks8SizeScale.SuperMedium: hours = 4.25f; break;
                case Tweaks8SizeScale.Medium: hours = 4f; break;
                case Tweaks8SizeScale.SubMedium: hours = 3.5f; break;
                case Tweaks8SizeScale.Small: hours = 3f; break;
                case Tweaks8SizeScale.VerySmall: hours = 2.5f; break;
            }

            hours *= flightDatas[0].timeMod[0];
            rawTurnaroundTime = TimeSpan.FromHours(hours);
            TurnaroundTimeSetRawGetTrue = rawTurnaroundTime;
        }
        public void FinalizeFlightDetails()
        {
            FlightModelExtensionMethods.IfNoPAXResetAsCargo(parent);

            parent.Aircraft.am.CurrentWasteStored = parent.currentTotalNbrOfArrivingPassengers.ClampMin(2) + ((parent.Aircraft.am.MaxPax-parent.currentTotalNbrOfArrivingPassengers)*0.33f).RoundToIntLikeANormalPerson();

            RefreshServices();
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
                    int lastTurnaroundPlayerBiasMins = TurnaroundPlayerBiasMins;

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
                        lastTurnaroundPlayerBiasMins = lastArrivalTime > commercialFlightModel.arrivalTimeDT ? lastTurnaroundPlayerBiasMins : other_ecfm.TurnaroundPlayerBiasMins;
                        
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

                    HashSet<CommercialFlightModel> cfmSet = eam.InstantiateFlightSeries(parent.aircraftTypeString, num, parent.arrivalRoute, parent.departureRoute, flightDatas[0], flightDatas[1]);
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
        public void CreateRefresherObj(float wait = 0.5f)
        {
            GameObject attachto = UnityEngine.GameObject.Find("CoreGameControllers");

            Refresher myRefresher = attachto.AddComponent<Refresher>();
            myRefresher.Me = this;
            myRefresher.maxWait = wait;
        }
        public string GetDescription(bool flight_type, bool size, bool international, bool duration)
        {
            string stringy = "";
            //string post = "";
            string timeword= "";

            if (parent.isAmbulance)
            {
                return parent.departureFlightNbr + " is an air-ambulance!";
            }
            if (parent.isEmergency)
            {
                return parent.departureFlightNbr + " has decalared an emergency!";
            }

            if (duration)
            {
                TimeSpan flightTime;
                
                FlightModelExtensionMethods.TakeoffDateTime(parent, out flightTime, 2f, 10f);
                
                if (flightTime.TotalMinutes < 165)
                {
                    timeword = "Short";
                    if (flightTime.TotalMinutes < 90)
                    {
                        timeword = "Very-Short";
                    }
                }
                else if (flightTime.TotalMinutes > 420)
                {
                    timeword = "Longhaul";
                    if (flightTime.TotalMinutes > 660)
                    {
                        timeword = "Ultra-Longhaul";
                    }
                }
                else
                {
                    timeword = "Mid-Range";
                }
            }
            if (size)
            {
                stringy += GetDynamicFlightSize().ToString();
                if (duration) { stringy += ", "; } else { stringy += " "; }
            }
            if (duration)
            {
                stringy += timeword;
            }
            if (flight_type)
            {
                stringy += " " + flightDatas[0].description;
            }
            if (international && IsInternational)
            {
                stringy += "; International.";
            }
            else
            {
                stringy += ".";
            }

            return stringy;
        }
        public int[] RefreshServices(bool lastPass = false)
        {
            int[] serviceDesireArray = new int[5] { 0, 0, 0, 0, 0 };

            if (turnaroundServices.Count < serviceDesireArray.Length)
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
                
                if (!turnaroundServices.ContainsKey(TurnaroundServices.Catering))
                {
                    turnaroundServices.Add(TurnaroundServices.Catering, new TurnaroundService(TurnaroundServices.Catering, parent, this, ParentAirlineExtension));
                }

                if (!turnaroundServices.ContainsKey(TurnaroundServices.Cleaning))
                {
                    turnaroundServices.Add(TurnaroundServices.Cleaning, new TurnaroundService(TurnaroundServices.Cleaning, parent, this, ParentAirlineExtension));
                }

                if (!turnaroundServices.ContainsKey(TurnaroundServices.Fueling))
                {
                    turnaroundServices.Add(TurnaroundServices.Fueling, new TurnaroundService(TurnaroundServices.Fueling, parent, this, ParentAirlineExtension));
                }

                if (!turnaroundServices.ContainsKey(TurnaroundServices.Baggage))
                {
                    turnaroundServices.Add(TurnaroundServices.Baggage, new TurnaroundService(TurnaroundServices.Baggage, parent, this, ParentAirlineExtension));
                }

                if (!turnaroundServices.ContainsKey(TurnaroundServices.RampService))
                {
                    turnaroundServices.Add(TurnaroundServices.RampService, new TurnaroundService(TurnaroundServices.RampService, parent, this, ParentAirlineExtension));
                }
            }


            for (var i = 0; i<serviceDesireArray.Length; i++)
            {
                TurnaroundService service;
                if (turnaroundServices.TryGetValue((TurnaroundServices)i, out service))
                {
                    serviceDesireArray[i] = (int)service.MyDesire;
                    service.ServiceRefresh(lastPass);
                }
                else
                {
                    Debug.LogError("ACEO Tweaks | ERROR: failed to find turnaround service index ==" + i.ToString());
                }
            }
            return serviceDesireArray;
        }
        public float GetPaymentPercentAndReportRating(bool rate = true)
        {
            satisfaction = 0;
            demerits = 0;
            float maxDelay = GetMaxDelay();

            RefreshServices(rate);

            if (parent.delayCounter > maxDelay)
            {
                SatisfactionAdder = -1;
            }
            else if (parent.delayCounter < maxDelay*.5)
            {
                SatisfactionAdder = 1;
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
            return maxDelay;
        }
        public CommercialFlightSaveData SerializedData()
        {
            CommercialFlightSaveData data = new CommercialFlightSaveData();
            data.referenceID = this.referenceID;
            data.parentFlightReferenceID = parent.referenceID;
            data.airlineString = parent.Airline.businessName;
            data.flightNumberString = parent.departureFlightNbr;
            data.arrivalDateTimeString = parent.arrivalTimeDTString;
            data.turnaroundDurationString = parent.turnaroundTimeString;

            data.satisfaction = satisfaction;
            data.demerits = demerits;

            data.flightDatas = flightDatas;

            List<TurnaroundService.TurnaroundServiceData> list = new List<TurnaroundService.TurnaroundServiceData>();
            foreach (KeyValuePair<TurnaroundServices,TurnaroundService> kvp in turnaroundServices)
            {
                list.Add(kvp.Value.MyTurnaroundServiceData);
            }
            data.turnaroundServiceDatas = list.ToArray();

            return data;
        }
        public bool TryGetAircraftTypeData(out AircraftTypeData singleAircraftTypeData)
        {
            if (parent == null)
            {
                singleAircraftTypeData = default;
                return false;
            }

            singleAircraftTypeData = parent.GetAircraftTypeData();

            if (singleAircraftTypeData.id == null || singleAircraftTypeData.id.Length == 0 || singleAircraftTypeData.id[0] == null || singleAircraftTypeData.id[0] == string.Empty)
            {
                return false;
            }
            return true;


            //if (TryGetAircraftTypeData(out singleAircraftTypeData, out int index))
            //{
            //    singleAircraftTypeData = singleAircraftTypeData.SingleAircraftTypeData(singleAircraftTypeData.id[index]);
            //    return true;
            //}
            //else
            //{
            //    singleAircraftTypeData = default;
            //    return false;
            //}
        }
        //public bool TryGetAircraftTypeData(out AircraftTypeData aircraftTypeData, out int index)
        //{
        //    return ParentAirlineExtension.TryGetAircraftData(parent.aircraftTypeString, out aircraftTypeData, out index);
        //}
        public Tweaks8SizeScale GetDynamicFlightSize()
        {
            if (TryGetAircraftTypeData(out AircraftTypeData aircraftTypeData))
            {
                float wingspan = aircraftTypeData.wingSpan_M;
                float length = aircraftTypeData.length_M;
                int PAX = aircraftTypeData.capacity_PAX[0];
                int abreast = aircraftTypeData.seatsAbreast[0];

                if (wingspan < 19.8 && length < 19.8)
                {
                    return Tweaks8SizeScale.VerySmall;
                }
                if (wingspan < 36.3 && length < 29.8)
                {
                    return Tweaks8SizeScale.Small;
                }
                if (wingspan < 48.2 && length < 46.8)
                {
                    if (PAX < 100)
                    {
                        return Tweaks8SizeScale.SubMedium;
                    }
                    else
                    {
                        return Tweaks8SizeScale.Medium;
                    }
                }
                if (wingspan < 52.1 && length < 56.7)
                {
                    if (abreast <= 6)
                    {
                        return Tweaks8SizeScale.SuperMedium;
                    }
                    else
                    {
                        return Tweaks8SizeScale.Large;
                    }
                }
                if (wingspan < 65.2 && length < 66.6)
                {
                    return Tweaks8SizeScale.VeryLarge;
                }
                if (wingspan < 82.2 && length < 79.3)
                {
                    return Tweaks8SizeScale.Jumbo;
                }
                else
                {
                    return Tweaks8SizeScale.NA;
                }
            }

            else
            {
                switch(parent.weightClass)
                {
                    case Enums.ThreeStepScale.Small:
                        if (parent.totalNbrOfArrivingPassengers/GameSettingManager.PassengerModifierValue < 10)
                        {
                            return Tweaks8SizeScale.VerySmall;
                        }
                        else
                        {
                            return Tweaks8SizeScale.Small;
                        }
                    case Enums.ThreeStepScale.Medium:
                        if (parent.totalNbrOfArrivingPassengers / GameSettingManager.PassengerModifierValue < 200)
                        {
                            return Tweaks8SizeScale.Medium;
                        }
                        else
                        {
                            return Tweaks8SizeScale.SuperMedium;
                        }
                    case Enums.ThreeStepScale.Large:
                        if (parent.totalNbrOfArrivingPassengers / GameSettingManager.PassengerModifierValue < 200)
                        {
                            return Tweaks8SizeScale.Large;
                        }
                        if (parent.totalNbrOfArrivingPassengers / GameSettingManager.PassengerModifierValue < 400)
                        {
                            return Tweaks8SizeScale.VeryLarge;
                        }
                        else
                        {
                            return Tweaks8SizeScale.Jumbo;
                        }
                    default: return Tweaks8SizeScale.NA;
                }
            }
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
                { demerits -= value; } // -(-value) ~~ demerits + (+)value
                satisfaction += value;
            }
        }
        public int TurnaroundPlayerBiasMins
        {
            get
            {
                return turnaroundPlayerBiasMins;
            }
            set
            {
                turnaroundPlayerBiasMins = value;
                DetermineTurnaroundTime();
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
        public bool IsRemote
        {
            get
            {
                try
                {
                    return parent.StandIsAssigned && parent.StandIsRemote;
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
        public bool IsInternational
        {
            get
            {
                return !TravelController.IsDomesticAirport(parent.arrivalRoute.FromAirport, parent.arrivalRoute.ToAirport);
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
        public TimeSpan TurnaroundTimeSetRawGetTrue // setter bakes in player bias
        {
            get
            {
                if (AirportCEOTweaksConfig.forceNormalTurnaroundTime)
                {
                    return AirTrafficController.GetTurnaroundTime(parent.weightClass, parent.isEmergency, IsRemote);
                }
                return turnaroundTime;
            }
            set
            {
                turnaroundTime = value + TimeSpan.FromMinutes(TurnaroundPlayerBiasMins);
                turnaroundTime = TimeSpan.FromMinutes(Utils.RoundToNearest(((float)turnaroundTime.TotalMinutes), 5f));
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
            public float maxWait = 1f;
            void Start()
            {
                StartCoroutine(RefreshCoRoutine());
            }
            IEnumerator RefreshCoRoutine()
            {
                //Debug.Log("ACEO Tweaks | Debug: Refresh works cool");
                
                while (!SaveLoadGameDataController.loadComplete)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, maxWait));

                try
                {
                    Singleton<ModsController>.Instance.GetExtensions(Me.parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
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
            private bool impactAssessed = false;
            private Desire myDesire = Desire.Unint;
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
                Desire des = MyDesire;
            }
            public TurnaroundService(TurnaroundServiceData data, FlightModel flightModel, Extend_CommercialFlightModel ecfm, Extend_AirlineModel eam)
            {
                failed = data.failed;
                succeeded = data.succeeded;
                impactAssessed = data.impactAssessed;
                nameString = data.namestring;
                service = data.service;
                Desire des = MyDesire;

                foreach (TurnaroundServiceData child in data.turnaroundServiceDataChildren)
                {
                    children.Add(new TurnaroundService(child, flightModel, ecfm, eam));
                }
            }
            [Serializable]
            public struct TurnaroundServiceData
            {
                public bool failed;
                public bool succeeded;
                public bool impactAssessed;
                public string namestring;
                public TurnaroundServices service;
                public TurnaroundServiceData[] turnaroundServiceDataChildren;
            }
            public TurnaroundServiceData MyTurnaroundServiceData
            {
                get
                {
                    TurnaroundServiceData[] childData = new TurnaroundServiceData[children.Count];
                    int i = 0;
                    foreach (TurnaroundService child in children)
                    {
                        childData[i] = child.MyTurnaroundServiceData;
                        i++;
                    }
                    return new TurnaroundServiceData
                    {
                        failed = Failed,
                        succeeded = Succeeded,
                        impactAssessed = impactAssessed,
                        namestring = nameString,
                        service = service,
                        turnaroundServiceDataChildren = childData
                    };
                }
            }
            public enum Desire
            {
                Unint = -99,
                Refused = -1,
                Indiffernt,
                Desired,
                Demanded
            }
            public Desire MyDesire
            {
                get 
                {
                    if (flightModel.isAmbulance || flightModel.isEmergency)
                    {
                        switch (service)
                        {
                            case TurnaroundServices.Fueling: return (Desire)EvaluateFuelingDesire();
                            case TurnaroundServices.RampService: return (Desire)EvaluateRampServiceDesire();
                            default: MyDesire = Desire.Refused; return Desire.Refused;
                        }
                    }
                    else if (myDesire == Desire.Unint)
                    {
                        switch (service)
                        {
                            case TurnaroundServices.Catering: return (Desire)EvaluateCateringDesire();
                            case TurnaroundServices.Cleaning: return (Desire)EvaluateCleaningDesire();
                            case TurnaroundServices.Fueling: return (Desire)EvaluateFuelingDesire();
                            case TurnaroundServices.RampService: return (Desire)EvaluateRampServiceDesire();
                            case TurnaroundServices.Baggage: return (Desire)EvaluateBaggageDesire();
                            default: Debug.LogError("ACEO Tweaks | ERROR : Turnaround service " + service.ToString() +"not given a MyDesire"); return Desire.Unint;
                        }
                    }
                    else
                    {
                        return myDesire;
                    }
                }
                set
                {
                    myDesire = value;
                }
            }
            public bool Succeeded
            {
                get
                {
                    return succeeded;
                }
                set
                {
                    //failed = !value;
                    succeeded = value;
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
                    //succeeded = !value;
                    failed = value;
                }
            }
            public bool Completed
            {
                get
                {
                    string stringy = nameString + "Completed";
                    bool value = (bool)flightModel.GetType().GetField(stringy).GetValue(flightModel);
                    if (value && !Failed)
                    {
                        ReportSucceed(value);
                    }
                    return value;
                }
                //set
                //{
                //    if (!AirportCEOTweaksConfig.flightTypes)
                //    { return; }
                //    string stringy = nameString + "Completed";
                //    flightModel.GetType().GetField(stringy).SetValue(flightModel,value);
                //}
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
                    string stringy = nameString + "Requested";
                    //bool oldValue = (bool)flightModel.GetType().GetField(stringy).GetValue(flightModel);
                    flightModel.GetType().GetField(stringy).SetValue(flightModel, value);
                }
            }
            public void ReportSucceed(bool succeeded = true)
            {
                Succeeded = succeeded;
                Failed = !succeeded;
                AssessServiceImpact(MyDesire, succeeded);
            }
            private int EvaluateCateringDesire()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available

                int result = 0;

                capable = Singleton<AirportController>.Instance.AirportData.cateringServiceEnabled;

                if(ecfm.TryGetAircraftTypeData(out AircraftTypeData aircraftTypeData))
                {
                    int points = 0;
                    try 
                    { 
                    points = aircraftTypeData.CateringPoints;
                    }
                    catch
                    {
                        Debug.LogWarning("AirportCEOTweaks | WARN: Aircraft " + flightModel.aircraftTypeString + " had an AircraftTypeData but a lookup failed for catering eval.");
                    }
                    if (points == 0)
                    {
                        capable = false;
                        MyDesire = Desire.Refused;
                        impactAssessed = true;
                        return -1;
                    }
                }
                if (ecfm.parent.StandIsAssigned && ecfm.parent.Stand.objectSize == Enums.ThreeStepScale.Small)
                {
                    capable = false;
                    MyDesire = Desire.Refused;
                    impactAssessed = true;
                    return -1;
                }

                switch (ecfm.flightDatas[1].catering[0])
                {
                    case RequestLevel.Demand: return 2;
                    case RequestLevel.Accept:
                        if (capable)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case RequestLevel.Reject: return -1;
                }

                if (ecfm.parent.weightClass == Enums.ThreeStepScale.Small)
                {
                    result = -10;
                }

                result = result.Clamp(-1, 2);
                MyDesire = (Desire)result;
                return result;
            }
            private int EvaluateCleaningDesire()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available
                int result = 0;
                capable = Singleton<AirportController>.Instance.AirportData.aircraftCabinCleaningServiceEnabled;

                if (ecfm.TryGetAircraftTypeData(out AircraftTypeData aircraftTypeData))
                {
                    int points = 0;
                    try
                    {
                        points = aircraftTypeData.CleaningPoints;
                    }
                    catch
                    {
                        Debug.LogWarning("AirportCEOTweaks | WARN: Aircraft " + flightModel.aircraftTypeString + " had an AircraftTypeData but a lookup failed for cleaning eval.");
                    }
                    if (points == 0)
                    {
                        capable = false;
                        MyDesire = Desire.Refused;
                        impactAssessed = true;
                        return -1;
                    }
                }
                if (ecfm.parent.StandIsAssigned && ecfm.parent.Stand.objectSize == Enums.ThreeStepScale.Small)
                {
                    capable = false;
                    MyDesire = Desire.Refused;
                    impactAssessed = true;
                    return -1;
                }

                switch (ecfm.flightDatas[0].cleaning[0])
                {
                    case RequestLevel.Demand: return 2;
                    case RequestLevel.Accept:
                        if (capable)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    case RequestLevel.Reject: return -1;
                }
                if (ecfm.parent.weightClass == Enums.ThreeStepScale.Small)
                {
                    result = -10;
                }
                result = result.Clamp(-1, 2);
                MyDesire = (Desire)result;
                return result;
            }
            private int EvaluateFuelingDesire()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available

                int result = 1;
                capable = ecfm.aircraftModel.FuelType == Enums.FuelType.JetA1 ? Singleton<AirportController>.Instance.AirportData.jetA1RefuelingServiceEnabled : Singleton<AirportController>.Instance.AirportData.avgas100LLRefuelingServiceEnabled;

                if (flightModel.departureRoute.routeDistance / ecfm.aircraftModel.rangeKM > 0.5)
                {
                    result += 3;
                }

                result = result.Clamp(-1, 2);
                MyDesire = (Desire)result;
                return result;
            }
            private int EvaluateRampServiceDesire()
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
                MyDesire = (Desire)result;
                return result;
            }
            private int EvaluateBaggageDesire()
            {
                // -1 = refuse
                //  0 = indifferent; might do it
                //  1 = will take if available, will be pleased
                //  2 = demanded, will be displeased if not available
                int result = 0;
                capable = Singleton<AirportController>.Instance.AirportData.baggageHandlingSystemEnabled;

                if (ecfm.aircraftModel.weightClass == Enums.ThreeStepScale.Small && AirportCEOTweaksConfig.smallPlaneBaggageOff)
                {
                    return -1;
                }
                try
                {
                    if (!flightModel.Stand.HasConnectedBaggageBay && AirportCEOTweaksConfig.disconnectedBaggageOff)
                    {
                        capable = false;
                    }
                }
                catch { } //disconnectedbaggagebaybaggagething

                RequestLevel level = (RequestLevel)Math.Max((int)ecfm.flightDatas[0].baggage[0], (int)ecfm.flightDatas[1].baggage[0]);

                switch (level)
                {
                    case RequestLevel.Demand: result = 2; break;
                    case RequestLevel.Accept:
                        if (capable)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
                        }
                        break;
                    case RequestLevel.Reject: result = -1; break;
                }

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
                MyDesire = (Desire)result;
                return result;
            }
            private void AssessServiceImpact(Desire desire, bool succeeded)
            {
                if (impactAssessed)
                {
                    return;
                }

                impactAssessed = true;

                if (desire == Desire.Desired || desire == Desire.Demanded)
                {
                    if (succeeded == true)
                    {
                        ecfm.SatisfactionAdder = 1;
                    }
                    else
                    {
                        ecfm.SatisfactionAdder = (desire == Desire.Demanded) ? -1 : 0;
                    }
                }
            }
            public void ServiceRefresh(bool lastPass = false, bool impact = true)
            {
                _ = Completed;
                if (impactAssessed)
                {
                    return;
                }
                //Trigger like fucntion in child services
                foreach (TurnaroundService child in children)
                {
                    child.ServiceRefresh(lastPass, false);
                }
                
                float hours = ((int)MyDesire - 1.25f).Clamp(-1f, 1f);
                int mod = primes[(int)service];

                try
                {
                    ecfm.CurrentTime.AddHours(hours);
                }
                catch
                {
                    hours = 0;
                    Debug.LogWarning("ACEO Tweaks | WARN: Turnaround service try/catch hours=0");
                } //dumb block for flight less than 1hr into world

                bool departing = flightModel.isAllocated ? (flightModel.departureTimeDT.AddHours(hours) < ecfm.CurrentTime) : false; //hours defines willingness to wait for critical services

                //set request
                switch (MyDesire)
                {
                    case Desire.Refused: Requested = false; break;
                    case Desire.Indiffernt: Requested = (flightModel.departureRoute.routeNbr % mod <= mod / 2) ? false : true; break;
                    //default: Requested = true; break;
                }

                if (!Requested)
                {
                    return;
                }

                if (flightModel.HasOccupiedStand)
                {
                    if(!capable)
                    {
                        Failed = true;
                        if (impact)
                        {
                            AssessServiceImpact(MyDesire, false);
                        }
                    }
                }

                if (departing || lastPass)
                {
                    if (!Completed)
                    {
                        Failed = true;
                    }
                    if (Completed && !Failed)
                    {
                        Succeeded = true;
                    }
                    if(impact)
                    {
                        AssessServiceImpact(MyDesire, Succeeded);
                    }
                }
            }
        }

        public enum TurnaroundServices
        {
            Catering,
            Cleaning,
            Fueling,
            RampService,
            Baggage,
            Jetbridge
        }
    
    
    }

    public static class CommercialFlightModelExtensionAccessor
    {
        public static Extend_CommercialFlightModel GetExtend_CommercialFlightModel (this CommercialFlightModel cfm)
        {
            Singleton<ModsController>.Instance.GetExtensions(cfm, out Extend_CommercialFlightModel ecfm, out _);
            return ecfm;
        }
    }
}