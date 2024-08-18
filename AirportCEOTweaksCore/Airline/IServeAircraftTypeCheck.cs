using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaksCore
{

    public class ServeAircraftByDLCCheck : IServeAircraftTypeCheck
    {
        public bool CanServeType(string aircraftType)
        {
            if (!AirTrafficController.OwnsDLCAircraft(aircraftType))
            {
                return false;
            }
            return true;
        }
    }
    public class ServeAircraftByStandSizeCheck : IServeAircraftTypeCheck
    {
        public bool CanServeType(string aircraftType)
        {
            AircraftModel aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(aircraftType);
            if (aircraftModel.isHelicopter)
            {
                switch (aircraftModel.weightClass)
                {
                    case Enums.ThreeStepScale.Small: if (Singleton<AirportController>.Instance.hasSmallHelipad) { return true; } break;
                    case Enums.ThreeStepScale.Medium: if (Singleton<AirportController>.Instance.hasMediumHelipad) { return true; } break;
                }
            }
            else
            {
                switch (aircraftModel.weightClass)
                {
                    case Enums.ThreeStepScale.Small: return true;
                    case Enums.ThreeStepScale.Medium: if (Singleton<AirportController>.Instance.hasMediumStand || Singleton<AirportController>.Instance.hasLargeStandThatAcceptsMedium) { return true; } break;
                    case Enums.ThreeStepScale.Large: if (Singleton<AirportController>.Instance.hasLargeStand) { return true; } break;
                }
            }
            return false;
        }
    }
    public class ServeAircraftByRunwaySizeCheck : IServeAircraftTypeCheck
    {
        public bool CanServeType(string aircraftType)
        {
            AircraftModel aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(aircraftType);
            if (aircraftModel.isHelicopter)
            {
                return true;
            }
            switch (aircraftModel.weightClass)
            {
                case Enums.ThreeStepScale.Small: return true;
                case Enums.ThreeStepScale.Medium: if (Singleton<AirportController>.Instance.hasMediumClassRunway || Singleton<AirportController>.Instance.hasLargeClassRunway) { return true; } break;
                case Enums.ThreeStepScale.Large: if (Singleton<AirportController>.Instance.hasLargeStand) { return true; } break;
            }
            return false;
        }
    }


    public interface IServeAircraftTypeCheck
    {
        public bool CanServeType(string aircraftType);
    }
}
