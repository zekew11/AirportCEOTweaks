using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOTweaksCore
{
    public class DefaultFlightGenerator : IFlightGenerator
    {
        public bool OverrideHarmonyPrefix { get; set; } = false;

        public bool GenerateFlight(AirlineModel airlineModel, bool isEmergency, bool isAmbulance)
        {
            //return airlineModel.ExtendAirlineModel(ref airlineModel).GenerateFlight(isEmergency, isAmbulance);
            Debug.Log("DefaultFlightGeneratorGenerateFlight00");
            OverrideHarmonyPrefix = true;
            bool success = airlineModel.GenerateFlight(isEmergency, isAmbulance);
            OverrideHarmonyPrefix = false;
            return success;
        }
    }

    public interface IFlightGenerator
    {
        bool OverrideHarmonyPrefix { get; }

        bool GenerateFlight(AirlineModel airlineModel, bool isEmergency, bool isAmbulance);
    }
}
