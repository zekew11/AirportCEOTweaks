using System;
using UModFramework.API;

namespace AirportCEOTweaks
{
    public class AirportCEOTweaksConfig
    {
        private static readonly string configVersion = "2.1.2";

        //Add your config vars here.
        public static UnityEngine.KeyCode increaseTurnaroundBind;
        public static UnityEngine.KeyCode decreaseTurnaroundBind;
        public static UnityEngine.KeyCode scheduleSoonerBind;
        public static UnityEngine.KeyCode scheduleLaterBind;
        public static UnityEngine.KeyCode pickupConfirmBind;
        public static UnityEngine.KeyCode overloadShiftBind;

        public static bool fixes;
        public static bool cargoSystem;
        public static bool plannerChanges;
        //public static bool longerFlightSeries;
        //public static bool airlineChanges;
        public static bool liveryExtensions;
        public static bool permisivePlanner;
        public static bool smallPlaneBaggageOff;
        public static bool disconnectedBaggageOff;

        public static bool liveryLogs;

        public static string[] cargoAirlineFlags;
        public static float cargoPayMod;

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
                    cargoSystem = cfg.Read("Cargo System", new UMFConfigBool(true, false, true), "Enable/Disable the (primitive) cargo flight system. Setting will be depreciated with full intrduction of the flight types system.");
                    smallPlaneBaggageOff = cfg.Read("Small Aircraft No-Baggage", new UMFConfigBool(false, false, true), "When enabled small aircraft will never request baggage service.");
                    disconnectedBaggageOff = cfg.Read("Disconnected Stands No-Baggage", new UMFConfigBool(true, false, true), "When enabled aircraft assigned to stands with no connected baggage bay will not request baggage service.");
                    plannerChanges = cfg.Read("Planner Changes", new UMFConfigBool(true, false, true), "Enable/Disable player controlled planner hacks such as hold-shift to reschedule all flights in series.");
                    //longerFlightSeries = cfg.Read("Longer Series", new UMFConfigBool(true, false, true), "Enable/Disable changes to how airlines generate repeating flight contracts.");
                    //airlineChanges = cfg.Read("Temp Airline Changes", new UMFConfigBool(false, false, true), "UNSTABLE Enable/Disable airline balance changes such as requesting differing turnaround services.");
                    liveryExtensions = cfg.Read("Livery Extensions", new UMFConfigBool(true, false, true), "Enable/Disable advanced livery functions.");
                    
                    

                    cargoAirlineFlags = cfg.Read("Cargo Airline Flags", new UMFConfigStringArray(new string[] { "cargo","freight","logistics", "mail", "dhl","fedex","ups", "kalitta","amazon air" }), "Define flags which, in the name of any airline, flag that airline as a cargo operator.");
                    cargoPayMod = cfg.Read("Cargo Flight Payment Modifier", new UMFConfigFloat(0.65f, 0f, 2f, 2, 1, false), "Cargo flight completion bonus is multiplied by this value");



                    liveryLogs = cfg.Read("Livery Author Log Files", new UMFConfigBool(false, false, false), "Enable/Disable extra log files for livery authors to debug active liveries");
                    permisivePlanner = cfg.Read("Permissive Flight Planning", new UMFConfigBool(false, false, false), "Unreasonably permissive flight planning rules for expirimentation and debug");

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