using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPS_Location
{
   public class LocationInfo
    {
       public Guid Id { get; set; }
       public double Longitude { get; set; }
       public double Latitude { get; set; }
       public string County { get; set; }

    }
}
