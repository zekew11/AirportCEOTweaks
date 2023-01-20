using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;


namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(SaveLoadGameDataController))]
    class Patch_SaveLoadGameDataController
    {
        [HarmonyPatch("InvokeSaveGameData")]
        [HarmonyPostfix]
        public static void InvokeTweaksSaveGameData(string savePath, string oldSaveName, string ___userSavedDataSearchPath)
        {
            //GameObject attachto = UnityEngine.GameObject.Find("CoreGameControllers");
            //SaveGameDataDoer saveDoer = attachto.AddComponent<SaveGameDataDoer>();
            //saveDoer.Init(savePath, oldSaveName, ___userSavedDataSearchPath);
        } //whenever a save is invoked, create my save-doer componet so that theres an instance in the world to gather up what needs saving

        [HarmonyPatch("LoadGameDataCoroutine")]
        [HarmonyPrefix]
        public static bool AddAircraftBeforeLoad()
        {
            return true;
        }
    }
    class SaveGameDataDoer : MonoBehaviour
    {
        //I'm a componet that gets created via Harmony patch when saving the game is requested. I use some coroutines to do my job, then destroy myself.
        
        public string savePath;
        public string tempSavePath;
        public string oldSaveName;
        public string userSavedDataSearchPath;
        private bool vannillaSaving = true;
        private bool modSaving = true;

        public void Init(string savePath, string oldSaveName, string userSavedDataSearchPath)
        {
            this.userSavedDataSearchPath = userSavedDataSearchPath;
            this.savePath = Path.Combine(this.userSavedDataSearchPath, savePath);
            this.tempSavePath = Path.Combine(this.userSavedDataSearchPath, savePath + "_temp_tweaks");

            base.StartCoroutine(this.SaveLifeCycle(savePath, oldSaveName)); //nesting the main coroutine lets us do stuff at the beginning and end of the save proccess even if we end up with multiple sub-routines involved
        }
        private IEnumerator SaveLifeCycle(string savePath, string oldSaveName = "")
        {
            base.StartCoroutine(SaveStatusSetter());
            yield return base.StartCoroutine(this.SaveModDataCoroutine(savePath, oldSaveName));
            Debug.Log("ACEO Tweaks | Log : Saving Finished");
            while (SaveLoadGameDataController.isSaving)
            {
                yield return new WaitForSeconds(0.05f);
            }
            Destroy(this);
        }
        private IEnumerator SaveModDataCoroutine(string savePath, string oldSaveName = "")
        {
            Debug.Log("ACEO Tweaks | Log: Saving In Progress...");

            Debug.Log("ACEO Tweaks | Debug: save path = " + savePath + " | oldSaveName = " + oldSaveName + " | userSavedDataSearchPath = " + userSavedDataSearchPath);

            yield return new WaitForEndOfFrame();

            bool isWorking = true;
            try
            {
                Utils.CreateFolderIfNotExist(tempSavePath);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Concat(new object[]
                    {
                    "[ACEO Tweaks Saving] ERROR: Couldn't create save folder: ",
                    tempSavePath,
                    ", ",
                    ex,
                    " - stack: ",
                    ex.StackTrace
                    }));
                isWorking = false;
            }
            if (!isWorking)
            {
                yield return null;
            }

            // --------------------------------------------------
            //            ACTUALLY SAVE STUFF
            // --------------------------------------------------

            if (!SaveOutCommercialFlightModelExtensions())
            {
                Debug.LogError("ACEO Tweaks | ERROR: Failed to save out commercial flight model extensions!");
                yield return null;
            }

            
            // --------------------------------------------------
            //              FINISH/CLEANUP
            // --------------------------------------------------

            while (vannillaSaving)
            {
                yield return null;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(tempSavePath);
            try
            {
                directoryInfo.MoveTo(savePath);
                Singleton<CameraController>.Instance.InvokeCaptureIssueScreenshot(Path.Combine(savePath, "thumbnail.png"), null);
            }
            catch (Exception ex3)
            {
                MonoBehaviour.print(string.Concat(new object[]
                {
                "[Saving] ERROR: Could not move from temp folder! Stack: ",
                ex3,
                " ",
                ex3.StackTrace
                }));
            }
        }
        private IEnumerator SaveStatusSetter()
        {
            // This is maybe in a race condition with the vanilla exit game funtionality and is possibly breakable
            for (; ;)
            {
                if (SaveLoadGameDataController.isSaving == false)
                {
                    SaveLoadGameDataController.isSaving = true;
                    vannillaSaving = false;
                }
                if (modSaving == false && vannillaSaving == false)
                {
                    SaveLoadGameDataController.isSaving = false;
                    break;
                }
                yield return null;
            }
        }
        private bool SaveOutCommercialFlightModelExtensions()
        {
            //Extend_CommercialFlightModel.CommercialFlightModelData[] dataArray = new Extend_CommercialFlightModel.CommercialFlightModelData[] { };
            List<Extend_CommercialFlightModel.CommercialFlightModelData> dataList = new List<Extend_CommercialFlightModel.CommercialFlightModelData>();
            string latestException; // vannilla code

            int i = 0;
            foreach (Extend_CommercialFlightModel ecfm in Singleton<ModsController>.Instance.AllExtendCommercialFlightModels())
            // get all the ecfm's from modscontroller, and have all of them give me thier serialized data forms
            {
                if (ecfm == null)
                { continue; }

                dataList.Add(ecfm.SerializedData());
                i++;
            }

            Wrapper<Extend_CommercialFlightModel.CommercialFlightModelData> obj = new Wrapper<Extend_CommercialFlightModel.CommercialFlightModelData>
            //vannilla code to wrap everything in top level braces
            {
                array = dataList.ToArray()
            };

            string text;
            try
            {
                text = JsonUtility.ToJson(obj);
            }
            catch (Exception ex3)
            {
                Debug.LogError("[Saving] ERROR: CommercialFlightModelDataWrapper object couldn't be serialized! Stack: " + ex3.StackTrace);
                latestException = ex3.StackTrace;
                return false;
            }
            if (!Utils.TryWriteFile(text, Path.Combine(tempSavePath, "CommercialFlightsData.json"), out latestException))
            {
                Debug.LogError("[Saving] ERROR: Error when writing save file to: " + tempSavePath + "/CommercialFlightsData.json");
                return false;
            }
            return true;
        } // this funtion can probably be gerenalized at some point
    }
}
