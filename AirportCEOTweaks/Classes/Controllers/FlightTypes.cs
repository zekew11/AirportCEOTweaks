using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft;
using Random = UnityEngine.Random;




namespace AirportCEOTweaks
{
    public static class FlightTypes
    {
        public enum FlightType
        {
            Vanilla,

            Economy,
            Commuter,
            Mainline,
            Flagship,
            VIP,

            Positioning,
            Divert,

            Cargo,
            SpecialCargo
        }
        public enum TurnaroundType
        {
            Vanilla,

            FuelOnly,
            Reduced,
            Normal,
            Full,
            Exended,

            Maintenance,

            Cargo,
            SpecialCargo
        }
    }
    public class FlightTypesController : SingletonNonDestroy<FlightTypesController>
    { 
        public FlightTypes.TurnaroundType GetTurnaroundType(CommercialFlightModel commercialFlightModel, FlightTypes.FlightType inBound, FlightTypes.FlightType outBound)
        {
            List<FlightTypes.TurnaroundType> types = new List<FlightTypes.TurnaroundType>();
            FlightTypes.FlightType switchOn = inBound;

            for (var i = 0; i < 2; i++)
            {
                switch (switchOn)
                {
                    case FlightTypes.FlightType.Economy:
                        types.Add(FlightTypes.TurnaroundType.Reduced);
                        types.Add(FlightTypes.TurnaroundType.Reduced);
                        types.Add(FlightTypes.TurnaroundType.Normal);
                        break;

                    case FlightTypes.FlightType.Commuter:
                        types.Add(FlightTypes.TurnaroundType.Reduced);
                        types.Add(FlightTypes.TurnaroundType.Normal);
                        break;

                    case FlightTypes.FlightType.Mainline:
                        types.Add(FlightTypes.TurnaroundType.Reduced);
                        types.Add(FlightTypes.TurnaroundType.Normal);
                        types.Add(FlightTypes.TurnaroundType.Normal);
                        types.Add(FlightTypes.TurnaroundType.Normal);
                        types.Add(FlightTypes.TurnaroundType.Normal);
                        break;

                    case FlightTypes.FlightType.Flagship:
                        types.Add(FlightTypes.TurnaroundType.Normal);
                        types.Add(FlightTypes.TurnaroundType.Full);
                        types.Add(FlightTypes.TurnaroundType.Full);
                        types.Add(FlightTypes.TurnaroundType.Exended);
                        break;

                    case FlightTypes.FlightType.VIP:
                        types.Add(FlightTypes.TurnaroundType.Full);
                        types.Add(FlightTypes.TurnaroundType.Exended);
                        //types.Add(FlightTypes.TurnaroundType.FuelOnly);
                        break;

                    case FlightTypes.FlightType.Positioning:
                        types.Add(FlightTypes.TurnaroundType.Maintenance);
                        if (inBound == FlightTypes.FlightType.Cargo)
                        { types.Add(FlightTypes.TurnaroundType.Cargo); }
                        else if (inBound == FlightTypes.FlightType.SpecialCargo)
                        { types.Add(FlightTypes.TurnaroundType.SpecialCargo); types.Add(FlightTypes.TurnaroundType.FuelOnly); }
                        else if (inBound == FlightTypes.FlightType.Positioning)
                        { types.Add(FlightTypes.TurnaroundType.FuelOnly); types.Add(FlightTypes.TurnaroundType.Maintenance); }
                        else if (inBound == FlightTypes.FlightType.Divert)
                        { types.Add(FlightTypes.TurnaroundType.Maintenance); types.Add(FlightTypes.TurnaroundType.FuelOnly); }
                        else
                        { types.Add(GetTurnaroundType(commercialFlightModel, inBound, inBound)); }
                        break;
                    case FlightTypes.FlightType.Cargo:
                        types.Add(FlightTypes.TurnaroundType.Cargo);
                        types.Add(FlightTypes.TurnaroundType.Cargo);
                        types.Add(FlightTypes.TurnaroundType.Cargo);
                        types.Add(FlightTypes.TurnaroundType.FuelOnly);
                        break;
                    case FlightTypes.FlightType.SpecialCargo:
                        types.Add(FlightTypes.TurnaroundType.SpecialCargo);
                        types.Add(FlightTypes.TurnaroundType.FuelOnly);
                        break;
                    default:
                        types.Add(FlightTypes.TurnaroundType.Vanilla);
                        break;
                }
                switchOn = outBound;
            }

            int seed = commercialFlightModel.departureRoute.routeNbr;
            UnityEngine.Random.InitState(seed);
            return types.ElementAt(UnityEngine.Random.Range(0, types.Count - 1));
        }

        public TimeSpan GetTurnaroundTime(Extend_CommercialFlightModel extCommercialFlightModel, FlightTypes.TurnaroundType turnaroundType, float Playerbias = 1f)
        {
            TimeSpan timeSpan = new TimeSpan();
            float timeMins = 180f;

            Playerbias = AirportCEOTweaksConfig.plannerChanges ? Playerbias : 1f;

            float bias = (Playerbias - 1f) + extCommercialFlightModel.turnaroundBias;
          

            Enums.ThreeStepScale size = extCommercialFlightModel.WeightClass;
            Enums.ThreeStepScale lg = Enums.ThreeStepScale.Large;
            Enums.ThreeStepScale md = Enums.ThreeStepScale.Medium;
            //Enums.ThreeStepScale sm = Enums.ThreeStepScale.Small;


            switch (turnaroundType)
            {
                
                case FlightTypes.TurnaroundType.Cargo: timeMins = size == lg ? (600f * bias).Clamp(360f, 1080f) : size == md ? (480f * bias).Clamp(300f, 840f) : (300f * bias).Clamp(180f, 600f); break;
                case FlightTypes.TurnaroundType.SpecialCargo: timeMins = size == lg ? (660f * bias).Clamp(360f, 1080f) : size == md ? (510f * bias).Clamp(300f, 840f) : (360f * bias).Clamp(180f, 600f); break;

                case FlightTypes.TurnaroundType.FuelOnly: timeMins = size == lg ? (150f * bias).Clamp(90f, 195f) : size == md ? (135f * bias).Clamp(75f, 180f) : (105f * bias).Clamp(60f, 150f); break;

                case FlightTypes.TurnaroundType.Reduced: timeMins = size == lg ? (200f * bias).Clamp(150f, 270f) : size == md ? (200f * bias).Clamp(120f, 240f) : (120f * bias).Clamp(90f, 180f); break;
                case FlightTypes.TurnaroundType.Normal: timeMins = size == lg ? 300f*bias : size == md ? 240f*bias : 180f*bias; break;
                case FlightTypes.TurnaroundType.Full: timeMins = size == lg ? (330f * bias).Clamp(255f, 480f) : size == md ? (270f * bias).Clamp(210f, 330f) : (210f * bias).Clamp(120f, 270f); break;

                case FlightTypes.TurnaroundType.Exended: timeMins = size == lg ? 600f * bias : size == md ? 480f * bias : 420f * bias; break;

                case FlightTypes.TurnaroundType.Maintenance: timeMins = size == lg ? (1440f * bias).ClampMin(1080) : size == md ? (1080f * bias).ClampMin(720) : (720f * bias).ClampMin(480); break;



                default: return TimeSpan.FromMinutes(AirTrafficController.GetTurnaroundTime(size, extCommercialFlightModel.IsEmergency, false).TotalMinutes*Playerbias);
            }

            timeSpan = TimeSpan.FromMinutes(timeMins);
            return timeSpan;
        }

        public FlightTypesController()
        { }

        public void ResetForMainMenu()
        { }

    }
}
