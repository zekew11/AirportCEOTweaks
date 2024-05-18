using AirportCEOModLoader.WatermarkUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks;

internal class ModLoaderInteractionHandler
{
    internal static void SetUpInteractions()
    {
        // More will probably be added!
        AirportCEOTweaks.LogInfo("Seting up ModLoader interactions");

        WatermarkUtils.Register(new WatermarkInfo(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION, false));

        AirportCEOTweaks.LogInfo("Completed ModLoader interactions!");
    }
}
