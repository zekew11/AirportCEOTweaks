using AirportCEOModLoader.WatermarkUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AirportCEOFlightLimitTweak
{

    internal class ModLoaderInteractionHandler
    {
        internal static void SetUpInteractions()
        {
            // More will probably be added!
            AirportCEOFlightLimitTweak.LogInfo("Seting up ModLoader interactions");

            WatermarkUtils.Register(new WatermarkInfo("T-FL", "", true));

            AirportCEOFlightLimitTweak.LogInfo("Completed ModLoader interactions!");
        }
    }
}
