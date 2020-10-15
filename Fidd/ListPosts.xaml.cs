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
    /// Interaction logic for PostList.xaml
    /// </summary>
    public partial class ListPosts : UserControl
    {
        public bool IncludeFeed { get; set; }

        List<Feed.Post> _posts = null;
        public List<Feed.Post> Posts
        {
            get => _posts;
            set
            {
                ListViewPosts.ItemsSource =
                    (from post in value
                     select new ItemPost() {
                         Title = post.Title,
                         Description = post.Description,
                         Feed = IncludeFeed? post.ParentFeed.Title : null,
                         Post = post
                     }).ToList();
                _posts = value;
            }
        }

        public Action<Feed.Post> OpenPost { get; set; }
        public ListPosts()
        {
            InitializeComponent();
        }

        private void SelectedPostChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewPosts.SelectedIndex != -1)
                OpenPost?.Invoke((ListViewPosts.SelectedItem as ItemPost).Post);
        }
    }
}
