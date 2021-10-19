using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JHACodeChallenge
{
    class TwitterServices : ITwitterServices
    {
        private readonly IConfiguration _config;
        private readonly ITweetTrack _track;
        private readonly ILogger<TwitterServices> _logger;

        public TwitterServices(IConfiguration config, ITweetTrack track, ILoggerFactory loggerFactory)
        {
            _config = config;
            _track = track;
            _logger = loggerFactory.CreateLogger<TwitterServices>();
        }
        public async Task StreamTweets()
        {
            _logger.LogInformation("Start Stream Tweet");
            string bearer_token = _config.GetSection("credentials:Bearer-Token").Value;
            try
            {
                string url = CreateUrl();
                using (HttpClient client = new HttpClient())
                {
                    // set client header
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearer_token);
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    
                    //Request
                    HttpRequestMessage request = new HttpRequestMessage();
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(url);

                    // sending request
                    using (HttpResponseMessage resp = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            // read 
                            using (Stream stream = await resp.Content.ReadAsStreamAsync())
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    while (!reader.EndOfStream)
                                    {
                                        var currentline = reader.ReadLine();
                                        //_logger.LogInformation(currentline);
                                        // analyze each line
                                        _track.Process(currentline);
                                        
                                    }
                                }
                            }
                        }

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("StreamTweets Exception: " + ex.Message);
                _logger.LogError("StreamTweets Exception: " + ex);
                return;
            }
        }

        protected string CreateUrl()
        {
            return _config.GetSection("Twitter-Sample-Stream-URL2").Value;
        }
    }
}
