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
                int speed = Singleton<AirTrafficController>.Instance.GetAircraftModel(flight.aircraftTypeString).speedKMh;
                float distance = flight.arrivalRoute.routeDistance;
                double hours = Utils.Clamp((distance / (float)speed) + .75, minHours, maxHours);
                flightTime = TimeSpan.FromHours(hours);
                return flight.arrivalTimeDT - flightTime;
            }
            catch
            {
                //Debug.LogError("TakeoffTime Catch!");
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
   

