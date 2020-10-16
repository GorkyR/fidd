using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata;
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
using MarkdownTransformer = Markdown.Xaml.Markdown;

namespace Fidd
{
    /// <summary>
    /// Interaction logic for PostContent.xaml
    /// </summary>
    public partial class PostContent : UserControl
    {
        public double PostMaxWidth { get; set; } = 800;
        public double PostMinPadding { get; set; } = 24;

        MarkdownTransformer markdown = new MarkdownTransformer();

        Feed.Post _post = null;
        public Feed.Post Post
        {
            get => _post;
            set
            {
                if (value != null)
                {
                    var content = App.Manager.LoadPostContent(value).Replace("## ", "### ").Replace("# ", "## ");
                    markdown.AssetPathRoot = value.Link;
                    markdown.HyperlinkCommand = new HyperlinkHandler(value);
                    var document = markdown.Transform($"# [{value.Title}]({value.Link})\n{content}");
                    document.PagePadding = PaddingFromWidth(FlowContent.RenderSize.Width);
                    FlowContent.Document = document;
                }
                _post = value;
            }
        }

        public PostContent()
        {
            InitializeComponent();
            markdown.DocumentStyle  = Resources["DocumentStyle"] as Style;
            markdown.Heading1Style  = Resources["H1Style"] as Style;
            markdown.Heading2Style  = Resources["H2Style"] as Style;
            markdown.Heading3Style  = Resources["H3Style"] as Style;
            markdown.Heading4Style  = Resources["H4Style"] as Style;
            markdown.LinkStyle      = Resources["LinkStyle"] as Style;
            markdown.ImageStyle     = Resources["ImageStyle"] as Style;
            markdown.SeparatorStyle = Resources["SeparatorStyle"] as Style;
            markdown.CodeStyle      = Resources["CodeStyle"] as Style;
        }

        private void LinkClicked(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                e.Cancel = true;
                var url = e.Uri.ToString();
                Process.Start("explorer", url);
            }
        }

        Thickness PaddingFromWidth(double width)
        {
            var horizontal_padding = (width - PostMaxWidth) / 2;
            horizontal_padding = horizontal_padding > PostMinPadding
                ? horizontal_padding
                : PostMinPadding;
            return new Thickness(
                left : horizontal_padding,
                right: horizontal_padding,
                top   : PostMinPadding,
                bottom: PostMinPadding
            );
        }
        private void PostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (FlowContent != null)
                if (FlowContent.Document != null)
                    FlowContent.Document.PagePadding = PaddingFromWidth(e.NewSize.Width);
        }
    }

    class HyperlinkHandler : ICommand
    {
        public string Root { get; set; }
        public event EventHandler CanExecuteChanged;

        public HyperlinkHandler(Feed.Post post)
        {
            var feed_uri = new Uri(post.ParentFeed.Link);
            Root = $"{feed_uri.Scheme}://{feed_uri.Host}";
        }
        public bool CanExecute(object parameter) => parameter is null? true : (parameter is string);
        public void Execute(object parameter)
        {
            var url = parameter as string;
            if (url.StartsWith("/"))
                url = $"{Root}{url}";
            Process.Start("explorer", url);
        }
    }
}
