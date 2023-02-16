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
	public class RouteGenerationController : MonoBehaviour
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
			//Debug.LogError("ACEO Tweaks | Debug: RouteGenerationController Init");

			this.airports = airports;
			//Debug.LogError("ACEO Tweaks | Debug: airports.length = " + airports.Length);
			this.cities = cities;
			//Debug.LogError("ACEO Tweaks | Debug: cities.length = " + cities.Length);
			this.countries = countries;
			//Debug.LogError("ACEO Tweaks | Debug: countries.length = " + countries.Length);
			this.continents = continents;
			//Debug.LogError("ACEO Tweaks | Debug: continents.length = " + continents.Length);

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
				if (TravelController.IsDomesticAirport(airport, playerAirport))
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
				float longthresh = (float)(0.00000152 * Math.Pow(playerAirport.latitude, 4) + 20);  // 15 at 0 deg lat; 27 at 45lat; 120 at 90 lat, close enough

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
			Enums.GenericSize[] relevantAirportSizes = { Enums.GenericSize.Gigantic, Enums.GenericSize.Huge, Enums.GenericSize.VeryLarge, Enums.GenericSize.Large };
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

					canidateAirports.UnionWith(airportsBySize[Enums.GenericSize.Tiny]);

				}
			}

			for (int i = 0; i < numberToGenerate; i++)
			{
				if (canidateAirports.Count <= 0)
                {
					break;
                }
				Airport airport = canidateAirports.ElementAt<Airport>(Random.Range(0, canidateAirports.Count));

				routeContainers.Add(
					new RouteContainer(
						new Route(airport.id, playerAirport.id, (float)Utils.GetDistanceBetweenCoordinates(airport.latitude, airport.longitude, playerAirport.latitude, playerAirport.longitude)
						)));
			}

			return routeContainers;
		}
		public SortedSet<RouteContainer> SelectRouteContainers(float maxRange = 16000, float minRange = 0, bool forceDomestic = false, bool forceOrigin = false, Country[] origin = null)
		{
			forceOrigin = origin == null ? false : forceOrigin;
			
			SortedSet<RouteContainer> returnSet = new SortedSet<RouteContainer>();
			float min = 0;
			float max = (float)routeContainers.Count;
			int rand;
			RouteContainer item;

			for (int i = 0; (i < 200 && returnSet.Count < 100); i++)
			{
				rand = (int)Random.Range(min, max);
				item = routeContainers.ElementAt(rand);
				if (item.Distance > maxRange)
				{
					max = rand;
				}
				else if (item.Distance < minRange)
				{
					min = rand;
				}
				else
				{
					returnSet.Add(item);
				}
			}

			if (forceDomestic)
			{
				returnSet = FilterDomestic(returnSet);
			}

			if (forceOrigin)
			{
				returnSet = FilterByOrigin(returnSet, origin);
			}

			return returnSet;
		}
		public SortedSet<RouteContainer> NewSelectRoutesByChance(SortedSet<RouteContainer> originalSet, short numToSelect = 1)
        {
			List<RouteContainer> list = new List<RouteContainer>();
			SortedSet<RouteContainer> returnSet = new SortedSet<RouteContainer>();
			

			if (originalSet.Count == 0)
            {
				return originalSet;
            }


			int setSizeSquare = (originalSet.Count+1)^2;

			list.AddRange(originalSet);
			list.OrderBy(route => route.Chance);

			for (int i = 0; i < numToSelect; i++)
            {
				int squareSelection = Random.Range(0, setSizeSquare);
				int selection = Utils.RoundToIntLikeANormalPerson((float)(Math.Sqrt(squareSelection))).Clamp(0,list.Count);
				setSizeSquare -= (selection ^ 2 - ((selection - 1) ^ 2)).Clamp(1,setSizeSquare);

				try { returnSet.Add(list.ElementAt(selection)); }
				catch { Debug.LogError("ACEO Tweaks | ERROR: list.count = " + list.Count + ", selection = " + selection); };
            }

			return returnSet;
        }
		public SortedSet<RouteContainer> SelectRoutesByChance(SortedSet<RouteContainer> originalSet, short numToSelect=1)
		{
			SortedSet<RouteContainer> returnSet = originalSet;
			int i = 0;
			RouteContainer a;
			RouteContainer b;
			int sumChance;

			numToSelect=numToSelect.ClampMin<short>(1);

			for (; ; )
            {
				if (returnSet.Count <= numToSelect)
                {
					if (returnSet.Count <=0)
                    {
						Debug.LogError("ACEO Tweaks | ERROR: SelectRoutesByChance Returned Empty("+ returnSet.Count +") Set! i="+i);
                    }
					return returnSet;
                }
				if (i + 1 < returnSet.Count)
				{
					a = returnSet.ElementAt(i);
					b = returnSet.ElementAt(i + 1);
					sumChance = a.Chance + b.Chance;
					if (Random.Range(0,sumChance) > a.Chance)
                    {
						returnSet.Remove(a);
                    }	
					else
                    {	
						returnSet.Remove(b);
                    }
					i++;
				}
				else
                {
					i = 0;
                }
            }
		}
		private SortedSet<RouteContainer> FilterDomestic(SortedSet<RouteContainer> originalSet)
        {
			return new SortedSet<RouteContainer>(originalSet.Where(route => route.Domestic).ToList());
        }
		private SortedSet<RouteContainer> FilterByOrigin(SortedSet<RouteContainer> originalSet, Country[] origin)
        {
			return new SortedSet<RouteContainer>(originalSet.Where(route => origin.Contains(route.country)).ToList());
		}
		private SortedSet<RouteContainer> FilterByForbidenOrigin(SortedSet<RouteContainer> originalSet, Country[] forbidden)
		{
			originalSet.RemoveWhere(route => forbidden.Contains(route.route.ToAirport.Country));
			originalSet.RemoveWhere(route => forbidden.Contains(route.route.FromAirport.Country));
			return originalSet;
		}
		private SortedSet<RouteContainer> FilterByDistance(SortedSet<RouteContainer> originalSet, float maxDistance, float minDistance)
		{
			return new SortedSet<RouteContainer>(originalSet.Where(route => route.Distance > minDistance && route.Distance < maxDistance));
		}
		private SortedSet<RouteContainer> FilterByDirection(SortedSet<RouteContainer> originalSet, short direction, short allowedHalfAngle)
		{
			return new SortedSet<RouteContainer>(originalSet.Where(route => ((route.Direction - direction + 360) % 360) < allowedHalfAngle));
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
			//int i= 0; //debug only
			Debug.Log("ACEO Tweaks | Debug: CoroutineGenerateRoutes Started");
			for (;;)
			{
				short numToGen = ((short)((airports.Length - routeContainers.Count) / (200))).Clamp<short>(1,20);
				if (numToGen < 4)
                {
					numToGen = 4;
					yield return new WaitForSeconds(10f);
				}

				//Debug.LogError("ACEO Tweaks | Debug: iteration " + i + " of GenerateRoutes; requested " + numToGen + " routes. Have " + routeContainers.Count+".");

				routeContainers.UnionWith(GenerateSomeRouteContainers(numToGen));

				//Debug.LogError("ACEO Tweaks | Debug: iteration " + i + "; now have " + routeContainers.Count + " routes.");

				yield return new WaitForSeconds(.5f);
				//i++;//debug
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
	
	public class RouteContainer : IComparable<RouteContainer> , IEquatable<Airport> , IEquatable<RouteContainer>
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

		public Route route;
		Airport airport;
		Enums.GenericSize size;
		Enums.GenericSize cargosize;
		public Country country;
		private sbyte domestic = -1; //Access Domestic
		private sbyte etops = -1;    //Access Etops
		private float distance;      //Access Distance
		private short direction;     //Access Direction
		private short directionr;    //Access Directionr
		private int chance = -1;			 //Access Chance


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
		public Airport Airport
        {
            get
            {
				return airport;
            }
        }
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
		} //get accessor with one time calc
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
		} //get accessor with one time calc
		public int Chance
        {
			get
            {
				if (chance < 0)
                {
					chance = Calculate_Chance(size);
                }
				return chance;
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

		private int Calculate_Chance(Enums.GenericSize size)
        {
			if (Distance<=0)
            {
				Debug.LogError("ACEO Tweaks | ERROR: A route has 0 or negative distance! ("+airport.airportName+")");
				return 0;
            }

			int chance = 0;
			switch (size)
            {
				case Enums.GenericSize.Tiny		: chance= 1;break;
				case Enums.GenericSize.VerySmall: chance= 3;break;
				case Enums.GenericSize.Small	: chance= 4;break;
				case Enums.GenericSize.Medium	: chance= 8;break;
				case Enums.GenericSize.Large	: chance=10;break;
				case Enums.GenericSize.VeryLarge: chance=25;break;
				case Enums.GenericSize.Huge		: chance=30;break;
				case Enums.GenericSize.Gigantic : chance=20;break;
			}

			if (Domestic)
            {
				chance *= 2;
            }

			chance *= (20000 / Distance).RoundToIntLikeANormalPerson();

			return chance;
        }
    }
}
