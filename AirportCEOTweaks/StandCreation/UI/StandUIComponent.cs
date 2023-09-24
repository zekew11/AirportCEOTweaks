using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AirportCEOTweaks
{
    class StandUIComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEngine.UI.Button assignedButton;
        public GameObject assignedObject;
        public Animator assignedAnimator;

        public void convertButtonToCustom()
        {
            // Make the button custom and remove all things on click, re-add the appropriate ones
            assignedButton.onClick.RemoveAllListeners();
            assignedButton.onClick.AddListener(delegate ()
            {
                CustomBuildingController.spawnItem(assignedObject); // Important part <----------


                Singleton<AudioController>.Instance.PlayAudio(Enums.AudioClip.PointerClick, false, 1f, 1f, false);
                Singleton<PlaceablePanelUI>.Instance.EnableDisableSearchFieldInput(false);
                Singleton<ObjectDescriptionPanelUI>.Instance.HidePanel();
                ObjectPlacementController.hasAttemptedBuild = false;
                EventSystem.current.SetSelectedGameObject(null);
            });
            assignedAnimator = assignedButton.GetComponent<Animator>();
        }
        
        public void OnPointerEnter(PointerEventData eventdata)
        {
            Singleton<ObjectDescriptionPanelUI>.Instance.HidePanel();
            Singleton<VariationsHandler>.Instance.TogglePanel(false);

            assignedAnimator.Play("BounceButton");
            Singleton<AudioController>.Instance.PlayAudio(Enums.AudioClip.PointerEnter, true, 1f, 1f, false);
            ObjectDescriptionPanelUI ObjectDescriptionPanel = Singleton<ObjectDescriptionPanelUI>.Instance;
            ObjectDescriptionPanel.ShowTemplatePanel(assignedButton.transform, "Tweaks Test", "Test for Tweaks");
            ObjectDescriptionPanel.contractorCostText.text = $"{LocalizationManager.GetLocalizedValue("ObjectDescriptionPanelUI.cs.key.21")} {0}";
            ObjectDescriptionPanel.operatingCostText.text = $"{LocalizationManager.GetLocalizedValue("ObjectDescriptionPanelUI.cs.key.27")} {0}";
            ObjectDescriptionPanel.objectImageInstructionText.text = "No Preview Availible For Tweaks...";
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            assignedAnimator.Play("BounceDown");
        }
    }
}
