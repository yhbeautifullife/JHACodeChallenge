using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace JHACodeChallenge
{
    public class App
    {
        private ITwitterServices _twitter_service;
        private IReporting _report;
        private IConfiguration _config;
        public App(ITwitterServices twitter_service, IReporting report, IConfiguration config)
        {
            _twitter_service = twitter_service;
            _report = report;
            _config = config;
        }
        public void Run()
        {
            // program will auto exit after interval set in the appsettings
            // if value is set negative (-1), program will not auto exit
            int seconds = Convert.ToInt32(_config.GetSection("appSettings:AppAutoCloseAfterMinutes").Value) * 60;
            using (Timer timer = new Timer())
            {
                if (seconds > 0)
                {
                    timer.Interval = seconds * 1000;
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                }
                // stream tweet
                Task t = _twitter_service.StreamTweets();

                Console.WriteLine("Processing ...");

                // start to report
                _report.Start();

                // stream until user stop it by click ctrl + c or reach the interval time
                t.Wait();
            }

        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
