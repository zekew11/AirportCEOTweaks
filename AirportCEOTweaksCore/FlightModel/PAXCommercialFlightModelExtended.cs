using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOTweaksCore
{
    public class PAXCommercialFlightModelExtended : CommercialFlightModel, IClassExtension, IEquatable<FlightModel>
    {
        public PAXCommercialFlightModelExtended(string airlineReference, bool isReoccuring, string aircraftType, Route arrivalRoute, Route departureRoute) : base(airlineReference, isReoccuring, aircraftType, arrivalRoute, departureRoute)
        {
            this.flightType = Enums.FlightType.Commercial;
            this.airlineReferenceID = airlineReference;
            this.isReoccuring = isReoccuring;
            AircraftModel aircraftModel = this.InitializeFlightDetails(aircraftType, arrivalRoute, departureRoute);
            this.departureFlightNbr = this.Airline.airlineFlightNbr + arrivalRoute.routeNbr;
            this.TurnaroundTime = AirTrafficController.GetTurnaroundTime(aircraftModel.weightClass, this.isEmergency, false);
            this.totalNbrOfArrivingPassengers = Utils.RandomRangeI((float)aircraftModel.MaxPax * 0.5f, (float)aircraftModel.MaxPax).ClampMin(1);
            this.totalNbrOfDepartingPassengers = Utils.RandomRangeI((float)aircraftModel.MaxPax * 0.5f, (float)aircraftModel.MaxPax).ClampMin(1);
            this.SetFlightPassengerTrafficValues(1f);
            this.checkInDuration = new TimeSpan(0, this.currentTotalNbrOfDepartingPassengers.ClampMin(30) / this.NbrOfRequiredCheckInDesks, 0);
            this.GenerateSeats();
        }
        
        public bool Equals(FlightModel other)
        {
            if (this.referenceID == other.referenceID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetupExtend(object oringinal)
        {
            if ((oringinal is CommercialFlightModel) && oringinal != null)
            {
                //do stuff
            }
            else
            {
                Debug.LogError("ACEO Tweaks | ERROR: tried to setup paxcommercialflightmodelextended with null original or origninal not commercialflightmodel");
            }
        }

        public DateTime EnrouteToPlayerAirportDateTime(out TimeSpan flightTime, float minFlightLengthHours = 5f, float maxFlightLengthHours = 12f)
        {
            try
            {
                int speed;
                try
                {
                    speed = Singleton<AirTrafficController>.Instance.GetAircraftModel(this.aircraftTypeString).speedKMh;
                }
                catch
                {
                    Debug.LogError("ACEO Tweaks | ERROR: TakeoffTime could not get aircraft speed for aircraft " + this.aircraftTypeString);
                    speed = 750;
                }
                if (speed < 100)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: TakeoffTime got too low aircrarft speed (" + speed + " KM/H) for aircraft " + this.aircraftTypeString);
                    speed = 750;
                }
                float distance;
                try
                {
                    distance = this.arrivalRoute.routeDistance;
                }
                catch
                {
                    Debug.LogError("ACEO Tweaks | ERROR: TakeoffTime could not get route distance!");
                    distance = 1000;
                }

                double hours = Utils.Clamp((distance / speed) + .75, minFlightLengthHours, maxFlightLengthHours);

                flightTime = TimeSpan.FromHours(hours);

                try
                {
                    return this.arrivalTimeDT - flightTime;
                }
                catch
                {
                    return Singleton<TimeController>.Instance.GetCurrentContinuousTime() + flightTime;
                }
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | ERROR: TakeoffTime Catch!");
                flightTime = new TimeSpan(minFlightLengthHours.RoundToIntLikeANormalPerson(), 0, 0);
                DateTime cTime = Singleton<TimeController>.Instance.GetCurrentContinuousTime();
                //Debug.LogError("Flight Time = " + flightTime.ToString());
                //Debug.LogError("ArrivalTime = "+ flight.arrivalTimeDT.ToString());
                //Debug.LogError("CurrentTime = " + cTime.ToString());
                return cTime + flightTime;
            }
        }

        public DateTime EnrouteToPlayerAirportDateTime(float minFlightLengthHours = 5f, float maxFlightLengthHours = 12f)
        {
            TimeSpan toss;
            return EnrouteToPlayerAirportDateTime(out toss, minFlightLengthHours, maxFlightLengthHours);
        }
        new void SetFlightPassengerTrafficValues(float modifier)
        {
            base.SetFlightPassengerTrafficValues(modifier);
        }
    }
}
