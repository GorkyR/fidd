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
                    TextUnread.Visibility = Visibility.Visible;
                }
                else
                    TextUnread.Visibility = Visibility.Collapsed;
            }
        }
        public ItemFeed()
        {
            InitializeComponent();
        }
    }
}
