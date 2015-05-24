using MiniHttpd;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace _3DSKontrollr.Server.DynamicContent
{
    public class Screen : IFile
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(int hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll")]

        static extern int GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        public Screen(string name, IDirectory parent)
        {
            this.name = name;
            this.parent = parent;
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
                request.Response.ContentType = ContentTypes.GetExtensionType(".jpg");
                request.Response.ResponseContent = new MemoryStream();
                StreamWriter writer =
                      new StreamWriter(request.Response.ResponseContent);
                // Create a StreamWriter to which we
                // can write some text, and write to it.
                ScreenCapture sC = new ScreenCapture();
                if(request.Uri.Query.Contains("cwnd"))
                {
                    Image Capp = sC.CaptureWindow(new IntPtr(GetForegroundWindow())).GetThumbnailImage(400, 220, new System.Drawing.Image.GetThumbnailImageAbort(onerror), System.IntPtr.Zero);
                    Capp.Save(request.Response.ResponseContent, ImageFormat.Jpeg);
                    request.Response.ResponseContent.Position = 0;
                }
                else
                {
                    Image Capp = sC.CaptureScreen().GetThumbnailImage(400, 220, new System.Drawing.Image.GetThumbnailImageAbort(onerror), System.IntPtr.Zero);
                    Capp.Save(request.Response.ResponseContent, ImageFormat.Jpeg);
                    request.Response.ResponseContent.Position = 0;
                }

                // Don't forget to flush!
                writer.Flush();
            }
            catch { };
        }

        public bool onerror()
        {
            return false;
        }

        public string ContentType
        {
            get { return ContentTypes.GetExtensionType(".jpg"); }
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
