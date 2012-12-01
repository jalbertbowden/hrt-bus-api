
namespace HRTBusAPI.gtfs
{
    public class Trip
    {
        public string RouteId { get; set; }
        public string ServiceId { get; set; }
        public string TripId { get; set; }
        public string BlockId { get; set; }
        public string DirectionId { get; set; }

        public static Trip Create(string line)
        {
            var parts = line.Split(',');
            return new Trip
                       {
                           RouteId = parts[0],
                           ServiceId = parts[1],
                           TripId = parts[2],
                           BlockId = parts[3],
                           DirectionId = parts[4]
                       };
        }
    }
}