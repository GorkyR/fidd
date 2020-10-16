using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using static Newtonsoft.Json.JsonConvert;

public class Feed
{
    [Newtonsoft.Json.JsonIgnore]
    public string ID { get; private set; }
    public string FeedLink { get; set; }
    public string Title { get; set; }
    public string Link { get; set; }
    public string ImagePath { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Description { get; set; }
    [Newtonsoft.Json.JsonIgnore]
    public List<Post> Posts { get; } = new List<Post>();

    public IReadOnlyList<Post> Unread => (from post in Posts where !post.Read select post).ToList().AsReadOnly();
    public class Post
    {
        [Newtonsoft.Json.JsonIgnore]
        public Feed ParentFeed { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ID { get; set; }
        public string GUID { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime Published { get; set; }
        public string Description { get; set; }
        public bool Read { get; set; }

        public Post(Feed feed, string id)
        {
            this.ParentFeed = feed;
            this.ID = id;
        }

        public override string ToString() => Title;
        public string Serialize() => SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        public static Post Deserialize(Feed feed, string id, string json)
        {
            var post = DeserializeObject<Post>(json);
            post.ParentFeed = feed;
            post.ID = id;
            return post;
        }
    }

    public Feed(string id, string feed_link)
    {
        this.ID = id;
        this.FeedLink = feed_link;
    }

    public override string ToString() => Title;
    public string Serialize() => SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
    public static Feed Deserialize(string id, string json)
    {
        var feed = DeserializeObject<Feed>(json);
        feed.ID = id;
        return feed;
    }
}

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

    public void AddFeed(string feed_url)
    {
        if (Feeds.Any(f => f.FeedLink == feed_url))
            throw new Exception("There's already a feed with that url.");
        else
        {
            var parsed_feed = FeedReader.Read(feed_url);
            var feed_id = NormalizeFilename(parsed_feed.Title);

            int suffix = 0;
            while (Directory.Exists(Path.Combine(FeedDirectory, $"{feed_id}{(suffix > 0 ? $"_{suffix}" : "")}.feed")))
                suffix++;
            feed_id = $"{feed_id}{(suffix > 0 ? $"_{suffix}" : "")}";

            string feed_image_path = null;
            if (!string.IsNullOrEmpty(parsed_feed.ImageUrl))
            {
                var downloader = new WebClient();
                var image_extension = Path.GetExtension(parsed_feed.ImageUrl);
                feed_image_path = $"{feed_id}{image_extension}";
                downloader.DownloadFile(parsed_feed.ImageUrl, Path.Combine(FeedDirectory, feed_image_path));
            }

            var feed_uri = new Uri(parsed_feed.Link);
            var feed = new Feed(feed_id, feed_url)
            {
                Title = parsed_feed.Title,
                Link = $"{feed_uri.Scheme}://{feed_uri.Host}",
                ImagePath = feed_image_path,
                LastUpdated = (DateTime)parsed_feed.LastUpdatedDate,
                Description = NormalizeDescription(parsed_feed.Description)
            };

            using (var file = new StreamWriter(Path.Combine(FeedDirectory, $"{feed_id}.feed")))
                file.Write(feed.Serialize());

            Directory.CreateDirectory(Path.Combine(FeedDirectory, feed_id));

            var markdown_converter = new ReverseMarkdown.Converter(
                new ReverseMarkdown.Config() { UnknownTags = ReverseMarkdown.Config.UnknownTagsOption.Drop }
            );

            foreach (var item in parsed_feed.Items)
            {
                var post_id = NormalizeFilename(item.Title);

                var post_content_path = Path.Combine(FeedDirectory, feed_id, $"{post_id}.content");
                using (var file = new StreamWriter(post_content_path))
                    file.Write(markdown_converter.Convert(item.Content.Trim()));

                var post = new Feed.Post(feed, post_id)
                {
                    GUID = item.Id,
                    Link = item.Link,
                    Title = item.Title,
                    Published = (DateTime)item.PublishingDate,
                    Description = NormalizeDescription(item.Description),
                };

                using (var file = new StreamWriter(Path.Combine(FeedDirectory, feed_id, $"{post_id}.post")))
                    file.Write(post.Serialize());

                feed.Posts.Add(post);
            }
            Feeds.Add(feed);
        }
    }
    public void MarkPostRead(Feed.Post post)
    {
        post.Read = true;
        using (var post_file = new StreamWriter(Path.Combine(FeedDirectory, post.ParentFeed.ID, $"{post.ID}.post")))
            post_file.Write(post.Serialize());
    }
    public void UpdateFeedMetadata(Feed feed)
    {
        using (var feed_file = new StreamWriter(Path.Combine(FeedDirectory, $"{feed.ID}.feed")))
            feed_file.Write(feed.Serialize());
    }
    public void FetchAndUpdateFeed(Feed feed)
    {
        var parsed_feed = FeedReader.Read(feed.FeedLink);
        var last_update = (DateTime)parsed_feed.LastUpdatedDate;
        if (last_update > feed.LastUpdated)
        {
            feed.LastUpdated = last_update;
            feed.Link = parsed_feed.Link;
            feed.Description = NormalizeDescription(parsed_feed.Description);

            foreach (var item in parsed_feed.Items.Where(i => !feed.Posts.Any(p => p.GUID == i.Id)))
            {
                var post_id = NormalizeFilename(item.Title);

                var post_content_path = Path.Combine(FeedDirectory, feed.ID, $"{post_id}.content");
                using (var file = new StreamWriter(post_content_path))
                    file.Write(item.Content);

                var post = new Feed.Post(feed, post_id)
                {
                    GUID = item.Id,
                    Link = item.Link,
                    Title = item.Title,
                    Published = (DateTime)item.PublishingDate,
                    Description = NormalizeDescription(item.Description),
                };

                using (var file = new StreamWriter(Path.Combine(FeedDirectory, feed.ID, $"{post_id}.post")))
                    file.Write(post.Serialize());

                feed.Posts.Add(post);
            }
        }
    }
    public string LoadPostContent(Feed.Post post)
    {
        using (var content_file = new StreamReader(Path.Combine(FeedDirectory, post.ParentFeed.ID, $"{post.ID}.content")))
            return content_file.ReadToEnd();
    }

    private static string NormalizeFilename(string path)
    {
        char[] invalid_chars = Path.GetInvalidFileNameChars();
        foreach (char ch in invalid_chars)
            path = path.Replace(ch, '_');
        return path.Replace(' ', '_').Replace('.', '_').ToLower();
    }
    private static string NormalizeDescription(string text)
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
}