using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;

namespace AirportCEOTweaks
{
	[HarmonyPatch(typeof(StandModel))]
	class Patch_StandModelForFlightTypes
    {
        [HarmonyPatch("CheckConnections")]
        [HarmonyPrefix]
        public static bool Patch_AllowMedNoBaggage(ref StandModel __instance)
        {
			if (!AirportCEOTweaksConfig.flightTypes)
			{ return true; }

			__instance.isBoardingDeskConnected = __instance.HasAssignedBoardingDesk;
			__instance.isSecurityCheckpointConnected = __instance.HasConnectedSecurityStation;
			//__instance.taxiwayConnectionValidator.CheckIfConnected(__instance.shouldBeVisible);
			//__instance.boardingDeskConnector.gameObject.AttemptEnableDisableGameObject(!__instance.AcceptGA && !__instance.isBoardingDeskConnected);
			if (__instance.HasTaxiwayConnection != __instance.isTaxiwayConnected)
			{
				Singleton<TaxiwayController>.Instance.UpdateAllRunwayConnections();
				__instance.isTaxiwayConnected = __instance.HasTaxiwayConnection;
			}
			if (__instance.isBuilt)
			{
				if (__instance.AcceptGA)
				{
					return true;
				}
				else
				{
					if (!__instance.isTaxiwayConnected || !__instance.isBoardingDeskConnected || !__instance.isSecurityCheckpointConnected || !Singleton<AirTrafficController>.Instance.IsStandConnectedToARunway(__instance) || __instance.IsMissingAttachedTerminal)
					{
						return true;
					}
					Singleton<NotificationController>.Instance.RemoveNotification(Enums.NotificationType.ObjectNotFunctional, __instance.ReferenceBytes);
				}
			}

			return false;
        }
    }
}
