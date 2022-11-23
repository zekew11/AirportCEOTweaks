using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AirportCEOTweaks
{
	class RouteGenerationController : MonoBehaviour
	{
		private Airport[] airports;
		private City[] cities;
		private Country[] countries;
		private Continent[] continents;

		Airport playerAirport;

		Dictionary<Enums.GenericSize, HashSet<Airport>> airportsBySize;
		Dictionary<Enums.GenericSize, HashSet<Airport>> airportsByCargoSize;
		HashSet<Airport> domesticAirports;
		HashSet<Airport> nearAirports;

		SortedSet<RouteContainer> routeContainers;

		public RouteGenerationController() 
        {

		}
		public void Init(Airport[] airports, City[] cities, Country[] countries, Continent[] continents)
        {
			//We construct this from the existing TravelController and pass in the private arrays of airports, cities, continents, ect...
			Debug.LogError("ACEO Tweaks | Debug: RouteGenerationController Init");

			this.airports = airports;
			Debug.LogError("ACEO Tweaks | Debug: airports.length = " + airports.Length);
			this.cities = cities;
			Debug.LogError("ACEO Tweaks | Debug: cities.length = " + cities.Length);
			this.countries = countries;
			Debug.LogError("ACEO Tweaks | Debug: countries.length = " + countries.Length);
			this.continents = continents;
			Debug.LogError("ACEO Tweaks | Debug: continents.length = " + continents.Length);

			playerAirport = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
			MakeDictionarysEct();

			routeContainers = new SortedSet<RouteContainer>();
			routeContainers.UnionWith(GenerateSomeRouteContainers(100));
			StartCoroutines();
		}
		private void MakeDictionarysEct()
        {
			airportsBySize = new Dictionary<Enums.GenericSize, HashSet<Airport>>();
			foreach (Airport airport in airports)
            {
				if (!airportsBySize.ContainsKey(airport.paxSize))
				{
					airportsBySize.Add(airport.paxSize, new HashSet<Airport>());
				}
				airportsBySize[airport.paxSize].Add(airport);
			}
			airportsByCargoSize = new Dictionary<Enums.GenericSize, HashSet<Airport>>();
			foreach (Airport airport in airports)
			{
				if (!airportsByCargoSize.ContainsKey(airport.cargoSize))
				{
					airportsByCargoSize.Add(airport.cargoSize, new HashSet<Airport>());
				}
				airportsByCargoSize[airport.cargoSize].Add(airport);
			}
			domesticAirports = new HashSet<Airport>();
			foreach (Airport airport in airports)
            {
				if (TravelController.IsDomesticAirport(airport,playerAirport))
                {
					domesticAirports.Add(airport);
                }
            }
			Debug.LogError("ACEO Tweaks | Debug: domesticAirports.count = " + domesticAirports.Count);

			nearAirports = new HashSet<Airport>();
			foreach (Airport airport in airports)
            {
				// 1200 nautical miles/60nm/degree = 20 deg
				
				float latdiff = (float)(playerAirport.latitude - airport.latitude) % 360;

				if (latdiff > 20)
                {
					continue;
                }

				//1200 / 60 >>> 0 = 20>180
				float longdiff = (float)(playerAirport.longitude - airport.longitude) % 360;
				float longthresh = (float)(0.00000152 * Math.Pow(playerAirport.latitude,4) + 20);  // 15 at 0 deg lat; 27 at 45lat; 120 at 90 lat, close enough

				if (longdiff > longthresh)
                {
					continue;
                }

				nearAirports.Add(airport);
            }
			Debug.LogError("ACEO Tweaks | Debug: nearAirports.count = " + nearAirports.Count);
		}
		private void StartCoroutines()
        {
			StartCoroutine(CoroutineGenerateRoutes());
        }
		public HashSet<RouteContainer> GenerateSomeRouteContainers(int numberToGenerate = 5)
		{
			Enums.GenericSize[] relevantAirportSizes = {Enums.GenericSize.Gigantic, Enums.GenericSize.Huge , Enums.GenericSize.VeryLarge, Enums.GenericSize.Large};
			HashSet<Airport> canidateAirports = new HashSet<Airport>();
			HashSet<RouteContainer> routeContainers = new HashSet<RouteContainer>();

			for (int i = 0; i < relevantAirportSizes.Length; i++)
			{
				canidateAirports.UnionWith(airportsBySize[relevantAirportSizes[i]]);
				canidateAirports.UnionWith(airportsByCargoSize[relevantAirportSizes[i]]);
				canidateAirports.UnionWith(domesticAirports);
				canidateAirports.UnionWith(nearAirports);
			}
			if (Utils.ChanceOccured(.5f))
            {
				canidateAirports.UnionWith(airportsBySize[Enums.GenericSize.Medium]);

				canidateAirports.UnionWith(airportsByCargoSize[Enums.GenericSize.Medium]);
				canidateAirports.UnionWith(airportsByCargoSize[Enums.GenericSize.Small]);

				if (Utils.ChanceOccured(.5f))
                {
					canidateAirports.UnionWith(airportsBySize[Enums.GenericSize.Small]);
					canidateAirports.UnionWith(airportsBySize[Enums.GenericSize.VerySmall]);

					canidateAirports.UnionWith(airportsByCargoSize[Enums.GenericSize.VerySmall]);

					if (Utils.ChanceOccured(.5f))
					{
						canidateAirports.UnionWith(airportsBySize[Enums.GenericSize.Tiny]);
					}
				}
			}

			for (int i = 0; i < numberToGenerate; i++)
			{
				Airport airport = canidateAirports.ElementAt<Airport>(Random.Range(0f,canidateAirports.Count).RoundToIntLikeANormalPerson());

				routeContainers.Add(
					new RouteContainer(
						new Route(airport.id, playerAirport.id, (float)Utils.GetDistanceBetweenCoordinates(airport.latitude, airport.longitude, playerAirport.latitude, playerAirport.longitude)
						)));
			}

			return routeContainers;
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

		IEnumerator CoroutineGenerateRoutes()
        {
			int i= 0; //debug only
			Debug.LogError("ACEO Tweaks | Debug: CoroutineGenerateRoutes Started");
			for (;;)
			{
				short numToGen = ((short)((airports.Length - routeContainers.Count) / (200))).Clamp<short>(1,20);
				if (numToGen < 4)
                {
					numToGen = 4;
					yield return new WaitForSeconds(10f);
				}

				Debug.LogError("ACEO Tweaks | Debug: iteration " + i + " of GenerateRoutes; requested " + numToGen + " routes. Have " + routeContainers.Count+".");

				routeContainers.UnionWith(GenerateSomeRouteContainers(numToGen));

				Debug.LogError("ACEO Tweaks | Debug: iteration " + i + "; now have " + routeContainers.Count + " routes.");

				yield return new WaitForSeconds(2f);
				i++;//debug
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
		    														  					
		      {true ,true ,true ,true ,true ,true ,false}	 //  OC / Oceania			-  Anything Not Domestic
		};
	
	}
	
	class RouteContainer : IComparable<RouteContainer> , IEquatable<Airport> , IEquatable<RouteContainer>
	{
		public int CompareTo(RouteContainer obj)
		{
			return Distance.CompareTo(obj.Distance);
		}
		public bool Equals(Airport other)
		{
			return airport == other;
		}
		public bool Equals(RouteContainer other)
		{
			return airport == other.airport;
		}

		Route route;
		Airport airport;
		Enums.GenericSize size;
		Enums.GenericSize cargosize;
		Country country;
		private sbyte domestic = -1; //Access Domestic
		private sbyte etops = -1;    //Access Etops
		private float distance;      //Access Distance
		private short direction;     //Access Direction
		private short directionr;    //Access Directionr


		public RouteContainer(Route route)
		{
			this.route = route;
			this.airport = TravelController.GetAirportById(route.fromAirport);
			this.size = airport.paxSize;
			this.cargosize = airport.cargoSize;
			this.country = airport.Country;
			distance = route.routeDistance;
			direction = short.MaxValue;//GetDirectionOfAirport(airport);
			directionr = short.MaxValue;//(short)((direction + 180) % 360); //reciprocal of direction
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
		public short Direction
        {
            get
            {
				if(direction>360) //only on init to short.MaxValue
                {
					direction = GetDirectionOfAirport(airport);
				}
				return direction;
            }
        }
		public short Directionr
		{
			get
			{
				if (directionr > 360) //only on init to short.MaxValue
				{
					directionr = (short)((Direction + 180) % 360);
				}
				return direction;
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
