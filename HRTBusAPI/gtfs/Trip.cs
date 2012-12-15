
namespace HRTBusAPI.gtfs
{
    public class Trip
    {
        public string RouteId { get; set; }
        public string ServiceId { get; set; }
        public string TripId { get; set; }
        public string BlockId { get; set; }
        public string DirectionId { get; set; }

        public static Trip Create(string line, TripFieldIndices indices)
        {
            var parts = line.Split(',');
            return new Trip
                       {
                           RouteId = parts[indices.RouteId],
                           ServiceId = parts[indices.ServiceId],
                           TripId = parts[indices.TripId],
                           BlockId = parts[indices.BlockId],
                           DirectionId = parts[indices.DirectionId]
                       };
        }
    }
}