using System;
using UModFramework.API;

namespace AirportCEOTweaks
{
    public class AirportCEOTweaksConfig
    {
        private static readonly string configVersion = "2.2.0c";
        public static string displayConfigVersion = "2.3.1 Alpha 3"; //This is displayed in the ACEO top bar (Patch_AppVersionLabel), can be changed independtly of other config version


        //Add your config vars here.
        public static UnityEngine.KeyCode increaseTurnaroundBind;
        public static UnityEngine.KeyCode decreaseTurnaroundBind;
        public static UnityEngine.KeyCode scheduleSoonerBind;
        public static UnityEngine.KeyCode scheduleLaterBind;
        public static UnityEngine.KeyCode pickupConfirmBind;
        public static UnityEngine.KeyCode overloadShiftBind;

        public static bool fixes;
        public static bool airlineNationality;
        public static bool cargoSystem;
        public static bool plannerChanges;
        public static bool forceNormalTurnaroundTime;
        //public static bool longerFlightSeries;
        //public static bool airlineChanges;
        public static bool liveryExtensions;
        public static bool permisivePlanner;
        public static bool smallPlaneBaggageOff;
        public static bool disconnectedBaggageOff;
        public static bool flightTypes;
        public static bool highFlightCap;

        public static bool liveryLogs;

        public static string[] cargoAirlineFlags;
        public static string[] noInternationalFlags;
        public static string[] yesInternationalFlags;
        public static int minimumStarsForInternational;
        public static int flightGenerationMultiplyer;
        public static float cargoPayMod;
        public static float structureRepairLevel; //Affects both stands and runways

        internal static void Load()
        {
            AirportCEOTweaks.Log("Loading settings.");
            try
            {
                using (UMFConfig cfg = new UMFConfig())
                {
                    string cfgVer = cfg.Read("ConfigVersion", new UMFConfigString());
                    if (cfgVer != string.Empty && cfgVer != configVersion)
                    {
                        cfg.DeleteConfig(true);
                        AirportCEOTweaks.Log("The config file was outdated and has been deleted. A new config will be generated.");
                    }

                    cfg.Write("SupportsHotLoading", new UMFConfigBool(false)); //Uncomment if your mod can't be loaded once the game has started.
                    cfg.Write("ModDependencies", new UMFConfigStringArray(new string[] { "" })); //A comma separated list of mod/library names that this mod requires to function. Format: SomeMod:1.50,SomeLibrary:0.60
                    cfg.Read("LoadPriority", new UMFConfigString("Normal"));
                    cfg.Write("MinVersion", new UMFConfigString("0.53.5"));
                    cfg.Write("MaxVersion", new UMFConfigString("0.54.99999.99999")); //This will prevent the mod from being loaded after the next major UMF release
                    cfg.Write("UpdateURL", new UMFConfigString("https://umodframework.com/updatemod?id=33"));
                    cfg.Write("ConfigVersion", new UMFConfigString(configVersion));

                    AirportCEOTweaks.Log("Finished UMF Settings.");

                    //Add your settings here

                    increaseTurnaroundBind = cfg.Read("Increase Turnaround Keybind", new UMFConfigKeyCode(UnityEngine.KeyCode.Equals), "Keybind to make turnaround time longer (while dragging)");
                    decreaseTurnaroundBind = cfg.Read("Decrease Turnaround Keybind", new UMFConfigKeyCode(UnityEngine.KeyCode.Minus), "Keybind to make turnaround time shorter (while dragging)");
                    //scheduleSoonerBind = cfg.Read("Schedule Sooner Keybind", new UMFConfigKeyCode(UnityEngine.KeyCode.LeftBracket), "Keybind to schedule a flight earlier in time");                       ------------1.3------------
                    //scheduleLaterBind = cfg.Read("Schedule Later Keybind", new UMFConfigKeyCode(UnityEngine.KeyCode.RightBracket), "keybind to schedule a flight later in time");                          ------------1.3------------
                    //pickupConfirmBind = cfg.Read("Grab/Confirm Keybind", new UMFConfigKeyCode(UnityEngine.KeyCode.Return), "keybind to begin editing the selected object or confirm the changes made");    ------------1.3------------

                    overloadShiftBind = cfg.Read("Shift-Key Duplicate Keybind", new UMFConfigKeyCode(UnityEngine.KeyCode.None), "Keybind which will replicate the shift-key's behavior within the mod, such as reschedulng all flights in series. Shift-key will continue to function, this keybind will duplicate the functionality.");


                    fixes = cfg.Read("Fixes for Disabled Modules", new UMFConfigBool(true, false, true), "Apply patches/workarounds that are not strictly neccessary for the selected configuration.");
                    airlineNationality = cfg.Read("Airline Nationality System", new UMFConfigBool(true, false, true), "When enabled, airlines will operate to and from their home nations. More options below...");
                    cargoSystem = cfg.Read("Cargo System", new UMFConfigBool(true, false, true), "Enable/Disable the (primitive) cargo flight system. Setting will be depreciated with full intrduction of the flight types system.");
                    forceNormalTurnaroundTime = cfg.Read("Force Vanilla Turnaround Times", new UMFConfigBool(false, true, false), "Expirimental: force vanilla turnaround time on all flights. Better autoplanner compatability.");
                    smallPlaneBaggageOff = cfg.Read("Small Aircraft No-Baggage", new UMFConfigBool(false, false, true), "When enabled small aircraft will never request baggage service.");
                    disconnectedBaggageOff = cfg.Read("Disconnected Stands No-Baggage", new UMFConfigBool(true, false, true), "When enabled aircraft of any size assigned to stands with no connected baggage bay will not request baggage service.");
                    flightTypes = cfg.Read("Flight Types System",new UMFConfigBool(true,false,true), "A large redesign of airline and flight interactions. Not a vanilla experiance.");
                    highFlightCap = cfg.Read("Increase Flight Cap", new UMFConfigBool(true, false, true), "A tweak to increase the flight cap by building multiple ATC towers.");
                    plannerChanges = cfg.Read("Planner Changes", new UMFConfigBool(true, false, true), "Enable/Disable player controlled planner hacks such as hold-shift to reschedule all flights in series.");
                    //longerFlightSeries = cfg.Read("Longer Series", new UMFConfigBool(true, false, true), "Enable/Disable changes to how airlines generate repeating flight contracts.");
                    //airlineChanges = cfg.Read("Temp Airline Changes", new UMFConfigBool(false, false, true), "UNSTABLE Enable/Disable airline balance changes such as requesting differing turnaround services.");
                    //liveryExtensions = cfg.Read("Livery Extensions", new UMFConfigBool(true, false, true), "Enable/Disable advanced livery functions.");
                    
                    

                    cargoAirlineFlags = cfg.Read("Cargo Airline Flags", new UMFConfigStringArray(new string[] { "cargo","freight","logistics", "mail", "dhl","fedex","ups", "kalitta","amazon air" }), "Define flags which, in the name of any airline, flag that airline as a cargo operator.");
                    cargoPayMod = cfg.Read("Cargo Flight Payment Modifier", new UMFConfigFloat(0.65f, 0f, 2f, 2, 1, false), "Cargo flight completion bonus is multiplied by this value");

                    
                    yesInternationalFlags = cfg.Read("Force International Flights Flags", new UMFConfigStringArray(new string[] { "international","global","world"}), "Define flags which, in the name of any airline, flag that airline as having international flights regaurdless of any other settings");
                    noInternationalFlags = cfg.Read("Domestic Flights Only Flags", new UMFConfigStringArray(new string[] {}), "Define flags which, in the name of any airline, flag that airline as having no international flights");
                    minimumStarsForInternational = cfg.Read("Minimum Airline Star-Rank for International Service", new UMFConfigInt(2, 1, 6, 1, false), "Airlines below this rank do not fly Internationally");
                    flightGenerationMultiplyer = cfg.Read("Flight Generation Multiplyer", new UMFConfigInt(3, 1, 10, 3, false), "At each opportunity airlines attempt to generate this many flights. High values may result in duplicate flights.");

                    liveryLogs = cfg.Read("Livery Author Log Files", new UMFConfigBool(false, false, false), "Enable/Disable extra log files for livery authors to debug active liveries");
                    permisivePlanner = cfg.Read("Permissive Flight Planning", new UMFConfigBool(false, false, false), "Unreasonably permissive flight planning rules for expirimentation and debug");
                    structureRepairLevel = cfg.Read("Structure Repair Level", new UMFConfigFloat(0.25f, 0.01f, 0.95f, 2, 0.25f, false), "Repairs stands and runways at the set level rather than the vanilla value");

                    AirportCEOTweaks.Log("ACEO Tweaks | Finished loading settings.");
                }
            }
            catch (Exception e)
            {
                AirportCEOTweaks.Log("ACEO Tweaks | Error loading mod settings: " + e.Message + "(" + e.InnerException?.Message + ")");
            }
        }
    }
}