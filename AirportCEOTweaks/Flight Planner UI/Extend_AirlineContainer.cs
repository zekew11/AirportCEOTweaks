using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace AirportCEOTweaks
{
    public class Extend_AirlineContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
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

            if (infoTransform.TryGetComponent(out VerticalLayoutGroup verticalLayoutGroup))
            {
                UnityEngine.Object.DestroyImmediate(verticalLayoutGroup);
            }
            if (!infoTransform.TryGetComponent(out HorizontalLayoutGroup horizontalLayoutGroup))
            {
                horizontalLayoutGroup = infoTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            if (!infoTransform.TryGetComponent(out RectTransform infoRectTransform))
            {
                Debug.LogError("ACEO Tweaks | ERROR: A airline container had unexpected lack of rectangle");
                return;
            }

            horizontalLayoutGroup.childControlWidth = true;
            //infoRectTransform.localPosition = new Vector3(-17f, 5f, 0f);
            infoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150);
            infoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);


            foreach (Transform child in infoTransform.GetComponentsInChildren<Transform>())
            {
                float alpha = 0f;


                if (child.name == "AllocationRatio")
                {
                    alpha = .667f;
                }
                if (child.parent.name == "AllocationRatio")
                {
                    alpha = 1f;
                }

                if (child.TryGetComponent(out Image image))
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
                }
                if (child.TryGetComponent(out TextMeshProUGUI text))
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                }
               
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
            foreach (Transform child in infoTransform.GetComponentsInChildren<Transform>())
            {
                float alpha = 1f;

                if (child.TryGetComponent(out Image image))
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
                }
                if (child.TryGetComponent(out TextMeshProUGUI text))
                {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                }

            }

            ///Layout Element Perfered Height on AirlineContainer obj

            if (airlineContainerUI.gameObject.TryGetComponent(out LayoutElement layout))
            {
                layout.preferredHeight = 50;
            }

            /// Airline Logo Size
            /// 

            if (logoTransform.gameObject != null && logoTransform.gameObject.TryGetComponent<RectTransform>(out RectTransform logoRectTransform))
            {
                logoRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
            }
        }
    }
}