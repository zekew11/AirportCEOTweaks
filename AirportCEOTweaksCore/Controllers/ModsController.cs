using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;


namespace AirportCEOTweaksCore
{
    public class ModsController : Singleton<ModsController>
    {
        public Dictionary<string, AirlineBusinessData> airlineBusinessDataByBusinessName = new Dictionary<string, AirlineBusinessData>();

        private void Start()
        {

            UpdateAirlineBuisinessDataDictionary();

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


        private void UpdateAirlineBuisinessDataDictionary()
        {
            string filetext;
            Debug.Log("Mods controller is creating the buisiness data dict");
            foreach (string path in AirportCEOTweaksCore.airlinePaths)
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
                    if (errors.Count > 0)
                    {
                        Debug.LogWarning("ACEO Tweaks | WARN: Airline buisness data deserialization encountered errors:\n\n");
                    }

                    if (data.name == null || data.name.Length == 0 || airlineBusinessDataByBusinessName.ContainsKey(data.name))
                    {
                        continue;
                    }

                    if (data.overwriteName != null && airlineBusinessDataByBusinessName.ContainsKey(data.overwriteName))
                    {
                        //merge them
                        Debug.LogError("ACEO Tweaks | Debug: Overwriting " + data.overwriteName + " with " + data.name);

                        AirlineBusinessData overwrittenData = airlineBusinessDataByBusinessName[data.overwriteName];

                        overwrittenData.shortName = data.shortName.Length > 0 ? data.shortName : overwrittenData.shortName;                        // Optional: Will eventually be used in certain GUI elements. Eg. "Such&Such Airlines - Retro Liveries" becomes "Such&Such Airlines"

                        overwrittenData.description = data.description.Length > 0 ? data.description : overwrittenData.description;                  // Optional: Used if overwriting
                        overwrittenData.CEOName = data.CEOName.Length > 0 ? data.CEOName : overwrittenData.CEOName;               // Optional: Used if overwriting
                        overwrittenData.flightPrefix = data.flightPrefix.Length > 0 ? data.flightPrefix : overwrittenData.flightPrefix;                       // Optional: Used if overwriting
                        overwrittenData.businessClass = data.businessClass;

                        overwrittenData.tweaksFleet = data.tweaksFleet.Length > 0 ? data.tweaksFleet : overwrittenData.tweaksFleet;                       // Optional: Overwrites "fleet" when tweaks is installed.
                        overwrittenData.tweaksFleetCount = data.tweaksFleetCount.Length > 0 ? data.tweaksFleetCount : overwrittenData.tweaksFleetCount;                     // Optional: Overwirtes "fleetCount" when teaks is installed. Future features will expect this to be a count of aircraft, not a ratio.
                        overwrittenData.arrayHomeCountryCodes = data.arrayHomeCountryCodes.Length > 0 ? data.arrayHomeCountryCodes : overwrittenData.arrayHomeCountryCodes;
                        overwrittenData.arrayForbiddenCountryCodes = data.arrayForbiddenCountryCodes.Length > 0 ? data.arrayForbiddenCountryCodes : overwrittenData.arrayForbiddenCountryCodes;        // Optional: Extends the "nationality" system by forbidding any flights to or from the listed countries.
                        overwrittenData.arrayHubIATAs = data.arrayHubIATAs.Length > 0 ? data.arrayHubIATAs : overwrittenData.arrayHubIATAs;                    // Optional: Can be used with other settings to refine airline routing.
                        overwrittenData.arrayRangesFromHubs_KM = data.arrayRangesFromHubs_KM.Length > 0 ? data.arrayRangesFromHubs_KM : overwrittenData.arrayRangesFromHubs_KM;               // Optional: Used with "arrayHubIATAs" to define maximum distances from each hub that the airline may operate to.

                        overwrittenData.internationalMustOriginateAtHub = data.internationalMustOriginateAtHub;    // Optional: Used with "arrayHubIATAs" to force international flights (those that require passport check) to originate at a listed hub IATA.
                        overwrittenData.allMustOriginateAtHub = data.allMustOriginateAtHub;                // Optional: Used with "arrayHubIATAs" to force all flights to have at least one endpoint at a listed "hub".
                        overwrittenData.remainWithinHomeCodes = data.remainWithinHomeCodes;                 // Optional: If true, airline cannot operate routes which have any endpoint outside of the Home Country(s) (either from "countryCode" or "arrayHomeCountryCodes")
                        overwrittenData.cargo = data.cargo;                                 // Optional: If true, airline only operates cargo service.

                        airlineBusinessDataByBusinessName[data.overwriteName] = overwrittenData;
                    }
                    else
                    {
                        airlineBusinessDataByBusinessName.Add(data.name, data);
                    }
                    break;
                }
            }
            AirportCEOTweaksCore.airlinePaths.Clear();
        }
    }
}
