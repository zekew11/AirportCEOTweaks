using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks
{
    public struct CommercialFlightData
    {
        Enums.TravelDirection travelDirection;

        bool baggage;
        bool jetBridge;
        bool catering;
        bool cleaning;

        float seatDensity; // % multiplier
        float vIP;         // % vip pax
    }
}
