using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using TMPro;
using System.Reflection;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(ApplicationVersionLabelUI), "Awake")]
    static class Patch_AppVersionLabel
    {
        public static void Postfix(ApplicationVersionLabelUI __instance)
        {
            TMP_Text tMP = __instance.transform.GetComponent<TextMeshProUGUI>();
            string str = tMP.text;

            str = str + "ACEO Tweaks " + AirportCEOTweaksConfig.displayConfigVersion;
            tMP.text = str;
            //Debug.LogError("ACEO Tweaks | DEBUG: AddTweaksLabel Ran");  <------ This does not log, it is not avaiable yet! It does work, with no errors.
        }
    }
}
