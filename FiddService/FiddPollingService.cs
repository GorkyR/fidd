using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;

namespace FiddService
{
    class FiddPollingService : ServiceControl
    {
        public Timer PollingTimer { get; set; }
        public List<string> FiddFolders { get; set; }
        public FiddPollingService()
        {
            FiddFolders = Directory.GetDirectories(@"C:\Users\")
                .Select(user_folder => Path.Combine(user_folder, "AppData", "Local", "Fidd"))
                .Where(path => Directory.Exists(path)).ToList();
        }

        bool ServiceControl.Start(HostControl hostControl)
        {
            PollingTimer = new Timer(15.Minutes());
            PollingTimer.Elapsed += async (s, e) => await PollAndUpdateFeeds();
            PollingTimer.Start();

            PollAndUpdateFeeds().Wait();

            return true;
        }

        private async Task PollAndUpdateFeeds()
        {
            foreach (var feeds_path in FiddFolders)
            {
                var manager = new FeedManager(feeds_path);
                foreach (var feed in manager.Feeds)
                {
                    try { await manager.FetchAndUpdateFeedAsync(feed); }
                    catch (Exception e)
                    {
                        await File.AppendAllTextAsync(
                            Path.Combine(feeds_path, "poller_error.log"),
                            $"[{DateTime.Now} - {feed.ID}]: {e}\n"
                        );
                    }
                }

                await File.WriteAllTextAsync(
                    Path.Combine(feeds_path, "poller_last_updated.log"),
                    DateTime.Now.ToString()
                );
            }
        }

        bool ServiceControl.Stop(HostControl hostControl) => true;
    }

    static class MillisecondExtensions
    {
        public static int    Hours(this int hours)        => hours * 3600 * 1000;
        public static double Hours(this double hours)     => hours * 3600 * 1000;
        public static int    Minutes(this int minutes)    => minutes * 60 * 1000;
        public static double Minutes(this double minutes) => minutes * 60 * 1000;
        public static int    Seconds(this int seconds)    => seconds * 1000;
        public static double Seconds(this double seconds) => seconds * 1000;
    }
}
