using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;




namespace AirportCEOTweaks
{
    public class ModsController : Singleton<ModsController>, IDontDestroyOnLoad
    {
        private Dictionary<string, Extend_CommercialFlightModel> commercialFlightExtensionRefDictionary = new Dictionary<string, Extend_CommercialFlightModel>();

        public Dictionary<CommercialFlightModel, CommercialFlightSaveData> commercialFlightLoadDataDict = new Dictionary<CommercialFlightModel, CommercialFlightSaveData>();

        private Dictionary<string, Extend_AirlineModel> airlineExtensionRefDictionary = new Dictionary<string, Extend_AirlineModel>();

        public Dictionary<string, AirlineBusinessData> airlineBusinessDataDic = new Dictionary<string, AirlineBusinessData>();

        private void Update()
        {
            if (FlightPlannerPanelUI.Instance != null && FlightPlannerPanelUI.Instance.isDisplayed == true)
            {
                FlightSlotContainerUI[] flightSlotContainerUIs = FlightPlannerPanelUI.Instance.transform.GetComponentsInChildren<FlightSlotContainerUI>();

                if (Input.GetKeyDown(AirportCEOTweaksConfig.increaseTurnaroundBind))
                {
                    turnaroundPlayerBiasBufferMins += 15f;
                    UpdateFlightSlot(flightSlotContainerUIs);
                }
                if (Input.GetKeyDown(AirportCEOTweaksConfig.decreaseTurnaroundBind))
                {
                    turnaroundPlayerBiasBufferMins -= 15f;
                    UpdateFlightSlot(flightSlotContainerUIs);
                }
                if (Input.mouseScrollDelta.y != 0)
                {
                    turnaroundPlayerBiasBufferMins += (Input.mouseScrollDelta.y * 5);
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

            //Debug.Log(StringAircraftStatistic());

            if (AirportCEOTweaksConfig.airlineNationality && !GameSettingManager.RealisticInternationalStands)
            {
                DialogPanel.Instance.ShowQuestionPanelCustomOptions(new Action<bool>(EnableRealisticInternational), "ACEO Tweaks airline nationality is enabled. Realistic international stands setting is recommended! \n \n (ACEO Tweaks options are available via shift-F10)", "Enable", "Ignore", true, false);
            }
            void EnableRealisticInternational(bool doit)
            {
                if (doit)
                {
                    GameSettingManager.RealisticInternationalStands = true;
                }
            }

            UpdateAirlineBuisinessDataDictionary();
            
        }

        public void ResetForMainMenu()
        {
            commercialFlightExtensionRefDictionary.Clear();
            airlineExtensionRefDictionary.Clear();
        }

        public static DateTime NextWeekday(Enums.Weekday weekday,int offset = 0)
        {
            if (offset == 0)
            {
                //return Singleton<TimeController>.Instance.GetNextPossibleDateBaseOnWeekday(weekday);
            }

            DateTime currentDayDT = Singleton<TimeController>.Instance.GetCurrentContinuousTime();

            int zeroDay = Singleton<TimeController>.Instance.GetTodaysIndex();
            int gotoDay = (int)weekday;

            if(zeroDay == -1 || gotoDay ==-1)
            {
                Debug.LogError("ACEO Tweaks | ERROR: NextWeekday Failed! (-1day)");
            }

            int diffDays = gotoDay - zeroDay;
            if (diffDays < 0) { diffDays += 7; }

            DateTime returnTime = new DateTime(currentDayDT.Year,currentDayDT.Month,currentDayDT.Day,0,0,0);
            returnTime = returnTime.AddDays(diffDays).AddDays(offset);
            return returnTime;
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

        private float turnaroundPlayerBiasMins = 0;
        public float turnaroundPlayerBiasBufferMins = 0;
        public void TurnaroundBiasFromBuffer()
        {
            //turnaroundPlayerBiasBufferMins = turnaroundPlayerBiasBufferMins.Clamp(-60, 90);
            TurnaroundPlayerBiasMins = turnaroundPlayerBiasBufferMins;
        }
        public float TurnaroundPlayerBiasMins
        {
            get
            {
                return turnaroundPlayerBiasMins;
            }
            set
            {
                turnaroundPlayerBiasMins       = value.Clamp(-90, 240).RoundToNearest(5);
                //turnaroundPlayerBiasBufferMins = value.Clamp(-90, 240).RoundToNearest(5);
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
            if (airline == null) { eam = null; return; }
            if (!airlineExtensionRefDictionary.TryGetValue(airline.referenceID, out eam) | eam == null)
            {
                eam = new Extend_AirlineModel(airline);
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
            UpdateAirlineBuisinessDataDictionary();
        }
        public Extend_CommercialFlightModel[] AllExtendCommercialFlightModels()
        {
            //Extend_CommercialFlightModel[] array = new Extend_CommercialFlightModel[] { };
            List<Extend_CommercialFlightModel> list = new List<Extend_CommercialFlightModel>();
            int i = 0;
            foreach (Extend_CommercialFlightModel ecfm in commercialFlightExtensionRefDictionary.Values)
            {
                if (ecfm == null)
                {
                    continue;
                }
                list.Add(ecfm);
                i++;
            }
            return list.ToArray();
        }
        private void UpdateAirlineBuisinessDataDictionary()
        {
            string filetext;

            foreach (string path in AirportCEOTweaks.airlinePaths)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (!file.EndsWith(".json"))
                    {
                        continue;
                    }
                    filetext = Utils.ReadFile(file);
                    if (filetext == null)
                    {
                        continue;
                    }

                    List<string> errors = new List<string>();
                    AirlineBusinessData data = JsonConvert.DeserializeObject<AirlineBusinessData>(
                        filetext,
                        new JsonSerializerSettings
                        {
                            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                            {
                                errors.Add(args.ErrorContext.Error.Message);
                                args.ErrorContext.Handled = true;
                            }
                        });
                    if (errors.Count>0)
                    {
                        Debug.LogWarning("ACEO Tweaks | WARN: Airline buisness data deserialization encountered errors:\n\n");
                    }

                    if (data.name == null || data.name.Length==0 || airlineBusinessDataDic.ContainsKey(data.name))
                    {
                        continue;
                    }

                    if (data.overwriteName != null && airlineBusinessDataDic.ContainsKey(data.overwriteName))
                    {
                        //merge them
                        Debug.LogError("ACEO Tweaks | Debug: Overwriting " + data.overwriteName + " with " + data.name);
                        
                        AirlineBusinessData overwrittenData = airlineBusinessDataDic[data.overwriteName];

                        overwrittenData.shortName = data.shortName.Length > 0 ? data.shortName : overwrittenData.shortName;                        // Optional: Will eventually be used in certain GUI elements. Eg. "Such&Such Airlines - Retro Liveries" becomes "Such&Such Airlines"

                        overwrittenData.description = data.description.Length > 0 ? data.description : overwrittenData.description;                  // Optional: Used if overwriting
                        overwrittenData.CEOName = data.CEOName.Length > 0 ? data.CEOName : overwrittenData.CEOName;               // Optional: Used if overwriting
                        overwrittenData.flightPrefix = data.flightPrefix.Length > 0 ? data.flightPrefix : overwrittenData.flightPrefix;                       // Optional: Used if overwriting
                        overwrittenData.businessClass = data.businessClass;

                        overwrittenData.tweaksFleet = data.tweaksFleet.Length > 0 ? data.tweaksFleet : overwrittenData.tweaksFleet;                       // Optional: Overwrites "fleet" when tweaks is installed.
                        overwrittenData.tweaksFleetCount = data.tweaksFleetCount.Length > 0 ? data.tweaksFleetCount : overwrittenData.tweaksFleetCount;                     // Optional: Overwirtes "fleetCount" when teaks is installed. Future features will expect this to be a count of aircraft, not a ratio.
                        overwrittenData.arrayHomeCountryCodes = data.arrayHomeCountryCodes.Length > 0 ? data.arrayHomeCountryCodes : overwrittenData.arrayHomeCountryCodes;
                        overwrittenData.arrayForbiddenCountryCodes = data.arrayForbiddenCountryCodes.Length > 0 ? data.arrayForbiddenCountryCodes : overwrittenData.arrayForbiddenCountryCodes;		   // Optional: Extends the "nationality" system by forbidding any flights to or from the listed countries.
                        overwrittenData.arrayHubIATAs = data.arrayHubIATAs.Length > 0 ? data.arrayHubIATAs : overwrittenData.arrayHubIATAs;					   // Optional: Can be used with other settings to refine airline routing.
                        overwrittenData.arrayRangesFromHubs_KM = data.arrayRangesFromHubs_KM.Length > 0 ? data.arrayRangesFromHubs_KM : overwrittenData.arrayRangesFromHubs_KM;               // Optional: Used with "arrayHubIATAs" to define maximum distances from each hub that the airline may operate to.

                        overwrittenData.internationalMustOriginateAtHub = data.internationalMustOriginateAtHub;	   // Optional: Used with "arrayHubIATAs" to force international flights (those that require passport check) to originate at a listed hub IATA.
                        overwrittenData.allMustOriginateAtHub = data.allMustOriginateAtHub;				   // Optional: Used with "arrayHubIATAs" to force all flights to have at least one endpoint at a listed "hub".
                        overwrittenData.remainWithinHomeCodes = data.remainWithinHomeCodes;                 // Optional: If true, airline cannot operate routes which have any endpoint outside of the Home Country(s) (either from "countryCode" or "arrayHomeCountryCodes")
                        overwrittenData.cargo = data.cargo;                                 // Optional: If true, airline only operates cargo service.

                        airlineBusinessDataDic[data.overwriteName] = overwrittenData;
                    }
                    else
                    {
                        airlineBusinessDataDic.Add(data.name, data);
                    }
                    break;
                }
            }
            AirportCEOTweaks.airlinePaths.Clear();
        }
        public static string StringAircraftStatistic()
        {
            string stringy="";
            foreach (string aircraftString in CustomEnums.GetAircraftArray())
            {
                CustomEnums.TryGetAircraftType(aircraftString, out AircraftType aircraftType);
                AircraftModel aircraftModel = Singleton<AirTrafficController>.Instance.GetAircraftModel(aircraftType.id);

                stringy = stringy.Insert(stringy.Length,aircraftModel.aircraftType);
                stringy = stringy.Insert(stringy.Length," model nbr = ");
                stringy = stringy.Insert(stringy.Length,aircraftModel.modelNbr);
                stringy = stringy.Insert(stringy.Length,"\n");
            }
            return stringy;
        }

        public bool IsDomestic(Country countryA, Country countryB = null)
        {
            if (countryA == null)
            {
                countryA = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country;
            }
            if (countryB == null)
            {
                countryB = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country;
            }
            return IsDomestic(new Country[] { countryA }, new Country[] { countryB });
        }
        public bool IsDomestic(Airport airportA, Airport airportB = null)
        {
            if (airportA == null)
            {
                airportA = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
            }
            if (airportB == null)
            {
                airportB = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
            }
            return IsDomestic(airportA.Country,airportB.Country);
        }
        public bool IsDomestic(Country[] countriesA, Country[] countriesB = null)
        {
            if (countriesA == null)
            {
                countriesA = new Country[] { GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country };
            }
            if (countriesB == null)
            {
                countriesB = new Country[] { GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country };
            }

            foreach(Country countryA in countriesA)
            {
                foreach (Country countryB in countriesB)
                {
                    if (countryA==countryB)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
