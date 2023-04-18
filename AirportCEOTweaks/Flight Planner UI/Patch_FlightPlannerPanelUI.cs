using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using System.IO.Ports;
using Michsky.UI.ModernUIPack;
using System.Linq;

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
    public class Extend_FlightPlannerPanelUI : MonoBehaviour
    {
        private FlightPlannerPanelUI flightPlannerPanelUI;

        private PoolHandler<FlightSlotContainerUI> flightSlotPool;
        private PoolHandler<FlightSlotContainerUI> unAllocatedFlightSlotPool;
        private PoolHandler<AirlineContainerUI> airlineSlotPool;

        private SwitchManager smallAircraftFilter; //public in parent for some reason
        private SwitchManager mediumAircraftFilter;
        private SwitchManager largeAircraftFilter;

        private List<FlightSlotContainerUI> currentlyDisplayedFlightSlots;
        private List<FlightSlotContainerUI> currentlyDisplayedUnallocatedFlightSlots;
        private List<AirlineContainerUI> currentlyDisplayedAirlineSlots;

        private TextMeshProUGUI allocationHelpText;

        public void ConstructMe(FlightPlannerPanelUI panelUI
                               , PoolHandler<FlightSlotContainerUI> flightSlotPool
                               , PoolHandler<FlightSlotContainerUI> unAllocatedFlightSlotPool
                               , PoolHandler<AirlineContainerUI> airlineSlotPool

                               , SwitchManager smallAircraftFilter
                               , SwitchManager mediumAircraftFilter
                               , SwitchManager largeAircraftFilter

                               , List<FlightSlotContainerUI> currentlyDisplayedFlightSlots
                               , List<FlightSlotContainerUI> currentlyDisplayedUnallocatedFlightSlots
                               , List<AirlineContainerUI> currentlyDisplayedAirlineSlots

                               , TextMeshProUGUI allocationHelpText
                               )
        {
            flightPlannerPanelUI = panelUI;

            this.flightSlotPool = flightSlotPool;
            this.unAllocatedFlightSlotPool = unAllocatedFlightSlotPool;
            this.airlineSlotPool = airlineSlotPool;

            this.smallAircraftFilter = smallAircraftFilter;
            this.mediumAircraftFilter = mediumAircraftFilter;
            this.largeAircraftFilter = largeAircraftFilter;

            this.currentlyDisplayedFlightSlots = currentlyDisplayedFlightSlots;
            this.currentlyDisplayedUnallocatedFlightSlots = currentlyDisplayedUnallocatedFlightSlots;
            this.currentlyDisplayedAirlineSlots = currentlyDisplayedAirlineSlots;

            this.allocationHelpText = allocationHelpText;
        }
        public void HideUnallocatedFlight(FlightSlotContainerUI flightSlot)
        {
            unAllocatedFlightSlotPool.ReturnPoolObject(flightSlot);
            currentlyDisplayedUnallocatedFlightSlots.Remove(flightSlot);
        }
        public void HideAirlineContainer(AirlineContainerUI airlineContainer)
        {
            airlineSlotPool.ReturnPoolObject(airlineContainer);
            currentlyDisplayedAirlineSlots.Remove(airlineContainer);
        }
        public void GenerateAirlineContainersInAllocationDisplayList()
        {
            int count = this.currentlyDisplayedAirlineSlots.Count;
            for (int i = 0; i < count; i++)
            {
                this.airlineSlotPool.ReturnPoolObject(this.currentlyDisplayedAirlineSlots[i]);
            }
            this.currentlyDisplayedAirlineSlots.Clear();

            ///
            ///
            ///

            List<Enums.ThreeStepScale> allowedSizes = new List<Enums.ThreeStepScale>();
            if (smallAircraftFilter.isOn) { allowedSizes.Add(Enums.ThreeStepScale.Small); }
            if (mediumAircraftFilter.isOn) { allowedSizes.Add(Enums.ThreeStepScale.Medium); }
            if (largeAircraftFilter.isOn) { allowedSizes.Add(Enums.ThreeStepScale.Large); }

            List<AirlineModel> list = (from a in SingletonNonDestroy<BusinessController>.Instance.GetArrayOfActiveBusinessesByType<AirlineModel>()
                                       orderby (from b in a.flightListObjects
                                                where allowedSizes.Contains(b.weightClass) && b.isAllocated == false
                                                select b).Count() descending
                                       select a).ToList<AirlineModel>();
            int num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].isMasterContract)
                {
                    num++;
                    this.LoadAirlineIntoContaier(list[i]);
                }
            }
            if (Singleton<AirTrafficController>.Instance.EmergencyFlightExists(Enums.FlightType.Commercial))
            {
                this.LoadAirlineIntoContaier(null);
            }
            this.allocationHelpText.transform.parent.gameObject.AttemptEnableDisableGameObject(num == 0);
            if (num == 0)
            {
                this.allocationHelpText.text = LocalizationManager.GetLocalizedValue("FlightPlannerPanelUI.cs.key.42");
            }
        }
        private void LoadAirlineIntoContaier(AirlineModel airline)
        {
            AirlineContainerUI airlineContainerUI;
            if (this.airlineSlotPool.TryGetPoolObject(false, out airlineContainerUI))
            {
                if (airline != null)
                {
                    airlineContainerUI.SetContainerValues(airline, new Action<AirlineModel>(flightPlannerPanelUI.GenerateFlightContainersInAllocationDisplayList));
                }
                else
                {
                    airlineContainerUI.SetContainerAsEmergency(new Action(flightPlannerPanelUI.GenerateEmergencyFlightContainersInAllocationDisplayList));
                }
                airlineContainerUI.Launch();
                if (airline != null)
                {
                    airlineContainerUI.transform.SetAsLastSibling();
                }
                this.currentlyDisplayedAirlineSlots.Add(airlineContainerUI);
            }
        }
        public void GenerateFlightContainersInAllocationDisplayList(AirlineModel airline)
        {

            int count1 = this.currentlyDisplayedUnallocatedFlightSlots.Count;
            for (int i = 0; i < count1; i++)
            {
                this.currentlyDisplayedUnallocatedFlightSlots[i].transform.SetParent(this.unAllocatedFlightSlotPool.GetParent());
                this.unAllocatedFlightSlotPool.ReturnPoolObject(this.currentlyDisplayedUnallocatedFlightSlots[i]);
            }
            this.currentlyDisplayedUnallocatedFlightSlots.Clear();

            ///
            ///
            ///

            HashSet<string> hashSet = new HashSet<string>();
            List<CommercialFlightModel> flightListObjects = (from a in airline.flightListObjects
                                                             orderby a.weightClass descending, a.arrivalRoute.routeDistance descending
                                                             select a)
                                                             .ToList<CommercialFlightModel>();
            int count2 = flightListObjects.Count;
            int num = 0;
            for (int i = 0; i < count2; i++)
            {
                CommercialFlightModel commercialFlightModel = flightListObjects[i];
                FlightSlotContainerUI flightSlotContainerUI;
                if (!commercialFlightModel.isAllocated && !commercialFlightModel.isCompleted && !hashSet.Contains(commercialFlightModel.departureFlightNbr) && (commercialFlightModel.weightClass != Enums.ThreeStepScale.Small || smallAircraftFilter.isOn) && (commercialFlightModel.weightClass != Enums.ThreeStepScale.Medium || mediumAircraftFilter.isOn) && (commercialFlightModel.weightClass != Enums.ThreeStepScale.Large || largeAircraftFilter.isOn) && this.unAllocatedFlightSlotPool.TryGetPoolObject(true, out flightSlotContainerUI))
                {
                    flightSlotContainerUI.SetContainerValues(commercialFlightModel, "");
                    flightSlotContainerUI.transform.SetParent(this.unAllocatedFlightSlotPool.GetParent());
                    hashSet.Add(commercialFlightModel.departureFlightNbr);
                    this.currentlyDisplayedUnallocatedFlightSlots.Add(flightSlotContainerUI);
                    num++;
                }
            }
            this.allocationHelpText.transform.parent.gameObject.AttemptEnableDisableGameObject(num == 0);
            if (num == 0)
            {
                this.allocationHelpText.text = LocalizationManager.GetLocalizedValue("FlightPlannerPanelUI.cs.key.45");
            }
        }
    }


}