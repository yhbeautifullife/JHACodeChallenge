using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JHACodeChallenge
{
    public class MyConstants
    {
        public const string cache_key_tweet_cnt = "cache_key_tweet_cnt";
        public const string cache_key_hashtag = "cache_key_hashtag";
        public const string cache_key_url = "cache_key_url";
        public const string cache_key_photo_url = "cache_key_photo_url";
        public const string cache_key_emoji = "cache_key_emoji";
        public const string datetime_utc_format = "yyyy-MM-ddTHH:mm:ss.fffZ";
    }

    public class TweetCountInfo
    {
        public long total_count { get; set; } = 0;
        public DateTime start_utc_time { get; set; }
        public DateTime last_utc_time { get; set; }

        // used for saving tweet count for certain interval, for example second, minute, hour. Note: not used
        public Dictionary<int, int> dicTweetCount { get; set; }
    }

    public class HashTagInfo
    {
        public int total_tweet_count { get; set; } = 0;
        public int tweet_count_include_hashtags { get; set; } = 0;

        // used for saving count for hashtag; key is hasktag, value is count
        public Dictionary<string, int> dic { get; set; }
    }

    public class UrlInfo
    {
        public int total_tweet_count { get; set; } = 0;
        public int tweet_count_include_urls { get; set; } = 0;

        // used for saving count for each domain; key is domain, value is count
        public Dictionary<string, int> dic { get; set; }
    }

    public class EmojisInfo
    {
        public int total_tweet_count { get; set; } = 0;
        public int tweet_count_include_emojis { get; set; } = 0;

        // used for saving count for each emoji; key is unicode of a emoji, value is total count for that emoji used
        public Dictionary<string, int> dic { get; set; }
    }
}
