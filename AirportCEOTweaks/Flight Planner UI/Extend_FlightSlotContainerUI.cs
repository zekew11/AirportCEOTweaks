using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Linq;

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
        private Extend_CommercialFlightModel extend_CommercialFlightModel;
        private Extend_AirlineModel extend_AirlineModel;
        AircraftModel aircraftModel;

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

            SyncFlightModel();
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
            paymentPerFlight.alignment = TextAlignmentOptions.CenterGeoAligned;

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
            flightSizeImage.gameObject.GetComponent<HoverToolTip>().headerToDisplay = $"{commercialFlightModel.weightClass.ToString()}; {extend_CommercialFlightModel.GetDynamicFlightSize().ToString()}";
            flightSizeImage.gameObject.GetComponent<HoverToolTip>().textToDisplay = $"The {aircraftModel.modelNbr} operating this flight requires {commercialFlightModel.weightClass} infustructure. " +
                                                                                    $"More specifically, this is a {extend_CommercialFlightModel.GetDynamicFlightSize().ToString()} flight.\n\n" +
                                                                                    $"Possible Sizes:\n\n" +
                                                                                        $"{Tweaks8SizeScale.VerySmall}     - eg C182 \n" +
                                                                                    $"{Tweaks8SizeScale.Small}             - eg ATR42\n\n" +
                                                                                        $"{Tweaks8SizeScale.SubMedium}     - eg Q400 \n" +
                                                                                     $"{Tweaks8SizeScale.Medium}           - eg A320 \n" +
                                                                                          $"{Tweaks8SizeScale.SuperMedium} - eg B757 \n\n" +
                                                                                    $"{Tweaks8SizeScale.Large}             - eg B767 \n" +
                                                                                        $"{Tweaks8SizeScale.VeryLarge}     - eg A330 \n" +
                                                                                    $"{Tweaks8SizeScale.Jumbo}             - eg B747 \n\n" +
                                                                                    $"(Implimenting the 8-size-scale is WIP)";
            
            
            //new thing for aircraft type
            aircraftTypeText.text = aircraftModel.aircraftType;
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
            aircraftTypeText.GetComponent<HoverToolTip>().headerToDisplay = $"{aircraftModel.manufacturer} {aircraftModel.modelNbr}";
            aircraftTypeText.GetComponent<HoverToolTip>().textToDisplay = AircraftTooltip();
        }
        public void ConfigureMinimized()
        {
            
        }
        public void ConfigureMaximized()
        {
            
        }

        public void SyncFlightModel()
        {
            this.commercialFlightModel = flightSlotContainer.flight;
            aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(commercialFlightModel.aircraftTypeString);
            Singleton<ModsController>.Instance.GetExtensions(commercialFlightModel, out extend_CommercialFlightModel, out extend_AirlineModel);
        }
        public string AircraftTooltip()
        {

            if (extend_CommercialFlightModel.TryGetAircraftTypeData(out AircraftTypeData aircraftTypeData))
            {
                int num = aircraftTypeData.numEngines;
                string engineCount = num == 1 ? "Single" : num == 2 ? "Twin" : num == 3 ? "Tri" : num == 4 ? "Quad" : num.ToString();
                string engineType = aircraftTypeData.engineType[0].Split('_').Last();

                string config = aircraftTypeData.vIP[0]   ? $"VIP layout; {aircraftModel.MaxPax} PAX capacity\n" :
                                aircraftTypeData.combi[0] ? $"Combi layout; {aircraftModel.MaxPax} PAX & {aircraftTypeData.capacityBulkCargo_KG} kg cargo capacity\n" :
                                aircraftTypeData.cargo[0] ? $"Freighter; {aircraftTypeData.capacityBulkCargo_KG} kg cargo capacity\n" :
                                                            $"PAX Capacity: {aircraftModel.MaxPax}\n";


                return
                       $"{(aircraftTypeData.sonicBoom[0] ? "Supersonic " : "Propulsion: ")}{engineCount}-{engineType}\n" +
                       $"{aircraftTypeData.numBuilt[0]} built {aircraftTypeData.yearIntroduced[0]}-{(aircraftTypeData.yearLastProduced[0] >= 2023 ? "Present" : aircraftTypeData.yearLastProduced[0].ToString())}\n" +
                       config +
                       $"Operating Range: {((float)aircraftModel.rangeKM).RoundToNearest(50)} km\n" +
                       $"Min Runway: {((float)aircraftTypeData.takeoffDistance_M[0]).RoundToNearest(25)} meters\n" +
                       $"MTOW: {((float)aircraftTypeData.maxTOW_KG[0]).RoundToNearest(100)} kg\n" +
                       $"Fuel Capacity: {aircraftModel.fuelTankCapacityLiters.RoundToNearest(10)} liters\n" +
                       $"Fuel Type : {aircraftModel.FuelType}\n\n" +
                       $"{(!aircraftTypeData.needStairs[0] ? "Built-in stairs\n" : "")}" +
                       $"{(!aircraftTypeData.needPaved[0] ? "Unpaved-capable\n" : "")}" +
                       $"{(aircraftTypeData.loud[0] ? "Heavy noise pollution\n" : "")}" +
                       $"{(aircraftTypeData.quiet[0] ? "Low noise pollution\n" : "")}";
            }
            else
            {
                return

                       $"PAX Capacity: {aircraftModel.MaxPax}\n" +
                       $"Operating Range: {((float)aircraftModel.rangeKM).RoundToNearest(50)} km\n" +
                       $"Fuel Capacity: {aircraftModel.fuelTankCapacityLiters.RoundToNearest(10)} liters\n" +
                       $"Fuel Type : {aircraftModel.FuelType}\n\n" +
                       $"(This Aircraft Not Yet Extended By Tweaks)";
            }
                   

        }
    }
}