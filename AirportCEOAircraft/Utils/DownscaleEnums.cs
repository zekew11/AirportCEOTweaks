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
            Original,
            Downscale2X,
            Downscale4X,
            [Description("Downscale8X - NOT RECOMMENDED")]
            Downscale8X
        }
    }
}
