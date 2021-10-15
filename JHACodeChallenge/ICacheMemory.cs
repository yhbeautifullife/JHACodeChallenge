using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JHACodeChallenge
{
    public interface ICacheMemory
    {
        T Get<T>(string key);
        void Set<T>(T o, string key);
        void Remove(string key);
    }
}
