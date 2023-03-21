using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;

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
            if (ecfm != null && AirportCEOTweaksConfig.flightTypes)
            {
                ecfm.RefreshServices();
            }
            return true;
        }
        [HarmonyPatch("SetDisplayAsFlightPlanner")]
        [HarmonyPostfix]
        public static void AddDescriptionPlanner(FlightModel flight, FlightSlotContainerUI __instance)
        {
            if (flight is CommercialFlightModel && AirportCEOTweaksConfig.flightTypes)
            {

                CommercialFlightModel cmf = flight as CommercialFlightModel;

                Transform transform2 = __instance.transform.Find("FlightInfo");

                TextMeshProUGUI FrqValueText = __instance.transform.Find("FlightInfo").Find("FlightFrequencyValueText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI FrqText = __instance.transform.Find("FlightInfo").Find("FlightFrequencyText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI expectedArrivingPassengersValueText = transform2.Find("ExpectedArrivingPassengersValueText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI expectedDepartingPassengersValueText = transform2.Find("ExpectedDepartingPassengersValueText").GetComponent<TextMeshProUGUI>();

                //expectedArrivingPassengersValueText.text =     cmf.currentTotalNbrOfArrivingPassengers.ToString() + " / " + cmf.totalNbrOfArrivingPassengers.ToString() ;
                //expectedDepartingPassengersValueText.text =   cmf.currentTotalNbrOfDepartingPassengers.ToString() + " / " + cmf.totalNbrOfDepartingPassengers.ToString();
                expectedArrivingPassengersValueText.text = "(" + cmf.totalNbrOfArrivingPassengers.ToString() + ")";
                expectedDepartingPassengersValueText.text = "(" + cmf.totalNbrOfDepartingPassengers.ToString() + ")";

                FrqText.text = "Desription:";

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
            if (ecfm != null && AirportCEOTweaksConfig.flightTypes)
            {
                ecfm.RefreshServices();
            }
            return true;
        }
        [HarmonyPatch("SetDisplayAsFlightInWorld")]
        [HarmonyPostfix]
        public static void AddDescriptionWorld(FlightModel flight, FlightSlotContainerUI __instance)
        {
            if (flight is CommercialFlightModel && AirportCEOTweaksConfig.flightTypes)
            {
                CommercialFlightModel cmf = flight as CommercialFlightModel;

                Transform transform2 = __instance.transform.Find("FlightInfo");

                TextMeshProUGUI FrqValueText = __instance.transform.Find("FlightInfo").Find("FlightFrequencyValueText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI FrqText = __instance.transform.Find("FlightInfo").Find("FlightFrequencyText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI expectedArrivingPassengersValueText = transform2.Find("ExpectedArrivingPassengersValueText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI expectedDepartingPassengersValueText = transform2.Find("ExpectedDepartingPassengersValueText").GetComponent<TextMeshProUGUI>();

                expectedArrivingPassengersValueText.text = " "+ cmf.currentTotalNbrOfArrivingPassengers.ToString()  + "\n(" + cmf.totalNbrOfArrivingPassengers.ToString()+ ")";
                expectedDepartingPassengersValueText.text = " "+cmf.currentTotalNbrOfDepartingPassengers.ToString() + "\n(" + cmf.totalNbrOfDepartingPassengers.ToString()+")";

                FrqText.text = "Flight Type:";

                Singleton<ModsController>.Instance.GetExtensions(flight as CommercialFlightModel, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam);
                if (ecfm != null)
                {
                    FrqValueText.text = ecfm.GetDescription(true, true, true, true);
                }
            }
        }
        [HarmonyPatch("SetOrderedTurnaroundServicesIcon")]
        [HarmonyPostfix]
        public static void ColorServiceIcons(ref FlightSlotContainerUI __instance, FlightModel ___flight)
        {
            if (!(___flight is CommercialFlightModel) || !AirportCEOTweaksConfig.flightTypes) { return; }

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
                //not requested
                if (!ecfm.turnaroundServices.GetValueSafe((Extend_CommercialFlightModel.TurnaroundServices)i).Requested)//Requested
                {
                    //switch on desire level
                    switch (serviceDesireArray[i])
                    {
                        case -1:
                        case 0:
                            icons[i].GetComponent<Image>().color = SingletonNonDestroy<DataPlaceholderColors>.Instance.clear;
                            continue;
                    }
                } // does not neccesarily continue

                //failed
                if (ecfm.turnaroundServices.GetValueSafe((Extend_CommercialFlightModel.TurnaroundServices)i).Failed)//Failed
                {
                    switch (serviceDesireArray[i])
                    {
                        case 0: icons[i].GetComponent<Image>().color = Color.white.Opacity(.15f); continue;
                        case 1: icons[i].GetComponent<Image>().color = Color.red; continue;
                        case 2: icons[i].GetComponent<Image>().color = Color.red; continue;
                        default:
                            icons[i].GetComponent<Image>().color = Color.blue;
                            Debug.LogWarning("ACEO Tweaks | WARN: turnaround service " + ((Extend_CommercialFlightModel.TurnaroundServices)i).ToString() + " is failed with unexpected desire == " + serviceDesireArray[i].ToString());
                            continue;
                    }
                } //continues
                
                //succeeded is green
                if (ecfm.turnaroundServices.GetValueSafe((Extend_CommercialFlightModel.TurnaroundServices)i).Succeeded)//Succeeded
                {
                    switch (serviceDesireArray[i])
                    {
                        case 0: icons[i].GetComponent<Image>().color = SingletonNonDestroy<DataPlaceholderColors>.Instance.lightGreen; continue;
                        case 1: icons[i].GetComponent<Image>().color = SingletonNonDestroy<DataPlaceholderColors>.Instance.lightGreen; continue;
                        case 2: icons[i].GetComponent<Image>().color = SingletonNonDestroy<DataPlaceholderColors>.Instance.lightGreen; continue;
                        default:
                            icons[i].GetComponent<Image>().color = Color.blue;
                            Debug.LogWarning("ACEO Tweaks | WARN: turnaround service " + ((Extend_CommercialFlightModel.TurnaroundServices)i).ToString() + " is succeeded with unexpected desire == " + serviceDesireArray[i].ToString());
                            continue;
                    }
                } //continues

                ////requested, completed, but not failed or succeeded is an error catch
                //if (ecfm.turnaroundServices.GetValueSafe((Extend_CommercialFlightModel.TurnaroundServices)i).Completed) //Completed
                //{
                //    //Debug.LogWarning("ACEO Tweaks | WARN: turnaround service " + ((Extend_CommercialFlightModel.TurnaroundServices)i).ToString() + " is completed but not failed nor succeeded with desire == " + serviceDesireArray[i].ToString());
                //    //icons[i].GetComponent<Image>().color = Color.grey; continue;
                //} //continues
                
                // now we are not refused, failed, succeeded, or completed, so we must be a pending

                //switch on desire level
                switch (serviceDesireArray[i])
                {
                    case 0:
                        icons[i].GetComponent<Image>().color = Color.white.Opacity(.3f);
                        continue;
                    case 1:
                        if (haveService[i])
                        {
                            icons[i].GetComponent<Image>().color = Color.white;
                        }
                        else
                        {
                            icons[i].GetComponent<Image>().color = Color.yellow;
                        }
                        continue;
                    case 2:
                        if (haveService[i])
                        {
                            icons[i].GetComponent<Image>().color = Color.white;
                        }
                        else
                        {
                            icons[i].GetComponent<Image>().color = SingletonNonDestroy<DataPlaceholderColors>.Instance.orange;
                        }
                        continue;
                    default:
                        Debug.LogWarning("ACEO Tweaks | WARN: turnaround service " + ((Extend_CommercialFlightModel.TurnaroundServices)i).ToString() + " is by deduction a pending request with unexpected desire == " + serviceDesireArray[i].ToString());
                        icons[i].GetComponent<Image>().color = Color.blue; continue;
                }
                
            }

            transform2.Find("PushbackIcon").GetComponent<Image>().color = SingletonNonDestroy<DataPlaceholderColors>.Instance.clear;
            transform2.Find("DeicingIcon").GetComponent<Image>().color = SingletonNonDestroy<DataPlaceholderColors>.Instance.clear;
        }

    }
   
}