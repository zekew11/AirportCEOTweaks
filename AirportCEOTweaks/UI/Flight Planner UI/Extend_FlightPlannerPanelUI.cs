using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using Michsky.UI.ModernUIPack;
using System.Linq;
using UnityEngine.UI;

namespace AirportCEOTweaks
{
    public class Extend_FlightPlannerPanelUI : MonoBehaviour
    {
        private FlightPlannerPanelUI flightPlannerPanelUI;

        private PoolHandler<FlightSlotContainerUI> flightSlotPool;
        private PoolHandler<FlightSlotContainerUI> unAllocatedFlightSlotPool;
        private Transform unAllocatedFlightSlotPoolParent;
        private PoolHandler<AirlineContainerUI> airlineSlotPool;

        private SwitchManager smallAircraftFilter; //public in parent for some reason
        private SwitchManager mediumAircraftFilter;
        private SwitchManager largeAircraftFilter;

        private List<FlightSlotContainerUI> currentlyDisplayedFlightSlots;
        private List<FlightSlotContainerUI> currentlyDisplayedUnallocatedFlightSlots;
        private List<AirlineContainerUI> currentlyDisplayedAirlineSlots;

        private TextMeshProUGUI allocationHelpText;
        private Image transparentOverlay;

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
                               , Image transparentOverlay
                               )
        {
            flightPlannerPanelUI = panelUI;

            this.flightSlotPool = flightSlotPool;
            this.unAllocatedFlightSlotPool = unAllocatedFlightSlotPool;
            this.unAllocatedFlightSlotPoolParent = unAllocatedFlightSlotPool.GetParent();
            this.airlineSlotPool = airlineSlotPool;

            this.smallAircraftFilter = smallAircraftFilter;
            this.mediumAircraftFilter = mediumAircraftFilter;
            this.largeAircraftFilter = largeAircraftFilter;

            this.currentlyDisplayedFlightSlots = currentlyDisplayedFlightSlots;
            this.currentlyDisplayedUnallocatedFlightSlots = currentlyDisplayedUnallocatedFlightSlots;
            this.currentlyDisplayedAirlineSlots = currentlyDisplayedAirlineSlots;

            this.allocationHelpText = allocationHelpText;
            this.transparentOverlay = transparentOverlay;
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
            this.transparentOverlay.enabled = false;
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
            List<AirlineModel> list;
            if (allowedSizes.Count == 3)
            {
                list = (from a in SingletonNonDestroy<BusinessController>.Instance.GetArrayOfActiveBusinessesByType<AirlineModel>()
                        orderby (from b in a.flightListObjects
                                 where b.isAllocated == false
                                 select b).Count() descending, a.businessName
                        select a).ToList<AirlineModel>();
            }
            else
            {
                list = (from a in SingletonNonDestroy<BusinessController>.Instance.GetArrayOfActiveBusinessesByType<AirlineModel>()
                        where (from b in a.flightListObjects
                               where allowedSizes.Contains(b.weightClass) && b.isAllocated == false
                               select b).Count() > 0
                        orderby (from b in a.flightListObjects
                                 where allowedSizes.Contains(b.weightClass) && b.isAllocated == false
                                 select b).Count() descending, a.businessName
                        select a).ToList<AirlineModel>();
            }

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
                else
                {
                    airlineContainerUI.transform.SetAsFirstSibling();
                }
                this.currentlyDisplayedAirlineSlots.Add(airlineContainerUI);
            }
        }
        public void GenerateFlightContainersInAllocationDisplayList(AirlineModel airline)
        {
            flightPlannerPanelUI.ReturnFlightsFromAllocationDisplayToPool();

            //int count1 = this.currentlyDisplayedUnallocatedFlightSlots.Count;
            //for (int i = 0; i < count1; i++)
            //{
            //    this.currentlyDisplayedUnallocatedFlightSlots[i].transform.SetParent(this.unAllocatedFlightSlotPool.GetParent());
            //    this.unAllocatedFlightSlotPool.ReturnPoolObject(this.currentlyDisplayedUnallocatedFlightSlots[i]);
            //}
            //this.currentlyDisplayedUnallocatedFlightSlots.Clear();

            ///
            ///
            ///

            HashSet<string> flightNbrHashSet = new HashSet<string>();
            List<CommercialFlightModel> flightListObjects = (from a in airline.flightListObjects
                                                             orderby a.arrivalRoute.routeDistance descending, a.departureFlightNbr
                                                             select a)
                                                             .ToList<CommercialFlightModel>();
            int count2 = flightListObjects.Count;
            int numberSlotsAdded = 0;
            for(int i = 0; i < count2; i++)
            {
                CommercialFlightModel commercialFlightModel = flightListObjects[i];
                
                FlightSlotContainerUI flightSlotContainerUI;
                
                if (
                    !commercialFlightModel.isAllocated &&
                    !commercialFlightModel.isCompleted && 
                    !flightNbrHashSet.Contains(commercialFlightModel.departureFlightNbr) && 
                    (commercialFlightModel.weightClass != Enums.ThreeStepScale.Small || smallAircraftFilter.isOn) && 
                    (commercialFlightModel.weightClass != Enums.ThreeStepScale.Medium || mediumAircraftFilter.isOn) && 
                    (commercialFlightModel.weightClass != Enums.ThreeStepScale.Large || largeAircraftFilter.isOn) && 
                    unAllocatedFlightSlotPool.TryGetPoolObject(true, out flightSlotContainerUI)
                   )
               
                {
                    flightSlotContainerUI.SetContainerValues(commercialFlightModel, "");
                    flightSlotContainerUI.transform.SetParent(this.unAllocatedFlightSlotPoolParent);
                    flightSlotContainerUI.transform.SetSiblingIndex(numberSlotsAdded);
                    flightNbrHashSet.Add(commercialFlightModel.departureFlightNbr);
                    this.currentlyDisplayedUnallocatedFlightSlots.Add(flightSlotContainerUI);
                    numberSlotsAdded++;
                }

            }

            this.allocationHelpText.transform.parent.gameObject.AttemptEnableDisableGameObject(numberSlotsAdded == 0);
            if (numberSlotsAdded == 0)
            {
                this.allocationHelpText.text = LocalizationManager.GetLocalizedValue("FlightPlannerPanelUI.cs.key.45");
            }
        }
    }


}