using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AirportCEONationality
{
	public class RouteGenerationController : MonoBehaviour
	{
		private Airport[] airports;
		private City[] cities;
		private Country[] countries;
		private Continent[] continents;

		Airport PlayerAirport
        {
			get
            {
				return GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
			}
        }

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
				if (TravelController.IsDomesticAirport(airport, PlayerAirport))
				{
					domesticAirports.Add(airport);
				}
			}
			Debug.Log("ACEO Tweaks | Debug: domesticAirports.count = " + domesticAirports.Count);

			nearAirports = new HashSet<Airport>();
			foreach (Airport airport in airports)
			{
				// 1200 nautical miles/60nm/degree = 20 deg

				float latdiff = (float)(PlayerAirport.latitude - airport.latitude) % 360;

				if (latdiff > 20)
				{
					continue;
				}

				//1200 / 60 >>> 0 = 20>180
				float longdiff = (float)(PlayerAirport.longitude - airport.longitude) % 360;
				float longthresh = (float)(0.00000152 * Math.Pow(PlayerAirport.latitude, 4) + 20);  // 15 at 0 deg lat; 27 at 45lat; 120 at 90 lat, close enough

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
			StartCoroutine(CoroutineGenerateRouteContainers());
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
						new Route(airport.id, PlayerAirport.id, (float)Utils.GetDistanceBetweenCoordinates(airport.latitude, airport.longitude, PlayerAirport.latitude, PlayerAirport.longitude)
						)));
			}

			return routeContainers;
		}
		public SortedSet<RouteContainer> SelectRouteContainers(
			ref HashSet<RouteContainer> preExistingContainers,
			float maxRange = 16000, float minRange = 0,
			bool forceDomestic = false, bool forceNationalOrigin = false, Country[] origin = null, Country[] forbidden = null,
			bool forceHUBOriginInternational = false, bool forceHUBOriginAll = false, bool forceHUBRange = false , Airport[] hUBs = null, float[] hUBRanges = null)
		{
			forceNationalOrigin = origin == null ? false : forceNationalOrigin;
			
			SortedSet<RouteContainer> returnSet = new SortedSet<RouteContainer>();
			HashSet<RouteContainer> returnHashSet = new HashSet<RouteContainer>();
			HashSet<RouteContainer> removeSet = new HashSet<RouteContainer>();

			
			float min = 0;
			float max = routeContainers.Count;
			int rand;
			int routesToAdd = 20;
			RouteContainer item;

			returnHashSet.UnionWith(preExistingContainers);
			
			foreach (RouteContainer container in returnHashSet)
            {
				if (container.Distance > maxRange)
				{ 
					removeSet.Add(container); 
				}
				else if (container.Distance < minRange)
                {
					removeSet.Add(container);
                }
            }
			returnHashSet.ExceptWith(removeSet);

			for (int i = 0; (i < 100); i++)
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
					if(returnHashSet.Add(item))
                    {
						routesToAdd--;

						if(routesToAdd <= 0)
						{ break; }
                    }
				}
			}

			returnSet.UnionWith(returnHashSet);

			if (forceDomestic)
			{
				returnSet = FilterDomestic(returnSet, origin);
			}

			if (forceNationalOrigin)
			{
				returnSet = FilterByOrigin(returnSet, origin);
			}

			if (forbidden != null && forbidden.Length>0)
            {
				returnSet = FilterByForbidenOrigin(returnSet, forbidden);
            }

			if (forceHUBOriginAll&& hUBs != null && hUBs.Length>0)
            {
				returnSet = FilterByAirportofOrigin(returnSet, hUBs);
            }

			if (forceHUBOriginInternational && hUBs != null && hUBs.Length > 0)
            {
				SortedSet<RouteContainer> domesticSubset = FilterDomestic(returnSet, origin);
				returnSet.ExceptWith(domesticSubset);
				returnSet = FilterByAirportofOrigin(returnSet, hUBs);
				returnSet.UnionWith(domesticSubset);
			}

			if (forceHUBRange &&  hUBs != null && hUBs.Length > 0 && hUBRanges != null && hUBRanges.Length > 0)
            {
				returnSet = FilterByDistanceFromHub(returnSet, hUBs, hUBRanges);
            }

			preExistingContainers.UnionWith(returnSet);
			return returnSet;
		}
		public SortedSet<RouteContainer> SelectRoutesByProbability(SortedSet<RouteContainer> originalSet, short numToSelect = 1)
        {
			if (numToSelect <= 0) { numToSelect = 1; }

			List<RouteContainer> list = new List<RouteContainer>();
			SortedSet<RouteContainer> returnSet = new SortedSet<RouteContainer>();
			

			if (originalSet.Count <= numToSelect)
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
		public SortedSet<RouteContainer> SelectRoutesByRandom(SortedSet<RouteContainer> originalSet, short numToSelect = 1)
        {
			if (numToSelect <= 0) { numToSelect = 1; }

			List<RouteContainer> list = new List<RouteContainer>();
			SortedSet<RouteContainer> returnSet = new SortedSet<RouteContainer>();


			if (originalSet.Count <= numToSelect)
			{
				return originalSet;
			}


			int setSize = originalSet.Count;

			list.AddRange(originalSet);

			for (int i = 0; i < numToSelect; i++)
			{
				int selection = Random.Range(0, setSize);
				setSize -= 1;

				try { returnSet.Add(list.ElementAt(selection)); }
				catch { Debug.LogError("ACEO Tweaks | ERROR: list.count = " + list.Count + ", selection = " + selection); };
			}

			return returnSet;
		}
		private SortedSet<RouteContainer> FilterDomestic(SortedSet<RouteContainer> originalSet, Country[] countries)
		{
			if(countries == null || countries.Length <= 1)
            {
				return new SortedSet<RouteContainer>(originalSet.Where
					(route =>
					(
					IsDomestic(route.country)
					)
					).ToList());
			}
			return new SortedSet<RouteContainer>(originalSet.Where
				(route =>
				(
				IsDomesticPermissive(new Country[] { route.country}, countries)
				&&
				IsDomesticPermissive(new Country[] {PlayerAirport.Country}, countries)
				)
				).ToList());
        }
		private SortedSet<RouteContainer> FilterByOrigin(SortedSet<RouteContainer> originalSet, Country[] origin)
        {
			return new SortedSet<RouteContainer>(originalSet.Where(route => origin.Contains(route.country)).ToList());
		}
		private SortedSet<RouteContainer> FilterByAirportofOrigin(SortedSet<RouteContainer> originalSet, Airport[] originHUB)
		{
			if(originHUB == null || originHUB.Length ==0 )
            {
				return originalSet;
            }
			return new SortedSet<RouteContainer>(originalSet.Where(route => originHUB.Contains(route.Airport)).ToList());
		}
		private SortedSet<RouteContainer> FilterByForbidenOrigin(SortedSet<RouteContainer> originalSet, Country[] forbidden)
		{
			originalSet.RemoveWhere(route => forbidden.Contains(route.route.ToAirport.Country));
			originalSet.RemoveWhere(route => forbidden.Contains(route.route.FromAirport.Country));
			return originalSet;
		}
		private SortedSet<RouteContainer> FilterByDistanceFromHub(SortedSet<RouteContainer> originalSet, Airport[] hUBs ,float[] maxDistance)
		{
			if(hUBs == null || maxDistance == null || hUBs.Length ==0 || maxDistance.Length <=0)
            {
				return originalSet;
            }
			
			return new SortedSet<RouteContainer>(originalSet.Where
				(route => 
				
				InNet(route)

				));
			bool InNet(RouteContainer route)
            {
				int j;
				for (int i = 0; i<hUBs.Length; i++)
				{
					j = maxDistance.Length < i ? 0 : i;
					if (TravelController.GetRouteDistance(new Route(hUBs[i].id,route.Airport.id,0f))<maxDistance[j])
                    {
						return true;
                    }
				}
				return false;
			}
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



		public bool IsDomestic(Country countryA, Country countryB = null)
		{
			if (countryA == null)
			{
				countryA = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country;
			}
			if (countryB == null)
			{
				countryB = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country;
			}
			return IsDomesticPermissive(new Country[] { countryA }, new Country[] { countryB });
		}
		public bool IsDomestic(Airport airportA, Airport airportB = null)
		{
			if (airportA == null)
			{
				airportA = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
			}
			if (airportB == null)
			{
				airportB = GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport;
			}
			return IsDomestic(airportA.Country, airportB.Country);
		}
		public bool IsDomesticPermissive(Country[] countriesA, Country[] countriesB = null)
		{
			if (countriesA == null)
			{
				countriesA = new Country[] { GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country };
			}
			if (countriesB == null)
			{
				countriesB = new Country[] { GameDataController.GetUpdatedPlayerSessionProfileData().playerAirport.Country };
			}

			foreach (Country countryA in countriesA)
			{
				foreach (Country countryB in countriesB)
				{
					if (countryA == countryB)
					{
						return true;
					}
				}
			}
			return false;
		}


		IEnumerator CoroutineGenerateRouteContainers()
        {
			//int i= 0; //debug only
			Debug.Log("ACEO Tweaks | Debug: CoroutineGenerateRoutes Started");
			for (;;)
			{
				short numToGen = ((short)((airports.Length - routeContainers.Count) / (200))).Clamp<short>(2,20);
				if (numToGen < 4)
                {
					yield return new WaitForSecondsRealtime(1f);
				}

				for (int i = 0; i < numToGen; i++)
				{
					if (airports.Length - routeContainers.Count == 0)
                    {
						yield break;
                    }
					routeContainers.UnionWith(GenerateSomeRouteContainers(1));
					yield return null;
				}

				yield return new WaitForSecondsRealtime(.1f);

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

}
