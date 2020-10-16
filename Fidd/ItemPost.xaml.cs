using System;
using System.Collections.Generic;
using System.Globalization;
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
        public string Author
        {
            get => TextAuthor.Text;
            set
            {
                if (value is null)
                {
                    TextBy.Visibility = Visibility.Collapsed;
                    TextAuthor.Visibility = Visibility.Collapsed;
                    TextAuthor.Text = null;
                }
                else
                {
                    TextBy.Visibility = Visibility.Visible;
                    TextAuthor.Text = value;
                    TextAuthor.Visibility = Visibility.Visible;
                }
            }
        }

        CultureInfo us_format = new CultureInfo("en-US");

        DateTime date;
        public DateTime Published
        {
            get => date;
            set
            {
                TextDate.Text = value.ToString("d MMM yyyy, h:mmtt", us_format);
                date = value;
            }
        }

        bool _read;
        public bool Read {
            get => _read;
            set
            {
                Fade.Visibility = value? Visibility.Visible : Visibility.Hidden;
                _read = value;
            }
        }

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
            Author = null;
        }
        public ItemPost(Feed.Post post, bool display_feed_title) : this()
        {
            Post        = post;
            Title       = post.Title;
            Description = post.Description;
            Published   = post.Published;
            Read        = post.Read;
            Feed        = display_feed_title? post.ParentFeed?.Title : null;
        }
    }
}
