using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx;

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
		int extensionDist = 40;


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
		//private void ClearRunwayPieces()
		//{
		//	//for (int i = 0; i < this.tbnList.Count; i++)
		//	//{
		//	//	Singleton<TaxiwayController>.Instance.RemoveBuilderNode(this.tbnList[i]);
		//	//}
		//	for (int j = 0; j < this.tnList.Count; j++)
		//	{
		//		this.tnList[j].NotifyOnRemove();
		//	}
		//	//this.tbnList.Clear();
		//	this.tnList.Clear();
		//	for (int k = 0; k < this.transform.childCount; k++)
		//	{
		//		UnityEngine.Object.Destroy(this.transform.GetChild(k).gameObject);
		//	}
		//}
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

			this.transform.position += extensionVector*(extensionDist/2);
			this.length += extensionDist;
			this.UpdateRunway();
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

			float midlength = objectSize == Enums.ThreeStepScale.Large ? length - 112 : length - 176;

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
			for (int i = 0; i < this.runwayEnds.Length; i++)
			{
				this.runwayEnds[i].sprite = SingletonNonDestroy<DataPlaceholderStructures>.Instance.GetRunwayPiece(this.objectSize, Foundation, 1);
				this.runwayEnds[i].transform.localScale = new Vector3(isGrass ? 1.52f : 1f, isGrass ? 1.52f : 1f, 1f);
				this.runwayEnds[i].material = SingletonNonDestroy<DataPlaceholderMaterials>.Instance.GetRunwayMaterial(isGrass);
				if (this.objectSize == Enums.ThreeStepScale.Small)
				{
					//float num = (float)(isGrass ? -130 : -123);
					float num = (length / 2) - 25;
					this.runwayEnds[i].transform.localPosition = new Vector3((i%2 == 0) ? Mathf.Abs(num) : num, 0f, 0f);
					Debug.LogWarning("ACEO Tweaks WARN: found a small runway");
				}
				else if (this.objectSize == Enums.ThreeStepScale.Medium)
				{
					float num = (length / 2) - 25;
					this.runwayEnds[i].transform.localPosition = new Vector3((float)((i%2 == 0) ? num : -num), 0f, 0f);
				}
				else
				{
					float num = (length / 2) - 15;
					this.runwayEnds[i].transform.localPosition = new Vector3((float)((i%2 == 0) ? num : -num), 0f, 0f);
				}
			}
			for (int j = 0; j < this.runwayAimsA.Length; j++)
			{
				this.runwayAimsA[j].sprite = SingletonNonDestroy<DataPlaceholderStructures>.Instance.GetRunwayPiece(this.objectSize, Foundation, 2);
				if (this.objectSize != Enums.ThreeStepScale.Large)
				{
					this.runwayAimsA[j].size = new Vector2(40f, 20.2f);
				}
			}
			for (int k = 0; k < this.runwayAimsB.Length; k++)
			{
				this.runwayAimsB[k].sprite = SingletonNonDestroy<DataPlaceholderStructures>.Instance.GetRunwayPiece(this.objectSize, Foundation, 3);
			}
			if (SaveLoadGameDataController.loadComplete)
			{
				Singleton<TaxiwayController>.Instance.UpdateAllTaxiwayNodes();
				if (this.isBuilt)
				{
					Singleton<ConstructionController>.Instance.TriggerConstructionEffect(this, Enums.ConstructionOperation.Construct);
				}
			}

			Vector3[] borderTransforms = this.boundary.GetAllBorderPositions(1);
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
			this.AddApproachLights();
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
		}
		public float GetExtensionPrice()
		{
			float num = 10000;
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
		private void UpdateRunway()
		{
			this.GenerateRunway();
		}
	}
}
