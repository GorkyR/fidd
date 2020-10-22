using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

public class Feed
{
    [JsonIgnore] public string ID { get; private set; }
    public string FeedLink { get; set; }
    public string Title { get; set; }
    public string Link { get; set; }
    public string ImagePath { get; set; }
    public DateTime DateLastUpdated { get; set; }
    public string Description { get; set; }
    [JsonIgnore] public List<Post> Posts { get; } = new List<Post>();
    [JsonIgnore] public IReadOnlyList<Post> Unread => (from post in Posts where !post.Read select post).ToList().AsReadOnly();

    public class Post
    {
        [JsonIgnore] public Feed ParentFeed { get; set; }
        [JsonIgnore] public string ID { get; set; }
        public string GUID { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime DatePublished { get; set; }
        public string Description { get; set; }
        public bool Read { get; set; }

        public Post(Feed feed, string id)
        {
            this.ParentFeed = feed;
            this.ID = id;
        }

        public override string ToString() => Title;
        public string Serialize() => SerializeObject(this, Formatting.Indented);
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
    public string Serialize() => SerializeObject(this, Formatting.Indented);
    public static Feed Deserialize(string id, string json)
    {
        var feed = DeserializeObject<Feed>(json);
        feed.ID = id;
        return feed;
    }
}