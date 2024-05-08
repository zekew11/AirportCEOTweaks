using System;
using System.Collections;
using UnityEngine;
using HarmonyLib;

namespace AirportCEOTweaks
{
	[HarmonyPatch(typeof(AirTrafficController))]
	static class Patch_FlightActivationTime
	{

		[HarmonyPostfix]
		[HarmonyPatch("InitializeAirTraffic")]
		public static void Patch_PreActivateFlights(AirTrafficController __instance)
		{
			if (AirportCEOTweaksConfig.PlannerChanges.Value)
			{
				__instance.StartCoroutine(Extend_AirTrafficController.PreActivateFlights());
			}
		}
	}
	[HarmonyPatch(typeof(AirTrafficController))]
	static class Patch_MaxSchedulableFlights
    {
		[HarmonyPrefix]
		[HarmonyPatch("GetMaxNbrOfScheduleableFlights")]
		public static bool Patch_MaxNbrOfScheduleableFlights(AirTrafficController __instance, int[] ___atcTowerMaxFlights, ref int __result)
        {
			if (GameDataController.Sandbox.UnlimitedFlights || !AirportCEOTweaksConfig.HigherFlightCap.Value)
			{
				return true;
			}

			PlaceableStructure[] arrayOfATCs = Singleton<BuildingController>.Instance.GetArrayOfSpecificStructureType(Enums.StructureType.ATCTower);
			if (arrayOfATCs.Length == 0)
			{
				__result = 0;
			}
			int[] atcOfSize = new int[] {0,0,0};
			int largestSize = -1;
			int maxFlights = 0;
			foreach (PlaceableStructure atc in arrayOfATCs)
			{
				if (atc.isBuilt)
				{
					
					int objectSize = (int)atc.objectSize;
					atcOfSize[objectSize] ++;
					if (objectSize > largestSize)
					{
						largestSize = objectSize;
					}
				}
			}
			if (largestSize > -1)
			{
				int towercounter = 0;
				for (int i = 2; i>=0; i--)
                {
					for (int j = atcOfSize[i].ClampMax(10); j>0; j--)
                    {
						maxFlights += (___atcTowerMaxFlights[i] / (float)Math.Pow(2,(towercounter))).RoundToNearest(5).RoundToIntLikeANormalPerson();
						towercounter++;
                    }
                }
			}

			PlaceableStructure[] arrayOfRadar = Singleton<BuildingController>.Instance.GetArrayOfSpecificStructureType(Enums.StructureType.RadarTower);

			int[] radarOfSize = new int[] { 0, 0, 0 };

			foreach (PlaceableStructure radar in arrayOfRadar)
			{
				if (radar.isBuilt)
				{
					int objectSize = (int)radar.objectSize;
					radarOfSize[objectSize]++;
				}
			}
			if (arrayOfRadar.Length > 0)
			{
				int radarcounter = 0;
				int[] radarMaxFlights = new int[] { 0, 50, 100 };
				for (int i = 2; i >= 0; i--)
				{
					for (int j = radarOfSize[i].ClampMax(10); j > 0; j--)
					{
						maxFlights += (radarMaxFlights[i] / (float)Math.Pow(2, (radarcounter))).RoundToNearest(5).RoundToIntLikeANormalPerson();
						radarcounter++;
					}
				}
			}

			if (maxFlights >= 500) { maxFlights = 9001; }

			__result = maxFlights;
			return false;
        }
    }
}