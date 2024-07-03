using AirportCEOModLoader.WatermarkUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaksCore
{

    internal class ModLoaderInteractionHandler
    {
        internal static void SetUpInteractions()
        {
            // More will probably be added!
            AirportCEOTweaksCore.LogInfo("Seting up ModLoader interactions");

            WatermarkUtils.Register(new WatermarkInfo(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION, false));

            AirportCEOTweaksCore.LogInfo("Completed ModLoader interactions!");
        }
    }
}
