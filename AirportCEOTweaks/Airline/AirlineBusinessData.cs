using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks
{
	public struct AirlineBusinessData
	{
		public string name;                                // Picks up the name field from the vanilla file for validation purposes.

		public string[] tweaksFleet;                       // Optional: Overwrites "fleet" when tweaks is installed.
		public int[] tweaksFleetCount;                     // Optional: Overwirtes "fleetCount" when teaks is installed. Future features will expect this to be a count of aircraft, not a ratio.
		public string[] arrayHomeCountryCodes;             // Optional: Like countryCode but can accept multiple. Overwrites countryCode if defined.
		public string[] arrayForbiddenCountryCodes;		   // Optional: Extends the "nationality" system by forbidding any flights to or from the listed countries.
		public string[] arrayHubIATAs;					   // Optional: Can be used with other settings to refine airline routing.
		public int[] arrayRangesFromHubs_KM;               // Optional: Used with "arrayHubIATAs" to define maximum distances from each hub that the airline may operate to. If only one value is provided this value applies to all hubs.
		public bool internationalMustOriginateAtHub;	   // Optional: Used with "arrayHubIATAs" to force international flights (those that require passport check) to originate at a listed hub IATA.
		public bool allMustOriginateAtHub;				   // Optional: Used with "arrayHubIATAs" to force all flights to have at least one endpoint at a listed "hub".
		public bool onlyFlyHubtoHub;                       // Optional: Used with "arrayHubIATAs" to force all flights to have both endpoints at a listed "hub". At this point you are fully definig the route network.
														   //           Not reccomended, envisioned as something an end user might do for a "hardcore simulation" playthrough.
		public bool domesticOnly;                          // Optional: If true, airline cannot operate routes which have any endpoint outside of the Home Country(s) (either from "countryCode" or "arrayHomeCountryCodes")


		public int[] overrideServiceLevelByAircraftType;   // Optional: [Not to be Implimeneted in 2.3.0] Previously the "flight types" system, flight service level is set by airline star rank:
														   //			
														   //			*     | LCC/ULCC
														   //			**    | Charter
														   //			***   | Proffessional Airline
														   //			****  | Premium Airline
														   //			***** | Very Premium Airline (Flag carriers ect)
														   //
														   //           This array allows operating some aircraft types at a service level other than the star rank. Putting only one entry in this array applies that value to all types.
														   //
		public int[] maximumFlightReccurances;             // Optional: [Not to be Implimeneted in 2.3.0] Maximum day-over-day repetitions offered for flights or renewed up to. Enter single value to apply to all aircraft types, or muliple on a per-type basis
		public float[] flightRenewalChanceModifier;        // Optional: [Not to be Implimeneted in 2.3.0] Modify (or eliminate) the chance the airline will renew a flight (of a given type).
		public int overridemaximumFlights;                 // Optional: [Not to be Implimeneted in 2.3.0] Modify the total number of flights the airline will offer/operate at 100% satisfaction
		public string[] commonBrand;                       // Optional: [Not to be Implimeneted in 2.3.0] Define other airlines that share a public brand with this operator, enables sharing stands and lounges.
														   //                                             Dummy airlines can be created with no aircraft to stand in for alliances.
		public string[] siblingCompanys;                   // Optional: [Not to be Implimeneted in 2.3.0] Define other airlines that share managment. Allows reputation impacts to effect partners.
		public string[] mergeMeIntoCompanys;               // Optional: [Not to be Implimeneted in 2.3.0] Define an airline (or airlines? probably not but I'll still let you) that this airline is entirly a part of.
														   //                                             Places all flights, contracts, negotiations, ect under the parent, to the point that player shouldn't be able to tell there's a distrinction
														   //                                             (unless you signal it, eg with a different logo/flight color)
	}
}
