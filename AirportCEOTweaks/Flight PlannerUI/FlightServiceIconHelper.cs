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
        public static string indifferedTooltip = "This service is not requested, but will be requested if available";
        public static string indifferedSuccessTooltip = "This service was requested and was successfully provided";
        public static string pendingRequestTooltip = "This service is requested and will be provided during turnaround";
        public static string errorTooltip = "There was an error with coloring this icon, so see the game log";
            
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
            ChangeAddTooltip(icon.gameObject, pendingRequestTooltip);
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

        // These are tooltips that change with desire level
        public static string BuildTooltipSucceeded(Extend_CommercialFlightModel.TurnaroundService.Desire desireLevel)
        {
            if (desireLevel == desired || desireLevel == demanded)
            {
                return $"This service was {desireLevel.ToString().ToLower()} and was successfully provided";
            }

            if (desireLevel == indiffernt)
            {
                return indifferedSuccessTooltip;
            }

            return string.Empty;
        }
        public static string BuildTooltipCantBeProvided(Extend_CommercialFlightModel.TurnaroundService.Desire desireLevel)
        {
            if (desireLevel == desired || desireLevel == demanded)
            {
                return $"This serive is {desireLevel.ToString().ToLower()}, but not able to be provided";
            }
            return string.Empty;
        }

        public static string BuildTooltipFailed(Extend_CommercialFlightModel.TurnaroundService.Desire desireLevel)
        {
            if (desireLevel == desired || desireLevel == demanded)
            { 
                return $"This serive is {desireLevel.ToString().ToLower()}, and can't be provided by your airport";
            }

            return string.Empty;
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