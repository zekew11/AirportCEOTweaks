using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirportCEOAircraft
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
		public AircraftTypeData SingleAircraftTypeData(string id)
		{
			if (id == null || id.Length==0 || this.id == null || this.id.Length == 0)
            {
				return new AircraftTypeData();
            }
			//Debug.Log("SingleAircraftTypeData: ");
			int tryIndex = 0;

			for (int i = 0; i < this.id.Length; i++)
			{
				if (id == this.id[i])
				{
					tryIndex = i;
					break;
				}
			}

			AircraftTypeData data = new AircraftTypeData();

			data.id = new string[] { id };
			//Debug.Log("SingleAircraftTypeData: start for " + id);
			data.copyFrom = this.copyFrom;
			data.numEngines = this.numEngines;
			data.wingSpan_M = this.wingSpan_M;
			data.length_M = this.length_M;
			data.forcedReScale = this.forcedReScale;
			data.shadowFix = this.shadowFix;
			data.size = this.size;
			data.threeStepSize = this.threeStepSize;

			//Debug.Log("SingleAircraftTypeData: fixxeds done");
			data.displayName = ArrayReducer(displayName, tryIndex);
			data.iCAOCode = ArrayReducer(iCAOCode , tryIndex);
			data.manufacturer = ArrayReducer(manufacturer , tryIndex);
			data.numBuilt = ArrayReducer( numBuilt, tryIndex);
			data.yearIntroduced = ArrayReducer(yearIntroduced , tryIndex);
			data.yearLastProduced = ArrayReducer(yearLastProduced , tryIndex);
			data.yearRetired = ArrayReducer(yearRetired, tryIndex);

			//Debug.Log("SingleAircraftTypeData: A");
			data.capacity_PAX = ArrayReducer(capacity_PAX, tryIndex);
			data.exitLimit_PAX = ArrayReducer(exitLimit_PAX , tryIndex);
			data.seatsAbreast = ArrayReducer(seatsAbreast , tryIndex);
			data.capacityULDLowerDeck = ArrayReducer(capacityULDLowerDeck, tryIndex);
			data.capacityULDUpperDeck = ArrayReducer(capacityULDUpperDeck, tryIndex);
			data.capacityBulkCargo_KG = ArrayReducer(capacityBulkCargo_KG, tryIndex);

			//Debug.Log("SingleAircraftTypeData: B");
			data.range_KM = ArrayReducer(range_KM, tryIndex);
			data.speed_KMH = ArrayReducer(speed_KMH, tryIndex);
			data.etops_Minutes = ArrayReducer(etops_Minutes, tryIndex);
			data.fuelCapacity_L = ArrayReducer(fuelCapacity_L, tryIndex);
			data.engineType = ArrayReducer(engineType, tryIndex);
			data.takeoffDistance_M = ArrayReducer(takeoffDistance_M, tryIndex);
			data.maxTOW_KG = ArrayReducer(maxTOW_KG, tryIndex);

			//Debug.Log("SingleAircraftTypeData: C");
			data.needStairs = ArrayReducer(needStairs, tryIndex);
			data.needPushback = ArrayReducer(needPushback, tryIndex);
			data.needPaved = ArrayReducer(needPaved, tryIndex);
			data.needHeavyPaved= ArrayReducer(needHeavyPaved, tryIndex);

			//Debug.Log("SingleAircraftTypeData: D");
			data.canGetPushback  = ArrayReducer(canGetPushback, tryIndex);

			data. jetbridgePoints = ArrayReducer(jetbridgePoints, tryIndex);
			data. conveyerPoints = ArrayReducer(conveyerPoints, tryIndex);
			data. cleaningPoints = ArrayReducer(cleaningPoints, tryIndex);
			data. cateringPoints = ArrayReducer(cateringPoints, tryIndex);

			//Debug.Log("SingleAircraftTypeData: E");
			data.sonicBoom = ArrayReducer(sonicBoom, tryIndex);
			data.vIP = ArrayReducer(vIP, tryIndex);
			data.combi = ArrayReducer(combi, tryIndex);
			data.cargo = ArrayReducer(cargo, tryIndex);

			data.loud = ArrayReducer(loud, tryIndex);
			data.quiet = ArrayReducer(quiet, tryIndex);

			//Debug.Log("SingleAircraftTypeData: last");
			return data;
		}
		public static T[] ArrayReducer<T>(T[] originalArray, int index)
		{
			T[] reducedArray = new T[1];

			if (originalArray == null || originalArray.Length == 0)
            {
				return reducedArray;
            }

			if (index < originalArray.Length)
			{
				reducedArray[0] = originalArray[index];
			}
			else
			{
				reducedArray[0] = originalArray[0];
			}

			return reducedArray;
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

		public string Id
		{
			get
			{
				return id[0];
			}
		}
		public string DisplayName             { get { return displayName?[0]; } }
		public string ICAOCode				  { get { return  iCAOCode?[0]; }}
		public string Manufacturer			  { get { return  manufacturer?		[0]; }}
											  				 					
		public short NumBuilt				  { get { return numBuilt				[0]; }}
		public short YearIntroduced			  { get { return yearIntroduced			[0]; }}
		public short YearLastProduced		  { get { return yearLastProduced		[0]; }}
		public short YearRetired			  { get { return yearRetired			[0]; }}
														 					
		public short Capacity_PAX			  { get { return capacity_PAX			[0]; }}
		public short ExitLimit_PAX			  { get { return exitLimit_PAX			[0]; }}
		public short SeatsAbreast			  { get { return seatsAbreast			[0]; }}
		public short CapacityULDLowerDeck	  { get { return capacityULDLowerDeck	[0]; }}
		public short CapacityULDUpperDeck	  { get { return capacityULDUpperDeck	[0]; }}
		public short CapacityBulkCargo_KG	  { get { return capacityBulkCargo_KG	[0]; }}


		public short  Range_KM				   { get { return 	range_KM			[0]; }}
		public short  Speed_KMH				   { get { return 	speed_KMH			[0]; }}
		public short  Etops_Minutes			   { get { return 	etops_Minutes		[0]; }}
		public int    FuelCapacity_L		   { get { return 	fuelCapacity_L	[0]; }}
		public string EngineType			   { get { return 	engineType?		[0]; }}
		public short  TakeoffDistance_M		   { get { return 	takeoffDistance_M	[0]; }}
		public int    MaxTOW_KG				   { get { return 	maxTOW_KG			[0]; }}
																				
		public bool   NeedStairs			   { get { return 	needStairs		[0]; }}
		public bool   NeedPushback			   { get { return 	needPushback		[0]; }}
		public bool   NeedPaved				   { get { return 	needPaved			[0]; }}
		public bool   NeedHeavyPaved		   { get { return 	needHeavyPaved	[0]; }}
		public bool   CanGetPushback		   { get { return 	canGetPushback	[0]; }}
																				
		public short  JetbridgePoints		   { get { return 	jetbridgePoints	[0]; }}
		public short  ConveyerPoints		   { get { return 	conveyerPoints	[0]; }}
		public short  CleaningPoints		   { get { return 	cleaningPoints	[0]; }}
		public short  CateringPoints		   { get { return 	cateringPoints	[0]; }}

		public bool   SonicBoom				   { get { return 	sonicBoom			[0]; }}
		public bool   VIP					   { get { return 	vIP				[0]; }}
		public bool   Combi					   { get { return 	combi				[0]; }}
		public bool   Cargo					   { get { return 	cargo				[0]; }}
		public bool   Loud					   { get { return 	loud				[0]; }}
		public bool   Quiet					   { get { return 	quiet				[0]; }}
	}

	public static class AircraftTypeDataUtilities
    {
		public static AircraftTypeData AircraftTypeDataCondensed(string aircraftType)
        {
			if (AirportCEOAircraft.aircraftTypeDataDict.TryGetValue(aircraftType, out AircraftTypeData aircraftTypeData))
			{
			}
			else
			{
				return new AircraftTypeData();
			}
			return aircraftTypeData.SingleAircraftTypeData(aircraftType);
		}
		public static AircraftTypeData GetAircraftTypeData(this AircraftModel aircraft)
        {
			return AircraftTypeDataCondensed(aircraft.aircraftType);
        }
		public static AircraftTypeData GetAircraftTypeData (this FlightModel FlightModel)
        {
			return AircraftTypeDataCondensed(FlightModel.aircraftTypeString);
        }
		public static T GetAtIndexOrZero<T>(this T[] array,int index)
        {

			if (array.Length>index)
            {
				return (T)array.GetValue(index);
            }
			else if (array.Length>0)
            {
				return (T)array.GetValue(0);
			}
			else
            {
				return default;
            }
        }
    }
}
