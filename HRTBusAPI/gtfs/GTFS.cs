using System;
using System.Collections.Generic;
using System.Linq;

namespace HRTBusAPI.gtfs
{
    public class GTFS
    {
        private static GTFS _singleton;
        private static GTFS Instance
        {
            get { return _singleton ?? (_singleton = new GTFS()); }
        }

        private readonly Dictionary<string, Service> _services;
        private readonly Dictionary<string, Trip> _trips;
        private readonly List<StopTime> _stopTimes;

        public static Dictionary<string, Service> Services { get { return Instance._services; } }
        public static Dictionary<string, Trip> Trips { get { return Instance._trips; } }
        public static List<StopTime> StopTimes { get { return Instance._stopTimes; } }

        private GTFS()
        {
            _services = new Dictionary<string, Service>();
            _trips = new Dictionary<string, Trip>();
            _stopTimes = new List<StopTime>();
        }

        public static void Clear()
        {
            Instance._services.Clear();
            Instance._trips.Clear();
            Instance._stopTimes.Clear();
        }

        public static List<Service> GetActiveServices(DateTime date)
        {
            return Services.Values.Where(s => s.Active(date)).ToList();
        }
    }
}