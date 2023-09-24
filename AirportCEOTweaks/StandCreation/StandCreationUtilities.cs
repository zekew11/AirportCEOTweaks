using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOTweaks
{
    public static class StandCreationUtilities
    {
        public static bool GetStandTemplateCopy(in Enums.ThreeStepScale scale, out GameObject stand)
        {
            try
            {
                GameObject referenceStand = GetStandTemplate(scale);
                stand = GameObject.Instantiate(referenceStand);

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetStandTemplateCopy));
                stand = null;
                return false;
            }
        }

        public static GameObject GetStandTemplate(in Enums.ThreeStepScale scale)
        {
            switch (scale)
            {
                case Enums.ThreeStepScale.Small:
                    return Singleton<BuildingController>.Instance.aircraftStructuresPrefabs.smallStand;
                case Enums.ThreeStepScale.Medium:
                    return Singleton<BuildingController>.Instance.aircraftStructuresPrefabs.mediumStand;
                case Enums.ThreeStepScale.Large:
                    return Singleton<BuildingController>.Instance.aircraftStructuresPrefabs.largeStand;
            }

            Log($"[Mod Error] Unexpected (and immpossible) outcome of {nameof(GetStandTemplate)}");
            return Singleton<BuildingController>.Instance.aircraftStructuresPrefabs.mediumStand;
        }

        private static void LogError(Exception ex, string funcName, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                Log($"[Mod Error] An error occured in {funcName}! Error: \"{ex.Message}\"");
                return;
            }

            Log($"[Mod Error] An error occured in {funcName}! Error: \"{ex.Message}\". {message}");
        }

        private static void Log(string message)
        {
            // This can be customized
            AirportCEOTweaks.Log("[Stand Creation Manager] " + message);
        }
    }
}
