using AirportCEOModLoader.WatermarkUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOAircraft
{

    internal class ModLoaderInteractionHandler
    {
        internal static void SetUpInteractions()
        {
            // More will probably be added!
            AirportCEOAircraft.LogInfo("Seting up ModLoader interactions");

            WatermarkUtils.Register(new WatermarkInfo(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION, false));

            AirportCEOAircraft.LogInfo("Completed ModLoader interactions!");
        }
    }
}
