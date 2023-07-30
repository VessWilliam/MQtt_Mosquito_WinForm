using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZANTIMEDBupdater
{
    public class Location
    {
        public int zone { get; set; }
        public string location { get; set; }
        public string date  { get; set; }  
        public int Zone => zone;
        public string LocaTion => location;     
        public string Date => date;

        public Location(int zone, string location , string date) 
        {
            this.zone = zone;
            this.location = location;
            this.date = date;
        }
        public Location(int zone, string location )
        {
            this.zone = zone;
            this.location = location;
        }
        public Location(){  }
    }
}
