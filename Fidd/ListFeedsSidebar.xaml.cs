using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fidd
{
    /// <summary>
    /// Interaction logic for SidebarFeedList.xaml
    /// </summary>
    public partial class ListFeedsSidebar : UserControl
    {
        Action<List<Feed.Post>, bool> _update_posts = null;
        public Action<List<Feed.Post>, bool> LoadPosts {
            get => _update_posts;
            set {
                _update_posts = value;
                UpdateFeedList();
                FilterClicked(FeedFilterAll, null);
            }
        }
        public ListFeedsSidebar()
        {
            InitializeComponent();
            UpdateFeedList();
        }
        private void ClearSelectedFeeds()
        {
            FeedFilterAll.Selected = false;
            FeedFilterUnread.Selected = false;
            foreach (ItemFeed feed_item in PanelSubscriptions.Children)
                feed_item.Selected = false;
        }
        private void UpdateFeedList()
        {
            PanelSubscriptions.Children.Clear();
            foreach (var feed in App.FeedManager.Feeds)
            {
                var feed_item = new ItemFeed()
                {
                    Title = feed.Title,
                    Unread = feed.Unread.Count
                };
                feed_item.Click += (s, e) =>
                {
                    ClearSelectedFeeds();
                    feed_item.Selected = true;
                    LoadPosts?.Invoke(feed.Posts, false);
                };
                PanelSubscriptions.Children.Add(feed_item);
            }
            FeedFilterUnread.Unread = App.FeedManager.Posts.Where(p => !p.Read).Count();
        }

        private void FilterClicked(object sender, RoutedEventArgs e)
        {
            ClearSelectedFeeds();
            if (sender == FeedFilterAll)
            {
                FeedFilterAll.Selected = true;
                LoadPosts?.Invoke(App.FeedManager.Posts, true);
            }
            else
            {
                FeedFilterUnread.Selected = true;
                LoadPosts?.Invoke(App.FeedManager.Posts.Where(p => !p.Read).ToList(), true);
            }
        }

        public void UpdateListWhilePreservingSelection()
        {
            var prev_filter = FeedFilterAll.Selected
                ? FeedFilterAll
                : (FeedFilterUnread.Selected ? FeedFilterUnread
                : null);
            var prev_feed = prev_filter is null
                ? (from item in PanelSubscriptions.Children.Cast<ItemFeed>() where item.Selected select item.Title).First()
                : null;

            UpdateFeedList();

            if (prev_filter != null)
                prev_filter.Selected = true;
            else
            {
                var prev_selected =
                    (from item in PanelSubscriptions.Children.Cast<ItemFeed>()
                     where item.Title == prev_feed
                     select item).First();
                prev_selected.Selected = true;
            }
        }
        
        private void AddFeedCommand(object sender, RoutedEventArgs e)
        {
            var add_feed_window = new WindowAddFeed();
            add_feed_window.ShowDialog();

            UpdateListWhilePreservingSelection();
        }
    }
}
