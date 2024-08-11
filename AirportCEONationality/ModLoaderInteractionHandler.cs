using AirportCEOModLoader.WatermarkUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AirportCEONationality
{

    internal class ModLoaderInteractionHandler
    {
        internal static void SetUpInteractions()
        {
            // More will probably be added!
            AirportCEONationality.LogInfo("Seting up ModLoader interactions");

            WatermarkUtils.Register(new WatermarkInfo("T-N", Assembly.GetExecutingAssembly().GetName().Version.ToString(), true));

            AirportCEONationality.LogInfo("Completed ModLoader interactions!");
        }
    }
}
