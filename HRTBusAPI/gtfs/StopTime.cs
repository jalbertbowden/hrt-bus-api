using System;

namespace HRTBusAPI.gtfs
{
    public class StopTime
    {
        public string TripId { get; set; }
        public TimeSpan Arrive { get; set; }
        public TimeSpan Depart { get; set; }
        public string StopId { get; set; }
        public int StopSequence { get; set; }
        public int Timepoint { get; set; }

        public static StopTime Create(string line, StopTimeFieldIndices indices)
        {
            var parts = line.Split(',');
            return new StopTime
                       {
                           TripId = parts[indices.TripId],
                           Arrive = TimeSpanFromString(parts[indices.ArrivalTime]),
                           Depart = TimeSpanFromString(parts[indices.DepartureTime]),
                           StopId = parts[indices.StopId],
                           StopSequence = int.Parse(parts[indices.StopSequence]),
                           Timepoint = int.Parse(parts[indices.Timepoint])
                       };
        }

        public static TimeSpan TimeSpanFromString(string time)
        {
            var parts = time.Split(':');
            return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }
    }
}