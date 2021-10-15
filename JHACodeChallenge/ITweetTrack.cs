using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JHACodeChallenge
{
    public interface ITweetTrack
    {
        //Task Process(string tweet_line);
        void Process(string tweet_line);
    }
}
