using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fidd
{
    /// <summary>
    /// Interaction logic for WindowEditFeed.xaml
    /// </summary>
    public partial class WindowEditFeed : Window
    {
        public Feed Feed { get; set; }

        public WindowEditFeed()
        {
            InitializeComponent();
            DataContext = this;
        }

        public WindowEditFeed(Feed feed) : this() { Feed = feed; }

        private async void DoUpdate(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Feed.ImagePath))
                Feed.ImagePath = null;
            await App.FeedManager.UpdateFeedMetadataAsync(Feed);
            Close();
        }
    }
}
