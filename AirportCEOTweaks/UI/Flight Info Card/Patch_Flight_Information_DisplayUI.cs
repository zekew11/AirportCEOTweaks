using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using System.IO.Ports;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(FlightInformationDisplayUI))]
    static class Patch_Flight_Information_DisplayUI
    {

        [HarmonyPatch(typeof(FlightInformationDisplayUI), "LoadPanel", new Type[] { typeof(FlightSlotContainerUI) })]
        public static void Postfix(FlightSlotContainerUI flightSlotContainer, FlightInformationDisplayUI __instance) //PlannerChangesMod
        {
            //rescedule changes

            if (AirportCEOTweaksConfig.plannerChanges == false) { return; }

            Button button = __instance.transform.Find("FlightAllocationButtons").Find("RescheduleFlightButton").GetComponent<Button>();
            button.interactable = true;
        }

        [HarmonyPatch("SetDisplayAsFlightPlanner")]
        [HarmonyPrefix]
        public static bool RefreshAsPlanner(FlightModel flight)
        {
            Singleton<ModsController>.Instance.GetExtensions(flight as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            if (ecfm != null)
            {
                ecfm.RefreshServices();
            }
            return true;
        }
        [HarmonyPatch("SetDisplayAsFlightPlanner")]
        [HarmonyPostfix]
        public static void AddDescriptionPlanner(FlightModel flight, FlightInformationDisplayUI __instance)
        {
            if (flight is CommercialFlightModel)
            {

                CommercialFlightModel cmf = flight as CommercialFlightModel;

                Transform transform2 = __instance.transform.Find("FlightInfo");

                TextMeshProUGUI FrqValueText = __instance.transform.Find("FlightInfo").Find("FlightFrequencyValueText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI FrqText = __instance.transform.Find("FlightInfo").Find("FlightFrequencyText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI expectedArrivingPassengersValueText = transform2.Find("ExpectedArrivingPassengersValueText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI expectedDepartingPassengersValueText = transform2.Find("ExpectedDepartingPassengersValueText").GetComponent<TextMeshProUGUI>();

                //expectedArrivingPassengersValueText.message =     cmf.currentTotalNbrOfArrivingPassengers.ToString() + " / " + cmf.totalNbrOfArrivingPassengers.ToString() ;
                //expectedDepartingPassengersValueText.message =   cmf.currentTotalNbrOfDepartingPassengers.ToString() + " / " + cmf.totalNbrOfDepartingPassengers.ToString();
                expectedArrivingPassengersValueText.text = "(" + cmf.totalNbrOfArrivingPassengers.ToString() + ")";
                expectedDepartingPassengersValueText.text = "(" + cmf.totalNbrOfDepartingPassengers.ToString() + ")";

                FrqText.text = "Flight Type: ";

                Singleton<ModsController>.Instance.GetExtensions(flight as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                if (ecfm != null)
                {
                    FrqValueText.text = ecfm.GetDescription(true, true, true, true); 
                }
            }
        }
        [HarmonyPatch("SetDisplayAsFlightInWorld")]
        [HarmonyPrefix]
        public static bool RefreshAsWorld(FlightModel flight)
        {
            Singleton<ModsController>.Instance.GetExtensions(flight as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
            if (ecfm != null)
            {
                ecfm.RefreshServices();
            }
            return true;
        }
        [HarmonyPatch("SetDisplayAsFlightInWorld")]
        [HarmonyPostfix]
        public static void AddDescriptionWorld(FlightModel flight, FlightInformationDisplayUI __instance)
        {
            if (flight is CommercialFlightModel)
            {
                CommercialFlightModel cmf = flight as CommercialFlightModel;

                Transform transform2 = __instance.transform.Find("FlightInfo");

                TextMeshProUGUI FrqValueText = __instance.transform.Find("FlightInfo").Find("FlightFrequencyValueText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI FrqText = __instance.transform.Find("FlightInfo").Find("FlightFrequencyText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI expectedArrivingPassengersValueText = transform2.Find("ExpectedArrivingPassengersValueText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI expectedDepartingPassengersValueText = transform2.Find("ExpectedDepartingPassengersValueText").GetComponent<TextMeshProUGUI>();

                expectedArrivingPassengersValueText.text = " "+ cmf.currentTotalNbrOfArrivingPassengers.ToString()  + "\n(" + cmf.totalNbrOfArrivingPassengers.ToString()+ ")";
                expectedDepartingPassengersValueText.text = " "+cmf.currentTotalNbrOfDepartingPassengers.ToString() + "\n(" + cmf.totalNbrOfDepartingPassengers.ToString()+")";

                FrqText.text = "Flight Type: ";

                Singleton<ModsController>.Instance.GetExtensions(flight as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                if (ecfm != null)
                {
                    FrqValueText.text = ecfm.GetDescription(true, true, true, true);
                }
            }
        }
        [HarmonyPatch("SetOrderedTurnaroundServicesIcon")]
        [HarmonyPostfix]
        public static void ColorServiceIcons(ref FlightInformationDisplayUI __instance, FlightModel ___flight)
        {
            if (!(___flight is CommercialFlightModel)) { return; }

            Transform transform2 = __instance.transform.Find("FlightInfo");
            AirportData airportData = Singleton<AirportController>.Instance.AirportData;

            bool[] haveService = { airportData.cateringServiceEnabled, airportData.aircraftCabinCleaningServiceEnabled, airportData.jetA1RefuelingServiceEnabled, airportData.rampAgentServiceRoundEnabled, airportData.baggageHandlingSystemEnabled };

            Transform[] icons = new Transform[5];

            icons[3] = transform2.Find("ServiceRoundIcon");
            icons[2] = transform2.Find("RefuelingIcon");
            icons[4] = transform2.Find("BaggageHandlingIcon");
            icons[0] = transform2.Find("CateringIcon");
            icons[1] = transform2.Find("CabinCleaningIcon");

            Singleton<ModsController>.Instance.GetExtensions(___flight as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);

            int[] serviceDesireArray = ecfm.RefreshServices();

            for (int i = 0; i < serviceDesireArray.Length; i++)
            {
                Image iconImage = icons[i].GetComponent<Image>();

                //not requested
                if (!ecfm.turnaroundServices.GetValueSafe((Extend_CommercialFlightModel.TurnaroundServices)i).Requested)//Requested
                {
                    //switch on desire level
                    switch (serviceDesireArray[i])
                    {
                        case -1:
                        case 0:
                            FlightServiceIconHelper.ColorIconClear(iconImage);
                            continue;
                    }
                } // does not neccesarily continue

                //failed
                if (ecfm.turnaroundServices.GetValueSafe((Extend_CommercialFlightModel.TurnaroundServices)i).Failed)//Failed
                {
                    FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltip((Extend_CommercialFlightModel.TurnaroundService.Desire)serviceDesireArray[i], (Extend_CommercialFlightModel.TurnaroundServices)i, true, false, !haveService[i]));
                    
                    switch (serviceDesireArray[i])
                    {
                        case 0: 
                            iconImage.color = SingletonNonDestroy<DataPlaceholderColors>.Instance.white.Opacity(0.15f);
 
                            continue;
                        case 1: 
                            iconImage.color = Color.red;
                            //FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltipFailed(Extend_CommercialFlightModel.TurnaroundService.Desire.Desired));
                            continue;
                        case 2: 
                            iconImage.color = Color.red;
                            //FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltipFailed(Extend_CommercialFlightModel.TurnaroundService.Desire.Demanded));
                            continue;
                        default:
                            FlightServiceIconHelper.ColorIconError(iconImage);
                            Debug.LogWarning("ACEO Tweaks | WARN: turnaround service " + ((Extend_CommercialFlightModel.TurnaroundServices)i).ToString() + " is failed with unexpected desire == " + serviceDesireArray[i].ToString());
                            continue;
                    }
                } //continue to next loop

                //succeeded is green
                if (ecfm.turnaroundServices.GetValueSafe((Extend_CommercialFlightModel.TurnaroundServices)i).Succeeded)//Succeeded
                {
                    FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltip((Extend_CommercialFlightModel.TurnaroundService.Desire)serviceDesireArray[i], (Extend_CommercialFlightModel.TurnaroundServices)i, false, true));
                    switch (serviceDesireArray[i])
                    {
                        case 0: 
                            iconImage.color = SingletonNonDestroy<DataPlaceholderColors>.Instance.lightGreen;
                            //FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltipSucceeded(Extend_CommercialFlightModel.TurnaroundService.Desire.Indiffernt));
                            continue;
                        case 1:                             
                            iconImage.color = SingletonNonDestroy<DataPlaceholderColors>.Instance.lightGreen;
                            //FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltipSucceeded(Extend_CommercialFlightModel.TurnaroundService.Desire.Desired));
                            continue;
                        case 2: 
                            iconImage.color = SingletonNonDestroy<DataPlaceholderColors>.Instance.lightGreen;
                            //FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltipSucceeded(Extend_CommercialFlightModel.TurnaroundService.Desire.Demanded));
                            continue;
                        default:
                            FlightServiceIconHelper.ColorIconError(iconImage);
                            Debug.LogWarning("ACEO Tweaks | WARN: turnaround service " + ((Extend_CommercialFlightModel.TurnaroundServices)i).ToString() + " is succeeded with unexpected desire == " + serviceDesireArray[i].ToString());
                            continue;
                    }
                } //continue to next loop

                // now we are not refused, failed, succeeded, or completed, so we must be a pending

                
                FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltip((Extend_CommercialFlightModel.TurnaroundService.Desire)serviceDesireArray[i], (Extend_CommercialFlightModel.TurnaroundServices)i, false, false, !haveService[i]));

                //switch on desire level
                switch (serviceDesireArray[i])
                {
                    case 0:
                        iconImage.color = SingletonNonDestroy<DataPlaceholderColors>.Instance.white.Opacity(0.3f);
                        //FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.indifferedTooltip);
                        continue;
                    case 1:
                        if (haveService[i])
                        {
                            FlightServiceIconHelper.ColorIconPending(iconImage);
                        }
                        else
                        {
                            iconImage.color = Color.yellow;
                            //FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltipCantBeProvided(Extend_CommercialFlightModel.TurnaroundService.Desire.Desired));
                        }
                        continue;
                    case 2:
                        if (haveService[i])
                        {
                            FlightServiceIconHelper.ColorIconPending(iconImage);
                        }
                        else
                        {
                            iconImage.color = SingletonNonDestroy<DataPlaceholderColors>.Instance.orange;
                            //FlightServiceIconHelper.ChangeAddTooltip(iconImage.gameObject, FlightServiceIconHelper.BuildTooltipCantBeProvided(Extend_CommercialFlightModel.TurnaroundService.Desire.Demanded));
                        }
                        continue;
                    default:
                        FlightServiceIconHelper.ColorIconError(iconImage);
                        Debug.LogWarning("ACEO Tweaks | WARN: turnaround service " + ((Extend_CommercialFlightModel.TurnaroundServices)i).ToString() + " is by deduction a pending request with unexpected desire == " + serviceDesireArray[i].ToString());
                        continue;
                }
                
            }

            FlightServiceIconHelper.ColorIconClear(transform2.Find("PushbackIcon").GetComponent<Image>());
            FlightServiceIconHelper.ColorIconClear(transform2.Find("DeicingIcon").GetComponent<Image>());
        }

    }
   
}