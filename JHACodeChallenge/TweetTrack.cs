using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JHACodeChallenge
{
    public class TweetTrack : ITweetTrack
    {
        private ICacheMemory _cache;
        private IConfiguration _config;
        private readonly ILogger<TweetTrack> _logger;
        
        public TweetTrack(ICacheMemory cache, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _cache = cache;
            _config = config;
            _logger = loggerFactory.CreateLogger<TweetTrack>();
        }

        /*public async Task Process(string tweet_line)
        {
            await Task.Run(() => track(tweet_line));
        }*/

        public void Process(string tweet_line)
        {
            // process tweet count info
            ProcessTweetCountInfo(tweet_line);

            // process hashtag info
            ProcessHashTagInfo(tweet_line);

            // process url info
            ProcessURLInfo(tweet_line);

            // process photo url info
            ProcessPhotoURLInfo(tweet_line);

            // process emoji info
            ProcessEmojiInfo(tweet_line);
        }

        private void ProcessTweetCountInfo(string tweet_line)
        {
            const string cache_key = MyConstants.cache_key_tweet_cnt;
            try
            {
                // if it is not empty
                if (!string.IsNullOrEmpty(tweet_line))
                {
                    //Console.WriteLine(tweet_line);
                    dynamic tweet_obj = JObject.Parse(tweet_line);
                    var tweet_data = tweet_obj.data;

                    // if data is there, and created_at is included in tweet_line
                    if (tweet_data != null && tweet_data.created_at != null)
                    {
                        string created_at = tweet_data.created_at;
                        DateTime dt_created_at = DateTime.Parse(created_at);

                        // load data from cache memory
                        TweetCountInfo TweetCntInfo = _cache.Get<TweetCountInfo>(cache_key);
                        if (TweetCntInfo == null)
                        {
                            TweetCntInfo = new TweetCountInfo();
                            TweetCntInfo.dicTweetCount = new Dictionary<int, int>();
                            TweetCntInfo.start_utc_time = dt_created_at; // set start time
                            TweetCntInfo.last_utc_time = dt_created_at;
                        }

                        // increase total count
                        TweetCntInfo.total_count += 1;
                        TweetCntInfo.last_utc_time = dt_created_at; // record tweet created time for reporting later

                        #region not_used
                        // it saved count for each second period, do I need it?? maynot need it, just left here
                        //if (TweetCntInfo.dicTweetCount.Count > 0)
                        //{
                        //    var last_pair = TweetCntInfo.dicTweetCount.OrderBy(o => o.Key).Last();
                        //    DateTime dtTmp = TweetCntInfo.last_utc_time.AddSeconds(1);
                        //    if (dt_created_at <= dtTmp) // within 1 second
                        //        TweetCntInfo.dicTweetCount[last_pair.Key] += 1;
                        //    else
                        //    {
                        //        TweetCntInfo.dicTweetCount[last_pair.Key + 1] = 1;
                        //        TweetCntInfo.last_utc_time = dt_created_at;
                        //    }
                        //}
                        //else
                        //{
                        //    TweetCntInfo.dicTweetCount[1] = 1;
                        //    TweetCntInfo.last_utc_time = dt_created_at;
                        //}
                        #endregion

                        _cache.Set<TweetCountInfo>(TweetCntInfo, cache_key);
                    }


                }
            }
            catch (Exception ex)
            {
                string err_message = $"ProcessTweetCountInfo exception: {ex.Message.ToString()}";
                // it should saved in log file in the future, now just display it in the console.
                Console.WriteLine(err_message);
                _logger.LogError(err_message);
            }

        }

        private void ProcessHashTagInfo(string tweet_line)
        {
            const string cache_key = MyConstants.cache_key_hashtag;
            try
            {
                // if it is not empty
                if (!string.IsNullOrEmpty(tweet_line))
                {
                    //Console.WriteLine(tweet_line);
                    dynamic tweet_obj = JObject.Parse(tweet_line);
                    var tweet_data = tweet_obj.data;

                    if (tweet_data != null)
                    {
                        // get hashtag info from cache memory
                        HashTagInfo htInfo = _cache.Get<HashTagInfo>(cache_key);

                        if (htInfo == null)
                        {
                            htInfo = new HashTagInfo();
                            htInfo.dic = new Dictionary<string, int>();
                        }

                        // increase total tweet count
                        htInfo.total_tweet_count += 1;

                        if (tweet_data.entities != null && tweet_data.entities.hashtags != null)
                        {
                            // if hashtag found, increase the count by 1
                            htInfo.tweet_count_include_hashtags += 1;

                            foreach (var item in tweet_data.entities.hashtags)
                            {
                                string key = item.tag;
                                if (htInfo.dic.Count > 0 && htInfo.dic.ContainsKey(key))
                                    htInfo.dic[key] += 1;
                                else
                                {
                                    htInfo.dic[key] = 1;
                                }
                            }
                        }

                        // save hashtag info into cache memory
                        _cache.Set<HashTagInfo>(htInfo, cache_key);
                    }

                }
            }
            catch (Exception ex)
            {
                string err_message = $"ProcessHashTagInfo exception: {ex.Message.ToString()}";
                // it should saved in log file in the future, now just display it in the console.
                //Console.WriteLine(err_message);
                _logger.LogError(err_message);
            }
        }

        // process url info which is for all urls include media and link
        private void ProcessURLInfo(string tweet_line)
        {
            const string cache_key = MyConstants.cache_key_url;
            try
            {
                // if it is not empty
                if (!string.IsNullOrEmpty(tweet_line))
                {
                    //Console.WriteLine(tweet_line);
                    dynamic tweet_obj = JObject.Parse(tweet_line);
                    var tweet_data = tweet_obj.data;


                    if (tweet_data != null)
                    {
                        // get info from cache
                        var info = _cache.Get<UrlInfo>(cache_key);

                        if (info == null)
                        {
                            info = new UrlInfo();
                            info.dic = new Dictionary<string, int>();
                        }

                        info.total_tweet_count += 1;

                        if (tweet_data.entities != null && tweet_data.entities.urls != null)
                        {
                            info.tweet_count_include_urls += 1;

                            foreach (var item in tweet_data.entities.urls)
                            {
                                string strUrl = item.expanded_url;
                                Uri my_uri = new Uri(strUrl);
                                string key = my_uri.Host;
                                if (info.dic.Count > 0 && info.dic.ContainsKey(key))
                                    info.dic[key] += 1;
                                else
                                {
                                    info.dic[key] = 1;
                                }
                            }
                        }

                        _cache.Set<UrlInfo>(info, cache_key);
                    }
                }
            }
            catch (Exception ex)
            {
                string err_message = $"ProcessURLInfo exception: {ex.Message.ToString()}";
                // save log in the file
                _logger.LogError(err_message);
            }
        }

        // process url which link to the photo
        private void ProcessPhotoURLInfo(string tweet_line)
        {
            const string cache_key = MyConstants.cache_key_photo_url;
            try
            {
                // if it is not empty
                if (!string.IsNullOrEmpty(tweet_line))
                {
                    //Console.WriteLine(tweet_line);
                    dynamic tweet_obj = JObject.Parse(tweet_line);

                    var info = _cache.Get<UrlInfo>(cache_key);
                    if (info == null)
                    {
                        // initial
                        info = new UrlInfo();
                        info.dic = new Dictionary<string, int>();
                    }

                    info.total_tweet_count += 1;

                    // it looks like we need check includes/media for photo url
                    var tweet_includes = tweet_obj.includes;

                    if (tweet_includes != null)
                    {

                        if (tweet_includes.media != null)
                        {
                            int cnt = 0;
                            foreach (var item in tweet_includes.media)
                            {
                                string type = item.type;
                                if (type == "photo" && item.url != null)
                                {
                                    if (++cnt == 1) // only add once for each tweet
                                        info.tweet_count_include_urls += 1;

                                    string strUrl = item.url;
                                    Uri my_uri = new Uri(strUrl);
                                    string key = my_uri.Host;
                                    if (info.dic.Count > 0 && info.dic.ContainsKey(key))
                                        info.dic[key] += 1;
                                    else
                                    {
                                        info.dic[key] = 1;
                                    }
                                }

                            }
                        }

                        _cache.Set<UrlInfo>(info, cache_key);
                    }

                }
            }
            catch (Exception ex)
            {
                string err_message = $"ProcessPhotoURLInfo exception: {ex.Message.ToString()}";
                _logger.LogError(err_message);
            }
        }

        // process emojis
        private void ProcessEmojiInfo(string tweet_line)
        {
            const string cache_key = MyConstants.cache_key_emoji;
            try
            {
                // if tweet_line is not empty
                if (!string.IsNullOrEmpty(tweet_line))
                {
                    //Console.WriteLine(tweet_line);
                    dynamic tweet_obj = JObject.Parse(tweet_line);
                    var tweet_data = tweet_obj.data;

                    var info = _cache.Get<EmojisInfo>(cache_key);
                    if (info == null)
                    {
                        // initial
                        info = new EmojisInfo();
                        info.dic = new Dictionary<string, int>();
                    }

                    info.total_tweet_count += 1;

                    if (tweet_data != null)
                    {
                        string text = tweet_data.text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            List<string> lstUnicode = FindEmojisInText(text);

                            if (lstUnicode.Count > 0)
                                info.tweet_count_include_emojis += 1;

                            foreach (string unicode in lstUnicode)
                            {
                                if (info.dic.ContainsKey(unicode))
                                    info.dic[unicode] += 1;
                                else
                                    info.dic[unicode] = 1;
                            }
                        }

                        _cache.Set<EmojisInfo>(info, cache_key);
                    }

                }
            }
            catch(Exception ex)
            {
                string err_message = $"ProcessEmojiInfo exception: {ex.Message.ToString()}";
                _logger.LogError(err_message);
            }


        }

        // must make public for unit testing
        public List<string> FindEmojisInText(string text)
        {
            List<string> lst = new List<string>();

            try
            {
                string regex = "(?:0\x20E3|1\x20E3|2\x20E3|3\x20E3|4\x20E3|5\x20E3|6\x20E3|7\x20E3|8\x20E3|9\x20E3|#\x20E3|\\*\x20E3|\xD83C(?:\xDDE6\xD83C(?:\xDDE8|\xDDE9|\xDDEA|\xDDEB|\xDDEC|\xDDEE|\xDDF1|\xDDF2|\xDDF4|\xDDF6|\xDDF7|\xDDF8|\xDDF9|\xDDFA|\xDDFC|\xDDFD|\xDDFF)|\xDDE7\xD83C(?:\xDDE6|\xDDE7|\xDDE9|\xDDEA|\xDDEB|\xDDEC|\xDDED|\xDDEE|\xDDEF|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF6|\xDDF7|\xDDF8|\xDDF9|\xDDFB|\xDDFC|\xDDFE|\xDDFF)|\xDDE8\xD83C(?:\xDDE6|\xDDE8|\xDDE9|\xDDEB|\xDDEC|\xDDED|\xDDEE|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF5|\xDDF7|\xDDFA|\xDDFB|\xDDFC|\xDDFD|\xDDFE|\xDDFF)|\xDDE9\xD83C(?:\xDDEA|\xDDEC|\xDDEF|\xDDF0|\xDDF2|\xDDF4|\xDDFF)|\xDDEA\xD83C(?:\xDDE6|\xDDE8|\xDDEA|\xDDEC|\xDDED|\xDDF7|\xDDF8|\xDDF9|\xDDFA)|\xDDEB\xD83C(?:\xDDEE|\xDDEF|\xDDF0|\xDDF2|\xDDF4|\xDDF7)|\xDDEC\xD83C(?:\xDDE6|\xDDE7|\xDDE9|\xDDEA|\xDDEB|\xDDEC|\xDDED|\xDDEE|\xDDF1|\xDDF2|\xDDF3|\xDDF5|\xDDF6|\xDDF7|\xDDF8|\xDDF9|\xDDFA|\xDDFC|\xDDFE)|\xDDED\xD83C(?:\xDDF0|\xDDF2|\xDDF3|\xDDF7|\xDDF9|\xDDFA)|\xDDEE\xD83C(?:\xDDE8|\xDDE9|\xDDEA|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF6|\xDDF7|\xDDF8|\xDDF9)|\xDDEF\xD83C(?:\xDDEA|\xDDF2|\xDDF4|\xDDF5)|\xDDF0\xD83C(?:\xDDEA|\xDDEC|\xDDED|\xDDEE|\xDDF2|\xDDF3|\xDDF5|\xDDF7|\xDDFC|\xDDFE|\xDDFF)|\xDDF1\xD83C(?:\xDDE6|\xDDE7|\xDDE8|\xDDEE|\xDDF0|\xDDF7|\xDDF8|\xDDF9|\xDDFA|\xDDFB|\xDDFE)|\xDDF2\xD83C(?:\xDDE6|\xDDE8|\xDDE9|\xDDEA|\xDDEB|\xDDEC|\xDDED|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF5|\xDDF6|\xDDF7|\xDDF8|\xDDF9|\xDDFA|\xDDFB|\xDDFC|\xDDFD|\xDDFE|\xDDFF)|\xDDF3\xD83C(?:\xDDE6|\xDDE8|\xDDEA|\xDDEB|\xDDEC|\xDDEE|\xDDF1|\xDDF4|\xDDF5|\xDDF7|\xDDFA|\xDDFF)|\xDDF4\xD83C\xDDF2|\xDDF5\xD83C(?:\xDDE6|\xDDEA|\xDDEB|\xDDEC|\xDDED|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF7|\xDDF8|\xDDF9|\xDDFC|\xDDFE)|\xDDF6\xD83C\xDDE6|\xDDF7\xD83C(?:\xDDEA|\xDDF4|\xDDF8|\xDDFA|\xDDFC)|\xDDF8\xD83C(?:\xDDE6|\xDDE7|\xDDE8|\xDDE9|\xDDEA|\xDDEC|\xDDED|\xDDEE|\xDDEF|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF7|\xDDF8|\xDDF9|\xDDFB|\xDDFD|\xDDFE|\xDDFF)|\xDDF9\xD83C(?:\xDDE6|\xDDE8|\xDDE9|\xDDEB|\xDDEC|\xDDED|\xDDEF|\xDDF0|\xDDF1|\xDDF2|\xDDF3|\xDDF4|\xDDF7|\xDDF9|\xDDFB|\xDDFC|\xDDFF)|\xDDFA\xD83C(?:\xDDE6|\xDDEC|\xDDF2|\xDDF8|\xDDFE|\xDDFF)|\xDDFB\xD83C(?:\xDDE6|\xDDE8|\xDDEA|\xDDEC|\xDDEE|\xDDF3|\xDDFA)|\xDDFC\xD83C(?:\xDDEB|\xDDF8)|\xDDFD\xD83C\xDDF0|\xDDFE\xD83C(?:\xDDEA|\xDDF9)|\xDDFF\xD83C(?:\xDDE6|\xDDF2|\xDDFC)))|[\xA9\xAE\x203C\x2049\x2122\x2139\x2194-\x2199\x21A9\x21AA\x231A\x231B\x2328\x23CF\x23E9-\x23F3\x23F8-\x23FA\x24C2\x25AA\x25AB\x25B6\x25C0\x25FB-\x25FE\x2600-\x2604\x260E\x2611\x2614\x2615\x2618\x261D\x2620\x2622\x2623\x2626\x262A\x262E\x262F\x2638-\x263A\x2648-\x2653\x2660\x2663\x2665\x2666\x2668\x267B\x267F\x2692-\x2694\x2696\x2697\x2699\x269B\x269C\x26A0\x26A1\x26AA\x26AB\x26B0\x26B1\x26BD\x26BE\x26C4\x26C5\x26C8\x26CE\x26CF\x26D1\x26D3\x26D4\x26E9\x26EA\x26F0-\x26F5\x26F7-\x26FA\x26FD\x2702\x2705\x2708-\x270D\x270F\x2712\x2714\x2716\x271D\x2721\x2728\x2733\x2734\x2744\x2747\x274C\x274E\x2753-\x2755\x2757\x2763\x2764\x2795-\x2797\x27A1\x27B0\x27BF\x2934\x2935\x2B05-\x2B07\x2B1B\x2B1C\x2B50\x2B55\x3030\x303D\x3297\x3299]|\xD83C[\xDC04\xDCCF\xDD70\xDD71\xDD7E\xDD7F\xDD8E\xDD91-\xDD9A\xDE01\xDE02\xDE1A\xDE2F\xDE32-\xDE3A\xDE50\xDE51\xDF00-\xDF21\xDF24-\xDF93\xDF96\xDF97\xDF99-\xDF9B\xDF9E-\xDFF0\xDFF3-\xDFF5\xDFF7-\xDFFF]|\xD83D[\xDC00-\xDCFD\xDCFF-\xDD3D\xDD49-\xDD4E\xDD50-\xDD67\xDD6F\xDD70\xDD73-\xDD79\xDD87\xDD8A-\xDD8D\xDD90\xDD95\xDD96\xDDA5\xDDA8\xDDB1\xDDB2\xDDBC\xDDC2-\xDDC4\xDDD1-\xDDD3\xDDDC-\xDDDE\xDDE1\xDDE3\xDDEF\xDDF3\xDDFA-\xDE4F\xDE80-\xDEC5\xDECB-\xDED0\xDEE0-\xDEE5\xDEE9\xDEEB\xDEEC\xDEF0\xDEF3]|\xD83E[\xDD10-\xDD18\xDD80-\xDD84\xDDC0]";
                //string regex = _config.GetSection("appSettings:Emoji-RegEx").Value;
                MatchCollection matches = Regex.Matches(text, regex);
                foreach (Match match in matches)
                {
                    string value = match.Value;
                    // As I learned that each emoji contain surrogate pair (high-surrogate, and low-surrogate)
                    if (value.Length == 2)
                    {
                        try
                        {
                            // convert to unicode
                            int unicode = Char.ConvertToUtf32(value[0], value[1]);
                            string str_unicode = string.Format("{0:X}", unicode);
                            lst.Add(str_unicode);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            // A valid high surrogate character should between 0xd800 and 0xdbff,
                            // inclusive. (Parameter 'highSurrogate')
                            string warning = $"FindEmojisInText {text} Exception: {ex}";
                            _logger.LogWarning(warning);
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // other exceptions
                Console.WriteLine(ex.Message);
                Console.WriteLine("Text in processing: " + text);
                return lst;
            }
            return lst;
        }
    }
}
