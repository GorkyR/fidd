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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        public FeedManager FeedManager = new FeedManager("Feeds");

        public WindowMain()
        {
            InitializeComponent();
            ListFeeds.LoadPosts = LoadPosts;
            ListPosts.OpenPost = OpenPost;
        }

        public void LoadPosts(List<Feed.Post> posts, bool include_feed = false)
        {
            ClosePost();
            ListPosts.DisplayFeedTitle = include_feed;
            ListPosts.Posts = posts;
        }

        public void ClosePost()
        {
            PostContent.Post = null;
        }
        public void OpenPost(Feed.Post post)
        {
            PostContent.Post = post;
            Title = $"Fidd — {post.ParentFeed.Title} — {post.Title}";
            App.Manager.MarkPostRead(post);
            ListFeeds.UpdateListWhilePreservingSelection();
        }
    }
}
