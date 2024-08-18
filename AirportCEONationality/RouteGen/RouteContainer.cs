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

		public bool VanillaDomestic
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

			if (VanillaDomestic)
            {
				chance *= 2;
            }

			chance *= (20000 / Distance).RoundToIntLikeANormalPerson();

			return chance;
        }
    }
}
