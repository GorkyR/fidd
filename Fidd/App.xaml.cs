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
        public static FeedManager FeedManager = new FeedManager(
            Environment.ExpandEnvironmentVariables(@"%userprofile%\Documents\Feeds")
        );
        public App() { }
    }
}
