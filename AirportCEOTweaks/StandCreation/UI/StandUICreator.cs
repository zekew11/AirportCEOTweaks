using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AirportCEOTweaks
{
    class StandUICreator
    {
        private static Dictionary<string, GameObject> UIPanels;

        public static void CreateStandUI(GameObject standObject)
        {
            if (!TryGetUIPanels())
            {
                return;
            }

            // UI
            GameObject panel = UIPanels["ZoneAndRoomViewport"];
            GameObject button = UIPanels["DecorationViewport"].transform.GetChild(0).gameObject;

            GameObject newButton = GameObject.Instantiate(button, Vector3.zero, button.transform.rotation);
            newButton.name = $"CustomItem";
            newButton.transform.SetParent(panel.transform);

            GameObject.Destroy(newButton.GetComponent<BuildButtonAssigner>());
            GameObject.Destroy(newButton.GetComponent<BuildButtonTextManager>());

            newButton.gameObject.AddComponent(typeof(StandUIComponent));
            StandUIComponent buildUI = newButton.GetComponent<StandUIComponent>();

            buildUI.assignedButton = newButton.GetComponent<Button>();
            buildUI.assignedObject = standObject;
            buildUI.assignedAnimator = newButton.GetComponent<Animator>();

            buildUI.convertButtonToCustom();
        }

        private static bool TryGetUIPanels()
        {
            if (UIPanels != null)
            {
                return true;
            }

            try
            {
                UIPanels = new Dictionary<string, GameObject>();
                List<Transform> panels = Singleton<PlaceablePanelUI>.Instance.availablePanels;

                for (int i = 0; i < panels.Count; i++)
                {
                    switch (panels[i].name)
                    {
                        case "ZoneAndRoomViewport":
                            UIPanels.Add("ZoneAndRoomViewport", panels[i].GetChild(0).GetChild(2).GetChild(0).gameObject);
                            break;
                        case "ConveyorBeltSystemViewport":
                            UIPanels.Add("ConveyorBeltSystemViewport", panels[i].GetChild(0).GetChild(3).GetChild(0).gameObject);
                            break;
                        case "StaffViewport":
                            UIPanels.Add("StaffViewport", panels[i].GetChild(0).GetChild(2).GetChild(0).gameObject);
                            break;
                        case "DeskViewport":
                            UIPanels.Add("DeskViewport", panels[i].GetChild(0).GetChild(2).GetChild(0).gameObject);
                            break;
                        case "SecurityViewport":
                            UIPanels.Add("SecurityViewport", panels[i].GetChild(0).GetChild(2).GetChild(0).gameObject);
                            break;
                        case "BathroomViewport":
                            UIPanels.Add("BathroomViewport", panels[i].GetChild(0).GetChild(1).GetChild(1).gameObject);
                            break;
                        case "ShopRoomViewport":
                            UIPanels.Add("ShopRoomViewport", panels[i].GetChild(0).GetChild(1).GetChild(0).gameObject);
                            break;
                        case "FoodRoomViewport":
                            UIPanels.Add("FoodRoomViewport", panels[i].GetChild(0).GetChild(1).GetChild(1).gameObject);
                            break;
                        case "AirlineLoungeViewport":
                            UIPanels.Add("AirlineLoungeViewport", panels[i].GetChild(0).GetChild(1).GetChild(0).gameObject);
                            break;
                        case "DecorationViewport":
                            UIPanels.Add("DecorationViewport", panels[i].GetChild(0).GetChild(0).GetChild(2).gameObject);
                            break;
                    }
                }

                AirportCEOTweaks.Log("[Mod Success] Got UI panels!");
                return true;
            }
            catch (Exception ex)
            {
                AirportCEOTweaks.Log("[Mod Error] Failed to get UI panel. Error: " + ex.Message);
                return false;
            }
        }

    }
}
