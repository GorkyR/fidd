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
        List<Feed> _feeds = null;
        public List<Feed> Feeds
        {
            get => _feeds;
            set {
                ListSubscriptions.ItemsSource =
                    (from feed in value
                     select new ItemFeed() {
                         Title = feed.Title,
                         Unread = feed.Unread.Count
                     }).ToList();
                _feeds = value;
            }
        }
        public Action<List<Feed.Post>, bool> UpdatePosts { get; set; }
        public ListFeedsSidebar()
        {
            InitializeComponent();
        }

        private void FilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListFilters.SelectedIndex != -1)
            {
                if (!(ListSubscriptions is null))
                    ListSubscriptions.SelectedIndex = -1;
                if (UpdatePosts != null)
                {
                    var index = ListFilters.SelectedIndex;
                    if (index == 0)
                        UpdatePosts(App.Manager.Posts.OrderByDescending(p => p.Published).ToList(), true);
                    else if (index == 1)
                        UpdatePosts((from post in App.Manager.Posts where !post.Read select post).ToList(), true);
                }
            }
        }

        private void FeedSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListSubscriptions.SelectedIndex != -1)
            {
                ListFilters.SelectedIndex = -1;
                if (UpdatePosts != null)
                {
                    var index = ListSubscriptions.SelectedIndex;
                    UpdatePosts(Feeds[index].Posts, false);
                }
            }
        }
    }
}
