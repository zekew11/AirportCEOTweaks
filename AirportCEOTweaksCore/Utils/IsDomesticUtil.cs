using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaksCore
{
    public static class IsDomesticUtil
    {
        public static bool IsDomestic(Country countryA, Country countryB = null)
        {
            if (countryA == null)
            {
                countryA = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country;
            }
            if (countryB == null)
            {
                countryB = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country;
            }
            return IsDomestic(new Country[] { countryA }, new Country[] { countryB });
        }
        public static  bool IsDomestic(Airport airportA, Airport airportB = null)
        {
            if (airportA == null)
            {
                airportA = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
            }
            if (airportB == null)
            {
                airportB = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
            }
            return IsDomestic(airportA.Country, airportB.Country);
        }
        public static bool IsDomestic(Country[] countriesA, Country[] countriesB = null)
        {
            if (countriesA == null)
            {
                countriesA = new Country[] { GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country };
            }
            if (countriesB == null)
            {
                countriesB = new Country[] { GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country };
            }

            foreach (Country countryA in countriesA)
            {
                foreach (Country countryB in countriesB)
                {
                    if (countryA == countryB)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
