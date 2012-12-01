using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Ionic.Zip;
using Newtonsoft.Json.Linq;

namespace HRTBusAPI.gtfs
{
    public static class GtfsPackageManager
    {
        private const string GTFS_PACKAGE_INFO_URL = "http://www.gtfs-data-exchange.com/api/agency?agency=hampton-roads-transit-hrt";
        public static void Refresh(DateTime? date)
        {
            DownloadData();

            var calendarPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           "calendar.txt");
            using(var file = File.OpenText(calendarPath))
            {
                GTFS.Services.Clear();
                file.ReadLine();
                while (!file.EndOfStream)
                {
                    var service = Service.Create(file.ReadLine());
                    if (date == null || service.Active((DateTime)date))
                        GTFS.Services.Add(service);
                }
            }

            var tripsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           "trips.txt");
            using (var file = File.OpenText(tripsPath))
            {
                GTFS.Trips.Clear();
                var services = GTFS.Services.Select(s => s.Name).Distinct().ToList();
                file.ReadLine();
                while (!file.EndOfStream)
                {
                    var trip = Trip.Create(file.ReadLine());
                    if (date == null || services.Contains(trip.ServiceId))
                        GTFS.Trips.Add(trip);
                }
            }

            var stopTimesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           "stop_times.txt");
            using (var file = File.OpenText(stopTimesPath))
            {
                GTFS.StopTimes.Clear();
                var trips = GTFS.Trips.Select(t => t.TripId).Distinct().ToList();
                file.ReadLine();
                while (!file.EndOfStream)
                {
                    var stopTime = StopTime.Create(file.ReadLine());
                    if (date == null || trips.Contains(stopTime.TripId))
                        GTFS.StopTimes.Add(stopTime);
                }
            }
        }

        private static void DownloadData()
        {
            var webClient = new WebClient();

            dynamic gtfsPackageInfo = JObject.Parse(webClient.DownloadString(GTFS_PACKAGE_INFO_URL));
            var gtfsPackageUrl = (string) gtfsPackageInfo.data.datafiles[0].file_url;
            var gtfsPackageMD5Sum = (string)gtfsPackageInfo.data.datafiles[0].md5sum;

            var gtfsZipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           "gtfs.zip");

            webClient.DownloadFile(gtfsPackageUrl, gtfsZipPath);

            using (var gtfsFileStream = new FileStream(gtfsZipPath, FileMode.Open))
            {
                var hash = new MD5CryptoServiceProvider().ComputeHash(gtfsFileStream);
                var computedMD5Sum = BitConverter.ToString(hash).Replace("-", "");
                if (String.Compare(computedMD5Sum, gtfsPackageMD5Sum, StringComparison.OrdinalIgnoreCase) != 0)
                    throw new ApplicationException("Failed to download GTFS package from " + gtfsPackageUrl +
                                                   " - MD5 Sums did not match.");
            }

            var filesToExtract = new List<string> {"calendar.txt", "trips.txt", "stop_times.txt"};

            using (var zip = ZipFile.Read(gtfsZipPath))
            {
                foreach (var file in zip.Where(file => filesToExtract.Contains(file.FileName)))
                {
                    file.Extract(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }
    }
}