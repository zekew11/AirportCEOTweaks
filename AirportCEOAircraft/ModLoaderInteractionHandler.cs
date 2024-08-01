using AirportCEOModLoader.WatermarkUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AirportCEOAircraft
{

    internal class ModLoaderInteractionHandler
    {
        internal static void SetUpInteractions()
        {
            // More will probably be added!
            AirportCEOAircraft.LogInfo("Seting up ModLoader interactions");

            WatermarkUtils.Register(new WatermarkInfo("T-AC", "", true));

            AirportCEOAircraft.LogInfo("Completed ModLoader interactions!");
        }
    }
}
