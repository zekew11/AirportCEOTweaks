using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks
{
    class RouteGenerationController
    {
		public static Route GenerateRoute(float aircraftRange, Enums.GenericSize size, Enums.ThreeStepScale flightSize, float minDistanceMultiplier = 0.1f, float maxDistanceMultiplier = 1f, bool domestic = true)
		{
			Airport playerAirport = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
			Enums.GenericSize[] relevantAirportSizes = GetRelevantAirportSizes(size);
			List<Airport> list = new List<Airport>();
			for (int i = 0; i < relevantAirportSizes.Length; i++)
			{
				list.AddRange(TravelController.airportsBySize[relevantAirportSizes[i]]);
			}
			float num = aircraftRange * minDistanceMultiplier;
			float num2 = aircraftRange * maxDistanceMultiplier;
			bool flag;
			switch (flightSize)
			{
				case Enums.ThreeStepScale.Small:
					flag = (GameSettingManager.RealisticInternationalStands && Utils.ChanceOccured(Singleton<AirportController>.Instance.smallDomesticStandRatio));
					num = num.ClampMax(100f);
					break;
				case Enums.ThreeStepScale.Medium:
					flag = (GameSettingManager.RealisticInternationalStands && Utils.ChanceOccured(Singleton<AirportController>.Instance.mediumDomesticStandRatio));
					num = num.ClampMax(250f);
					break;
				case Enums.ThreeStepScale.Large:
					flag = (GameSettingManager.RealisticInternationalStands && Utils.ChanceOccured(Singleton<AirportController>.Instance.largeDomesticStandRatio));
					break;
				default:
					flag = false;
					break;
			}
			int j = 0;
			int num3 = flag ? TravelController.domesticAirports.Count : list.Count;
			int num4 = flag ? Utils.RandomIndexInCollection<int>(TravelController.domesticAirports) : Utils.RandomIndexInCollection<Airport>(list);
			while (j < num3)
			{
				j++;
				num4++;
				if (num4 >= num3)
				{
					num4 = 0;
				}
				Airport airport = flag ? TravelController.airportsById[TravelController.domesticAirports[num4]] : list[num4];
				if ((TravelController.IsDomesticAirport(airport, playerAirport) || TravelController.InternationalStandExistForSize(flightSize)) && !airport.airportIATACode.Equals(playerAirport.airportIATACode))
				{
					float num5 = (float)Utils.GetDistanceBetweenCoordinates(airport.latitude, airport.longitude, playerAirport.latitude, playerAirport.longitude);
					if (num5 <= num2 && num5 >= num)
					{
						return new Route(airport.id, playerAirport.id, num5);
					}
				}
			}
			j = 0;
			while (j < 1000)
			{
				j++;
				Airport airport2 = Utils.RandomItemInCollection<Airport>(list);
				float num6 = (float)Utils.GetDistanceBetweenCoordinates(airport2.latitude, airport2.longitude, playerAirport.latitude, playerAirport.longitude);
				if (num6 <= num2 && num6 >= num)
				{
					return new Route(airport2.id, playerAirport.id, num6);
				}
			}
			Airport airport3 = Utils.RandomItemInCollection<Airport>(list);
			return new Route(airport3.id, playerAirport.id, (float)Utils.GetDistanceBetweenCoordinates(airport3.latitude, airport3.longitude, playerAirport.latitude, playerAirport.longitude));
		}
		private static Enums.GenericSize[] GetRelevantAirportSizes(Enums.GenericSize genericSize)
		{
			switch (genericSize)
			{
				case Enums.GenericSize.Gigantic:
					return new Enums.GenericSize[]
					{
			Enums.GenericSize.Gigantic,
			Enums.GenericSize.Huge,
					};
				case Enums.GenericSize.Huge:
					return new Enums.GenericSize[]
					{
			Enums.GenericSize.Gigantic,
			Enums.GenericSize.Huge,
			Enums.GenericSize.VeryLarge,
					};
				case Enums.GenericSize.VeryLarge:
					return new Enums.GenericSize[]
					{
			Enums.GenericSize.Gigantic,
			Enums.GenericSize.Huge,
			Enums.GenericSize.VeryLarge,
			Enums.GenericSize.Large,
					};
				case Enums.GenericSize.Large:
				case Enums.GenericSize.Medium:
					return new Enums.GenericSize[]
					{
			Enums.GenericSize.Gigantic,
			Enums.GenericSize.Huge,
			Enums.GenericSize.VeryLarge,
			Enums.GenericSize.Large,
			Enums.GenericSize.Medium,
			Enums.GenericSize.Small,
					};
				case Enums.GenericSize.Small:
				case Enums.GenericSize.VerySmall:
				case Enums.GenericSize.Tiny:
					return new Enums.GenericSize[]
					{
			Enums.GenericSize.Large,
			Enums.GenericSize.Medium,
			Enums.GenericSize.Small,
			Enums.GenericSize.VerySmall,
			Enums.GenericSize.Tiny
					};
				default:
					return new Enums.GenericSize[]
					{
			Enums.GenericSize.Gigantic,
			Enums.GenericSize.Huge,
			Enums.GenericSize.VeryLarge,
			Enums.GenericSize.Large,
			Enums.GenericSize.Medium,
			Enums.GenericSize.Small,
			Enums.GenericSize.VerySmall,
			Enums.GenericSize.Tiny
					};
			}
		}

		//Map of continent vs continent twin-engine restriction
		public readonly bool[,] etopsContinents ={
		    //  EU    AS    NA    AF    AN    SA    OC
		      {false,false,true ,false,true ,true ,true },   //  EU / Europe          	-  Falklands
		    														  					
		      {false,false,true ,false,true ,true ,true },	 //  AS / Asia				-
		    														  					
		      {true ,true ,false,true ,true ,false,true },	 //  NA / North America		-  Hawaii
		    														  					
		      {false,false,true ,false,true ,true ,true },	 //  AF / Africa			-
		    														  					
		      {true ,true ,true ,true ,false,false,true },	 //  AN / Antarctica		-
		    														  					
		      {true ,true ,false,true ,false,false,true },	 //  SA / South America		-
		    														  					
		      {true ,true ,true ,true ,true ,true ,false}	 //  OC / Oceania			-
		};
	}
	class RouteContainer : IComparable
	{
		public int CompareTo(object obj)
		{
			return Distance.CompareTo(obj);
		}

		Route route;
		Airport airport;
		Enums.GenericSize size;
		Enums.GenericSize cargosize;
		Country country;
		private sbyte domestic = -1; //Access Domestic
		private sbyte etops = -1;    //Access Etops
		private float distance;      //Access Distance
		short direction;
		short directionr;


		RouteContainer(Route route)
		{
			this.route = route;
			this.airport = TravelController.GetAirportById(route.fromAirport);
			this.size = airport.paxSize;
			this.cargosize = airport.cargoSize;
			this.country = airport.Country;
			distance = route.routeDistance;
			direction = GetDirectionOfAirport(airport);
			directionr = (short)((direction + 180) % 360); //reciprocal of direction
		}

		//Accessors
		public float Distance
		{
			get
			{
				return distance;
			}
		} //plain get accessor
		public bool Domestic
		{
			get
			{
				if (domestic == -1)
				{
					if (TravelController.IsDomesticAirport(GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport, airport))
					{
						domestic = 1;
					}
					else
					{
						domestic = 0;
					}
				}
				return domestic.Equals(1);
			}
		} //get accessor with one time calc
		public bool Etops //TODO make continent rules for coarse ETOPs enforcment. Currently just returns false
		{
			get
			{
				return false;
			}
		}



        private short GetDirectionBetweenCoordinates(float lat1, float long1, float lat2, float long2)
        {
			//modified from...
			//https://www.movable-type.co.uk/scripts/latlong.html

			float y = (float)(Math.Sin(long2 - long1) * Math.Cos(lat2));
			float x = (float)(Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(long2 - long1));
			float θ = (float)Math.Atan2(y, x);
			return (short)((θ * 180 / Math.PI + 360) % 360); // in degrees
		}
		private short GetDirectionOfAirport(Airport airport)
        {
			// real math in GetDirectionBetweenCoordinates
			// here we're just fetching those cords
			
			Airport myAirport = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;

			float lat1 = ((float)myAirport.latitude);
			float long1 = ((float)myAirport.longitude);

			float lat2 = ((float)airport.latitude);
			float long2 = ((float)airport.longitude);

			return GetDirectionBetweenCoordinates(lat1, long1, lat2, long2);
        }
	}
}
