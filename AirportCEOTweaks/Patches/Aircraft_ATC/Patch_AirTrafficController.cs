using System;
using System.Collections;
using UnityEngine;
using HarmonyLib;

namespace AirportCEOTweaks
{
	[HarmonyPatch(typeof(AirTrafficController))]
	static class Patch_FlightActivation
	{

		[HarmonyPostfix]
		[HarmonyPatch("InitializeAirTraffic")]
		public static void Patch_PreActivateFlights(AirTrafficController __instance)
		{
			if (AirportCEOTweaksConfig.fixes || AirportCEOTweaksConfig.plannerChanges)
			{
				__instance.StartCoroutine(Extend_AirTrafficController.PreActivateFlights());
			}
		}
	}
	[HarmonyPatch(typeof(AirTrafficController))]
	static class Patch_MaxFlights
    {
		[HarmonyPrefix]
		[HarmonyPatch("GetMaxNbrOfScheduleableFlights")]
		public static bool Patch_MaxNbrOfScheduleableFlights(AirTrafficController __instance, int[] ___atcTowerMaxFlights, ref int __result)
        {
			if (GameDataController.Sandbox.UnlimitedFlights)
			{
				return true;
			}

			//Debug.LogError("Tweaked max flights");

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
				//atcOfSize[largestSize]--;
				//maxFlights += ___atcTowerMaxFlights[largestSize];

				int towercounter = 0;
				for (int i = 2; i>=0; i--)
                {
					for (int j = atcOfSize[i].ClampMax(10); j>0; j--)
                    {
						maxFlights += (___atcTowerMaxFlights[i] / (float)Math.Pow(2,(towercounter))).RoundToNearest(5).RoundToIntLikeANormalPerson();
						towercounter++;
                    }
                }
				//maxFlights += (atcOfSize[0] * (.6f) * ___atcTowerMaxFlights[0]).RoundToIntLikeANormalPerson().ClampMax(___atcTowerMaxFlights[0]);
				//maxFlights += (atcOfSize[1] * (.6f) * ___atcTowerMaxFlights[1]).RoundToIntLikeANormalPerson().ClampMax(___atcTowerMaxFlights[1]);
				//maxFlights += (atcOfSize[2] * (.6f) * ___atcTowerMaxFlights[2]).RoundToIntLikeANormalPerson().ClampMax(___atcTowerMaxFlights[2]);
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
				//maxFlights += (radarOfSize[1] * 50).ClampMax(80);
				//maxFlights += (radarOfSize[2] * 100).ClampMax(200);
			}

			if (maxFlights >= 500) { maxFlights = 999; }

			__result = maxFlights;
			return false;
        }
    }

	public class Extend_AirTrafficController
	{
		public static IEnumerator PreActivateFlights()
		{
			while (!SaveLoadGameDataController.loadComplete)
			{
				yield return null;
			}
			DateTime cTime;
            int hours;
            for (; ; )
			{
				yield return Utils.oneSecondWait;
				cTime = Singleton<TimeController>.Instance.GetCurrentContinuousTime();
				bool flag = cTime.Second % 15 == 0;


				for (int i = 0; i < Singleton<AirTrafficController>.Instance.flights.Length; i++)
				{
					FlightModel f = Singleton<AirTrafficController>.Instance.flights.array[i];
					if (f != null && !f.isCompleted && !f.isCanceled && f.isAllocated && !f.isActivated)
					{
						hours = (f.arrivalTimeDT - cTime).Hours;

						if (hours > 24 || (hours > 8 && !flag)) { continue; }
                        if (Extend_FlightModel.TakeoffTime(f, out TimeSpan fT,3f,24f) < cTime)
						{
							f.isActivated = true;
							f.ActivateFlight();
						}

					}
					if (i % 15 == 0) { yield return Utils.extremelyShortWait; cTime = Singleton<TimeController>.Instance.GetCurrentContinuousTime(); }
				}
			}
			//yield break;
		}
	}

}