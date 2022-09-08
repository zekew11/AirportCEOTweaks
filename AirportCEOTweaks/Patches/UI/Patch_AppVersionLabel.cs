using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using TMPro;
using System.Reflection;
/*
namespace AirportCEOTweaks
{
    //[HarmonyPatch(typeof(ApplicationVersionLabelUI))]             ------------- DISABLED
    static class Patch_AppVersionLabel
    {
        //[HarmonyPostfix]
        //[HarmonyPatch("Awake")]
        public static void Patch_AddTweaksLabel(ref ApplicationVersionLabelUI __instance)
        {
            //doesn't fire; awake is too soon
            
            Debug.LogError("ACEO Tweaks | DEBUG: AddTweaksLabel Ran");

            TMP_Text tMP = __instance.transform.GetComponent<TextMeshProUGUI>();
            string str = tMP.text;

            Version version = Assembly.GetEntryAssembly().GetName().Version;

            str = str + " - AirportCEO Tweaks v" + version.ToString();
            tMP.text = str;
            Debug.LogError(str);
        }
    }
}
*/