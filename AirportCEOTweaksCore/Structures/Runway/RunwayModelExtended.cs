using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx;
using Unity.Mathematics;

namespace AirportCEOTweaksCore
{
    public class RunwayModelExtended : RunwayModel, IMonoClassExtension
    {
		public Color asphaltTearColor;
		float startPos;
		float endPos;
		Vector2 middlePosition;
		//private List<TaxiwayBuilderNode> tbnList;
		//private List<TaxiwayNodeAttacher> tnList;
		int extensionDist = 20;

        public new float Length => ((float)(length + (Math.Pow(length, 2) * 0.0045))).RoundToNearest(20f);
        public void SetupExtend(MonoBehaviour runwayModel)
        {
			if (runwayModel is RunwayModel)
			{
				foreach (var field in typeof(RunwayModel).GetFields(HarmonyLib.AccessTools.all))
				{
					field.SetValue(this, field.GetValue(runwayModel));

				}
			}
			middlePosition = new Vector2(runwayMiddle.transform.position.x, runwayMiddle.transform.position.y);
			startPos = runwayEnds[1].transform.localPosition.x;
			endPos = runwayEnds[0].transform.localPosition.x;
		}
		public bool CanExtendRunway(int newStart)
		{
			Vector3[] position = {this.runwayMiddle.transform.position};
			if (this.direction == Enums.Direction.N)
			{
				position[0] = new Vector2(middlePosition.x, middlePosition.y + (float)newStart);
			}
			else if (this.direction == Enums.Direction.S)
			{
				position[0] = new Vector2(middlePosition.x, middlePosition.y - (float)newStart);
			}
			else if (this.direction == Enums.Direction.E)
			{
				position[0] = new Vector2(middlePosition.x + (float)newStart, middlePosition.y);
			}
			else if (this.direction == Enums.Direction.W)
			{
				position[0] = new Vector2(middlePosition.x - (float)newStart, middlePosition.y);
			}
			string temp = "";
			return Singleton<GridController>.Instance.IsLegalWorldPosition(position,ref temp);
		}
		public void ExtendRunway(bool reversed = false)
		{
			short reverseMult = (short)(reversed ? -1 : 1);
			float oldStartOrEnd = reversed ? startPos : endPos;
			Vector3 extensionVector = new Vector3(0,0,0);

			switch (direction)
            {
				case Enums.Direction.N:
					extensionVector.y = reversed ? -1 : 1;
					break;
				case Enums.Direction.S:
					extensionVector.y = reversed ? 1 : -1;
					break;
				case Enums.Direction.E:
					extensionVector.x = reversed ? -1 : 1;
					break;
				case Enums.Direction.W:
					extensionVector.x = reversed ? 1 : -1;
					break;
			}

			int newStartorEnd = (int)(oldStartOrEnd + this.extensionDist*reverseMult);


			if (!this.CanExtendRunway(newStartorEnd))
			{
				DialogPanel.Instance.ShowMessagePanel("Cannot extend runway outside world!");
				return;
			}
			float extensionPrice = this.GetExtensionPrice();
			if (!EconomyController.Instance.CanAffordObject(extensionPrice))
			{
				GameMessagePanelUI.Instance.ShowTextMiddle("Insufficient funds!", Color.red, true, Singleton<DataPlaceholderColors>.Instance.softBlack, 1f);
				return;
			}

			this.startPos -= extensionDist / 2;
			this.endPos += extensionDist / 2;
			this.transform.position += extensionVector*(extensionDist/2);
			this.runwayEnds[0].transform.localPosition = new Vector3(endPos, 0, 0);
			this.runwayEnds[1].transform.localPosition = new Vector3(startPos, 0, 0);
			this.length += extensionDist;


			this.GenerateRunway();
			Singleton<TaxiwayController>.Instance.UpdateAllTaxiwayBuilders();
			Singleton<EnvironmentController>.Instance.AttemptRemoveTerrainObjects(this.GetAllBorderPositions());
			Singleton<TaxiwayController>.Instance.UpdateAllTaxiwayNodes();
			Singleton<TaxiwayController>.Instance.UpdateAllRunwayConnections();
			StructureUpgradeUI.Instance.SetUpgradePanel(this);
			EconomyController.Instance.PayForConstruction(extensionPrice, this.Position);
		}
		public void ExtendRunway()
        {
			ExtendRunway(false);
        }
		public void ExtendRunwayReversed()
		{
			ExtendRunway(true);
		}
		private void GenerateRunway()
		{
			this.SetDirection();
			//this.ClearRunwayPieces();
			this.SetSpriteDamage(base.Condition);
			bool isGrass = Foundation == Enums.FoundationType.Grass;

			//float midlength = objectSize == Enums.ThreeStepScale.Large ? length - 56 : length - 88;
			float midlength = objectSize == Enums.ThreeStepScale.Large ? length-28 : length-44 ;

			this.runwayMiddle.sprite = SingletonNonDestroy<DataPlaceholderStructures>.Instance.GetRunwayPiece(this.objectSize, Foundation, 0);
			if (this.objectSize == Enums.ThreeStepScale.Large)
			{
				this.runwayMiddle.size = new Vector2(midlength, 28f);
			}
			else if (this.objectSize == Enums.ThreeStepScale.Medium)
			{
				this.runwayMiddle.size = new Vector2(midlength, 20.2f);
			}
			else
			{
				this.runwayMiddle.size = new Vector2((float)(isGrass ? midlength/1.52f : midlength), isGrass ? 13.3f : 20.2f);
				this.runwayMiddle.tileMode = (isGrass ? SpriteTileMode.Continuous : SpriteTileMode.Adaptive);
			}
			this.runwayMiddle.transform.localScale = new Vector3(isGrass ? 1.52f : 1f, isGrass ? 1.52f : 1f, 1f);
			this.runwayMiddle.material = SingletonNonDestroy<DataPlaceholderMaterials>.Instance.GetRunwayMaterial(isGrass);



			Vector3[] borderTransforms = this.boundary.GetAllBorderPositions(0);
			if (this.direction == Enums.Direction.N || this.direction == Enums.Direction.S)
			{
				if (this.direction == Enums.Direction.N)
				{
					this.middlePosition = new Vector2(runwayMiddle.transform.position.x, runwayMiddle.transform.position.y + (float)((this.startPos + this.endPos) / 2));
				}
				else if (this.direction == Enums.Direction.S)
				{
					this.middlePosition = new Vector2(runwayMiddle.transform.position.x, runwayMiddle.transform.position.y - (float)((this.startPos + this.endPos) / 2));
				}
				this.boundary.SetBorder(new Vector2(borderTransforms[0].x, this.middlePosition.y - this.length / 2f), new Vector2(borderTransforms[1].x, this.middlePosition.y + this.length / 2f));
				base.AddToMainGrid();
			}
			else if (this.direction == Enums.Direction.E || this.direction == Enums.Direction.W)
			{
				if (this.direction == Enums.Direction.E)
				{
					this.middlePosition = new Vector2(runwayMiddle.transform.position.x + (float)((this.startPos + this.endPos) / 2), runwayMiddle.transform.position.y);
				}
				else if (this.direction == Enums.Direction.W)
				{
					this.middlePosition = new Vector2(runwayMiddle.transform.position.x - (float)((this.startPos + this.endPos) / 2), runwayMiddle.transform.position.y);
				}
				this.boundary.SetBorder(new Vector2(this.middlePosition.x - this.length / 2f, borderTransforms[0].y), new Vector2(this.middlePosition.x + this.length / 2f, borderTransforms[1].y));
				base.AddToMainGrid();
			}

			this.objectGridSize = new Vector2(this.length, this.objectGridSize.y);
			if (SelectionController.Instance.selectedObject != null)
			{
				SelectionHighlightController.Instance.SetSelectionHighlightSizeAndPosition(this.objectGridSize, this.middlePosition, base.transform.rotation);
			}
			

			this.SetDamageLevel();
			this.RemoveApproachLights();
			//this.AddApproachLights(); needs update
			TaxiwayNodeCreator taxiwayNodeCreator = this.transform.GetComponentInChildren<TaxiwayNodeCreator>();
			taxiwayNodeCreator.runwaySize = length.RoundToIntLikeANormalPerson();
			taxiwayNodeCreator.NotifyOnPlaced();
			this.ActivateNodes();
			this.aircraftSpawnPoints[0].localPosition = new Vector2((float)(-700 + this.startPos + 150), 0f);
			this.touchDownZones[0].localPosition = new Vector2((float)(-150 + this.startPos + 150), 0f);
			this.aircraftSpawnPoints[1].localPosition = new Vector2((float)(700 + this.endPos - 150), 0f);
			this.touchDownZones[1].localPosition = new Vector2((float)(150 + this.endPos - 150), 0f);
			//for (int j = 0; j < this.tbnList.Count; j++)
			//{
			//	this.tbnList[j].UpdatePiece();
			//}
			//for (int k = 0; k < this.tnList.Count; k++)
			//{
			//	tnList[k].UpdateTaxiwayNode();
			//}
			if (SaveLoadGameDataController.loadComplete)
			{
				Singleton<TaxiwayController>.Instance.UpdateAllTaxiwayNodes();
				if (this.isBuilt)
				{
					Singleton<ConstructionController>.Instance.TriggerConstructionEffect(this, Enums.ConstructionOperation.Construct);
				}
			}

		}
		public float GetExtensionPrice()
		{
			float num = 5000;
			if (this.Foundation == Enums.FoundationType.Grass)
			{
				return num;
			}
			if (this.Foundation == Enums.FoundationType.Asphalt)
            {
				num *= 2;
            }
			else
            {
				num *= 3;
            }
			if (this.objectSize == Enums.ThreeStepScale.Large)
            {
				num *= 1.75f;
            }
			return num.RoundToNearest(5000);
		}
		private void SetDamageLevel()
		{
			if (this.tearBack != null)
			{
				for (int i = 0; i < this.tearBack.transform.childCount; i++)
				{
					SpriteRenderer component = this.tearBack.transform.GetChild(i).GetComponent<SpriteRenderer>();
					component.color = new Color(this.asphaltTearColor.r, this.asphaltTearColor.g, this.asphaltTearColor.b, 1f - base.Condition);
				}
			}
			if (this.tearFront != null)
			{
				for (int j = 0; j < this.tearFront.transform.childCount; j++)
				{
					SpriteRenderer component2 = this.tearFront.transform.GetChild(j).GetComponent<SpriteRenderer>();
					component2.color = new Color(this.asphaltTearColor.r, this.asphaltTearColor.g, this.asphaltTearColor.b, 1f - base.Condition);
				}
			}
		}

	}
}
