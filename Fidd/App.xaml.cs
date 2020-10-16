using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Fidd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static FeedManager Manager = new FeedManager(@"C:\Users\Admin\Documents\Feeds");
        public App()
        {
            if (Manager.Feeds.Count == 0)
            {
                try
                {
                    Manager.AddFeed("http://deathisbadblog.com/feed/");
                    Manager.AddFeed("https://tonsky.me/blog/atom.xml");
                    Manager.AddFeed("https://slatestarcodex.com/feed/");
                    Manager.AddFeed("https://blog.acolyer.org/feed/");
                } catch(Exception e) {
                    ;//NOOP
                }
            }
        }
    }
}
