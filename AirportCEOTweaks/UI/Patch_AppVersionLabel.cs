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

            //str = str + "ACEO Tweaks " + AirportCEOTweaksConfig.displayConfigVersion + "\n";
            
            str = str + "ACEO Tweaks " + GetVersion() + "\n";

            tMP.text = str;

        }
        public static string GetVersion()
        {
        if(Assembly
        .GetExecutingAssembly()
        ?.GetName() == null)
            {
                return "null2";
            }
            if (Assembly
            .GetExecutingAssembly()
             ?.GetName().Version == null)
            {
                return "null3";
            }

            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
