using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZANTIMEDBupdater
{
    public class GetTImeGMT
    {
        public DateTime dateGmt7 { get; set;}

        public DateTime dateGmt9 { get; set; }


        public DateTime IndoWestGMT7(DateTime wholeDateTime)
        {
            TimeZoneInfo IndoWestZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime IndoWestTimeNow = TimeZoneInfo.ConvertTime(wholeDateTime, TimeZoneInfo.Local, IndoWestZone);
            //Console.WriteLine(IndoWestTimeNow.ToString());       
            return  dateGmt7 = IndoWestTimeNow;
        }

        public DateTime indoeastGMT9(DateTime wholeDateTime)
        {
            TimeZoneInfo IndoEastZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTime IndoEastTimeNow = TimeZoneInfo.ConvertTime(wholeDateTime, TimeZoneInfo.Local, IndoEastZone);
           //Console.WriteLine(IndoEastTimeNow.ToString());
            return dateGmt9 = IndoEastTimeNow ;
        }
    }
}
