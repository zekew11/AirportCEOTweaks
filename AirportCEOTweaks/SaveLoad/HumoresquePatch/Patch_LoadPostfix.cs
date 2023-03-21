using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Newtonsoft.Json;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(SaveLoadGameDataController))]
    static class Patch_LoadPostfix
    {
        private static string savePath;

        [HarmonyPatch("LoadGameDataCoroutine")]
        public static void Prefix(SaveLoadGameDataController __instance)
        {
            savePath = __instance.savePath;
        }

        [HarmonyPatch("LoadGameDataCoroutine", MethodType.Enumerator)]
        public static void Postfix(SaveLoadGameDataController __instance, ref bool __result)
        {
            // This makes sure there are no more elements to go through (makes sure its a true postfix!)
            if (__result)
            {
                return;
            }
            SaveLoadUtility.quicklog("Starting Custom Load Round!", true);
            // Make sure the diretory exists!
            if (!Directory.Exists(savePath))
            {
                SaveLoadUtility.quicklog("The save directory does not exist!", true);
                return;
            }


            // Path based stuff... Is there a file?
            string path = savePath + "\\TweaksFlightSaveData.json";
            if (!File.Exists(path))
            {
                SaveLoadUtility.quicklog("The CustomSaveData.json file does not exist. Skipped loading.", false);
                return;
            }

            string JSON;
            try
            {
                // Read the file!
                JSON = Utils.ReadFile(path);
            }
            catch (Exception ex)
            {
                SaveLoadUtility.quicklog("Failed to read JSON! Error: " + ex.Message, true);
                return;
            }
            if (string.IsNullOrEmpty(JSON))
            {
                SaveLoadUtility.quicklog("Empty or null JSON string!", true);
                return;
            }

            // Now we know that the string has something...
            Extend_CommericialFlightModelSerializableWrapper customItemSerializableWrapper = null;
            try
            {
                customItemSerializableWrapper = JsonConvert.DeserializeObject<Extend_CommericialFlightModelSerializableWrapper>(JSON);
            }
            catch (Exception ex)
            {
                SaveLoadUtility.quicklog("JSON deserialized failed! Error: " + ex.Message, true);
                return;
            }

            if (customItemSerializableWrapper == null)
            {
                SaveLoadUtility.quicklog("JSON deserialized object is null!!", true);
                return;
            }


            SaveLoadUtility.quicklog("A total of " + customItemSerializableWrapper.customSerializables.Count + " custom commercial flights were found and will be loaded/changed", false);

            // Now we know the Item info is valid

            CommercialFlightModel flightModel = null;

            foreach (Extend_CommercialFlightModelSerializable flight in customItemSerializableWrapper.customSerializables)
            {
                if (!string.IsNullOrEmpty(flight.flightData.parentFlightReferenceID))
                {
                    flightModel = Singleton<AirTrafficController>.Instance.GetFlightByReferenceID<CommercialFlightModel>(flight.flightData.parentFlightReferenceID);
                    
                    if (flight.flightData.parentFlightReferenceID == "null")
                    {
                        Debug.LogWarning("ACEO Tweaks | WARN: flight save data contains the word null as a reference id!");
                        continue;
                    }
                }
                else
                {
                    Debug.LogError("ACEO Tweaks | Error: a flight data refrenceid is null or empty!");
                    continue;
                }
                if (flightModel == null)
                {
                    //Debug.LogError("ACEO Tweaks | Error: a vanilla saved flight ref id returns a null flight!");
                    continue;
                }
                if (!Singleton<ModsController>.Instance.commercialFlightLoadDataDict.ContainsKey(flightModel))
                {
                    Singleton<ModsController>.Instance.commercialFlightLoadDataDict.Add(flightModel, flight.flightData);
                }
                else
                {
                    Debug.LogError("ACEO Tweaks | WARN: overlapping flight datas for a flight; save data likely incorrectly loaded!");
                }
            }

            // DoStuff

            SaveLoadUtility.quicklog("Tweaks save data loaded successfully!", true);
        }
    }
}