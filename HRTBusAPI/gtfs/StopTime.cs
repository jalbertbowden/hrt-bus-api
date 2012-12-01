using System;

namespace HRTBusAPI.gtfs
{
    public class StopTime
    {
        public string TripId { get; set; }
        public TimeSpan Arrive { get; set; }
        public TimeSpan Depart { get; set; }
        public string StopId { get; set; }
        public byte StopSequence { get; set; }

        public static StopTime Create(string line)
        {
            var parts = line.Split(',');
            return new StopTime
                       {
                           TripId = parts[0],
                           Arrive = TimeSpanFromString(parts[1]),
                           Depart = TimeSpanFromString(parts[2]),
                           StopId = parts[3],
                           StopSequence = byte.Parse(parts[4])
                       };
        }

        public static TimeSpan TimeSpanFromString(string time)
        {
            var parts = time.Split(':');
            return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }
    }
}