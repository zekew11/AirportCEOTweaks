using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirportCEOTweaksCore;
using UnityEngine;

namespace AirportCEONationality
{
    class NationalityIFlightGenerator : IFlightGenerator
    {
        public bool OverrideHarmonyPrefix => false;

        public bool GenerateFlight(AirlineModel airlineModel, bool isEmergency, bool isAmbulance)
        {
            AirlineModelExtended airlineME = airlineModel.ExtendAirlineModel(ref airlineModel);

            //Check Possible to Gen a Flight



            if (airlineModel.fleetCount.Length == 0)
            {
                Debug.LogWarning("ACEO Tweaks | WARN: Generate flight for " + airlineModel.businessName + " failed due to FleetCount.Length==0");
                airlineModel.CancelContract();
                Debug.LogWarning("ACEO Tweaks | WARN: Airline " + airlineModel.businessName + "contract canceled due to no valid fleet!");
                return true;
            } //error catch
            if (airlineModel.aircraftFleetModels.Length == 0)
            {
                Debug.LogWarning("ACEO Tweaks | WARN: Generate flight for " + airlineModel.businessName + " failed due to FleetModels.Length==0");
                airlineModel.CancelContract();
                Debug.LogWarning("ACEO Tweaks | WARN: Airline " + airlineModel.businessName + "contract canceled due to no valid fleet!");
                return true;
            }

            //
            //
            // Preselect route number ...............................................................................................
            //
            //

            int maxflightnumber = (((int)airlineModel.businessClass + 3) ^ 2) * 50 + Utils.RandomRangeI(100f, 200f);
            int flightnumber = Utils.RandomRangeI(1f, maxflightnumber);

            //duplicate checking
            for (int i = 0; ; i++)
            {
                if (Singleton<ModsController>.Instance.FlightsByFlightNumber(airlineModel, airlineModel.airlineFlightNbr + flightnumber).Count > 0)
                {
                    flightnumber = Utils.RandomRangeI(1f, maxflightnumber);
                    if (i > 200)
                    {
                        Debug.LogWarning("ACEO Tweaks | WARN: Generate flight for " + airlineModel.businessName + " failed due to no available flight number");
                        return false;
                    }
                }
                else
                {
                    break;
                }
            } //no duplicate flight #s

            //Select Aircraft



            //Select Route

            //Instantiate the Flights
        }
    }
}
