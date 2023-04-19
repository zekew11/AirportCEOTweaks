using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace AirportCEOTweaks
{
    public class Extend_AirlineContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler, IPointerClickHandler
    {
        public AirlineContainerUI AirlineContainerUI { get; private set; }
        public Transform InfoTransform { get; private set; }
        public Transform LogoTransform { get; private set; }
        public void ConstructMe(AirlineContainerUI airlineContainerUI, Transform infoTransform, Image airlineLogo)
        {
            AirlineContainerUI = airlineContainerUI;
            InfoTransform = infoTransform;
            LogoTransform = airlineLogo.transform;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            ConfigureMaximized();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ConfigureMinimized();
        }

        private void ConfigureMinimized()
        {
            ConfigureMinimized(AirlineContainerUI, InfoTransform, LogoTransform);
        }
        private void ConfigureMaximized()
        {
            ConfigureMaximized(AirlineContainerUI, InfoTransform, LogoTransform);
        }
        public static void ConfigureMinimized(AirlineContainerUI airlineContainerUI, Transform infoTransform, Transform logoTransform)
        {
            
            ///Info Cards

            if (!infoTransform.TryGetComponent(out RectTransform infoRectTransform))
            {
                Debug.LogError("ACEO Tweaks | ERROR: A airline container had unexpected lack of rectangle");
                return;
            }

            infoTransform.TryGetComponent(out VerticalLayoutGroup layoutGroup);

            layoutGroup.childControlWidth = true;
            //infoRectTransform.localPosition = new Vector3(-17f, 5f, 0f);
            infoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 60);
            infoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);


            for (int i = 0; i<infoTransform.childCount; i++)
            {
                Transform child = infoTransform.GetChild(i);

                if (child.name == "AllocationRatio" || child.parent.name == "AllocationRatio")
                {
                    continue;
                }

                child.AttemptEnableDisableGameObject(false);
            }


            ///Layout Element Perfered Height on AirlineContainer obj

            if (airlineContainerUI.gameObject.TryGetComponent(out LayoutElement layout))
            {
                layout.preferredHeight = 35;
            }

            /// Airline Logo Size
            /// 

            if (logoTransform.gameObject != null && logoTransform.gameObject.TryGetComponent<RectTransform>(out RectTransform logoRectTransform))
            {
                logoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 35);
            }
        }

        public static void ConfigureMaximized(AirlineContainerUI airlineContainerUI, Transform infoTransform, Transform logoTransform)
        {

            if (!infoTransform.TryGetComponent(out RectTransform infoRectTransform))
            {
                Debug.LogError("ACEO Tweaks | ERROR: A airline container had unexpected lack of rectangles");
                return;
            }
            infoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 60);
            infoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 60);

            for (int i = 0; i < infoTransform.childCount; i++)
            {
                Transform child = infoTransform.GetChild(i);
                child.AttemptEnableDisableGameObject(true);
            }

            ///Layout Element Perfered Height on AirlineContainer obj

            if (airlineContainerUI.gameObject.TryGetComponent(out LayoutElement layout))
            {
                layout.preferredHeight = 60;
            }

            /// Airline Logo Size
            /// 

            if (logoTransform.gameObject != null && logoTransform.gameObject.TryGetComponent<RectTransform>(out RectTransform logoRectTransform))
            {
                logoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button==PointerEventData.InputButton.Right)
            {

            }
        }
    }
}