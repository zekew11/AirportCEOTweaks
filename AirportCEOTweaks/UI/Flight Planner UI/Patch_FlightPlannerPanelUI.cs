using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using System.IO.Ports;
using Michsky.UI.ModernUIPack;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(FlightPlannerPanelUI))]
    static class Patch_FlightPlannerPanelUI
    {
        [HarmonyPatch("InitializeFlightPlannerPanel")]
        [HarmonyPostfix]
        public static void AddMyExtension(FlightPlannerPanelUI __instance
                                        , PoolHandler<FlightSlotContainerUI> ___flightSlotPool
                                        , PoolHandler<FlightSlotContainerUI> ___allocationFlightSlotPool
                                        , PoolHandler<AirlineContainerUI> ___airlineSlotPool

                                        , SwitchManager ___smallAircraftFilter
                                        , SwitchManager ___mediumAircraftFilter
                                        , SwitchManager ___largeAircraftFilter

                                        , List<FlightSlotContainerUI> ___currentlyDisplayedFlightSlots
                                        , List<FlightSlotContainerUI> ___currentlyDisplayedAllocationFlightSlots
                                        , List<AirlineContainerUI> ___currentlyDisplayedAirlineSlots

                                        , TextMeshProUGUI ___allocationHelpText
                                        )
        {
            if (!__instance.gameObject.TryGetComponent<Extend_FlightPlannerPanelUI>(out Extend_FlightPlannerPanelUI extension))
            {
                extension = __instance.gameObject.AddComponent<Extend_FlightPlannerPanelUI>();
                extension.ConstructMe(__instance
                                       , ___flightSlotPool
                                       , ___allocationFlightSlotPool
                                       , ___airlineSlotPool

                                       , ___smallAircraftFilter
                                       , ___mediumAircraftFilter
                                       , ___largeAircraftFilter

                                       , ___currentlyDisplayedFlightSlots
                                       , ___currentlyDisplayedAllocationFlightSlots
                                       , ___currentlyDisplayedAirlineSlots

                                       , ___allocationHelpText
                    );
            }

            ///do stuff?


        }


        [HarmonyPatch("GenerateAirlineContainersInAllocationDisplayList")]
        [HarmonyPostfix]
        public static void SortAirlines(FlightPlannerPanelUI __instance)
        {
            if (!__instance.gameObject.TryGetComponent<Extend_FlightPlannerPanelUI>(out Extend_FlightPlannerPanelUI extension))
            {
                Debug.LogWarning("ACEO Tweaks | Warning: Expected flight planner extension... attempting to add one...");
                __instance.InitializeFlightPlannerPanel();                                                                  //this is what I postfix to add the extension
                if (!__instance.gameObject.TryGetComponent<Extend_FlightPlannerPanelUI>(out extension))
                {
                    Debug.LogError("ACEO Tweaks | ERROR: Failed to add extension to planner before sorting airlines");
                    return;
                }
            }

            extension.GenerateAirlineContainersInAllocationDisplayList();
        }


        [HarmonyPatch("GenerateFlightContainersInAllocationDisplayList")]
        [HarmonyPostfix]
        public static void SortFlights(FlightPlannerPanelUI __instance, AirlineModel airline)
        {
            if (!__instance.gameObject.TryGetComponent<Extend_FlightPlannerPanelUI>(out Extend_FlightPlannerPanelUI extension))
            {
                Debug.LogWarning("ACEO Tweaks | Warning: Expected flight planner extension... attempting to add one...");
                __instance.InitializeFlightPlannerPanel();                                                                  //this is what I postfix to add the extension
                if (!__instance.gameObject.TryGetComponent<Extend_FlightPlannerPanelUI>(out extension))
                {
                    Debug.LogError("ACEO Tweaks | ERROR: Failed to add extension to planner before sorting airlines");
                    return;
                }
            }

            extension.GenerateFlightContainersInAllocationDisplayList(airline);
        }

        [HarmonyPatch("FilterLargeAircraft")]
        [HarmonyPatch("FilterMediumAircraft")]
        [HarmonyPatch("FilterSmallAircraft")]
        [HarmonyPostfix]
        public static void RefreshAfterFilter(FlightPlannerPanelUI __instance)
        {
            __instance.RefreshPlanner();
        }
    }


}