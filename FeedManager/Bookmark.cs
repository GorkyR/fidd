using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

public class Bookmark
{
    [JsonIgnore] public string ID { get; private set; }
    public string GUID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }
    public string FeedTitle { get; set; }
    public string FeedLink { get; set; }
    public DateTime DatePublished { get; set; }
    public DateTime DateBookmarked { get; set; }

    public Bookmark(string id)
    {
        ID = id;
        DateBookmarked = DateTime.Now;
    }

    public string Serialize() => SerializeObject(this, Formatting.Indented);
    public static Bookmark Deserialize(string id, string json)
    {
        var bookmark = DeserializeObject<Bookmark>(json);
        bookmark.ID = id;
        return bookmark;
    }
}
