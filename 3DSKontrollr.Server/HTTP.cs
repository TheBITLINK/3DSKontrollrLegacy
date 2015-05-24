using MiniHttpd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DSKontrollr.Server
{
    public class HTTP : HttpWebServer
    {
        public static HTTP current { get; private set; }
        public static VirtualDirectory staticDir { get; private set; }
        public HTTP(int port)
        {
            current = this;
            this.Port = port;
            VirtualDirectory root  = new VirtualDirectory();
            this.Root = root;
            VirtualDirectory sDir = new VirtualDirectory("static", root);
            root.AddDirectory(sDir);
            staticDir = sDir;
            this.Start();
            Console.WriteLine("HTTP Server Started at Port " + port);
        }
    }
}
