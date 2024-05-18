using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(AirlineContainerUI))]
    static class Patch_AirlineContainerUI
    {

        [HarmonyPatch("SetContainerValues")]
        [HarmonyPostfix]
        static void ShortenContainer(ref AirlineContainerUI __instance, ref Transform ___infoTransform, ref Image ___airlineLogo)
        {
            if (!AirportCEOTweaksConfig.PlannerUIModifications.Value)
            {
                return;
            }

            ///Add a mouse enter/leave listener
            ///
            if (!__instance.gameObject.TryGetComponent<Extend_AirlineContainer>(out Extend_AirlineContainer extension))
            {
                extension = __instance.gameObject.AddComponent<Extend_AirlineContainer>();
                extension.ConstructMe(__instance, ___infoTransform, ___airlineLogo);
            }

            ///swap vert for horz and resize

            Extend_AirlineContainer.ConfigureMinimized(__instance, ___infoTransform, ___airlineLogo.transform);

        }

    }
}