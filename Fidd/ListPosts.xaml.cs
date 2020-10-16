using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool DisplayFeedTitle { get; set; }

        List<Feed.Post> _posts = null;
        public List<Feed.Post> Posts
        {
            get => _posts;
            set
            {
                ListViewPosts.ItemsSource = from post in value select new ItemPost(post, DisplayFeedTitle);
                _posts = value;
            }
        }

        public Func<Feed.Post, Task> OpenPost { get; set; }
        public ListPosts()
        {
            InitializeComponent();
        }

        private async void SelectedPostChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewPosts.SelectedIndex != -1)
            {
                var selected_post = ListViewPosts.SelectedItem as ItemPost;
                await OpenPost?.Invoke(selected_post.Post);
                selected_post.Read = true;
            }
        }
    }
}
