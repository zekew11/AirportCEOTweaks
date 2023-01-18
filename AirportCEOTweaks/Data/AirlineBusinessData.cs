using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks
{
	public struct AirlineBusinessData
	{
		public string[] tweaksFleet;
		public int[] tweaksFleetCount;
		public string[] arrayHomeCountryCodes;
		public string[] arrayForbiddenCountryCodes;
		public string[] arrayHubIATAs;
		public int[] arrayRangesFromHubs_KM;
		public bool domesticOnly;
		public bool internationalMustOriginateAtHub;
		public bool allMustOriginateAtHub;
		public bool onlyFlyHubtoHub;
		public int[] overrideServiceLevelByAircraftType;
		public int overridemaximumFlights;
		public string[] commonBrand;
		public string[] siblingCompanys;
		public string[] mergeMeIntoCompanys;
	}
}
