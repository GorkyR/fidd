using System;

namespace FeedManagerTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var feed_manager = new FeedManager("Feeds");
            try { feed_manager.AddFeedAsync("tonsky.me").Wait(); } catch (Exception e) { ; }
            try { feed_manager.AddFeedAsync("devonzuegel.com").Wait(); } catch (Exception e) { ; }
            try { feed_manager.AddFeedAsync("slatestarcodex.com").Wait(); } catch (Exception e) { ; }
        }
    }
}
