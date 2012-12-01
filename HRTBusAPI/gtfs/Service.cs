using System;
using System.Collections.Generic;

namespace HRTBusAPI.gtfs
{
    public class Service
    {
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<DayOfWeek> DaysActive { get; set; }

        public Service()
        {
            DaysActive = new List<DayOfWeek>();
        }

        public static Service Create(string line)
        {
            var parts = line.Split(',');
            var service = new Service
                              {
                                  Name = parts[0],
                                  Start = DateTime.ParseExact(parts[1], "yyyyMMdd", null),
                                  End = DateTime.ParseExact(parts[2], "yyyyMMdd", null)
                              };

            if (parts[3] == "1")
                service.DaysActive.Add(DayOfWeek.Monday);
            if (parts[4] == "1")
                service.DaysActive.Add(DayOfWeek.Tuesday);
            if (parts[5] == "1")
                service.DaysActive.Add(DayOfWeek.Wednesday);
            if (parts[6] == "1")
                service.DaysActive.Add(DayOfWeek.Thursday);
            if (parts[7] == "1")
                service.DaysActive.Add(DayOfWeek.Friday);
            if (parts[8] == "1")
                service.DaysActive.Add(DayOfWeek.Saturday);
            if (parts[9] == "1")
                service.DaysActive.Add(DayOfWeek.Sunday);

            return service;
        }

        public bool Active(DateTime date)
        {
            return date >= Start && date <= End && DaysActive.Contains(date.DayOfWeek);
        }
    }
}