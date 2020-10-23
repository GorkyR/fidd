using System;
using System.Collections.Generic;
using System.IO;
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

namespace Fidd
{
    /// <summary>
    /// Interaction logic for ItemFeed.xaml
    /// </summary>
    public partial class ItemFeed : UserControl
    {
        ImageSource _icon = null;
        public ImageSource Icon
        {
            get => _icon;
            set
            {
                if (value is null)
                    ImageIcon.Source = new BitmapImage(new Uri(@"/Icons/rss_feed.png", UriKind.Relative));
                else
                    ImageIcon.Source = value;
                _icon = value;
            }
        }
        public string Title {
            get => TextFeedName.Text;
            set { TextFeedName.Text = value; }
        }

        int _unread = 0;
        public int Unread
        {
            get => _unread;
            set
            {
                _unread = value;
                if (value > 0)
                {
                    TextUnread.Text = $"{value}";
                    BoxUnread.Visibility = Visibility.Visible;
                    TextUnread.Visibility = Visibility.Visible;
                }
                else
                {
                    BoxUnread.Visibility = Visibility.Collapsed;
                    TextUnread.Visibility = Visibility.Collapsed;
                }
            }
        }

        bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                Overlay.Opacity = value? 1 : 0;
                _selected = value;
            }
        }

        public Feed Feed { get; set; }

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
        "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemFeed));

        public event RoutedEventHandler Click { add => AddHandler(ClickEvent, value); remove => RemoveHandler(ClickEvent, value); }

        public ItemFeed()
        {
            InitializeComponent();
            Unread = 0;
        }

        public ItemFeed(Feed feed) : this()
        {
            Feed = feed;
            Title = feed.Title;
            Unread = feed.Unread.Count;
            if (!(feed.ImagePath is null))
                Icon = App.LoadImageFile(Path.Combine(App.FeedManager.FeedDirectory, feed.ImagePath));
            ToolTip = feed.Description;
        }

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

        private void PopUpEditFeedWindow(object sender, RoutedEventArgs e)
        {
            if (Feed != null)
            {
                new WindowEditFeed(Feed).ShowDialog();
                Title   = Feed.Title;
                if (Feed.ImagePath is null)
                    Icon = App.LoadImageResource("/Icons/rss_feed.png");
                else
                    Icon = App.LoadImageFile(Path.Combine(App.FeedManager.FeedDirectory, Feed.ImagePath));
                ToolTip = Feed.Description;
            }
        }

        private async void MarkFeedReadAsync(object sender, RoutedEventArgs e)
        {
            if (Feed != null)
                foreach (var post in Feed.Unread)
                    await App.FeedManager.MarkPostReadAsync(post);
        }

        private void UnsbscribeFromFeed(object sender, RoutedEventArgs e)
        {
            if (Feed != null)
            {
                var confirmation = MessageBox.Show(
                    $"Are you sure you want to unsubscribe from\n{Feed.Title} ({Feed.Link})?\n\nAll posts will be deleted and not all of them may be available in the future.",
                    $"Unsubscribe from \"{Feed.Title}\"",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirmation == MessageBoxResult.Yes)
                {
                    App.FeedManager.DeleteFeed(Feed);
                    // ↓↓↓ BAARFFFFF
                    (Application.Current.MainWindow as WindowMain).ListFeeds.UpdateListWhilePreservingSelection();
                }
            }
        }

        private void EndHover(object sender, MouseEventArgs e)
        {
            if (!Selected)
                Overlay.Opacity = 0;
        }
    }
}
