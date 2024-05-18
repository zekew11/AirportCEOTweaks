using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Mono.CompilerServices.SymbolWriter;

namespace AirportCEOTweaks;

public class AirportCEOTweaksConfig
{
    // Versioning and watermark offloaded to mod loader

    public static ConfigEntry<KeyboardShortcut> IncreaseTurnaroundBind { get; private set; }
    public static ConfigEntry<KeyboardShortcut> DecreaseTurnaroundBind { get; private set; }

    // No references?
    //public static ConfigEntry<KeyboardShortcut> scheduleSoonerBind { get; private set; }
    //public static ConfigEntry<KeyboardShortcut> scheduleLaterBind { get; private set; }
    //public static ConfigEntry<KeyboardShortcut> pickupConfirmBind { get; private set; }

    public static ConfigEntry<KeyboardShortcut> OverloadShiftBind { get; private set; }

    public static ConfigEntry<bool> AirlineNationality { get; private set; }
    public static ConfigEntry<bool> PlannerChanges { get; private set; }
    public static ConfigEntry<bool> ForceNormalTurnaroundTime { get; private set; }
    public static ConfigEntry<bool> PermisivePlanner { get; private set; }
    public static ConfigEntry<bool> SmallPlaneBaggageOff { get; private set; }
    public static ConfigEntry<bool> DisconnectedBaggageOff { get; private set; }
    public static ConfigEntry<bool> HigherFlightCap { get; private set; }
    public static ConfigEntry<bool> PlannerUIModifications { get; private set; }
    public static ConfigEntry<bool> LiveryLogs { get; private set; }

    public static ConfigEntry<string> CargoAirlineFlags { get; private set; }
    public static string[] CargoAirlineFlagsArray
    {
        get
        {
            string[] values = CargoAirlineFlags.Value.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                values[i].Trim(' ', '.');
            }
            return values;
        }
        set
        {
            CargoAirlineFlags.Value = string.Join(", ", value);
        }
    }
    public static ConfigEntry<int> FlightGenerationMultiplyer { get; private set; }
    public static ConfigEntry<string> PathToCrosshairImage { get; private set; }
    
    // Struture repair removed (to an external mod maybe)

    internal static void SetUpConfig()
    {
        IncreaseTurnaroundBind = ConfigRef.Bind("Keybinds", "Increase Turnaround Keybind", new KeyboardShortcut(UnityEngine.KeyCode.Equals), "Keybind to make turnaround time longer (while dragging)");
        DecreaseTurnaroundBind = ConfigRef.Bind("Keybinds", "Decrease Turnaround Keybind", new KeyboardShortcut(UnityEngine.KeyCode.Minus), "Keybind to make turnaround time shorter (while dragging)");
        OverloadShiftBind = ConfigRef.Bind("Keybinds", "Shift-Key Duplicate Keybind", new KeyboardShortcut(UnityEngine.KeyCode.None), "Keybind which will replicate the shift-key's behavior within the mod, such as reschedulng all flights in series. Shift-key will continue to function, this keybind will duplicate the functionality.");

        AirlineNationality = ConfigRef.Bind("General", "Airline Nationality System", true, "When enabled, airlines will operate to and from their home nations. More options below...");
        ForceNormalTurnaroundTime = ConfigRef.Bind("General", "Force Vanilla Turnaround Times", false, "Expirimental: force vanilla turnaround time on all flights. Better autoplanner compatability.");
        SmallPlaneBaggageOff = ConfigRef.Bind("General", "Small Aircraft No-Baggage", false, "When enabled small aircraft will never request baggage service.");
        DisconnectedBaggageOff = ConfigRef.Bind("General", "Disconnected Stands No-Baggage", true, "When enabled aircraft of any size assigned to stands with no connected baggage bay will not request baggage service.");
        HigherFlightCap = ConfigRef.Bind("General", "Flight Cap Changes", true, "A tweak to increase the flight cap by building multiple ATC towers.");
        CargoAirlineFlags = ConfigRef.Bind("General", "Cargo Airline Flags", string.Join(", ", "cargo","freight","logistics", "mail", "dhl","fedex","ups", "kalitta","amazon air"), "Define flags which, in the name of any airline, flag that airline as a cargo operator.");
        FlightGenerationMultiplyer = ConfigRef.Bind("General", "Flight Generation Multiplyer", 3, "At each opportunity airlines attempt to generate this many flights. High values may result in duplicate flights.");

        PlannerChanges = ConfigRef.Bind("Planner", "Planner Changes", true, "Enable/Disable player controlled planner hacks such as hold-shift to reschedule all flights in series.");
        PlannerUIModifications = ConfigRef.Bind("Planner", "Planner UI Modifications", false, "Disables planner UI changes. Enabling results in (frequent) CTD upon opening planner!");
        PermisivePlanner = ConfigRef.Bind("Planner", "Permissive Flight Planning", false, "Unreasonably permissive flight planning rules for expirimentation and debug");

        LiveryLogs = ConfigRef.Bind("Debug", "Livery Author Log Files", false, "Enable/Disable extra log files for livery authors to debug active liveries");
        PathToCrosshairImage = ConfigRef.Bind("Debug", "Path to crosshair", "", "Path to crosshair for mod devs. If empty function will not work");
    }

    private static ConfigFile ConfigRef => AirportCEOTweaks.ConfigReference;
}