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
    /// Interaction logic for WindowUpdateFeeds.xaml
    /// </summary>
    public partial class WindowUpdateFeeds : Window
    {
        public WindowUpdateFeeds()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressFeedUpdate.Maximum = App.FeedManager.Feeds.Count;
            for (int i = 0; i < App.FeedManager.Feeds.Count; i++)
            {
                ProgressFeedUpdate.Value = i + 1;
                var feed = App.FeedManager.Feeds[i];
                TextStatus.Text = $"{i + 1}/{App.FeedManager.Feeds.Count} {feed.Title}";
                await App.FeedManager.FetchAndUpdateFeedAsync(feed, (n_posts) => {
                    TextStatus.Text += $": Adding {n_posts} new post{(n_posts == 1?"":"s")}";
                });
            }
            Close();
        }
    }
}
