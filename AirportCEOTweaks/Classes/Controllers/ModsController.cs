using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Newtonsoft;
using TMPro;
using System.Reflection;




namespace AirportCEOTweaks
{
    public class ModsController : Singleton<ModsController>, IDontDestroyOnLoad
    {
        private Dictionary<string, Extend_CommercialFlightModel> commercialFlightExtensionRefDictionary = new Dictionary<string, Extend_CommercialFlightModel>();
        
        private Dictionary<string, Extend_AirlineModel> airlineExtensionRefDictionary = new Dictionary<string, Extend_AirlineModel>();
        public ModsController()
        { }
        private void Update()
        {
            if (FlightPlannerPanelUI.Instance != null && FlightPlannerPanelUI.Instance.isDisplayed == true)
            {
                FlightSlotContainerUI[] flightSlotContainerUIs = FlightPlannerPanelUI.Instance.transform.GetComponentsInChildren<FlightSlotContainerUI>();

                if (Input.GetKeyDown(AirportCEOTweaksConfig.increaseTurnaroundBind))
                {
                    turnaroundBiasBuffer += 0.05f;
                    UpdateFlightSlot(flightSlotContainerUIs);
                }
                if (Input.GetKeyDown(AirportCEOTweaksConfig.decreaseTurnaroundBind))
                {
                    turnaroundBiasBuffer -= 0.05f;
                    UpdateFlightSlot(flightSlotContainerUIs);
                }
                if (Input.mouseScrollDelta.y != 0)
                {
                    turnaroundBiasBuffer += (Input.mouseScrollDelta.y * 0.03f);
                    UpdateFlightSlot(flightSlotContainerUIs);
                }
            }
            void UpdateFlightSlot(FlightSlotContainerUI[] flightSlotContainerUIs)
            {
                foreach (FlightSlotContainerUI flightSlot in flightSlotContainerUIs)
                {
                    if (flightSlot.GetComponent<Canvas>().overrideSorting && flightSlot.draggingIsAllowed)
                    {
                        TurnaroundBiasFromBuffer();
                        PointerEventData pd = new PointerEventData(eventSystem: EventSystem.current);
                        bool flag = flightSlot.isBeingDragged;
                        pd.dragging = true;
                        //pd.position = flightSlot.transform.position; ----------------------------------1.3

                        ExecuteEvents.Execute<IDragHandler>(flightSlot.gameObject, pd, ExecuteEvents.dragHandler);
                        if (!flag)
                        {
                            ExecuteEvents.Execute<IEndDragHandler>(flightSlot.gameObject, pd, ExecuteEvents.endDragHandler);
                        }
                        break;
                    }
                }
            }
        }
        private void Start()
        {
            TestSerializer();
            
            //base.Awake();
            //Debug.LogError("ACEOTweaks | Mods Controller Awake");

            GameObject appLabel = GameObject.Find("ApplicationVersionLabel");
            ApplicationVersionLabelUI applicationVersionLabelUI = appLabel.GetComponent<ApplicationVersionLabelUI>();
            //Patch_AddTweaksLabel();

            /*void Patch_AddTweaksLabel()
            {
                Debug.LogError("ACEO Tweaks | DEBUG: AddTweaksLabel Ran");

                TMP_Text tMP = appLabel.transform.GetComponent<TextMeshProUGUI>();
                string str = tMP.text;

                Version version = Assembly.GetEntryAssembly().GetName().Version;

                str = str + " - AirportCEO Tweaks v" + version.ToString();
                tMP.text = str;
                Debug.LogError(str);
            }*/
        }

        public void ResetForMainMenu()
        {
            commercialFlightExtensionRefDictionary.Clear();
            airlineExtensionRefDictionary.Clear();
        }

        public static DateTime NextWeekday(Enums.Weekday weekday,int offset = 0)
        {
            DateTime currentDayDT = Singleton<TimeController>.Instance.GetCurrentContinuousTime();

            int zeroDay = Singleton<TimeController>.Instance.GetTodaysIndex();
            int gotoDay = ((int)weekday);
            int diffDays = gotoDay - zeroDay;
            if (diffDays < 0) { diffDays += 7; }

            return new DateTime(currentDayDT.Year,currentDayDT.Month,currentDayDT.Day+diffDays+offset,0,0,0);
        }

        public HashSet<CommercialFlightModel> FlightsByFlightNumber(AirlineModel airline, string flightNumber)
        {
            HashSet<CommercialFlightModel> series = new HashSet<CommercialFlightModel>();
            foreach ( CommercialFlightModel commercialFlightModel in airline.flightListObjects)
            {
                if (commercialFlightModel != null && !commercialFlightModel.isCompleted && !commercialFlightModel.isEmergency && commercialFlightModel.departureFlightNbr.Equals(flightNumber))
                {
                    series.Add(commercialFlightModel);
                }
            }
            return series;
        }
        public HashSet<CommercialFlightModel> FutureFlightsByFlightNumber(AirlineModel airline, string flightNumber, DateTime date)
        {
            HashSet<CommercialFlightModel> series = FlightsByFlightNumber(airline, flightNumber);
            HashSet<CommercialFlightModel> seriestemp = new HashSet<CommercialFlightModel>();
            //Debug.LogError("futureflightsinitcount = " + series.Count);
            foreach (CommercialFlightModel flight in series)
            {
                try
                {
                    if (flight.isAllocated && flight.departureTimeDT < date)
                    {
                        seriestemp.Add(flight);
                        //Debug.LogError("remove ++ now = " + seriestemp.Count);
                    }
                    else if (flight.isCanceled || flight.isCompleted)
                    {
                        seriestemp.Add(flight);
                        //Debug.LogError("remove ++ now = " + seriestemp.Count + "   (else if)");
                    }
                }
                catch
                {
                    try
                    {
                        seriestemp.Add(flight);
                        //Debug.LogError("remove ++ now = " + seriestemp.Count + "   (catch!)");
                    }
                    catch
                    {
                        //Debug.LogError("could not remove a flight from the series!");
                    }
                }
            }
            series.ExceptWith(seriestemp);
            //Debug.LogError("futureflightscount returning as " + series.Count);
            return series;
        }

        public List<string> LiveryGroupWords()
        {
            List<string> groups = new List<string>();
            groups.Add("wings"); //search by wings
            groups.Add("tail"); //search by tail
            groups.Add("shadow"); //specific: the object
            groups.Add("lights"); //specific: the group
            groups.Add("effects"); //specific: the group
            groups.Add("flaps"); //search by flaps
            groups.Add("windows"); //specific: the group
            groups.Add("frontdoors"); // doors forward of default 0 position
            groups.Add("reardoors"); // doors behind default 0 position
            groups.Add("towbar"); // doors behind default 0 position
            groups.Add("audio"); //specific: group
            groups.Add("groundequipment"); // specific:group
            groups.Add("livery"); // doors behind default 0 position
            groups.Add("self"); //specific: this livery object
            groups.Add("aircraftconfig"); //special: things like PAX numbers ect.
            groups.Add("exactly"); //Last: trigger search for a transform by name

            return groups;
        }
        public List<string> LiveryActionWords()
        {
            
            List<string> verbs = new List<string>();

            verbs.Add("setpax"); // pax capacity
            verbs.Add("setrows"); // rows abreast (for PAX seat numbers)
            verbs.Add("setrange"); // kilometers
            verbs.Add("setstairs"); // true for aircraft that have own stairs / don't need jetway
            verbs.Add("settype"); // Preset aircraft types. To be used in generating flights for custom aircraft.
            verbs.Add("settitle"); // In-Game Title. Can add a little more detail than just the aircraft type, EG "747-400 (Converted Freighter)".
            verbs.Add("setbuilder"); // Boeing, Airbus, Ect...
            verbs.Add("setreg"); // Registration number(s). Makes each reg # listed a unique aircraft with no duplication.
            verbs.Add("setsize"); //small/med/large


            verbs.Add("disable"); // turn off/hide
            verbs.Add("enable"); // turn on/show. usefull if you want to hide all members of a group except one or two: hide the group by keyword and then show the member to keep.
            verbs.Add("moveabs"); //move to a new absolute position
            verbs.Add("moverel"); //change position by this amount
            verbs.Add("makeshadow"); //change layer/shader to shadow. used with "self".
            verbs.Add("setlayerorder"); 
            verbs.Add("makewindow"); 
            verbs.Add("makenonlit"); //change layer/shader to nonlit. used with "self".
            verbs.Add("makelighting"); //make a light source. Simular to taxi lights. Implimentation TBD. used with "self".
            verbs.Add("makelightsprite"); //make a light texture. Simular to night windows. used with "self".
            verbs.Add("makechildof"); //put the oject in a group. EG put put custom night windows in the night windows group so the game knows to toggle them at night-time.

            return verbs;
        }

        private float turnaroundBias = 1f;
        public float turnaroundBiasBuffer = 1f;
        public void TurnaroundBiasFromBuffer()
        {
            turnaroundBiasBuffer = turnaroundBiasBuffer.Clamp(0.8f, 1.25f);
            turnaroundBias = turnaroundBiasBuffer;
        }
        public float TurnaroundBias
        {
            get
            {
                return turnaroundBias.Clamp(0.8f, 1.25f);
            }
            set
            {
                turnaroundBiasBuffer = value.Clamp(0.8f, 1.25f);
            }
        }
        public void GetExtensions(CommercialFlightModel cfm, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam)
        {
            //Singleton<ModsController>.Instance.GetExtensions(parent, out Extend_CommercialFlightModel ecfm, out Extend_AirlineModel eam)
            
            if (cfm == null)
            {
                Debug.LogError("ACEO Tweaks | ERROR: Get extensions called with null cfm"); ecfm = null; eam = null; return;
            }
            if (cfm.Airline == null)
            {
                Debug.LogError("ACEO Tweaks | ERROR: Get extensions called with cfm's airline null"); ecfm = null; eam = null; return;
            }
            if (cfm.Airline.referenceID == null)
            {
                Debug.LogError("ACEO Tweaks | ERROR: Get extensions called with cfm's airline's refid null"); ecfm = null; eam = null; return;
            }
            if (cfm.ReferenceID == null)
            {
                Debug.LogError("ACEO Tweaks | ERROR: Get extensions called with cfm's refid null"); ecfm = null; eam = null; return;
            }

            GetExtensions(cfm.Airline, out eam);
            
            if (!commercialFlightExtensionRefDictionary.TryGetValue(cfm.ReferenceID, out ecfm) || ecfm == null)
            {
                ecfm = new Extend_CommercialFlightModel(cfm, eam);
                ecfm.Initialize();
            }
        }
        public void GetExtensions(AirlineModel airline, out Extend_AirlineModel eam)
        {
            if (!airlineExtensionRefDictionary.TryGetValue(airline.referenceID, out eam) | eam == null)
            {
                eam = new Extend_AirlineModel(airline);
            }
        }
        public void RegisterECFM(ref Extend_CommercialFlightModel ecfm, CommercialFlightModel cfm)
        {
            Extend_CommercialFlightModel tempecfm;
            if (commercialFlightExtensionRefDictionary.TryGetValue(cfm.ReferenceID,out tempecfm))
            {
                ecfm = tempecfm;
                Debug.LogError("ACEO Tweak | WARN: Attempted to double-register ECFM for " + cfm.departureFlightNbr);
                return;
            }
            else
            {
                commercialFlightExtensionRefDictionary.Add(cfm.ReferenceID, ecfm);
            }
        }
        public void RegisterEAM(ref Extend_AirlineModel eam, AirlineModel am)
        {
            Extend_AirlineModel tempeam;
            if(airlineExtensionRefDictionary.TryGetValue(am.referenceID,out tempeam))
            {
                eam = tempeam;
                Debug.LogError("ACEO Tweak | WARN: Attempted to double-register EAM for " + am.businessName);
                return;
            }
            else
            {
                airlineExtensionRefDictionary.Add(am.referenceID, eam);
            }
        }
        public void RegisterThisECFM(Extend_CommercialFlightModel ecfm, CommercialFlightModel cfm)
        {
            if (commercialFlightExtensionRefDictionary.ContainsKey(cfm.ReferenceID))
            {
                Debug.LogError("ACEO Tweak | WARN: Attempted to double-register ECFM for " + cfm.departureFlightNbr + "in ECFM creation!");
                commercialFlightExtensionRefDictionary.Remove(cfm.ReferenceID);
            }

            commercialFlightExtensionRefDictionary.Add(cfm.ReferenceID, ecfm);

        }
        public void RegisterThisEAM(Extend_AirlineModel eam, AirlineModel am)
        {
            if (airlineExtensionRefDictionary.ContainsKey(am.referenceID))
            {
                Debug.LogError("ACEO Tweak | WARN: Attempted to double-register EAM for " + am.businessName + "in EAM creation!");
                airlineExtensionRefDictionary.Remove(am.referenceID);
            }

            airlineExtensionRefDictionary.Add(am.referenceID, eam);
            
        }

        public static void TestSerializer()
        {
            Debug.LogError("ACEO Tweaks | Test Serializer Init");

            string assetsPath = UModFramework.API.UMFData.AssetsPath;
            Debug.LogError("ACEO Tweaks | UMF AsssetsPath = " + assetsPath);

            string savePath = Path.Combine(Singleton<SaveLoadGameDataController>.Instance.GetUserSavedDataSearchPath(), Singleton<SaveLoadGameDataController>.Instance.saveName);
            Debug.LogError("ACEO Tweaks | ACEO SavePath = " + savePath);

            savePath = assetsPath;

            AircraftTypeData datum = new AircraftTypeData
            {
                id = "A318",

                size = Enums.GenericSize.Medium,

                displayName = "A318",
                manufacturer = "Airbus",
                iCAOCode = "A318",

                capacity_PAX = 107,
                exitLimit_PAX = 136,
                seatsAbreast = 6,
                capacity_ULD = 0,

                range_KM = 5740,
                speed_KMH = 829,
                etops_Minutes = 180,
                fuelCapacity_L = 24210,
                jP1 = true,

                takeoffDistance_M = 1780,
                maxTOW_KG = 68000,
                iCAO_Class = 'C',
                numEngines = 2,
                engineType = Enums.AircraftEngineType.Jet,

                numBuilt = 81,
                yearIntroduced = 2003,
                yearLastProduced = 2013,
                yearRetired = -1,

                needStairs = true,
                needPushback = true,
                needPaved = true,
                heavy = false,
                canJetbridge = true,
                canPushback = true,
                sonicBoom = false,
                vIP = false,
                combi = false,
                cargo = false,

                loud = false,
                quiet = false
            };

            List<AircraftTypeData> aircraftTypeDatas = new List<AircraftTypeData>
            {
                datum
            };

            //reused from TrySerializeAircrafts
            Wrapper<AircraftTypeData> obj = new Wrapper<AircraftTypeData>
            {
                array = aircraftTypeDatas.ToArray()
            };

            string text;

            try
            {
                text = JsonUtility.ToJson(datum);
            }

            catch (Exception ex3)
            {
                Debug.LogError("ACEO Tweaks | ERROR: AircraftTypeData object couldn't be serialized! Stack: " + ex3.StackTrace);
                return;
            }
            if (Utils.CreateFromJSON<AircraftModelSerialized>(text) == null)
            {
                Debug.LogError("ACEO Tweaks | ERROR: .json AircraftTypeData not validated!");
            }
            if (!Utils.TryWriteFile(text, Path.Combine(savePath, "AircraftTypeData.json"), out string latestException))
            {
                Debug.LogError("[Saving] ERROR: Error when writing save file to: " + savePath + "/AircraftData.json");
            }
            Debug.LogError("ACEO Tweaks | Debug: Finished Test Serialize");
            
        }
    }
}
