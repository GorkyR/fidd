using System;

namespace FeedManagerTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var feed_manager = new FeedManager(Environment.ExpandEnvironmentVariables("Feeds"));
            try { feed_manager.AddFeedAsync("tonsky.me").Wait(); } catch { }
            try { feed_manager.AddFeedAsync("deathisbadblog.com").Wait(); } catch { }
        }
    }
}
