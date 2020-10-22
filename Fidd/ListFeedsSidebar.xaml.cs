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
                FilterClicked(FeedFilterUnread, null);
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
            FeedFilterBookmarks.Selected = false;
            foreach (ItemFeed feed_item in PanelSubscriptions.Children)
                feed_item.Selected = false;
        }
        private void UpdateFeedList()
        {
            PanelSubscriptions.Children.Clear();
            foreach (var feed in App.FeedManager.Feeds)
            {
                var feed_item = new ItemFeed(feed);
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
            else if (sender == FeedFilterUnread)
            {
                FeedFilterUnread.Selected = true;
                LoadPosts?.Invoke(App.FeedManager.Posts.Where(p => !p.Read).ToList(), true);
            }
            else if (sender == FeedFilterBookmarks)
            {
                FeedFilterBookmarks.Selected = true;
                LoadPosts?.Invoke(
                    (from bookmark in App.FeedManager.Bookmarks
                    orderby bookmark.DateBookmarked descending
                    select App.FeedManager.MockPostFromBookmark(bookmark)).ToList(), true);
            }
        }

        public void UpdateListWhilePreservingSelection()
        {
            var prev_selected_feeds = (from item in PanelSubscriptions.Children.Cast<ItemFeed>() where item.Selected select item.Feed.ID);
            string prev_selected_feed = null;
            if (prev_selected_feeds.Count() > 0)
                prev_selected_feed = prev_selected_feeds.First();

            UpdateFeedList();

            if (prev_selected_feed != null)
            {
                var should_be_selected =
                    from item in PanelSubscriptions.Children.Cast<ItemFeed>()
                    where item.Feed.ID == prev_selected_feed
                    select item;
                if (should_be_selected.Count() == 0)
                    FilterClicked(FeedFilterUnread, null);
                else
                    should_be_selected.First().Selected = true;
            }
        }
        
        private void AddFeedCommand(object sender, RoutedEventArgs e)
        {
            var add_feed_window = new WindowAddFeed();
            add_feed_window.ShowDialog();

            UpdateListWhilePreservingSelection();
        }

        private void UpdateFeedsContent(object sender, RoutedEventArgs e)
        {
            (new WindowUpdateFeeds()).ShowDialog();
            UpdateListWhilePreservingSelection();
        }
    }
}
