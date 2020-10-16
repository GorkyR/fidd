using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using HtmlAgilityPack;


public class FeedManager
{
    public string FeedDirectory { get; set; }
    public List<Feed> Feeds { get; } = new List<Feed>();

    public List<Feed.Post> Posts => (from feed in Feeds from post in feed.Posts orderby post.Published descending select post).ToList();

    public FeedManager(string directory)
    {
        FeedDirectory = directory;
        if (Directory.Exists(directory))
        {
            foreach (var feed_file in Directory.GetFiles(directory, "*.feed"))
            {
                Feed feed;
                using (var file = new StreamReader(feed_file))
                    feed = Feed.Deserialize(Path.GetFileNameWithoutExtension(feed_file), file.ReadToEnd());

                foreach (var post_file in Directory.GetFiles(Path.Combine(directory, feed.ID), "*.post"))
                {
                    Feed.Post post;
                    using (var file = new StreamReader(post_file))
                        post = Feed.Post.Deserialize(feed, Path.GetFileNameWithoutExtension(post_file), file.ReadToEnd());
                    feed.Posts.Add(post);
                }

                feed.Posts.Sort((x, y) => y.Published.CompareTo(x.Published));

                Feeds.Add(feed);
            }
        }
        else
            Directory.CreateDirectory(directory);
    }

    public async Task AddFeedAsync(string feed_url)
    {
        var found_feed_urls = from url in await FeedReader.GetFeedUrlsFromUrlAsync(feed_url) select url.Url;
        if (found_feed_urls.Count() > 0)
        {
            var blog_url = found_feed_urls.First();
            feed_url = blog_url.StartsWith('/') ? $"{feed_url.TrimEnd('/')}{blog_url}" : blog_url;
        }

        if (Feeds.Any(f => f.FeedLink == feed_url))
        {
            throw new Exception("There's already a feed with that url.");
        }
        else
        { 
            var parsed_feed = await FeedReader.ReadAsync(feed_url);

            var feed_uri = new Uri(parsed_feed.Link);
            var feed_title = string.IsNullOrWhiteSpace(parsed_feed.Title) ? feed_uri.Host : parsed_feed.Title;
            var feed_id = AvoidFeedIdCollision(MakeSafeId(feed_title));

            string feed_image_path = await DownloadFeedImageAsync(feed_id, parsed_feed.ImageUrl);

            var feed = new Feed(feed_id, feed_url)
            {
                Title = feed_title,
                Link = $"{feed_uri.Scheme}://{feed_uri.Host}",
                ImagePath = feed_image_path,
                LastUpdated = parsed_feed.LastUpdatedDate is null ? DateTime.Now : (DateTime)parsed_feed.LastUpdatedDate,
                Description = ExtractTextFromHtml(parsed_feed.Description)
            };

            await WriteToFileAsync(Path.Combine(FeedDirectory, $"{feed_id}.feed"), feed.Serialize());

            Directory.CreateDirectory(Path.Combine(FeedDirectory, feed_id));

            await AddPostsToFeedAsync(feed, parsed_feed.Items);

            Feeds.Add(feed);
        }

        string AvoidFeedIdCollision(string id)
        {
            var pre_path = Path.Combine(FeedDirectory, id);
            if (Directory.Exists($"{pre_path}.feed"))
            {
                int suffix = 2;
                while (Directory.Exists($"{pre_path}_{suffix}.feed"))
                    suffix++;
                id = $"{id}_{suffix}";
            }
            return id;
        }

        async Task<string> DownloadFeedImageAsync(string feed_id, string feed_image_url)
        {
            string feed_image_path = null;
            if (!string.IsNullOrEmpty(feed_image_url))
            {
                var downloader = new WebClient();
                string image_extension = ExtractExtensionFromUrl(feed_image_url);
                feed_image_path = $"{feed_id}{image_extension}";
                await downloader.DownloadFileTaskAsync(feed_image_url, Path.Combine(FeedDirectory, feed_image_path));
            }

            return feed_image_path;
        }
    }
    public async Task FetchAndUpdateFeedAsync(Feed feed)
    {
        var parsed_feed = await FeedReader.ReadAsync(feed.FeedLink);
        var last_update = (DateTime)parsed_feed.LastUpdatedDate;
        if (last_update > feed.LastUpdated)
        {
            feed.LastUpdated = last_update;
            feed.Link = parsed_feed.Link;
            feed.Description = ExtractTextFromHtml(parsed_feed.Description);

            await AddPostsToFeedAsync(feed, parsed_feed.Items.Where(i => !feed.Posts.Any(p => p.GUID == i.Id)));
        }
    }
    public async Task MarkPostReadAsync(Feed.Post post)
    {
        post.Read = true;
        using (var post_file = new StreamWriter(Path.Combine(FeedDirectory, post.ParentFeed.ID, $"{post.ID}.post")))
            await post_file.WriteAsync(post.Serialize());
    }
    public async Task UpdateFeedMetadataAsync(Feed feed)
    {
        using (var feed_file = new StreamWriter(Path.Combine(FeedDirectory, $"{feed.ID}.feed")))
            await feed_file.WriteAsync(feed.Serialize());
    }
    public string LoadPostContent(Feed.Post post)
    {
        using (var content_file = new StreamReader(Path.Combine(FeedDirectory, post.ParentFeed.ID, $"{post.ID}.content")))
            return content_file.ReadToEnd();
    }
    async Task AddPostsToFeedAsync(Feed feed, IEnumerable<FeedItem> feed_items)
    {
        foreach (var item in feed_items)
        {
            var post_title = ExtractTextFromHtml(item.Title);
            var post_id    = MakeSafeId(post_title);

            var post = new Feed.Post(feed, post_id)
            {
                GUID = item.Id,
                Link = item.Link,
                Title = post_title,
                Published = (DateTime)item.PublishingDate,
                Description = ExtractTextFromHtml(item.Description),
            };

            var post_content_filename = Path.Combine(FeedDirectory, feed.ID, post_id);

            var post_content = (await EmbedImagesInHtmlAsync(item.Content is null ? item.Description : item.Content, post)).Trim();

            await WriteToFileAsync($"{post_content_filename}.post", post.Serialize());
            await WriteToFileAsync($"{post_content_filename}.content", post_content.Trim());

            feed.Posts.Add(post);
        }
    }

    static string MakeSafeId(string path)
    {
        char[] invalid_chars = Path.GetInvalidFileNameChars();
        foreach (char ch in invalid_chars)
            path = path.Replace(ch, '_');
        return path.Replace(' ', '_').Replace('.', '_').ToLower().Substring(0, path.Length > 60? 60 : path.Length);
    }
    static string ExtractTextFromHtml(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        else
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(text);
            text = doc.DocumentNode.InnerText;
            text = System.Web.HttpUtility.HtmlDecode(text);
            return text.Trim();
        }
    }
    static async Task WriteToFileAsync(string filepath, string content)
    {
        using (var file = new StreamWriter(filepath))
            await file.WriteAsync(content);
    }
    static string ExtractExtensionFromUrl(string url)
    {
        return string.Join("", Path.GetExtension(url).TakeWhile(c => c != '?'));
    }
    static async Task<string> EmbedImagesInHtmlAsync(string content, Feed.Post post)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        var images = (from node in doc.DocumentNode.Descendants()
                     where string.Equals(node.Name, "img", StringComparison.OrdinalIgnoreCase)
                     select node).ToList();

        var downloader = new WebClient();

        foreach (var image in images)
        {
            var src = image.GetAttributeValue("src", "");
            if (!src.StartsWith("data"))
            {
                try
                {
                    var image_extension = ExtractExtensionFromUrl(src).Substring(1);
                    string image_data_base64;
                    try { image_data_base64 = Convert.ToBase64String(await downloader.DownloadDataTaskAsync(src)); }
                    catch {
                        src = src.StartsWith('/') ? $"{post.ParentFeed.Link}{src}" : $"{post.Link.TrimEnd('/')}/{src}";
                        image_data_base64 = Convert.ToBase64String(await downloader.DownloadDataTaskAsync(src));
                    }
                    image.SetAttributeValue("src", $"data:image/{image_extension};base64,{image_data_base64}");
                }
                catch { image.Remove(); }
            }
        }

        return doc.DocumentNode.InnerHtml;
    }
}