using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using UModFramework.API;

namespace AirportCEOTweaks
{
    [HarmonyPatch(typeof(DevelopmentPanelUI))]
    static class Patch_DevelopmentPanelUI
    {
        [HarmonyPostfix]
        [HarmonyPatch("InitializeDevelopmentPanel")]
        public static void Patch_AddMyButtons(DevelopmentPanelUI __instance)
        {
            Debug.LogError("ACEO Tweaks | DEBUG: AddMyButtons Ran");

            Transform cols = __instance.gameObject.transform.Find("ActionDisplay").Find("Columns");
            Vector3 pos = new Vector3(628, -500, 0);
            Quaternion quat = new Quaternion(0, 0, 0, 0); 
            GameObject col2 = cols.Find("Column2").gameObject;

            Debug.LogError("ACEO Tweaks | DEBUG: col2 pos = " + col2.transform.position.ToString());

            GameObject col3 = GameObject.Instantiate(col2, pos, quat, cols);
            col3.name = "Column3";
            col3.transform.name = "Column3";
            


            Transform group = col3.transform.Find("PathStatsGroup");
            group.name = "ACEOTweaksGroup";

            col3.transform.Find("ACEOTweaksGroup/PathStatsText").gameObject.AttemptEnableDisableGameObject(false);

            group.Find("Header").gameObject.GetComponent<Text>().text = "ACEO Tweaks";

            Button cancelFlightsButton = group.Find("ResetStats").gameObject.GetComponent<Button>();
            cancelFlightsButton.transform.Find("Text").gameObject.GetComponent<Text>().text = "Bulk Cancel Flights";

            cancelFlightsButton.onClick.RemoveAllListeners();
            cancelFlightsButton.onClick.AddListener(delegate ()
            {
                DialogPanel.Instance.ShowQuestionPanelCustomOptions(new Action<bool>(CancelFlights.TaskOnClick), "Cancel all unallocated flights, or ALL current flights?\n(Abort using the x-button)", "Unallocated", "ALL", true, false);
            });

            Button drawLiveryOrigins = GameObject.Instantiate<GameObject>(cancelFlightsButton.gameObject,group.transform).GetComponent<Button>();
            drawLiveryOrigins.transform.Find("Text").gameObject.GetComponent<Text>().text = "DrawLiveryOrigins";
            //drawLiveryOrigins.transform.localPosition = drawLiveryOrigins.transform.localPosition.SetY(drawLiveryOrigins.transform.localPosition.y + 20);

            drawLiveryOrigins.onClick.RemoveAllListeners();
            drawLiveryOrigins.onClick.AddListener(delegate ()
            {
                DrawOrigins.LiveryOrigins();
            }
            );
        }
    }

    public static class CancelFlights
    {
        public static void TaskOnClick(bool onlyUnAllocated)
        {
            Debug.LogError("ACEO Tweaks | Debug: Attempting to cancel flights!");
            foreach (FlightModel flight in Singleton<AirTrafficController>.Instance.GetAllFlights())
            {
                if (flight == null)
                {
                    continue;
                }
                if (flight.isAllocated && onlyUnAllocated)
                {
                    continue;
                }
                flight.CancelFlight(true);
            }
        }
    }
    public static class DrawOrigins
    {
        public static void LiveryOrigins()
        {
            Texture2D debugCross = UMFAsset.LoadTexture2D("crosshair.png");
            
            Sprite debugCrossSprite = Sprite.Create(debugCross, new Rect(0.0f, 0.0f, debugCross.width, debugCross.height), new Vector2(0.5f, 0.5f), 180.0f);
            
            foreach (FlightModel flight in Singleton<AirTrafficController>.Instance.GetAllFlights())
            {
                if (flight == null)
                {
                    continue;
                }
                if (flight.AircraftIsAssigned)
                {
                    AircraftController aircraft = flight.Aircraft;
                    GameObject gameObject = new GameObject("DebugCenter");

                    gameObject.transform.SetParent(aircraft.gameObject.transform.Find("Sprite/Livery").transform);
                    //Vector3 newPos = new Vector3(gameObject.transform.parent.position.x, gameObject.transform.parent.position.transform.position.y, -0.09f);
                    //gameObject.transform.position = gameObject.transform.parent.position.SetZ(-0.095f);
                    gameObject.transform.localPosition = Vector3.zero;
                    SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = debugCrossSprite;
                    //renderer.gameObject.layer = 9;
                    renderer.sortingLayerName = "Aircraft";
                    renderer.material = SingletonNonDestroy<DataPlaceholderMaterials>.Instance.nonLitMateral;
                    renderer.sortingOrder = 100;
                }
            }
        }
    }
}
