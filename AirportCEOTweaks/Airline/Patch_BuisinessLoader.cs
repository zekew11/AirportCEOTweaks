using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.IO;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(BusinessLoader))]
    class Patch_BuisinessLoader
    {
        [HarmonyPatch("ProcessSingleBusiness")]
        [HarmonyPostfix]
        private static void Patch_GetAirlineData(BusinessLoadType busType)
        {
            if (busType.businessType != Enums.BusinessType.Airline)
            {
                return;
            }
            AirportCEOTweaks.airlinePaths.Add(busType.path);
        }
    }
}
