using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOFlightLimitTweak
{
    public static class AirlineModelStaticFunctions
    {
        public static AirlineModelExtended Extend<T>(this T a, ref AirlineModel me) where T:AirlineModel
        {
            if (a as AirlineModelExtended != null)
            {
                Debug.Log("Extend found already extended");
                return a as AirlineModelExtended;
            }
            Debug.Log("Extend about to create a new AirlineModelExtended");

            AirlineModelExtended aa = MakeAirlineModelExtended(ref me);
            Debug.Log("Extend created a new AirlineModelExtended");
            //me = aa;
            return (AirlineModelExtended)aa;
        }
    

        static AirlineModelExtended MakeAirlineModelExtended(ref AirlineModel me)
        {
            Debug.Log("Entered MakeAirlineModelExtended");
            Airline airline = Singleton<BusinessController>.Instance.GetAirline(me.businessName);
            Debug.Log("MakeAirlineModelExtended airline.name = " + airline.name);
            AirlineModelExtended aa = new AirlineModelExtended(airline, ref me);
            Debug.Log("MakeAirlineModelExtended aa.name = " + aa.businessName);
            return (AirlineModelExtended)aa;
        }

    }

}
