using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace AirportCEOAircraft
{
    [HarmonyPatch(typeof(AircraftController))]
    static class Patch_AircraftController
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AircraftController), "SetLivery", new Type[] { })]
        public static void Patch_LiveryAddActive(AircraftController __instance)
        {
            //if (AirportCEOTweaksConfig.liveryExtensions == false) { return; }
            try
            {
                Transform liveryTransform = __instance.Transform.Find("Sprite").Find("Livery").GetChild(0);
                GameObject liveryGameObject = liveryTransform.gameObject;
                Livery livery = liveryGameObject.GetComponent<Livery>();
                List<GameObject> componentGameObjects = new List<GameObject>();
                
                for (int i = 0;   i<liveryTransform.childCount; i++)
                {
                    componentGameObjects.Add(liveryTransform.GetChild(i).gameObject);
                }

                LiveryActiveComponent lac = __instance.gameObject.GetComponent<LiveryActiveComponent>();

                if (lac == null)
                {
                    lac = __instance.gameObject.AddComponent<LiveryActiveComponent>();
                }

                foreach (GameObject obj in componentGameObjects)
                {
                    lac.DoLiveryComponentActions(obj);
                }
            }
            catch
            {
                Debug.LogError("ACEO Tweaks | Error: failed to postfix SetLivery in Aircraft Controller");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ResetLivery")]
        public static bool Patch_LetParentScaleLiv(GameObject livery, Transform ___liveryTransform)
        {
            if (livery != null)
            {
                livery.transform.SetParent(___liveryTransform, false);
                livery.transform.localPosition = Vector3.zero;
                livery.transform.localEulerAngles = Vector3.zero;
            }
            return false;
        }
    }
}
   

