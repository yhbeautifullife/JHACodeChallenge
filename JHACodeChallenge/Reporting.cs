using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace JHACodeChallenge
{
    public class Reporting : IReporting
    {
        private IConfiguration _config;
        private ICacheMemory _cache;

        private TweetCountInfo tweetCntInfo;
        private List<string> reportInfo = new List<string>();

        public Reporting(IConfiguration config, ICacheMemory cache)
        {
            _config = config;
            _cache = cache;
        }
        public void Start()
        {
            // get time interval from appsettings
            int seconds = Convert.ToInt32(_config.GetSection("appSettings:ReportTimeIntervalSeconds").Value);
            var timer = new Timer(seconds *1000);
            // add elapsed event handler
            timer.Elapsed += new ElapsedEventHandler(Process);
            timer.Enabled = true;
        }

        private void Process(object sender, ElapsedEventArgs e)
        {
            tweetCntInfo = _cache.Get<TweetCountInfo>(MyConstants.cache_key_tweet_cnt);
            if(tweetCntInfo != null)
            {
                // report interval
                string temp = $"++++++ Report Interval (UTC) {tweetCntInfo.start_utc_time.ToString(MyConstants.datetime_utc_format)} - " +
                    $"{tweetCntInfo.last_utc_time.ToString(MyConstants.datetime_utc_format)} ++++++";
                reportInfo.Add(temp);

                // time interval
                TimeSpan ts = tweetCntInfo.last_utc_time - tweetCntInfo.start_utc_time;
                temp = $"interval time in seconds: {(int)Math.Round(Math.Abs(ts.TotalSeconds))}";
                if ((int)Math.Round(Math.Abs(ts.TotalSeconds)) >= 60) // interval more than/equal a minute
                {
                    temp += string.Format(" interval time in minute(s): {0: 0.0}", Math.Abs(ts.TotalSeconds)/60);
                }
                if ((int)Math.Round(Math.Abs(ts.TotalSeconds)) >= 3600) // interval more than/equal an hour
                {
                    temp += string.Format(" interval time in hour(s): {0: 0.0}", Math.Abs(ts.TotalSeconds) / 3600);
                }
                reportInfo.Add(temp);

                // total tweet
                temp = $"* Total number of tweets received: {tweetCntInfo.total_count}";
                reportInfo.Add(temp);

                // Average tweets per second
                temp = string.Format("* Aerage tweets per second: {0: 0.0}",tweetCntInfo.total_count /Math.Abs(ts.TotalSeconds));
                reportInfo.Add(temp);

                // average tweets per minute
                if((int)Math.Round(Math.Abs(ts.TotalSeconds)) >= 60)
                {
                    double min = Math.Abs(ts.TotalSeconds) / 60;
                    temp = string.Format("* Aerage tweets per minute: {0: 0.0}", tweetCntInfo.total_count / min);
                    reportInfo.Add(temp);
                }

                // average tweets per hour
                if ((int)Math.Round(Math.Abs(ts.TotalSeconds)) >= 3600)
                {
                    double hours = Math.Abs(ts.TotalSeconds) / 3600;
                    temp = string.Format("* Aerage tweets per hour: {0: 0.0}", tweetCntInfo.total_count / hours);
                    reportInfo.Add(temp);
                }

                // hashtag info
                var htInfo = _cache.Get<HashTagInfo>(MyConstants.cache_key_hashtag);
                if (htInfo != null)
                {
                    // take top 5
                    var sortedDic = htInfo.dic.OrderByDescending(o => o.Value).Take(5).ToDictionary(pair => pair.Key, pair => pair.Value);
                    if (sortedDic != null && sortedDic.Count >0)
                    {
                        temp = $"* Top 5 Hashtags";
                        reportInfo.Add(temp);
                        foreach (var p in sortedDic)
                        {
                            temp = $"   #{p.Key}: {p.Value}";
                            reportInfo.Add(temp);
                        }

                        temp = string.Format("* Percent of tweets that contain hashtag(s): {0: 0.00}%", ((double)htInfo.tweet_count_include_hashtags *100 / htInfo.total_tweet_count));
                        reportInfo.Add(temp);
                    }
                }

                // url info for all url
                var urlInfo = _cache.Get<UrlInfo>(MyConstants.cache_key_url);
                if(urlInfo != null)
                {
                    // take top 5 domain
                    var sortedDic = urlInfo.dic.OrderByDescending(o => o.Value).Take(5).ToDictionary(pair => pair.Key, pair => pair.Value);
                    if (sortedDic != null && sortedDic.Count > 0)
                    {
                        temp = $"* Top 5 domains for urls in tweets";
                        reportInfo.Add(temp);
                        foreach (var p in sortedDic)
                        {
                            temp = $"   #{p.Key}: {p.Value}";
                            reportInfo.Add(temp);
                        }

                        temp = string.Format("* Percent of tweets that contain url(s): {0: 0.00}%", ((double)urlInfo.tweet_count_include_urls * 100 / urlInfo.total_tweet_count));
                        reportInfo.Add(temp);
                    }
                }

                // phot url info
                var photoUrlInfo = _cache.Get<UrlInfo>(MyConstants.cache_key_photo_url);
                if(photoUrlInfo != null)
                {
                    temp = string.Format("* Percent of tweets that contain photo url(s): {0: 0.00}%", ((double)photoUrlInfo.tweet_count_include_urls * 100 / photoUrlInfo.total_tweet_count));
                    reportInfo.Add(temp);
                }

                // emojis info
                var emojiInfo = _cache.Get<EmojisInfo>(MyConstants.cache_key_emoji);
                if(emojiInfo != null)
                {
                    var sortedDic = emojiInfo.dic.OrderByDescending(o => o.Value).Take(5).ToDictionary(pair => pair.Key, pair => pair.Value);
                    if (sortedDic != null && sortedDic.Count > 0)
                    {
                        temp = $"* Top 5 emojis used in tweets";
                        reportInfo.Add(temp);
                        foreach (var p in sortedDic)
                        {
                            temp = $"   Emoji with unicode U+{p.Key}: {p.Value}";
                            reportInfo.Add(temp);
                        }    
                    }
                    temp = string.Format("* Percent of tweets that contain emoji(s): {0: 0.00}%", ((double)emojiInfo.tweet_count_include_emojis * 100 / emojiInfo.total_tweet_count));
                    reportInfo.Add(temp);
                }
                
                // print to console
                Print();

            }
        }

        private void Print()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode; // use unicode
            foreach(string str in reportInfo)
            {
                Console.WriteLine(str.Normalize());
            }
            Console.WriteLine();

            // after print reset reportinfo
            reportInfo.Clear();
        }
  
    }
}
