using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace AirportCEOTweaks
{
    public class Extend_FlightSlotContainerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
    {
        private FlightSlotContainerUI flightSlotContainer;
        private RectTransform rectTransform;
        private LayoutElement layoutElement;
        private Image containerImage;
        private Transform rightEdge;
        private Transform leftEdge             ;
        private Image airlineLogo              ;
        private TextMeshProUGUI flightNbrText  ; 
        private TextMeshProUGUI paymentPerFlight;
        private Image flightSizeImage;
        private Image flightBusinessClassImage;
        private Image reoccuringImage;
        private Image internationalIcon;
        private Image flightCompletionImage;
        private Image overlayBackgroundImage;
        private Transform flightTimeDisplay;
        private Image flightTimeDisplayImage;
        private TextMeshProUGUI arrivalTimeValueText;
        private TextMeshProUGUI turnaroundTimeValueText;
        private TextMeshProUGUI departureTimeValueText;
        private Button confirmFlightPlanButton;
        private Image flightActivationIndicator;
        private RectTransform banner;
        private RectTransform reoccuringGroup;
        private TextMeshProUGUI aircraftTypeText;
        private CommercialFlightModel commercialFlightModel;
        AircraftModel aircraft;

        public void ConstructMe(FlightSlotContainerUI flightSlotContainer, CommercialFlightModel commercialFlightModel)
        {
            this.commercialFlightModel = commercialFlightModel;
            this.flightSlotContainer = flightSlotContainer;

            InitializeContainer();
            PreConfigure();
        }

        private void InitializeContainer()
        {
            rectTransform = base.GetComponent<RectTransform>();
            layoutElement = base.GetComponent<LayoutElement>();
            containerImage = base.GetComponent<Image>();
            rightEdge = base.transform.Find("RightEdge");
            leftEdge = base.transform.Find("LeftEdge");
            airlineLogo = base.transform.Find("AirlineLogo").GetComponent<Image>();
            flightNbrText = base.transform.Find("FlightNbrText").GetComponent<TextMeshProUGUI>();
            paymentPerFlight = base.transform.Find("PaymentPerFlight").GetComponent<TextMeshProUGUI>();
            flightSizeImage = base.transform.Find("FlightClass").GetComponent<Image>();
            flightBusinessClassImage = base.transform.Find("FlightBusinessClass").GetComponent<Image>();
            reoccuringImage = base.transform.Find("ReoccuringGroup/ReoccuringImage").GetComponent<Image>();
            reoccuringGroup = base.transform.Find("ReoccuringGroup").GetComponent<RectTransform>();
            internationalIcon = base.transform.Find("InternationalIcon").GetComponent<Image>();
            flightCompletionImage = base.transform.Find("FlightCompletionImage").GetComponent<Image>();
            overlayBackgroundImage = base.transform.Find("OverlayBackground").GetComponent<Image>();
            flightTimeDisplay = base.transform.Find("FlightTimeDisplay").transform;
            flightTimeDisplayImage = this.flightTimeDisplay.GetComponent<Image>();
            arrivalTimeValueText = this.flightTimeDisplay.Find("ArrivalTimeValueText").GetComponent<TextMeshProUGUI>();
            turnaroundTimeValueText = this.flightTimeDisplay.Find("TurnaroundTimeValueText").GetComponent<TextMeshProUGUI>();
            departureTimeValueText = this.flightTimeDisplay.Find("DepartureTimeValueText").GetComponent<TextMeshProUGUI>();
            confirmFlightPlanButton = this.flightTimeDisplay.Find("ConfirmFlightPlanButton").GetComponent<Button>();
            flightActivationIndicator = base.transform.Find("FlightActivationIndicator").GetComponent<Image>();
            banner = base.transform.Find("BottomBanner").GetComponent<RectTransform>();

            //Aircraft Type Text
            GameObject typeText = GameObject.Instantiate(paymentPerFlight.gameObject,base.gameObject.transform);
            typeText.name = "typeText";
            aircraftTypeText = typeText.GetComponent<TextMeshProUGUI>();

            aircraft = Singleton<AirTrafficController>.Instance.GetAircraftModel(commercialFlightModel.aircraftTypeString);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ConfigureMaximized();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ConfigureMinimized();
        }

        public void PreConfigure()
        {
            Color darkBgTextColor = Color.white;
            
            //Airline logo centered and width-limited

            airlineLogo.rectTransform.anchorMin = new Vector2(.25f, 1);
            airlineLogo.rectTransform.anchorMax = new Vector2(.25f, 1);
            airlineLogo.rectTransform.ForceUpdateRectTransforms();
            airlineLogo.rectTransform.anchoredPosition = new Vector2(50, -30);
            airlineLogo.rectTransform.ForceUpdateRectTransforms();
            airlineLogo.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 80);
            airlineLogo.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);

            // center stars

            flightBusinessClassImage.gameObject.AttemptEnableDisableGameObject(true);
            flightBusinessClassImage.rectTransform.anchorMin = new Vector2(.5f, 1);
            flightBusinessClassImage.rectTransform.anchorMax = new Vector2(.5f, 1);
            flightBusinessClassImage.rectTransform.ForceUpdateRectTransforms();
            flightBusinessClassImage.rectTransform.anchoredPosition = new Vector2(0, -6);
            flightBusinessClassImage.rectTransform.ForceUpdateRectTransforms();
            flightBusinessClassImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 12);

            //payment

            paymentPerFlight.rectTransform.anchorMin = new Vector2(.5f, 0);
            paymentPerFlight.rectTransform.anchorMax = new Vector2(.5f, 0);
            paymentPerFlight.rectTransform.ForceUpdateRectTransforms();
            paymentPerFlight.rectTransform.anchoredPosition = new Vector2(0, 8);

            //recouring

            reoccuringGroup.anchoredPosition = new Vector2(32, -60);
            reoccuringGroup.GetComponentInChildren<Image>().color = darkBgTextColor;
            reoccuringGroup.GetComponentInChildren<TextMeshProUGUI>().color = darkBgTextColor;

            //international

            internationalIcon.rectTransform.anchorMin = new Vector2(0, 0);
            internationalIcon.rectTransform.anchorMax = new Vector2(0, 0);
            internationalIcon.rectTransform.ForceUpdateRectTransforms();
            internationalIcon.rectTransform.anchoredPosition = new Vector2(58, 8);
            internationalIcon.color = darkBgTextColor;

            //size
            flightSizeImage.rectTransform.anchorMin = new Vector2(.85f, 0.5f);
            flightSizeImage.rectTransform.anchorMax = new Vector2(.85f, 0.5f);
            flightSizeImage.rectTransform.ForceUpdateRectTransforms();
            flightSizeImage.rectTransform.anchoredPosition = new Vector2(0, 0);
            flightSizeImage.rectTransform.ForceUpdateRectTransforms();
            flightSizeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30);
            flightSizeImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 30);
            
            
            //new thing for aircraft type
            aircraftTypeText.text = aircraft.aircraftType;
            aircraftTypeText.color = darkBgTextColor;
            aircraftTypeText.overflowMode = TextOverflowModes.Ellipsis;
            aircraftTypeText.alignment = TextAlignmentOptions.CenterGeoAligned;
            aircraftTypeText.rectTransform.anchorMin = new Vector2(.85f, 0f);
            aircraftTypeText.rectTransform.anchorMax = new Vector2(.85f, 0f);
            aircraftTypeText.rectTransform.ForceUpdateRectTransforms();
            aircraftTypeText.rectTransform.anchoredPosition = new Vector2(0, 8);
            aircraftTypeText.rectTransform.ForceUpdateRectTransforms();
            aircraftTypeText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (rightEdge.localPosition.x - aircraftTypeText.rectTransform.localPosition.x) * 1.9f);
            //aircraftTypeText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right,1,60);
            aircraftTypeText.GetComponent<HoverToolTip>().headerToDisplay = $"{aircraft.manufacturer} {aircraft.modelNbr}";
            aircraftTypeText.GetComponent<HoverToolTip>().textToDisplay = AircraftTooltip();
        }
        public void ConfigureMinimized()
        {
            
        }
        public void ConfigureMaximized()
        {
            
        }
        public string AircraftTooltip()
        {
            

            return 
                   $"PAX Capacity: {aircraft.MaxPax}\n" +
                   $"Fuel Capacity: {aircraft.fuelTankCapacityLiters} liters\n" +
                   $"Operating Range: {aircraft.rangeKM} km\n";

        }
    }
}