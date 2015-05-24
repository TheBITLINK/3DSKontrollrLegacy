using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DSKontrollr.Server
{
    public static class StaticContent
    {
        public static void Add(string path)
        {
            HTTP.staticDir.AddFile(path);
        }

        public static void Remove(string name)
        {
            HTTP.staticDir.Remove(name);
        }
    }
}
