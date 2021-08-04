using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for ItemPost.xaml
    /// </summary>
    public partial class ItemPost : UserControl
    {
        public string Title
        {
            get => TextTitle.Text;
            set { TextTitle.Text = value; }
        }
        public string Description
        {
            get => TextDescription.Text;
            set { TextDescription.Text = value; }
        }

        bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                Overlay.Opacity = value ? 1 : 0;
                _selected = value;
            }
        }

        DateTime date;
        public DateTime Published
        {
            get => date;
            set
            {
                TextDate.Text = (DateTime.Now - value).ToApproximateDeltaString();
                date = value;
            }
        }



        bool _read;
        public bool Read {
            get => _read;
            set
            {
                if (!DisableFadeOnRead)
                    GridContent.Opacity = value ? .5 : 1;
                MenuItemUnread.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                _read = value;
            }
        }

        bool _bookmarked;
        public bool Bookmarked
        {
            get => _bookmarked;
            set
            {
                IconBookmarked.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                MenuItemBookmark.Header = value ? "Unbookmark" : "Bookmark";
                _bookmarked = value;
            }
        }

        public bool DisableFadeOnRead { get; set; }

        public Feed.Post Post { get; set; }
        public string Feed
        {
            get => TextFeed.Text;
            set
            {
                if (value == null)
                {
                    TextFrom.Visibility = Visibility.Collapsed;
                    TextFeed.Visibility = Visibility.Collapsed;
                    TextFeed.Text = null;
                }
                else
                {
                    TextFrom.Visibility = Visibility.Visible;
                    TextFeed.Text = value;
                    TextFeed.Visibility = Visibility.Visible;
                }
            }
        }

        public ItemPost()
        {
            InitializeComponent();
            Feed = null;
        }
        public ItemPost(Feed.Post post, bool display_feed_title, bool disable_fade_on_read = false) : this()
        {
            DisableFadeOnRead = disable_fade_on_read;
            Post        = post;
            Title       = post.Title;
            Description = post.Description;
            Published   = post.DatePublished;
            Read        = post.Read;
            Bookmarked  = App.FeedManager.Bookmarks.Any(b => b.GUID == post.GUID);
            Feed        = display_feed_title? post.ParentFeed?.Title : null;
        }

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemPost));

        public event RoutedEventHandler Click { add => AddHandler(ClickEvent, value); remove => RemoveHandler(ClickEvent, value); }

        bool mouse_down_inside = false;
        private void RememberMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                mouse_down_inside = true;
        }

        private void CheckMouseState(object sender, MouseEventArgs e)
        {
            if (!Selected)
                Overlay.Opacity = 1d/3d;
            if (e.LeftButton == MouseButtonState.Released)
                mouse_down_inside = false;
        }

        private void CheckClick(object sender, MouseButtonEventArgs e)
        {
            if (mouse_down_inside)
            {
                DoClick();
                mouse_down_inside = false;
            }
        }

        public void DoClick()
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        private void EndHover(object sender, MouseEventArgs e)
        {
            if (!Selected)
                Overlay.Opacity = 0;
        }

        private async void MarkPostAsUnread(object sender, RoutedEventArgs e)
        {
            await App.FeedManager.MarkPostUnreadAsync(Post);
            Read = false;
            (Application.Current.MainWindow as WindowMain).ListFeeds.UpdateListWhilePreservingSelection();
        }

        private async void BookmarkPost(object sender, RoutedEventArgs e)
        {
            if (Bookmarked)
            {
                if (!App.FeedManager.Posts.Any(p => p.GUID == Post.GUID))
                {
                    var response = MessageBox.Show(
                        "This post is no longer in your feeds.\n"
                        + "(You may have unsubscribed from its parent feed at some point.)\n"
                        + "\n"
                        + "Removing this bookmark will delete its contents, and it may no longer be available through the original feed in the future.\n"
                        + "\n"
                        + "Do you want to remove this bookmark and potentially lose this data?",
                        "Remove bookmark? Potential loss of data", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (response != MessageBoxResult.Yes)
                        return;
                }
                if (Post.ParentFeed.ID == "bookmarks")
                {
                    try { (Parent as StackPanel).Children.Remove(this); } catch (Exception ex) { ; }
                }
                var bookmark = App.FeedManager.Bookmarks.First(b => b.GUID == Post.GUID);
                App.FeedManager.RemoveBookmark(bookmark);
                Bookmarked = false;
            }
            else
            {
                await App.FeedManager.BookmarkPostAsync(Post);
                Bookmarked = true;
            }
        }
    }

    static class TimeSpanExtensions
    {
        public static string ToApproximateDeltaString(this TimeSpan delta_time)
        {
            var days   = (double)delta_time.Days;
            var months = days / 30;
            var years  = days / 365;
            if (years >= 1)
            {
                int ys = (int)Math.Round(years);
                return $"~{ys:0} year{(ys == 1 ? "" : "s")}";
            }
            else if (months >= 2)
            {
                int mths = (int)Math.Round(months);
                return $"~{mths:0} month{(mths == 1 ? "" : "s")}";
            }
            else if (days >= 1)
                return $"{days} day{(days == 1 ? "" : "s")}";
            else if (delta_time.Hours >= 1)
                return $"{delta_time.Hours} hour{(delta_time.Hours == 1 ? "" : "s")}";
            else if (delta_time.Minutes >= 1)
                return $"{delta_time.Minutes} minute{(delta_time.Minutes == 1? "" : "s")}";
            else
                return "$~1 minute";
        }
    }
}
