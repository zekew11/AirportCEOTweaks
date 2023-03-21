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
		public AircraftModel CopyFromAircraftModel
		{
			get
			{
				return Singleton<AirTrafficController>.Instance.GetAircraftModel(copyFrom);
			}
		}
		public char ICAOClass
		{
			get
			{
				if (wingSpan_M >= 65)
				{
					return 'F';
				}
				else if (wingSpan_M >= 52)
				{
					return 'E';
				}
				else if (wingSpan_M >= 36)
				{
					return 'D';
				}
				else if (wingSpan_M >= 24)
				{
					return 'C';
				}
				else if (wingSpan_M >= 15)
				{
					return 'B';
				}
				else if (wingSpan_M < 15)
				{
					return 'A';
				}
				else
				{
					return 'X';
				}
			}
		}
		public string Id
        {
            get
            {
				return id[0];
            }
        }

		public string filePath;

		public string[] id;
		public string copyFrom;
		public short numEngines;
		public float wingSpan_M;
		public float length_M;
		public float forcedReScale;
		public float shadowFix;

		public Enums.GenericSize size;
		public Enums.ThreeStepScale threeStepSize;

		public string[] displayName;
		public string[] iCAOCode;
		public string[] manufacturer;

		public  short[] numBuilt;
		public  short[] yearIntroduced;
		public  short[] yearLastProduced;
		public  short[] yearRetired;

		public  short[] capacity_PAX;
		public  short[] exitLimit_PAX;
		public  short[] seatsAbreast;
		public  short[] capacityULDLowerDeck;
		public  short[] capacityULDUpperDeck;
		public  short[] capacityBulkCargo_KG;

		public  short[] range_KM;
		public  short[] speed_KMH;
		public  short[] etops_Minutes;
		public    int[] fuelCapacity_L;
		public string[] engineType;  //radial,inline,turboprop,turbojet,low_turbofan,turbofan,high_turbofan
		public  short[] takeoffDistance_M;
		public    int[] maxTOW_KG;

		public bool[] needStairs;
		public bool[] needPushback;
		public bool[] needPaved;
		public bool[] needHeavyPaved;
		
		public bool[] canGetPushback;       //taildraggers == false

		public short[] jetbridgePoints;
		public short[] conveyerPoints;
		public short[] cleaningPoints;
		public short[] cateringPoints;

		public bool[] sonicBoom;
		public bool[] vIP;
		public bool[] combi;
		public bool[] cargo;

		public bool[] loud;
		public bool[] quiet;
	}

}
