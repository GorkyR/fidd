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
        public bool DisableFadeOnRead { get; set; }

        List<Feed.Post> _posts = null;
        public List<Feed.Post> Posts
        {
            get => _posts;
            set
            {
                StackPosts.Children.Clear();
                foreach (var post in value)
                {
                    var item = new ItemPost(post, DisplayFeedTitle, DisableFadeOnRead);
                    item.Click += async (s, e) =>
                    {
                        ClearSelectedPosts();
                        item.Selected = true;
                        await OpenPost?.Invoke(item.Post);
                        item.Read = true;
                    };
                    StackPosts.Children.Add(item);
                }
                _posts = value;
            }
        }

        public Func<Feed.Post, Task> OpenPost { get; set; }
        public ListPosts()
        {
            InitializeComponent();
        }

        void ClearSelectedPosts()
        {
            foreach (var item in StackPosts.Children.Cast<ItemPost>())
                item.Selected = false;
        }
    }
}
