using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;




namespace AirportCEOAircraft
{
    public class ModsController : Singleton<ModsController>
    {

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
    }
}
