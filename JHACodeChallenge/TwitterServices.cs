using Microsoft.Extensions.Configuration;
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

        public TwitterServices(IConfiguration config, ITweetTrack track)
        {
            _config = config;
            _track = track;
        }
        public async Task StreamTweets()
        {
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
                                        //Console.WriteLine(currentline); // for testing
                                        var currentline = reader.ReadLine();
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
                Console.WriteLine("StreamTweets Exception: " + ex.Message);
                return;
            }
        }

        protected string CreateUrl()
        {
            return _config.GetSection("Twitter-Sample-Stream-URL2").Value;
        }
    }
}
