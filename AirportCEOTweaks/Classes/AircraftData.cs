using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks
{
	[Serializable]
	public struct AircraftTypeData
	{
		public Enums.ThreeStepScale Size
		{
			get
			{
				switch (this.size)
				{
					case Enums.GenericSize.Gigantic:
						return Enums.ThreeStepScale.Large;
					case Enums.GenericSize.Huge:
						return Enums.ThreeStepScale.Large;
					case Enums.GenericSize.VeryLarge:
						return Enums.ThreeStepScale.Large;
					case Enums.GenericSize.Large:
						return Enums.ThreeStepScale.Medium;
					case Enums.GenericSize.Medium:
						return Enums.ThreeStepScale.Medium;
					case Enums.GenericSize.Small:
						return Enums.ThreeStepScale.Small;
					case Enums.GenericSize.VerySmall:
						return Enums.ThreeStepScale.Small;
					case Enums.GenericSize.Tiny:
						return Enums.ThreeStepScale.Small;
					default:
						return Enums.ThreeStepScale.Small;
				}
			}
		}
		// Token: 0x04001FCC RID: 8140
		public string id;
		// Token: 0x04001FCD RID: 8141
		public Enums.GenericSize size;

		public string displayName;
		public string manufacturer;
		public string iCAOCode;

		public short capacity_PAX;
		public short exitLimit_PAX;
		public short seatsAbreast;
		public short capacity_ULD;
		public short capacityCargo_KG;

		public short range_KM;
		public short speed_KMH;
		public short etops_Minutes;
		public int fuelCapacity_L;
		public bool jP1;

		public short takeoffDistance_M;
		public int maxTOW_KG;
		public char iCAO_Class;
		public short numEngines;
		public Enums.AircraftEngineType engineType;

		public short numBuilt;
		public short yearIntroduced;
		public short yearLastProduced;
		public short yearRetired;

		public bool needStairs;
		public bool needPushback;
		public bool needPaved;
		public bool heavy;
		public bool canJetbridge;
		public bool canPushback;
		public bool sonicBoom;
		public bool vIP;
		public bool combi;
		public bool cargo;

		public bool loud;
		public bool quiet;
	}

}
