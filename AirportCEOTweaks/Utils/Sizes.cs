using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOTweaks
{
    public static class Sizes
    {
        public static Tweaks6SizeScale To6Size<T>(T inSize)
        {
            if (inSize is Tweaks8SizeScale eightSize)
            {
                switch (eightSize)
                {
                    case Tweaks8SizeScale.Jumbo       : return Tweaks6SizeScale.F;
                    case Tweaks8SizeScale.VeryLarge   : return Tweaks6SizeScale.E;
                    case Tweaks8SizeScale.Large       :
                    case Tweaks8SizeScale.SuperMedium : return Tweaks6SizeScale.D;
                    case Tweaks8SizeScale.Medium      :
                    case Tweaks8SizeScale.SubMedium   : return Tweaks6SizeScale.Cplus;
                    case Tweaks8SizeScale.Small       : return Tweaks6SizeScale.C;
                    case Tweaks8SizeScale.VerySmall   : return Tweaks6SizeScale.Aplus;
                }
            }
            else if (inSize is Enums.GenericSize genSize)
            {
                switch (genSize)
                {
                    case Enums.GenericSize.Gigantic  : return Tweaks6SizeScale.F;
                    case Enums.GenericSize.Huge      : return Tweaks6SizeScale.E;
                    case Enums.GenericSize.VeryLarge :
                    case Enums.GenericSize.Large     : return Tweaks6SizeScale.D;
                    case Enums.GenericSize.Medium    : return Tweaks6SizeScale.Cplus;
                    case Enums.GenericSize.Small     : 
                    case Enums.GenericSize.VerySmall : return Tweaks6SizeScale.C;
                    case Enums.GenericSize.Tiny      : return Tweaks6SizeScale.Aplus;
                }
            }
            return Tweaks6SizeScale.NA;
        }
        public static Tweaks8SizeScale To8Size(Enums.GenericSize genericSize)
        {
                switch (genericSize)
                {
                    case Enums.GenericSize.Gigantic  : return Tweaks8SizeScale.Jumbo;
                    case Enums.GenericSize.Huge      : return Tweaks8SizeScale.VeryLarge;
                    case Enums.GenericSize.VeryLarge : return Tweaks8SizeScale.Large;
                    case Enums.GenericSize.Large     : return Tweaks8SizeScale.SuperMedium;
                    case Enums.GenericSize.Medium    : return Tweaks8SizeScale.Medium;
                    case Enums.GenericSize.Small     :
                    case Enums.GenericSize.VerySmall : return Tweaks8SizeScale.Small;
                    case Enums.GenericSize.Tiny      : return Tweaks8SizeScale.VerySmall;
                    default: return Tweaks8SizeScale.NA;
                }
        }
    }
    public enum Tweaks8SizeScale
    {
        NA = -1,
        VerySmall,
        Small,
        SubMedium,
        Medium,
        SuperMedium,
        Large,
        VeryLarge,
        Jumbo
    }
    public enum Tweaks6SizeScale
    {
        NA = -1,
        Aplus,
        C,
        Cplus,
        D,
        E,
        F
    }
}
