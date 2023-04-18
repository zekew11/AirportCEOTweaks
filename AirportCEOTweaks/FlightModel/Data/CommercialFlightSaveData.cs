using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks
{
    [Serializable]
    public struct CommercialFlightSaveData
    {
        public string referenceID;

        public string airlineString;
        public string parentFlightReferenceID;
        public string flightNumberString;
        public string arrivalDateTimeString;
        public string turnaroundDurationString;

        public int satisfaction;
        public int demerits;

        public FlightTypeData[] flightDatas;

        public Extend_CommercialFlightModel.TurnaroundService.TurnaroundServiceData[] turnaroundServiceDatas;
    }
}
