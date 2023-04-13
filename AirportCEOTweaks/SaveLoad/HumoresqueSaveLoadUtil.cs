using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Nodes;
using System.Net;
using Newtonsoft.Json;

namespace AirportCEOTweaks
{
    class SaveLoadUtility : MonoBehaviour
    {
        // Function vars
        static float gameTimeWhenSaving = 1f;

        // JSON vars
        [SerializeField] public static List<Extend_CommercialFlightModelSerializable> JSONInfoArray = new List<Extend_CommercialFlightModelSerializable>(); // <-------------- Will be reconfigured in future update

        // Function Utilities
        void Start()
        {
            quicklog("Save Load Utility is online with instance set!");
        }
        public static void quicklog(string message, bool logAsUnityError = false)
        {
            if (logAsUnityError)
            {
                Debug.LogError("[SaveLoadUtility] " + message);
            }
            Debug.Log("[SaveLoadUtility] " + message);
        }

        // Game Utilities
        public static void makeGameReadyForSaving()
        {
            try
            {
                // Removes player interaction and object movement
                Singleton<MainInteractionPanelUI>.Instance.EnableDispableSavingTextPanel(true);
                gameTimeWhenSaving = Singleton<TimeController>.Instance.currentSpeed;
                PlayerInputController.SetPlayerControlAllowed(false);
                if (gameTimeWhenSaving != 0f)
                {
                    Singleton<TimeController>.Instance.TogglePauseTime();
                }
            }
            catch (Exception ex)
            {
                quicklog("Error making game ready for saving! Error: " + ex.Message, true);
            }
        }
        public static void revetGameAfterSaving()
        {
            try
            {
                // Allows player interaction and object movement
                if (gameTimeWhenSaving != 0f)
                {
                    Singleton<TimeController>.Instance.TogglePauseTime();
                }
                PlayerInputController.SetPlayerControlAllowed(true);
                if (gameTimeWhenSaving == 100f)
                {
                    Singleton<TimeController>.Instance.InvokeSkipToNextDay();
                }
                Singleton<MainInteractionPanelUI>.Instance.EnableDispableSavingTextPanel(false);
            }
            catch (Exception ex)
            {
                quicklog("Error reverting game after saving! Error: " + ex.Message, true);
            }
        }

        // JSON making!
        /// <summary>
        /// Creates the JSON file for saveLoadUtility
        /// </summary>
        /// <param name="path">This *MUST* be the save courtine's own input!</param>
        /// <param name="fileName">If you want to use a custom file name, specify it</param>
        public static void createJSON(string path, string fileName = "TweaksFlightSaveData.json")
        {
            // Make sure the array isn't empty!
            if (JSONInfoArray == null || JSONInfoArray.Count == 0 || string.IsNullOrEmpty(path))
            {
                return;
            }

            // Get basepath, add it if not allready there
            string basepath = Singleton<SaveLoadGameDataController>.Instance.GetUserSavedDataSearchPath();
            if (!string.Equals(path.SafeSubstring(0, 2), "C:"))
            {
                path = Path.Combine(basepath.Remove(basepath.Length - 1), path);
            }

            // Make sure the directory does exist
            if (!Directory.Exists(path))
            {
                quicklog("The directory for saving does not exist! The path was \"" + path + "\".", true);
                return;
            }

            // Add the filename itself
            path = Path.Combine(path, fileName);
            quicklog("Full path is \"" + path + "\"", false);

            // Make sure the file doesn't allready exist
            if (File.Exists(path))
            {
                quicklog("The file allready exists!", true);
                return;
            }

            // JSON creation vars and systems
            string JSON;
            Extend_CommericialFlightModelSerializableWrapper JSONWrapper = new Extend_CommericialFlightModelSerializableWrapper(JSONInfoArray);
            try
            {
                JSON = JsonConvert.SerializeObject(JSONWrapper, Formatting.Indented); // We pretty print :)
            }
            catch (Exception ex)
            {
                quicklog("Error Converting classes to JSON. Error: " + ex.Message, true);
                return;
            }


            // Saving
            string exception;
            try
            {
                Utils.TryWriteFile(JSON, path, out exception);
                if (!string.IsNullOrEmpty(exception))
                {
                    quicklog("An exception occured! It was: " + exception, true);
                    return;
                }
            }
            catch (Exception ex)
            {
                quicklog("Outer error writing file to JSON. Error: " + ex.Message, true);
                return;
            }

            quicklog("JSON creation succesfull, and JSON saving finished! Yay", true);
        }
    }
}