using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using AirportCEOTweaksCore;

namespace AirportCEORunways
{
    [HarmonyPatch(typeof(StructureUpgradeUI))]
    class Patch_StructureUpgradeUI
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StructureUpgradeUI), "AddUpgradeContainers", new Type[] {typeof(RunwayModel[])})]
        public static bool Patch_AddUpgradeContainers(RunwayModel[] runways, ref StructureUpgradeUI __instance)
        {
            RunwayModel runway = runways[0];
            RunwayModelExtended runwayext = (RunwayModelExtended)runway.ExtendMono<RunwayModel,RunwayModelExtended>(ref runway);
            
            if (runway == null)
            {
                return false;
            }
            
            __instance.InstantiateUpdgradeContainer(runwayext.GetExtensionPrice(), "Extend runway towards " + runway.direction.ReverseDirection(), new Action(runwayext.ExtendRunwayReversed));
            __instance.InstantiateUpdgradeContainer(runwayext.GetExtensionPrice(), "Extend runway towards " + runway.direction, new Action(runwayext.ExtendRunway));

            return true;
        }
    }
}
