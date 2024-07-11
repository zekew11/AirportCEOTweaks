using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace AirportCEOTweaksCore
{
    public class RunwayUIExtended : RunwayUI, IMonoClassExtension
    {
        TextMeshProUGUI runwayLengthText;
        TextMeshProUGUI runwayLengthValueText;
        RunwayModelExtended runwayModel;
        public void SetupExtend(MonoBehaviour runwayUi)
        {

        }
        public void UpdateText(RunwayModelExtended runwayModel)
        {
            
            if (runwayModel == null)
            {
                Debug.LogWarning("Runway ui extended runwaymodel null");
                return; 
            }
            else
            {
                this.runwayModel = runwayModel;
            }

            Transform containerList = transform.Find("InformationPanel/List");
            Transform flightsInPattern = containerList.Find("FlightsInPattern");
            Vector3 pos;
           
            foreach (Transform transform in containerList.GetComponentsInChildren<Transform>())
            {
                pos = transform.localPosition;
                transform.localPosition = new Vector3(pos.x, pos.y - 20, pos.z);
            }
            GameObject runwayLengthTextGameObj = Instantiate<GameObject>(flightsInPattern.gameObject, containerList, true);
            runwayLengthTextGameObj.name = "RunwayLengthText";
            pos = runwayLengthTextGameObj.transform.localPosition;
            runwayLengthTextGameObj.transform.localPosition = new Vector3(pos.x, pos.y + 20, pos.z);
            
            runwayLengthText = runwayLengthTextGameObj.transform.Find("FlightsInPatternText").GetComponent<TextMeshProUGUI>();
            runwayLengthValueText = runwayLengthTextGameObj.transform.Find("FlightsInPatternValueText").GetComponent<TextMeshProUGUI>();
            
            runwayLengthText.text = "Runway Length: ";
            runwayLengthValueText.text = runwayModel.Length.ToString() + "m";
        }

    }
}
