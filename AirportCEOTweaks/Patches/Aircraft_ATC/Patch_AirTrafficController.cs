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