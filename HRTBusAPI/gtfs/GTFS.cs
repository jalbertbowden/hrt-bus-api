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

        private readonly List<Service> _services;
        private readonly List<Trip> _trips;
        private readonly List<StopTime> _stopTimes;

        public static List<Service> Services { get { return Instance._services; } }
        public static List<Trip> Trips { get { return Instance._trips; } }
        public static List<StopTime> StopTimes { get { return Instance._stopTimes; } }

        private GTFS()
        {
            _services = new List<Service>();
            _trips = new List<Trip>();
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
            return Services.Where(s => s.Active(date)).ToList();
        }
    }
}