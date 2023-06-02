using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOTweaks
{
    class CustomBuildingController
    {
        public static void spawnItem(GameObject item)
        {
            // Get template
            ObjectPlacementController placementController = Singleton<BuildingController>.Instance.GetComponent<ObjectPlacementController>();

            if (item != null && placementController != null)
            {
                if (!item.activeSelf)
                {
                    item.SetActive(true);
                }

                //VariationsHandler.currentVariationIndex;

                placementController.SetObject(item, 0);
            }
        }
    }
}
