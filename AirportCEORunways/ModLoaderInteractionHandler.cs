using AirportCEOModLoader.WatermarkUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AirportCEORunways
{

    internal class ModLoaderInteractionHandler
    {
        internal static void SetUpInteractions()
        {
            // More will probably be added!
            AirportCEORunways.LogInfo("Seting up ModLoader interactions");

            WatermarkUtils.Register(new WatermarkInfo("Runways", Assembly.GetExecutingAssembly().GetName().Version.ToString(), false));

            AirportCEORunways.LogInfo("Completed ModLoader interactions!");
        }
    }
}
