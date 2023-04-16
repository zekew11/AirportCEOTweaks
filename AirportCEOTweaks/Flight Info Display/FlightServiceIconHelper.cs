using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;


namespace AirportCEOTweaks
{
    static class FlightServiceIconHelper
    {
        // These are hard-coded tooltips that don't have a desire level in them
        public static string indifferedTooltip =        "This service is not requested, but will be requested if available";
        public static string indifferedSuccessTooltip = "This service was requested and was successfully provided";
        public static string pendingRequestTooltip =    "This service is requested and will be provided during turnaround";
        public static string errorTooltip =             "There was an error with coloring this icon; see game log";
            
        // For easier access
        private static Extend_CommercialFlightModel.TurnaroundService.Desire desired = Extend_CommercialFlightModel.TurnaroundService.Desire.Desired;
        private static Extend_CommercialFlightModel.TurnaroundService.Desire demanded = Extend_CommercialFlightModel.TurnaroundService.Desire.Demanded;
        private static Extend_CommercialFlightModel.TurnaroundService.Desire indiffernt = Extend_CommercialFlightModel.TurnaroundService.Desire.Indiffernt;
        

        // Utils, make it easier to code repeat tasks
        public static void ColorIconClear(Image icon)
        {
            icon.color = SingletonNonDestroy<DataPlaceholderColors>.Instance.clear;
            RemoveTooltip(icon.gameObject);
        }
        public static void ColorIconPending(Image icon)
        {
            icon.color = Color.white;
            //ChangeAddTooltip(icon.gameObject, pendingRequestTooltip);
        }
        public static void ColorIconError(Image icon)
        {
            icon.color = Color.blue;
            ChangeAddTooltip(icon.gameObject, errorTooltip);
        }


        // Tooltip adder
        public static void ChangeAddTooltip(GameObject icon, string tooltip)
        {
            // Make sure it responds to hover!
            icon.GetComponent<Image>().raycastTarget = true;

            FlightServiceIconComponent coloredIconHover;
            if (icon.gameObject.TryGetComponent<FlightServiceIconComponent>(out coloredIconHover))
            {
                coloredIconHover.message = tooltip;
                return;
            }

            coloredIconHover = icon.gameObject.AddComponent<FlightServiceIconComponent>();
            coloredIconHover.message = tooltip;
        }

        public static void RemoveTooltip(GameObject icon)
        {
            if (icon.gameObject.TryGetComponent<FlightServiceIconComponent>(out FlightServiceIconComponent coloredIconHover))
            {
                coloredIconHover.message = "";
                return;
            }
        }       
        public static string BuildTooltip(Extend_CommercialFlightModel.TurnaroundService.Desire desireLevel, Extend_CommercialFlightModel.TurnaroundServices thisService ,bool failed = false, bool succeeded = false, bool unavailable = false)
        {

            string desire;
            if (desireLevel == Extend_CommercialFlightModel.TurnaroundService.Desire.Indiffernt)
            {
                desire = "optional";
            }
            else if (desireLevel == Extend_CommercialFlightModel.TurnaroundService.Desire.Demanded)
            {
                desire = "required";
            }
            else
            {
                desire = desireLevel.ToString().ToLower();
            }

            string service = thisService.ToString();

            if (thisService == Extend_CommercialFlightModel.TurnaroundServices.RampService)
            {
                service = "Ground handling";
            }

            string tense = (succeeded || failed) ? "was" : "is";
            string postmortem;

            if(failed)
            {
                if (unavailable)
                {
                    postmortem = "Failed. Airport does not offer this service!";
                }
                else
                {
                    postmortem = "Failed.";
                }
            }
            else if (succeeded)
            {
                postmortem = $"Sucessfully provided {service.ToLower()}.";
            }
            else if (unavailable)
            {
                if (desireLevel == Extend_CommercialFlightModel.TurnaroundService.Desire.Demanded)
                {
                    postmortem = $"Make {service.ToLower()} service available before scheduling to avoid penalty.";
                    //tense = "";
                }
                else if (desireLevel == Extend_CommercialFlightModel.TurnaroundService.Desire.Desired)
                {
                    postmortem = $"Make {service.ToLower()} service available before scheduling for full reward.";
                    //tense = "";
                }
                else //indiff or reject
                {
                    postmortem = $"Airport does not offer {service.ToLower()} service";
                }
            }
            else
            {
                postmortem = "Pending...";
            }

            return $"{service} {tense} {desire}. {postmortem}";
        }
    }

    public class FlightServiceIconComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
    {
        public string message;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            // This gets the normal tooltip stuff, so it doesn't too look bad
            Color panelColor = Traverse.Create(HoverPanel.Instance).Field<Color>("defaultPanelColor").Value;
            Color textColor = Traverse.Create(HoverPanel.Instance).Field<Color>("defaultHeaderTextColor").Value;

            // We need to do this for the prefferd length, so that we can adjust based on the strings length.
            Text panelText = GameMessagePanelUI.Instance.messageText;
            panelText.text = message;

            // Find the location on screen it should be, then use it to display panel
            Vector2 location = new Vector2(gameObject.transform.position.x + 5f + panelText.preferredWidth / 2, gameObject.transform.position.y + 90f);
            GameMessagePanelUI.Instance.ShowTextAtPos(message, textColor, true, location, panelColor);
            
            // Override uppercasing
            panelText.text = message;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameMessagePanelUI.Instance.HidePanel();
        }
    }
}