using UnityEngine;
using HarmonyLib;
using System;

namespace AirportCEOTweaks
{
    public static class FlightModelUtils
    {
        public static void IfNoPAX(FlightModel flight) // CargoMod
        {
            if (AirportCEOTweaksConfig.fixes == false && AirportCEOTweaksConfig.cargoSystem == false) { return; }
            
            CommercialFlightModel commFlight = flight as CommercialFlightModel;
            if (commFlight.currentTotalNbrOfArrivingPassengers == 0 && commFlight.currentTotalNbrOfDepartingPassengers == 0)
            {
                flight.cabinCleaningServiceRequested = false;
                flight.cateringServiceRequested = false;

                flight.cabinCleaningServiceCompleted = true;
                flight.cateringServiceCompleted = true;

                flight.boardingRequested = false;
                flight.remoteBoardingRequested = false;
                flight.boardingCompleted = true;
                flight.remoteBoardingCompleted = true;

                flight.deboardingRequested = false;
                flight.deboardingCompleted = true;
                flight.remoteDeboardingRequested = false;
                flight.remoteDeboardingCompleted = true;

                flight.stairAccessServiceRequested = false;
                flight.stairAccessServiceCompleted = true;

                commFlight.checkinClosed = true;
                commFlight.boardingClosed = true;

                NightLightHide n;
                if (flight.Aircraft.gameObject.TryGetComponent<NightLightHide>(out n))
                { }
                else
                {
                    flight.Aircraft.gameObject.AddComponent<NightLightHide>();
                }
            }
        }

        public static DateTime TakeoffTime(FlightModel flight, out TimeSpan flightTime, float minHours=5f, float maxHours=12f)
        {
            try
            {
                int speed;
                try
                {
                    speed = Singleton<AirTrafficController>.Instance.GetAircraftModel(flight.aircraftTypeString).speedKMh;
                }
                catch
                {
                    Debug.LogError("ACEO Tweaks | ERROR: TakeoffTime could not get aircraft speed!");
                    speed = 750;
                }
                if(speed < 100)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: TakeoffTime got too low aircrarft speed!");
                    speed = 750;
                }
                float distance;
                try
                {
                    distance = flight.arrivalRoute.routeDistance;
                }
                catch
                {
                    Debug.LogError("ACEO Tweaks | ERROR: TakeoffTime could not get route distance!");
                    distance = 1000;
                }

                double hours = Utils.Clamp((distance / speed) + .75, minHours, maxHours);

                flightTime = TimeSpan.FromHours(hours);

                try
                {
                    return flight.arrivalTimeDT - flightTime;
                }
                catch
                {
                    return Singleton<TimeController>.Instance.GetCurrentContinuousTime() + flightTime;
                }
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | ERROR: TakeoffTime Catch!");
                flightTime = new TimeSpan(minHours.RoundToIntLikeANormalPerson(),0,0);
                DateTime cTime = Singleton<TimeController>.Instance.GetCurrentContinuousTime();
                //Debug.LogError("Flight Time = " + flightTime.ToString());
                //Debug.LogError("ArrivalTime = "+ flight.arrivalTimeDT.ToString());
                //Debug.LogError("CurrentTime = " + cTime.ToString());
                return cTime + flightTime;
            }
        }
    }
}
   

