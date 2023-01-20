using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using TMPro;
using System.Reflection;
using UnityEngine.UI;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(LocalizationManager), "LoadLocalizedText")]
    static class Patch_FlightPlannerTooltips
    {
        public static void Postfix()
        {
            if (!AirportCEOTweaksConfig.plannerChanges)
            {
                return;
            }

            // Get dictionary, replace strings with new content
            Dictionary<string,string> localizedtext = Traverse.Create(typeof(LocalizationManager)).Field<Dictionary<string, string>>("localizedText").Value;
            localizedtext["flight-info-display.key.reschedule-description"] = "Click to initiate a reschedule of the flight. Hold the shift key when confirming the reschedule to " +
                "reschedule the other flights in the same series.";
            localizedtext["flight-info-display.key.cancel-description"] = "Click to cancel the current flight. Note that cancelling a flight that is en-route will incur a penalty, " +
                "and flights that have landed can't be cancelled at all. Hold the shift key when confirming the cancelation to cancel the other flights in the same series.";

            // If trying to get vanilla content, use input keys below to get content (In UMF Log)
            /*
            string value;
            localizedtext.TryGetValue("flight-info-display.key.reschedule-description", out value);
            AirportCEOTweaks.Log(value);
            localizedtext.TryGetValue("flight-info-display.key.cancel-description", out value);
            AirportCEOTweaks.Log(value);
            */
        }
    }

    /*[HarmonyPatch(typeof(FlightInformationDisplayUI), "Awake")]   <----------- This is for getting the localization keys only.
    static class Patch_FlightPlannerTooltipsKeys
    {
        public static void Postfix(FlightInformationDisplayUI __instance)
        {
            Button rescheduleflightbutton = Traverse.Create(__instance).Field<Button>("rescheduleFlightButton").Value;
            Button cancelflightbutton = Traverse.Create(__instance).Field<Button>("cancelFlightButton").Value;

            AirportCEOTweaks.Log("res text " + rescheduleflightbutton.gameObject.GetComponent<HoverToolTip>().textToDisplay);
            AirportCEOTweaks.Log("res head " + rescheduleflightbutton.gameObject.GetComponent<HoverToolTip>().headerToDisplay);
            AirportCEOTweaks.Log("can text " + cancelflightbutton.gameObject.GetComponent<HoverToolTip>().textToDisplay);
            AirportCEOTweaks.Log("can head " + cancelflightbutton.gameObject.GetComponent<HoverToolTip>().headerToDisplay);

        }
    }*/
}
