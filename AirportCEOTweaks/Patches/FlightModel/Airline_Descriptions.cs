using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks
{
    public class Airline_Descriptions
    {
        public string Replace_Description(AirlineModel airline)        
        {
            

            switch (airline.businessName)
            {
                case "Maple Express": return "Through Maple Express, the Maple group can extend its routes from busy hub airports out to remote destinations. Maple express flys shorter routes, providing acceptable comfort for passengers connecting to longer, larger flights or transiting between differnt more remote areas.";
                case "Yuri Air": return "In the furvor of the space race, the primier national carrier was renamed to honor a national hero. While no longer considered primier, the organization continues to competently and efficently move people and goods in the air.";
                case "Nordic Vintage": return "Nordic is a small airline with a simple fleet. Delivering on-time flights with great attention to service, Nordic has been able to attract many travellers across the nordic countries and abroad.";
                case "Swiftly": return "Swiftly has been a legendary name in aviation since the mid 1950s. The airline's early ability to adapt to and lead changes in the industry through the peak of the jet-age left it one of the largest in the world. Perhaps becasue of this size, the airline now appears behind the curve on some developments, but does not seem to be in any significant financial danger.";
                case "Swiftly Vintage": return "Swiftly has grown quickly in the past two decades to become a true titan of aviation. The airline's ability to adapt to and lead changes in the industry through has left it one of the most sucessful in the world.";
                case "Tulip Airlines": return "Tulip airlines became a prominent national carrier in the 1950s. While the airline’s early transatlantic services were widely praised, the jet-age left the carrier struggling to adapt. As the original long-haul fleet retired the aircraft were not able to be replaced. The airline shifted its attention to regional activities by the end of the 1960s. \n \nThe once-struggling carrier has been slowly but steadily expanding its regional services since the early 2000s. While the airline no longer caters to an elite audience, competent management has ensured that the airline’s customers are always treated with respect: leading to excellent public perception of the brand. \n \nWith a well developed network of regional routes, Tulip proudly announced that it would resume long-haul operations on the 50th anniversary of its last transatlantic flight - December 10 2017 - and thereafter operate a fleet of four 787-9s on routes around the world!";
                case "Tulip Airlines Vintage": return "Tulip airlines was was founded in late 1947 and became a prominent national carrier in the early 1950s. While the airline’s transatlantic services are widely praised for their luxury, the jet-age sees the carrier struggling to adapt to the new realities of commercial aviation. While the airline's regional buisness remains strong, the long-haul fleet is aging and these aircraft are unlikly to be replaced.";
                case "Siberian Airlines": return "Siberian Airlines operates with participation from major governemnts and private investors. It is considered the most modern and comfortable carrier in the eastern world, though the proletariat is conspicuously underrepresented amoung its audience.";
                default: return airline.businessDescription;
            }

        }
        public string Generate_Description(Extend_AirlineModel eam) //currently overridden by return
        {
            string stringy = "";

            switch (eam.economyTier.RoundToIntLikeANormalPerson())
            {
                case 1: //economy
                    switch (eam.starRank)
                    {
                        case Enums.BusinessClass.Cheap: stringy = " is a very small carrier which has been known to cut corners to save some money."; break;
                        case Enums.BusinessClass.Small: stringy = " is a small economy-class carrier."; break;
                        case Enums.BusinessClass.Medium: stringy = " is a somewhat established economy-class carrier.";break;
                        case Enums.BusinessClass.Large: stringy = " is one of the leading airlines in the low-cost sector";break;
                        case Enums.BusinessClass.Exclusive: stringy = " is a multi-national giant among the low-cost airlines.";break;
                        case Enums.BusinessClass.Unspecified: stringy = "";break;
                    }
                    break;
                case 2: //mainline
                    switch (eam.starRank)
                    {
                        case Enums.BusinessClass.Cheap: stringy = " is a very small carrier attempting to make it in the airline industry."; break;
                        case Enums.BusinessClass.Small: stringy = " is small but established airline offering balanced services."; break;
                        case Enums.BusinessClass.Medium: stringy = " is a mid-sized airline with balanced services."; break;
                        case Enums.BusinessClass.Large: stringy = " is an important player in the industry with hundereds of flights per day."; break;
                        case Enums.BusinessClass.Exclusive: stringy = " is among the largest airlines in the world offering relativly standard services within an enormous network."; break;
                        case Enums.BusinessClass.Unspecified: stringy = ""; break;
                    }
                    break;
                case 3: //flagship 
                    switch (eam.starRank)
                    {
                        case Enums.BusinessClass.Cheap: stringy = " is a very small carrier with a well-respected and comfortable service niche."; break;
                        case Enums.BusinessClass.Small: stringy = " is small but airline with a cult following thanks to exceptional customer service."; break;
                        case Enums.BusinessClass.Medium: stringy = " is a mid-to-large sized airline loved by those who can afford to fly with them."; break;
                        case Enums.BusinessClass.Large: stringy = " is a large airline commited to the standards of quality most common at the dawn of the jet-age."; break;
                        case Enums.BusinessClass.Exclusive: stringy = " is a large and prestigious carrier that invests significantly more than average in passenger experaince: They have a reputation to uphold!"; break;
                        case Enums.BusinessClass.Unspecified: stringy = ""; break;
                    }
                    break;
                case 4: //VIP
                    switch (eam.starRank)
                    {
                        case Enums.BusinessClass.Cheap: stringy = " is a very small VIP and charter airline."; break;
                        case Enums.BusinessClass.Small: stringy = " is small VIP and charter airline."; break;
                        case Enums.BusinessClass.Medium: stringy = " is a mid sized airline catering to a very wealthy auidence."; break;
                        case Enums.BusinessClass.Large: stringy = " is a large airline that caters to VIPs and those of status."; break;
                        case Enums.BusinessClass.Exclusive: stringy = " is a large and prestigious airline offering one of the most exclusive scheduled services in operation."; break;
                        case Enums.BusinessClass.Unspecified: stringy = ""; break;
                    }
                    break;
                case 5: //other/notspeciified
                    break;
                case 6: //cargo
                    switch (eam.starRank)
                    {
                        case Enums.BusinessClass.Cheap: stringy = " is a very small cargo carrier."; break;
                        case Enums.BusinessClass.Small: stringy = " is small cargo airline."; break;
                        case Enums.BusinessClass.Medium: stringy = " is a mid sized cargo airline."; break;
                        case Enums.BusinessClass.Large: stringy = " is a large cargo airline."; break;
                        case Enums.BusinessClass.Exclusive: stringy = " is a world-leading cargo airline."; break;
                        case Enums.BusinessClass.Unspecified: stringy = ""; break;
                    }
                    break;
                case 7: //special cargo
                    switch (eam.starRank)
                    {
                        case Enums.BusinessClass.Cheap: stringy = " is a very small contract air-courier."; break;
                        case Enums.BusinessClass.Small: stringy = " is a small contract air-courier."; break;
                        case Enums.BusinessClass.Medium: stringy = " is a mid sized special cargo service."; break;
                        case Enums.BusinessClass.Large: stringy = " is a powerful specialty freight service."; break;
                        case Enums.BusinessClass.Exclusive: stringy = " is a world-leading specialty logistics service."; break;
                        case Enums.BusinessClass.Unspecified: stringy = ""; break;
                    }
                    break;
            }
            return string.Empty;
            return string.Concat(eam.parent.businessName, stringy);
        }
    }
}
