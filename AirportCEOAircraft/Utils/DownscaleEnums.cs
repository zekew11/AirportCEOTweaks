using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportCEOAircraft
{
    public class DownscaleEnums
    {
        public enum DownscaleLevel
        {
            [Description("Full Quality")]
            Original,
            [Description("Downscale2X - Recommended")]
            Downscale2X,
            [Description("Downscale4X - Aggressive")]
            Downscale4X,
            [Description("Downscale8X - Not Recommended")]
            Downscale8X
        }
    }
}
