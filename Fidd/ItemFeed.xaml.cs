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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fidd
{
    /// <summary>
    /// Interaction logic for ItemFeed.xaml
    /// </summary>
    public partial class ItemFeed : UserControl
    {
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

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
        "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemFeed));

        public event RoutedEventHandler Click { add => AddHandler(ClickEvent, value); remove => RemoveHandler(ClickEvent, value); }

        bool mouse_down_inside = false;
        public ItemFeed()
        {
            InitializeComponent();
            Unread = 0;
        }

        private void RememberMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                mouse_down_inside = true;
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

        private void CheckMouseState(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                mouse_down_inside = false;
        }
    }
}
