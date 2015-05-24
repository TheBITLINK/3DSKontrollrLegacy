using MiniHttpd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DSKontrollr.Server.DynamicContent
{
    public class IndexPage : IFile
    {
        public IndexPage(string name, IDirectory parent, string Data)
        {
            this.name = name;
            this.parent = parent;
            this.Data = Data;
        }
        string name;
        IDirectory parent;
        string Data;

        public void OnFileRequested(HttpRequest request,
                                    IDirectory directory)
        {
            try
            {
                // Assign a MemoryStream to hold the response content.
                request.Response.ResponseContent = new MemoryStream();
                StreamWriter writer =
                      new StreamWriter(request.Response.ResponseContent);
                // Create a StreamWriter to which we
                // can write some text, and write to it.
                if (request.UserAgent.Contains("Nintendo 3DS"))
                {
                    writer.WriteLine(Data);
                }

                // Don't forget to flush!
                writer.Flush();
            }
            catch { };
        }

        public string ContentType
        {
            get { return ContentTypes.GetExtensionType(".html"); }
        }
        public string Name
        {
            get { return name; }
        }

        public IDirectory Parent
        {
            get { return parent; }
        }

        public void Dispose()
        {
        }
    }
}
