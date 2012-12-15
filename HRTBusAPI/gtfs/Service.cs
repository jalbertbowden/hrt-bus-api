using System;
using System.Collections.Generic;

namespace HRTBusAPI.gtfs
{
    public class Service
    {
        public string ServiceId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<DayOfWeek> DaysActive { get; set; }

        public Service()
        {
            DaysActive = new List<DayOfWeek>();
        }

        public static Service Create(string line, ServiceFieldIndices indices)
        {
            var parts = line.Split(',');
            var service = new Service
                              {
                                  ServiceId = parts[indices.ServiceId],
                                  Start = DateTime.ParseExact(parts[indices.StartDate], "yyyyMMdd", null),
                                  End = DateTime.ParseExact(parts[indices.EndDate], "yyyyMMdd", null)
                              };

            if (parts[indices.Monday] == "1")
                service.DaysActive.Add(DayOfWeek.Monday);
            if (parts[indices.Tuesday] == "1")
                service.DaysActive.Add(DayOfWeek.Tuesday);
            if (parts[indices.Wednesday] == "1")
                service.DaysActive.Add(DayOfWeek.Wednesday);
            if (parts[indices.Thursday] == "1")
                service.DaysActive.Add(DayOfWeek.Thursday);
            if (parts[indices.Friday] == "1")
                service.DaysActive.Add(DayOfWeek.Friday);
            if (parts[indices.Saturday] == "1")
                service.DaysActive.Add(DayOfWeek.Saturday);
            if (parts[indices.Sunday] == "1")
                service.DaysActive.Add(DayOfWeek.Sunday);

            return service;
        }

        public bool Active(DateTime date)
        {
            return date >= Start && date <= End && DaysActive.Contains(date.DayOfWeek);
        }
    }
}