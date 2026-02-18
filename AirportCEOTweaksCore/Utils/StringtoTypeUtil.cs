using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOTweaksCore
{
    public static class StringtoTypeUtil
    {
        public static List<Country> countrysFromCodes(string[] codes)
        {
            if (codes == null || codes.Length == 0)
            {
                return null;
            }

            HashSet<string> codeList = new HashSet<string>();

            foreach (string code in codes)
            {
                if (code == null || code == "")
                {
                    continue;
                }
                codeList.Add(code);
            }

            List<Country> countryList = new List<Country>();
            Country country;

            foreach (string code in codeList)
            {
                try
                {
                    country = TravelController.GetCountryByCode(code);
                    if (country != null && !countryList.Contains(country))
                    {
                        countryList.Add(country);
                    }
                }
                catch
                {
                    if (code == "")
                    {

                    }
                    else
                    {
                        Debug.LogError("ACEO Tweaks | ERROR: Could not get country for counrty code [" + code + "]!");
                    }
                }
            }
            return countryList;
        }

        private static Dictionary<string, Airport> airportByIATADictionary = new Dictionary<string, Airport>();
        public static Airport airportFromIATA(string code)
        {
            if (airportByIATADictionary.ContainsKey(code))
            {
                return airportByIATADictionary[code];
            }
            foreach (Airport airport in TravelController.airports)
            {
                if (airport.airportIATACode == code)
                {
                    airportByIATADictionary.Add(code, airport);
                    return airport;
                }
            }
            return null;
        }
    }
}
