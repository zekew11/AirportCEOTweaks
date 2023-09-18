using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using System.Reflection;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(BusinessLoader))]
    class Patch_BuisinessLoader
    {
        [HarmonyPatch("ProcessSingleBusiness")]
        [HarmonyPostfix]
        private static void Patch_GetAirlineData(BusinessLoadType busType)
        {
			//Patch_BuisinessLoaderForAirlineExt.currentBusType = busType;
			if (busType.businessType != Enums.BusinessType.Airline)
            {
                return;
            }
            AirportCEOTweaks.airlinePaths.Add(busType.path);
        }
    }

	/*    -------- Code below tries to patch a generic and does so, but fails to disabmiguate the type. Leaving as an example for future work.

    [HarmonyPatch]
    public static class Patch_BuisinessLoaderForAirlineExt
    {
		
		public static BusinessLoadType currentBusType;

		static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(BusinessLoader).GetMethodExt("LoadBusinessModels", BindingFlags.NonPublic | BindingFlags.Instance, typeof(string)).MakeGenericMethod(typeof(Airline)); //
		}

        static bool Prefix(BusinessLoader __instance, string files, Business __result, Action<Business> ___addToBusinesses)
        {
			Debug.Log("ACEO Tweaks | Log: Doing a prefix!");
			
			if (currentBusType.businessType != Enums.BusinessType.Airline)
            {
				return true;
            }
			
			__result = default(Airline);
			string[] files2 = Directory.GetFiles(files, "*.json");
			if (files2.Length != 0)
			{
				string path = files2[0].Replace("\\", "/");
				try
				{
					__result = JsonConvert.DeserializeObject<Airline>(Utils.ReadFile(path));
					if (__result.customDescription != null)
					{
						string description;
						if (__result.customDescription.TryGetValue(GameSettingManager.GameLanguage, out description))
						{
							__result.description = description;
						}
						else
						{
							__result.description = LocalizationManager.GetLocalizedValue(__result.description);
						}
					}
					else if (__result.description.Contains("business"))
					{
						__result.description = LocalizationManager.GetLocalizedValue(__result.description);
					}
					__result.logoPath = files + "/" + __result.logoPath;
					__result.invLogoPath = files + "/" + __result.invLogoPath;
					if (!__result.name.Equals("GA"))
					{
						___addToBusinesses(__result);
					}
					if (__result is Airline)
					{
						Airline airline = __result as Airline;
						if (airline.isCustom)
						{
							LiveryImporter.LoadCustomLivery(files, airline.name);
						}
						int num = airline.fleet.Length;
						if (airline.fleetCount == null || airline.fleetCount.Length != num)
						{
							airline.fleetCount = new int[num];
							for (int i = 0; i < num; i++)
							{
								airline.fleetCount[i] = 1;
							}
						}
					}
				}
				catch (ArgumentException)
				{
					Debug.LogError("ERROR: An error occurred while reading JSON file: " + Path.GetFileName(path));
				}
			}
			Debug.Log("ACEO Tweaks | Log: Done a prefix!");
			return false;
		}

		*
    }*/
}
