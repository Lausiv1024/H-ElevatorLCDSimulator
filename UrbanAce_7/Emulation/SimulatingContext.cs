using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanAce_7
{
    public class SimulationContext
    {
        public string[] AvailableFloors { get; set; }
        public bool RoundTrip { get; set; }
        public bool Loop { get; set; }
        public int startPos { get; set; }
    }
}
