using AirportCEOModLoader.WatermarkUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AirportCEOPlanningReplanned
{

    internal class ModLoaderInteractionHandler
    {
        internal static void SetUpInteractions()
        {
            // More will probably be added!
            AirportCEOPlanningReplanned.LogInfo("Seting up ModLoader interactions");

            WatermarkUtils.Register(new WatermarkInfo("T-PR", "", true));

            AirportCEOPlanningReplanned.LogInfo("Completed ModLoader interactions!");
        }
    }
}
