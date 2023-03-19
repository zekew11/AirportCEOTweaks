using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(SaveLoadGameDataController))]
    static class Patch_SavePostfix
    {
        private static string inputSavePath;

        [HarmonyPatch("SaveGameData")]
        public static void Prefix(SaveLoadGameDataController __instance, string savePath)
        {
            // This is to get the appropriate variable, savePath, since the other patch can't get it
            inputSavePath = savePath;
        }

        [HarmonyPatch("SaveGameData", MethodType.Enumerator)]
        public static void Postfix(SaveLoadGameDataController __instance, ref bool __result)
        {
            // This makes sure there are no more elements to go through (makes sure its a true postfix!)
            if (__result)
            {
                return;
            }

            // Configure Game World before saving
            SaveLoadUtility.quicklog("Starting Tweaks Custom Save Round!", true);
            SaveLoadUtility.makeGameReadyForSaving();


            // Custom code!
            try
            {
                foreach (Extend_CommercialFlightModel ecfm in Singleton<ModsController>.Instance.AllExtendCommercialFlightModels())
                {
                    SaveLoadUtility.JSONInfoArray.Add(new Extend_CommercialFlightModelSerializable(ecfm.SerializedData()));
                }
                if (string.IsNullOrEmpty(inputSavePath))
                {
                    inputSavePath = Singleton<SaveLoadGameDataController>.Instance.saveName;
                }

                SaveLoadUtility.createJSON(inputSavePath);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error in custom save code! error: " + ex.Message);
            }


            // Revert Game World after saving  <---------------------------------------- IMPORTANT
            SaveLoadUtility.revetGameAfterSaving();
            SaveLoadUtility.JSONInfoArray.Clear();
        }
    }
}
