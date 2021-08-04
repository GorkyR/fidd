using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using Newtonsoft.Json;

public class FeedManager
{
    public string FeedDirectory { get; set; }
    public List<Feed> Feeds { get; } = new List<Feed>();
    public List<Feed.Post> Posts => (from feed in Feeds from post in feed.Posts orderby post.DatePublished descending select post).ToList();
    public List<Bookmark> Bookmarks { get; } = new List<Bookmark>();

    public FeedManager(string directory)
    {
        FeedDirectory = directory;
        LoadFeedsFromStorage();
    }

    public void LoadFeedsFromStorage()
    {
        Feeds.Clear();
        Bookmarks.Clear();
        if (Directory.Exists(FeedDirectory))
        {
            foreach (var feed_file in Directory.GetFiles(FeedDirectory, "*.feed"))
            {
                Feed feed;
                using (var file = new StreamReader(feed_file))
                    feed = Feed.Deserialize(Path.GetFileNameWithoutExtension(feed_file), file.ReadToEnd());

                foreach (var post_file in Directory.GetFiles(Path.Combine(FeedDirectory, feed.ID), "*.post"))
                {
                    Feed.Post post;
                    using (var file = new StreamReader(post_file))
                        post = Feed.Post.Deserialize(feed, Path.GetFileNameWithoutExtension(post_file), file.ReadToEnd());
                    feed.Posts.Add(post);
                }

                feed.Posts.Sort((x, y) => y.DatePublished.CompareTo(x.DatePublished));

                Feeds.Add(feed);
            }
            foreach (var bookmark_file in Directory.GetFiles(Path.Combine(FeedDirectory, "bookmarks"), "*.bookmark"))
            {
                Bookmark bookmark;
                using (var file = new StreamReader(bookmark_file))
                    bookmark = Bookmark.Deserialize(Path.GetFileNameWithoutExtension(bookmark_file), file.ReadToEnd());
                Bookmarks.Add(bookmark);
            }
        }
        else Directory.CreateDirectory(Path.Combine(FeedDirectory, "bookmarks"));
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

            string feed_image_path = await DownloadFeedImageAsync(feed_id, parsed_feed.ImageUrl, feed_uri.Host);

            var feed = new Feed(feed_id, feed_url)
            {
                Title = feed_title,
                Link = $"{feed_uri.Scheme}://{feed_uri.Host}",
                ImagePath = feed_image_path,
                DateLastUpdated = parsed_feed.LastUpdatedDate is null ? DateTime.Now : (DateTime)parsed_feed.LastUpdatedDate,
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

        async Task<string> DownloadFeedImageAsync(string feed_id, string feed_image_url, string feed_host)
        {
            string feed_image_path = null;
            if (string.IsNullOrEmpty(feed_image_url))
            {
                try
                {
                    var web_client = new WebClient();
                    web_client.Headers.Add(HttpRequestHeader.UserAgent, "Fidd RSS Reader v1.0");
                    var favicon_api_response = await web_client.DownloadStringTaskAsync($"http://favicongrabber.com/api/grab/{feed_host}");
                    var deserialized_response = JsonConvert.DeserializeObject<FaviconGrabberResponse>(favicon_api_response);
                    if (deserialized_response.Icons.Count > 0)
                        feed_image_url = deserialized_response.Icons.First().Source;
                } catch(Exception e) { ; }
            }
            if (!string.IsNullOrEmpty(feed_image_url))
            {
                if (feed_image_url.StartsWith("data:image/"))
                {
                    feed_image_url = feed_image_url.Substring(11);
                    var image_extension = feed_image_url.Substring(0, feed_image_url.IndexOf(';'));
                    feed_image_path = $"{feed_id}.{image_extension}";
                    var image_data_base64 = feed_image_url.Substring(feed_image_url.IndexOf(',') + 1);
                    await File.WriteAllBytesAsync(Path.Combine(FeedDirectory, feed_image_path), Convert.FromBase64String(image_data_base64));
                }
                else
                {
                    var downloader = new WebClient();
                    string image_extension = ExtractExtensionFromUrl(feed_image_url);
                    feed_image_path = $"{feed_id}{image_extension}";
                    await downloader.DownloadFileTaskAsync(feed_image_url, Path.Combine(FeedDirectory, feed_image_path));
                }
            }

            return feed_image_path;
        }
    }
    public void DeleteFeed(Feed feed)
    {
        if (Feeds.Contains(feed))
        {
            var feed_path = Path.Combine(FeedDirectory, feed.ID);
            File.Delete($"{feed_path}.feed");
            Directory.Delete(feed_path, true);
            Feeds.Remove(feed);
        }
    }
    public async Task FetchAndUpdateFeedAsync(Feed feed)
    {
        await FetchAndUpdateFeedAsync(feed, (x) => { }); 
    }
    public async Task FetchAndUpdateFeedAsync(Feed feed, Action<int> progress_callback)
    {
        var parsed_feed = await FeedReader.ReadAsync(feed.FeedLink);
        var last_update = parsed_feed.LastUpdatedDate is null ? DateTime.Now : (DateTime)parsed_feed.LastUpdatedDate;
        if (last_update > feed.DateLastUpdated)
        {
            var posts_to_add = parsed_feed.Items.Where(i => !feed.Posts.Any(p => p.GUID == i.Id));
            var n_posts_to_add = posts_to_add.Count();
            if (n_posts_to_add > 0)
            {
                progress_callback(n_posts_to_add);
                await AddPostsToFeedAsync(feed, posts_to_add);
                feed.Posts.Sort((a, b) => b.DatePublished.CompareTo(a.DatePublished));
            }

            feed.DateLastUpdated = last_update;
            feed.Link        = parsed_feed.Link;
            feed.Description = ExtractTextFromHtml(parsed_feed.Description);
            await UpdateFeedMetadataAsync(feed);
        }
    }

    public async Task MarkPostReadAsync(Feed.Post post)
    {
        if (!post.Read)
        {
            post.Read = true;
            var post_filepath = Path.Combine(FeedDirectory, post.ParentFeed.ID, $"{post.ID}.post");
            if (File.Exists(post_filepath))
            using (var post_file = new StreamWriter(post_filepath))
                await post_file.WriteAsync(post.Serialize());
        }
    }
    public async Task MarkPostUnreadAsync(Feed.Post post)
    {
        if (post.Read)
        {
            post.Read = false;
            var post_filepath = Path.Combine(FeedDirectory, post.ParentFeed.ID, $"{post.ID}.post");
            if (File.Exists(post_filepath))
                using (var post_file = new StreamWriter(post_filepath))
                    await post_file.WriteAsync(post.Serialize());
        }
    }

    public async Task UpdateFeedMetadataAsync(Feed feed)
    {
        try {
            using (var feed_file = new StreamWriter(Path.Combine(FeedDirectory, $"{feed.ID}_updated.feed"))) {
                await feed_file.WriteAsync(feed.Serialize());
            }
            File.Move(Path.Combine(FeedDirectory, $"{feed.ID}_updated.feed"), Path.Combine(FeedDirectory, $"{feed.ID}.feed"), true);
        } catch (Exception ex) {
            using (var log_file = new StreamWriter(Path.Combine(FeedDirectory, "feed_update_error.log"), true)) {
                await log_file.WriteLineAsync($"[{DateTime.Now:dd/MM/yyyy hh:mm t} - {feed.ID}] {ex.GetType().Name} : {ex.Message}");
            }
        }
    }
    public string LoadPostContent(Feed.Post post)
    {
        using (var content_file = new StreamReader(Path.Combine(FeedDirectory, post.ParentFeed.ID, $"{post.ID}.content")))
            return content_file.ReadToEnd();
    }
    public string LoadBookmarkContent(Bookmark bookmark)
    {
        using (var content_file = new StreamReader(Path.Combine(FeedDirectory, "bookmarks", $"{bookmark.ID}.content")))
            return content_file.ReadToEnd();
    }
    public async Task BookmarkPostAsync(Feed.Post post)
    {
        var bookmark = new Bookmark(post.ID)
        {
            GUID = post.GUID,
            Title = post.Title,
            Link  = post.Link,
            FeedTitle = post.ParentFeed.Title,
            FeedLink  = post.ParentFeed.Link,
            DatePublished = post.DatePublished,
            Description = post.Description,
        };

        var bookmark_path_pre = Path.Combine(FeedDirectory, "bookmarks", bookmark.ID);

        File.Copy(
            Path.Combine(FeedDirectory, post.ParentFeed.ID, $"{post.ID}.content"),
            $"{bookmark_path_pre}.content"
        );

        using (var file = new StreamWriter($"{bookmark_path_pre}.bookmark"))
            await file.WriteAsync(bookmark.Serialize());

        Bookmarks.Add(bookmark);
    }
    public Feed.Post MockPostFromBookmark(Bookmark bookmark)
    {
        var subscribed_feed = Feeds.FirstOrDefault(f => f.Link == bookmark.FeedLink);
        var feed = new Feed("bookmarks", bookmark.FeedLink)
        {
            Title = subscribed_feed is null? bookmark.FeedLink : subscribed_feed.Title
        };
        var post = new Feed.Post(feed, bookmark.ID)
        {
            GUID = bookmark.GUID,
            Title = bookmark.Title,
            Link = bookmark.Link,
            DatePublished = bookmark.DatePublished,
            Description = bookmark.Description,
            Read = true
        };
        return post;
    }
    public void RemoveBookmark(Bookmark bookmark)
    {
        if (Bookmarks.Contains(bookmark))
        {
            var bookmark_path_pre = Path.Combine(FeedDirectory, "bookmarks", bookmark.ID);
            File.Delete($"{bookmark_path_pre}.bookmark");
            File.Delete($"{bookmark_path_pre}.content");
            Bookmarks.Remove(bookmark);
        }
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
                DatePublished = (DateTime)item.PublishingDate,
                Description = ExtractTextFromHtml(item.Description),
            };

            var post_content_filename = Path.Combine(FeedDirectory, feed.ID, post_id);

            try
            {
                var post_content = (await ProcessHtmlAsync(item.Content is null ? item.Description : item.Content, post)).Trim();

                await WriteToFileAsync($"{post_content_filename}.post", post.Serialize());
                await WriteToFileAsync($"{post_content_filename}.content", post_content.Trim());

                feed.Posts.Add(post);
            }
            catch (Exception e) { ; }
        }
    }

    static string MakeSafeId(string path)
    {
        char[] invalid_chars = Path.GetInvalidFileNameChars();
        foreach (char ch in invalid_chars)
            path = path.Replace(ch, '_');
        return path.Replace(' ', '_').Replace('.', '_').ToLower().Substring(0, path.Length > 200? 200 : path.Length);
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
    static async Task<string> ProcessHtmlAsync(string content, Feed.Post post)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        void RemoveElementsNamed(string name)
        {
            foreach (var node in doc.DocumentNode.Descendants(name).ToList())
                node.Remove();
        }

        RemoveElementsNamed("col");
        RemoveElementsNamed("style");
        RemoveElementsNamed("script");
        RemoveElementsNamed("iframe");

        foreach (var node in doc.DocumentNode.Descendants())
        {
            node.Attributes.Remove("style");
            node.Attributes.Remove("class");
            node.Attributes.Remove("srcset");
        }

        var images = doc.DocumentNode.Descendants("img").ToList();

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

public class FaviconGrabberResponse
{
    public string Domain { get; set; }
    public List<FaviconGrabberResponseIcons> Icons { get; set; }

    public class FaviconGrabberResponseIcons
    {
        [JsonProperty("src")] public string Source { get; set; }
        public string Type { get; set; }
        public string Sizes { get; set; }
    }
}