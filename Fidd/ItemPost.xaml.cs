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
        DateTime date;
        public DateTime Published
        {
            get => date;
            set
            {
                TextDate.Text = value.ToShortDateString();
                date = value;
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
    }
}
