using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Mono.CompilerServices.SymbolWriter;

namespace AirportCEORunways
{

    public class AirportCEORunwaysConfig
    {

        //public static ConfigEntry<bool> LiveryLogs { get; private set; }
        //public static ConfigEntry<string> PathToCrosshairImage { get; private set; }

        // Struture repair removed (to an external mod maybe)

        internal static void SetUpConfig()
        {
            //LiveryLogs = ConfigRef.Bind("Debug", "Livery Author Log Files", false, "Enable/Disable extra log files for livery authors to debug active liveries");
            //PathToCrosshairImage = ConfigRef.Bind("Debug", "Path to crosshair", "", "Path to crosshair for mod devs. If empty function will not work");
        }

        private static ConfigFile ConfigRef => AirportCEORunways.ConfigReference;
    }
}