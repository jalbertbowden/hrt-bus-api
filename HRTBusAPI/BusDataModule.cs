﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using HRTBusAPI.API;
using HRTBusAPI.gtfs;
using Nancy;

namespace HRTBusAPI
{
    public class BusDataModule : NancyModule
    {
        private static readonly List<BusCheckin> Checkins = new List<BusCheckin>();

        public BusDataModule()
        {
            Get["/example/ui"] = parameters => View["busfinder.html", null];

            Get["/refresh"] = parameters => RefreshBusData();

            Get["/refresh/GTFS"] = parameters => RefreshGtfsData();

            Get["/refresh/GTFS/today"] = parameters => RefreshGtfsData(true);

            Get["/api/routes"] =
                parameters =>
                    {
                        var routes = Checkins.Where(c => c.HasRoute).Select(c => c.Route);
                        var result = routes.Distinct().ToList();
                        result.Sort();
                        return Response.AsJson(result);
                    };

            Get["/api/buses"] =
                parameters =>
                    {
                        var checkins = Checkins;
                        if (Request.Query.route)
                            checkins = Checkins.Where(c => c.HasRoute).ToList();
                        if (Request.Query.route != null && !Request.Query.route)
                            checkins = Checkins.Where(c => c.HasRoute).ToList();

                        var busIds = checkins.Select(c => c.BusId);
                        var result = busIds.Distinct().ToList();
                        result.Sort();
                        return Response.AsJson(result);
                    };

            Get["/api/route/{route}"] =
                parameters =>
                    {
                        var checkins = Checkins.FindAll(c => c.Route == parameters.route);
                        if ((int)parameters.route == 0)
                            checkins = Checkins.FindAll(c => c.HasRoute == false);

                        var result = new RouteModel { route = parameters.route };
                        foreach (var checkin in checkins.Where(checkin => !result.buses.Exists(b=>b.id == checkin.BusId)))
                        {
                            result.buses.Add(new BusCheckinModel(checkin));
                        }
                        result.buses.Sort((x, y) => x.id.CompareTo(y.id));
                        return Response.AsJson(result);
                    };

            Get["/api/bus/{id}"] =
                parameters =>
                {
                    var checkins = Checkins.FindAll(c => c.BusId == parameters.id);
                    var result = checkins.Select(checkin => new BusCheckinModel(checkin)).ToList();
                    return Response.AsJson(result);
                };

            Get["/api/nextbus/{route}/{stop}"] =
                parameters =>
                    {
                        var route = (string)parameters.route;
                        var stop = (string)parameters.stop;
                        var now = DateTime.Now;
                        var today = DateTime.Today;

                        while (route.Length < 3)
                            route = "0" + route;

                        var activeServices = GTFS.GetActiveServices(now).Select(s=>s.ServiceId);

                        var trips = GTFS.Trips
                                        .Where(t =>
                                               t.Value.RouteId == route &&
                                               activeServices.Contains(t.Value.ServiceId))
                                        .Select(t => t.Value.TripId);

                        var stopTimes = GTFS.StopTimes
                                            .Where(st =>
                                                   st.StopId == stop &&
                                                   trips.Contains(st.TripId) &&
                                                   today.Add(st.Arrive) > now)
                                            .Select(st => today.Add(st.Arrive))
                                            .ToList();

                        stopTimes.Sort((x, y) => x.CompareTo(y));

                        var list = new List<dynamic>();
                        
                        foreach (var dateTime in stopTimes)
                        {
                            dynamic o = new ExpandoObject();
                            o.stopTime = dateTime;
                            o.stopTimeStr = dateTime.ToString();
                            list.Add(o);
                        }
                        return Response.AsJson(list);
                    };
        }

        private static string RefreshGtfsData(bool onlyToday = false)
        {
            DateTime? date = null;
            if (onlyToday)
                date = DateTime.UtcNow.AddHours(-5);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            GtfsPackageManager.Refresh(date);
            stopwatch.Stop();
            return string.Format("Loaded {0} Services, {1} Trips, and {2} Stop Times in {3}.",
                                 GTFS.Services.Count,
                                 GTFS.Trips.Count,
                                 GTFS.StopTimes.Count,
                                 stopwatch.Elapsed);
        }

        private static string RefreshBusData()
        {
                var contents = GetFileFromServer(new Uri("ftp://216.54.15.3/Anrd/hrtrtf.txt"));
                var newCheckins = GetBusCheckinsFromFile(contents);   // oldest first

                var duplicates = 0;
                var withoutRoute = 0;
                var routeLookedup = 0;

                foreach (var checkin in newCheckins)
                {
                    if(Checkins.Exists(c=>c.CheckinTime == checkin.CheckinTime && c.BusId == checkin.BusId))
                    {
                        duplicates++;
                    }
                    else
                    {
                        if(!checkin.HasRoute)
                        {
                            withoutRoute++;
                            var oldCheckinWithRoute = Checkins.FirstOrDefault(c => c.HasRoute && c.BusId == checkin.BusId);
                            if(oldCheckinWithRoute != null)
                            {
                                routeLookedup++;
                                checkin.HasRoute = true;
                                checkin.RouteLookedUp = true;
                                checkin.Route = oldCheckinWithRoute.Route;
                                checkin.Direction = oldCheckinWithRoute.Direction;
                            }
                        }
                        Checkins.Insert(0, checkin); // newest first
                    }
                }

                var removed = Checkins.RemoveAll(c => c.CheckinTime < DateTime.UtcNow.AddHours(-5).AddHours(-1));
                var percentWithRoute = Checkins.FindAll(c => c.HasRoute).Count*100.0/Checkins.Count;

            return string.Format("{0} checkins in FTP file.<br>" +
                                 "{1} were new.<br>" +
                                 "{2} new checkins didn't have a route. Routes were found for {3} of those.<br>" +
                                 "{4} were removed because they were more than an hour old.<br>" +
                                 "{5} checkins now in memory.<br>" +
                                 "{6}% have a route.<br>" +
                                 "{7} auto refreshes.<br>" +
                                 "Last auto-refresh at {8}",
                                 newCheckins.Count,
                                 newCheckins.Count - duplicates,
                                 withoutRoute,
                                 routeLookedup,
                                 removed,
                                 Checkins.Count,
                                 (int) percentWithRoute,
                                 Global.RefreshCount,
                                 Global.LastRefresh);
        }

        public static string GetFileFromServer(Uri serverUri)
        {
            var newFileData = new WebClient().DownloadData(serverUri.ToString());
            return System.Text.Encoding.UTF8.GetString(newFileData);
        }

        public static List<BusCheckin> GetBusCheckinsFromFile(string file)
        {
            if (String.IsNullOrEmpty(file))
                return new List<BusCheckin>();

            return file.Split('\n', '\r')
                .Select(BusCheckin.Parse)
                .Where(checkin => checkin != null)
                .ToList();
        }
    }
}