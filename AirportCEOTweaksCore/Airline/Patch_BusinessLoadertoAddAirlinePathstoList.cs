using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;

namespace AirportCEOFlightLimitTweak
{
	[HarmonyPatch(typeof(BusinessLoader))]
	static class Patch_BusinessLoadertoAddAirlinePathstoList
	{
        [HarmonyPatch("ProcessSingleBusiness")]
        [HarmonyPostfix]
        private static void Patch_GetAirlineData(ref BusinessLoadType busType)
        {
            //Patch_BuisinessLoaderForAirlineExt.currentBusType = busType;
            if (busType.businessType != Enums.BusinessType.Airline)
            {
                return;
            }
            
            AirportCEOTweaksCore.airlinePaths.Add(busType.path);
        }
	}


}
