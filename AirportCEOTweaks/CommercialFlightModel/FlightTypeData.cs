using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace AirportCEOTweaks
{
    [Serializable]
    public struct FlightTypeData
    {
        private float routeDistance;
        public int toAirport;
        public int fromAirport;
        public Enums.TravelDirection travelDirection;
        public string flightReferenceID;
        public string description; // AA136 is a {short,/ - / long-haul,} {domestic/international} YOUR-TEXT-HERE

        //arrays may hold info for short/med/long | sm/med/lg or a single value for all conditions

        public float[] paxMod;
        public float[] payMod;
        public float[] timeMod;
        public RequestLevel[] baggage;
        public RequestLevel[] jetBridge;
        public RequestLevel[] catering;
        public RequestLevel[] cleaning;
        public RequestLevel[] ULDLower;
        public RequestLevel[] ULDUpper;

        public bool[] canRenew;
        public int[] minOfferRepetition;
        public int[] maxOfferRepetition;
        public int[] maxRenewedRepetition;

        [JsonIgnore]
        public Route Route
        {
            get
            {
                return new Route(fromAirport,toAirport,routeDistance);
            }
            set
            {
                routeDistance = value.routeDistance;
                toAirport = value.ToAirport.id;
                fromAirport = value.FromAirport.id;
            }
        }
    }
    public enum RequestLevel : short
    {
        Reject = -1,
        Accept,
        Demand
    }
    public static class FlightDataBuilder
    {
        public static FlightTypeData[] GetSpecificFlightDatasArray(CommercialFlightModel commercialFlightModel)
        {
            if (commercialFlightModel == null)
            {
                Debug.LogError("ACEO Tweaks | ERROR: in GetSpecificFlightDatasArray commercialFlightModel is null!");
                return new FlightTypeData[] { };
            }
            
            Singleton<ModsController>.Instance.GetExtensions(commercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            AircraftModel aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(commercialFlightModel.aircraftTypeString);
            Route arrivalRoute = commercialFlightModel.arrivalRoute;
            Route departureRoute = commercialFlightModel.departureRoute;
            
            GetSpecificFlightDatas(eam, aircraftModel, arrivalRoute, departureRoute, out FlightTypeData inBoundFlightData, out FlightTypeData outBoundFlightData);
            return new FlightTypeData[]
            {
                inBoundFlightData,outBoundFlightData
            };
        }
        public static void GetSpecificFlightDatas(Extend_AirlineModel eam, AircraftModel aircraftModel, Route arrivalRoute, Route departureRoute, out FlightTypeData inBoundFlightData, out FlightTypeData outBoundFlightData)
        {
            inBoundFlightData =  GetSpecificFlightData(eam, aircraftModel, arrivalRoute);
            outBoundFlightData = GetSpecificFlightData(eam, aircraftModel, departureRoute);
        }
        private static FlightTypeData GetSpecificFlightData(Extend_AirlineModel eam, AircraftModel aircraftModel, Route route)
        {
            FlightTypeData genFlightData = GetFlightDataGeneral(eam, aircraftModel, route);
            Enums.ThreeStepScale size = Enums.ThreeStepScale.Small;
            Enums.ThreeStepScale distance = Enums.ThreeStepScale.Small;

            if (aircraftModel.MaxPax >= 100)
            {
                size = Enums.ThreeStepScale.Medium;
                if(aircraftModel.seatRows >=7)
                {
                    size = Enums.ThreeStepScale.Large;
                }
            }

            if (route.routeDistance/aircraftModel.flyingSpeed > 3f)
            {
                distance = Enums.ThreeStepScale.Medium;
                if (route.routeDistance / aircraftModel.flyingSpeed > 8f)
                {
                    distance = Enums.ThreeStepScale.Large;
                }
            }

            return new FlightTypeData
            {
                Route = route,
                description = genFlightData.description,

                paxMod = new float[] { (int)distance < genFlightData.paxMod.Length ? genFlightData.paxMod[(int)distance] : genFlightData.paxMod[0] },
                payMod = new float[] { (int)size < genFlightData.payMod.Length ? genFlightData.payMod[(int)size] : genFlightData.payMod[0] },
                timeMod = new float[] { (int)size < genFlightData.timeMod.Length ? genFlightData.timeMod[(int)size] : genFlightData.timeMod[0] },

                baggage = new RequestLevel[] { (int)size < genFlightData.baggage.Length ? genFlightData.baggage[(int)size] : genFlightData.baggage[0] },
                jetBridge = new RequestLevel[] { (int)size < genFlightData.jetBridge.Length ? genFlightData.jetBridge[(int)size] : genFlightData.jetBridge[0] },
                catering = new RequestLevel[] { (int)distance < genFlightData.catering.Length ? genFlightData.catering[(int)distance] : genFlightData.catering[0] },
                cleaning = new RequestLevel[] { (int)distance < genFlightData.cleaning.Length ? genFlightData.cleaning[(int)distance] : genFlightData.cleaning[0] },

                ULDLower = new RequestLevel[] { (int)size < genFlightData.ULDLower.Length ? genFlightData.ULDLower[(int)size] : genFlightData.ULDLower[0] },
                ULDUpper = new RequestLevel[] { (int)size < genFlightData.ULDUpper.Length ? genFlightData.ULDUpper[(int)size] : genFlightData.ULDUpper[0] },

                canRenew = new bool[] { (int)size < genFlightData.canRenew.Length ? genFlightData.canRenew[(int)size] : genFlightData.canRenew[0] },
                minOfferRepetition = ArrayReducer(genFlightData.minOfferRepetition, size),
                maxOfferRepetition = ArrayReducer(genFlightData.maxOfferRepetition, size),
                maxRenewedRepetition = ArrayReducer(genFlightData.maxRenewedRepetition, size)
            };
        }
        private static T[] ArrayReducer<T>(T[] array, Enums.ThreeStepScale size)
        {
            T[] array2 = new T[1];

            if ((int)size < array.Length)
            {
                array2[0] = array[(int)size];
            }
            else
            {
                array2[0] = array[0];
            }

            return array2;

            
        }
        private static FlightTypeData GetFlightDataGeneral(Extend_AirlineModel eam, AircraftModel aircraftModel, Route route)
        {
            FlightTypeData flightData = new FlightTypeData();
            int serviceLevel = eam.GetServiceLevel(aircraftModel.aircraftType);
            bool cargo = eam.GetCargo(aircraftModel.aircraftType);

            flightData.Route = route;

            //Default settings for LCCs / 1-star
            flightData.description = "";
            flightData.baggage   = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Accept };
            flightData.jetBridge = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Accept };
            flightData.catering  = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Accept };
            flightData.cleaning  = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Accept };

            flightData.ULDLower  = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Accept };
            flightData.ULDUpper  = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Reject };

            flightData.paxMod  = new float[] { 1f};
            flightData.payMod  = new float[] { 1f};
            flightData.timeMod = new float[] { 1f};

            flightData.canRenew = new bool[] { true };
            flightData.minOfferRepetition   = new int[] { 2 };
            flightData.maxOfferRepetition   = new int[] { 5 };
            flightData.maxRenewedRepetition = new int[] { 5 };


            switch ((Enums.BusinessClass)serviceLevel)
            {
                case Enums.BusinessClass.Cheap:
                    //default case prefilled
                    flightData.description = "Low-Cost Flight";
                    flightData.paxMod = new float[]  { 1f, 1.25f, 1.15f };
                    flightData.payMod = new float[]  { 1f, 0.75f, 0.75f };
                    flightData.timeMod = new float[] { 1f, 0.75f, 0.85f };
                    break;
                case Enums.BusinessClass.Small:
                    flightData.description = "Flight";
                    flightData.baggage   = new RequestLevel[] { RequestLevel.Accept, RequestLevel.Accept, RequestLevel.Demand };
                    flightData.jetBridge = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Accept, RequestLevel.Accept };
                    flightData.catering  = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Accept, RequestLevel.Accept };
                    flightData.cleaning  = new RequestLevel[] { RequestLevel.Accept, RequestLevel.Accept, RequestLevel.Accept };
                    flightData.paxMod = new float[]  { 1f, 1.1f, 1.05f };
                    flightData.payMod = new float[]  { 0.8f };
                    flightData.timeMod = new float[] { 1f, 0.9f, 0.85f };
                    flightData.minOfferRepetition   = new int[] { 2  };
                    flightData.maxOfferRepetition   = new int[] { 5  };
                    flightData.maxRenewedRepetition = new int[] { 7  };
                    break;
                case Enums.BusinessClass.Medium:
                    flightData.description = "Flight";
                    flightData.baggage   = new RequestLevel[] { RequestLevel.Accept, RequestLevel.Accept, RequestLevel.Demand };
                    flightData.jetBridge = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Accept, RequestLevel.Demand };
                    flightData.catering  = new RequestLevel[] { RequestLevel.Accept, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.cleaning  = new RequestLevel[] { RequestLevel.Accept, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.paxMod = new float[]  { 1f };
                    flightData.payMod = new float[]  { 1f };
                    flightData.timeMod = new float[] { 1f };
                    flightData.minOfferRepetition   = new int[] { 3  };
                    flightData.maxOfferRepetition   = new int[] { 6  };
                    flightData.maxRenewedRepetition = new int[] { 9  };
                    break;
                case Enums.BusinessClass.Large:
                    flightData.description = "Premium Flight";
                    flightData.baggage   = new RequestLevel[] { RequestLevel.Accept, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.jetBridge = new RequestLevel[] { RequestLevel.Accept, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.catering  = new RequestLevel[] { RequestLevel.Accept, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.cleaning  = new RequestLevel[] { RequestLevel.Demand, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.paxMod = new float[]  { 0.90f, 0.80f, 0.80f };
                    flightData.payMod = new float[]  { 1.50f, 1.25f, 1.25f };
                    flightData.timeMod = new float[] { 1.00f, 1.15f, 1.25f };
                    flightData.minOfferRepetition   = new int[] { 5  };
                    flightData.maxOfferRepetition   = new int[] { 7 };
                    flightData.maxRenewedRepetition = new int[] { 14 };
                    break;
                case Enums.BusinessClass.Exclusive:
                    flightData.description = "Premium Flight";
                    flightData.baggage   = new RequestLevel[] { RequestLevel.Demand, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.jetBridge = new RequestLevel[] { RequestLevel.Demand, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.catering  = new RequestLevel[] { RequestLevel.Demand, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.cleaning  = new RequestLevel[] { RequestLevel.Demand, RequestLevel.Demand, RequestLevel.Demand };
                    flightData.paxMod = new float[]  { .75f };
                    flightData.payMod = new float[]  { 2f, 1.5f, 1.75f };
                    flightData.timeMod = new float[] { 1.25f, 1.25f, 1.5f };
                    flightData.minOfferRepetition   = new int[] { 5  };
                    flightData.maxOfferRepetition   = new int[] { 10 };
                    flightData.maxRenewedRepetition = new int[] { 14 };
                    break;
            }

            if (cargo)

            {
                flightData.description = "Cargo Service";
                flightData.baggage   = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Reject };
                flightData.jetBridge = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Reject };
                flightData.catering  = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Reject };

                flightData.ULDLower  = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Reject, RequestLevel.Demand };
                flightData.ULDUpper  = new RequestLevel[] { RequestLevel.Reject, RequestLevel.Demand, RequestLevel.Demand };
            }

            return flightData;
        }
    }
}
