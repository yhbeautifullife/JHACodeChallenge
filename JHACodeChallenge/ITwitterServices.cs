using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JHACodeChallenge
{
    public interface ITwitterServices
    {
        Task StreamTweets();
    }
}
