using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOAircraft
{
    public static class AirlineModelStaticFunctions
    {
        public static AirlineModelExtended Extend(this AirlineModel a, ref AirlineModel me)
        {
            if (a as AirlineModelExtended != null)
            {
                Debug.Log("Extend found already extended");
                return a as AirlineModelExtended;
            }
            Debug.Log("Extend about to create a new AirlineModelExtended");
            AirlineModelExtended aa = null;
            try
            {
                aa = MakeAirlineModelExtended(me);
                Debug.Log("Extend created a new AirlineModelExtended");
            }
            catch
            {
                Debug.LogError("Extend failed to create a new AirlineModelExtended");
            }
            //me = aa;
            return aa;
        }

        static AirlineModelExtended MakeAirlineModelExtended(AirlineModel a)
        {
            Airline airline = Singleton<BusinessController>.Instance.GetAirline(a.businessName);
            
            a = new AirlineModelExtended(airline, ref a); //using the constructor airline = this
            return (AirlineModelExtended)a;
        }

    }

}
