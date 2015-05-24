using InputManager;
using MiniHttpd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TAFactory.IconPack;

namespace _3DSKontrollr
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public const int KEYEVENTF_KEYUP = 0x0002;
        public int VK_UP = 0x26;
        public int VK_DOWN = 0x28;
        public int VK_LEFT = 0x25;
        public int VK_RIGHT = 0x27;
        public int VK_RETURN = 0x0D;
        public int VK_BACK = 0x08;

        [DllImport("user32.dll")]
        static extern int MapVirtualKey(int uCode, uint uMapType);

        const uint MAPVK_VK_TO_CHAR = 0x02;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        [DllImport("user32.dll")]

        static extern int GetForegroundWindow();

        [DllImport("user32.dll")]

        static extern int GetWindowText(int hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        _3DSKontrollr.Server.HTTP httpSrv;
        bool n3dsfc = false;
        string n3dsip { get; set; }
        bool connected = false;
        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);
        int handle = 0;
        uint pidd;
        DispatcherTimer dpT = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
        DispatcherTimer ConnectionTimeout = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
        bool ConStab;
        DoubleAnimation HdA = new DoubleAnimation() { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(200) };
        DoubleAnimation SdA = new DoubleAnimation() { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200) };
        Overlay ovr = new Overlay();
        public MainWindow()
        {
            httpSrv = new _3DSKontrollr.Server.HTTP(3000) { Log = TextWriter.Null, LogConnections = false, LogRequests = false };
            ((VirtualDirectory)httpSrv.Root).AddDirectory("src", Environment.CurrentDirectory);
            InitializeComponent();
            httpSrv.ClientConnected += httpSrv_ClientConnected;
            httpSrv.RequestReceived += httpSrv_RequestReceived;
            dpT.Tick += dpT_Tick;
            dpT.Start();
            ovr.Show();
            ovr.SetPopup("Esperando Conexión", Overlay.PopupStatus.Wait);
            ovr.ShowPopup();
            ConnectionTimeout.Tick += ConnectionTimeout_Tick;
            connectsc.BeginAnimation(Grid.OpacityProperty, SdA);
        }

         void ConnectionTimeout_Tick(object sender, EventArgs e)
         {
             if(!ConStab)
             {
                 n3dsip = "";
                 connected = false;
                 connectedsc.Visibility = System.Windows.Visibility.Hidden;
                 connectedsc.BeginAnimation(Grid.OpacityProperty, HdA);
                 connectsc.Visibility = System.Windows.Visibility.Visible;
                 connectsc.BeginAnimation(Grid.OpacityProperty, SdA);
                 n3dsfc = false;
                 awaitng.Content = "Escanea el Código desde tu 3DS para conectarte.";
                 qrCd.Visibility = System.Windows.Visibility.Visible;
                 n3ds.Visibility = System.Windows.Visibility.Hidden;
                 ovr.SetPopup("Conexión Perdida!", Overlay.PopupStatus.Warn);
                 if (!ovr.PopupStat)
                 {
                     ovr.ShowPopup();
                 }
                 ovr.SetPopup("Esperando Conexión...", Overlay.PopupStatus.Wait, 2000);
                 ConStab = false;
                 ConnectionTimeout.Stop();
             }
             ConStab = false;
         }

        async void dpT_Tick(object sender, EventArgs e)
        {
            string OldBuff = Buff.ToString(); 
            handle = GetForegroundWindow();
            if (GetWindowText(handle, Buff, nChars) > 0)
                {
                    if (Buff.ToString() != OldBuff)
                    {
                    StreamWriter ewe = File.CreateText("./Client/CurrWindow.txt");
                    try
                    {
                        await (ewe.WriteLineAsync(Buff.ToString()));
                    }
                    catch
                    {
                        Buff.Clear();
                    }
                    ewe.Close();
                    System.IntPtr hann = new IntPtr(handle);
                    GetWindowThreadProcessId(hann, out pidd);
                    Process curr = Process.GetProcessById(Convert.ToInt32(pidd));
                    System.Drawing.Icon ico;
                    try
                    {
                        ico = IconHelper.ExtractIcon(curr.MainModule.FileName, 0);
                    }
                    catch
                    {
                        try
                        {
                            ico = System.Drawing.Icon.ExtractAssociatedIcon(curr.MainModule.FileName);
                        }
                        catch
                        {
                            ico = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                            Console.Beep();
                        }
                    }
                    System.Drawing.Bitmap owo = ico.ToBitmap();
                    owo.Save("./Client/CurrentWindowIcon.png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        void httpSrv_RequestReceived(object sender, RequestEventArgs e)
        {
            ConStab = true;
            if (!n3dsfc)
            {
                if (e.Request.UserAgent.Contains("Nintendo 3DS"))
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        awaitng.Content = "Toca el botón \"Conectar\" en la pantalla táctil.";
                        qrCd.Visibility = System.Windows.Visibility.Hidden;
                        n3ds.Visibility = System.Windows.Visibility.Visible;
                        n3dsfc = true;
                    }));
                }
                if (e.Request.UserAgent.Contains("New Nintendo 3DS"))
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        awaitng.Content = "La New Nintendo 3DS no está soportada de momento.";
                    }));
                }
            }
                if (connected && e.HttpClient.RemoteAddress != n3dsip)
                {
                    e.Request.Response.ResponseContent = new MemoryStream();
                    e.Request.Response.BeginChunkedOutput();
                }
                string RelUri = (e.Request.Uri.ToString().Split('/'))[3];
                if (RelUri == "connect")
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                        {
                            n3dsip = e.HttpClient.RemoteAddress;
                            connected = true;
                            connectedsc.Visibility = System.Windows.Visibility.Visible;
                            connectedsc.BeginAnimation(Grid.OpacityProperty, SdA);
                            connectsc.Visibility = System.Windows.Visibility.Hidden;
                            connectsc.BeginAnimation(Grid.OpacityProperty, HdA);
                            n3dsipld.Content = "IP: " + n3dsip;
                            n3dslangld.Content = "Idioma: " + ((e.Request.UserAgent.Split('(')[1]).Split(')')[0]).Split(';')[3];
                            n3dsregionld.Content = "Región: " + e.Request.UserAgent.Split('.')[3];
                            ovr.SetPopup("Conexión Establecida!", Overlay.PopupStatus.NDS);
                            if (!ovr.PopupStat)
                            {
                                ovr.ShowPopup();
                            }
                            ovr.PopupTimeout(2000);
                            ConnectionTimeout.Start();
                        }));
                }
                if (RelUri.Contains(".press"))
                {
                    if (connected && e.HttpClient.RemoteAddress == n3dsip)
                    {
                        PressKey(RelUri);
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            gPressKey(RelUri);
                            ovr.gPressKey(RelUri);
                        }
                ));
                    }
                }
                if (RelUri.Contains(".release"))
                {
                    if (connected && e.HttpClient.RemoteAddress == n3dsip)
                    {
                        ReleaseKey(RelUri);
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            gReleaseKey(RelUri);
                            ovr.gReleaseKey(RelUri);
                        }
                ));
                    }
                }
                if (RelUri.Contains("touch-"))
                {
                    // Posiciones Relativas al táctil de la 3DS.
                    double RelX = Convert.ToDouble(RelUri.Split('-')[1]);
                    double RelY = Convert.ToDouble(RelUri.Split('-')[2]);
                    // Convertimos esos valores relativos a valores absolutos en la pantalla del PC.
                    // De momento, solo la pantalla Principal estará soportada.
                    System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
                    double AbsX = mainScreen.Bounds.Width * RelX;
                    double AbsY = mainScreen.Bounds.Height * RelY;
                    // Cambiamos de lugar el cursor.
                    System.Windows.Forms.Cursor mC = System.Windows.Forms.Cursor.Current;
                    System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle(System.Drawing.Point.Empty, mainScreen.Bounds.Size);
                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(Convert.ToInt32(AbsX), Convert.ToInt32(AbsY));
                }
                if (RelUri == "upload")
                {
                    //MessageBox.Show(e.Request.Headers.Get("Content-Type"));
                    FileStream fS = new FileStream("Test.jpg", FileMode.Create);
                    e.Request.PostData.CopyTo(fS);
                }
                if (RelUri == "disconnect")
                {
                    if (connected && e.HttpClient.RemoteAddress == n3dsip)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                            {
                                n3dsip = "";
                                connected = false;
                                connectedsc.Visibility = System.Windows.Visibility.Hidden;
                                connectsc.BeginAnimation(Grid.OpacityProperty, SdA);
                                connectsc.Visibility = System.Windows.Visibility.Visible;
                                connectedsc.BeginAnimation(Grid.OpacityProperty, HdA);
                                n3dsfc = false;
                                awaitng.Content = "Escanea el Código desde tu 3DS para conectarte.";
                                qrCd.Visibility = System.Windows.Visibility.Visible;
                                n3ds.Visibility = System.Windows.Visibility.Hidden;
                                ovr.SetPopup("Conexión Perdida!", Overlay.PopupStatus.Warn);
                                if(!ovr.PopupStat)
                                {
                                    ovr.ShowPopup();
                                }
                                ovr.SetPopup("Esperando Conexión...", Overlay.PopupStatus.Wait, 2000);
                                ConStab = false;
                                ConnectionTimeout.Stop();
                            }));
                    }
                }
            }

        public void gPressKey(string data)
        {
            string rawkey = data.Split('.')[0];
            switch (rawkey)
            {
                case "a":
                    aind.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "b":
                    bind.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "up":
                    upind.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "down":
                    dowind.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "left":
                    leftind.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "right":
                    rightind.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        public void gReleaseKey(string data)
        {
            string rawkey = data.Split('.')[0];
            switch (rawkey)
            {
                case "a":
                    aind.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case "b":
                    bind.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case "up":
                    upind.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case "down":
                    dowind.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case "left":
                    leftind.Visibility = System.Windows.Visibility.Hidden;
                    break;
                case "right":
                    rightind.Visibility = System.Windows.Visibility.Hidden;
                    break;
            }
        }

        public void PressKey(string data)
        {
            string rawkey = data.Split('.')[0];
            switch (rawkey)
            {
                case "a":
                    InputManager.Keyboard.KeyDown((System.Windows.Forms.Keys)VK_RETURN);
                    break;
                case "b":
                    InputManager.Keyboard.KeyDown((System.Windows.Forms.Keys)VK_BACK);
                    break;
                case "up":
                    InputManager.Keyboard.KeyDown((System.Windows.Forms.Keys)VK_UP);
                    break;
                case "down":
                    InputManager.Keyboard.KeyDown((System.Windows.Forms.Keys)VK_DOWN);
                    break;
                case "left":
                    InputManager.Keyboard.KeyDown((System.Windows.Forms.Keys)VK_LEFT);
                    break;
                case "right":
                    InputManager.Keyboard.KeyDown((System.Windows.Forms.Keys)VK_RIGHT);
                    break;
            }

        }

        public void ReleaseKey(string data)
        {
            string rawkey = data.Split('.')[0];
            switch (rawkey)
            {
                case "a":
                    InputManager.Keyboard.KeyUp((System.Windows.Forms.Keys)VK_RETURN);
                    break;
                case "b":
                    InputManager.Keyboard.KeyUp((System.Windows.Forms.Keys)VK_BACK);
                    break;
                case "up":
                    InputManager.Keyboard.KeyUp((System.Windows.Forms.Keys)VK_UP);
                    break;
                case "down":
                    InputManager.Keyboard.KeyUp((System.Windows.Forms.Keys)VK_DOWN);
                    break;
                case "left":
                    InputManager.Keyboard.KeyUp((System.Windows.Forms.Keys)VK_LEFT);
                    break;
                case "right":
                    InputManager.Keyboard.KeyUp((System.Windows.Forms.Keys)VK_RIGHT);
                    break;
            }
        }

        void httpSrv_ClientConnected(object sender, ClientEventArgs e)
        {
            
        }

        public string server_ip { get; private set; }

        private void IP_Loaded(object sender, RoutedEventArgs e)
        {

            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress a in localIPs)
            {
                if (a.AddressFamily == AddressFamily.InterNetwork)
                {
                    server_ip = a.ToString();
                }
            }
            ((Label)sender).Content = server_ip;
            string MainPage = "<!DOCTYPE html>"+
                "<html><head><title>3DSKontrollr</title><meta name=\"viewport\" content=\"width=400\">"+
                "<script src=\"./src/Client/handler.js\"></script><link rel=\"stylesheet\" href=\"/src/client/Styles.css\"></head><body>" +
                "<img style=\"width: 400px; height: 220px;\" id=\"currsc\"><div id=\"bottomscreen\"><div id=\"bartop\"><img src=\"./src/client/3ds-icon.png\"></img>3DSKontrollr<input type=\"file\" id=\"fileinput\"></input><button id=\"connection\">Conectar</button><img style=\"display:none\" id=\"photoupd\" src=\"./src/client/photosbtn.png\"></div>" +
                "<p>&nbsp;</p><center><img style=\"width: 32px; height: 32px;\" id=\"currprocicn\"><p id=\"currproc\"></p></center><p style=\"position: fixed; bottom:16px; margin: 0px; color: #FF0000;\">DEBUG MODE</p><p style=\"position: fixed; bottom: 0px; margin: 0px; font-size: 80%;\">1294923183</p></div><div id=\"toucharea\" style=\"position: fixed; width: 100%; height: 212px; top: 252px;\"></body></html>";
            _3DSKontrollr.Server.DynamicContent.IndexPage iP = new Server.DynamicContent.IndexPage("index.html", httpSrv.Root, MainPage);
            ((VirtualDirectory)httpSrv.Root).AddFile(iP);
            httpSrv.IndexPage = iP;
            _3DSKontrollr.Server.DynamicContent.Screen sC = new Server.DynamicContent.Screen("sc.jpg", httpSrv.Root);
            ((VirtualDirectory)httpSrv.Root).AddFile(sC);
    }

        private async void Image_Loaded(object sender, RoutedEventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(50);
            System.Drawing.Image im = Pitchfork.QRGenerator.QRCodeImageGenerator.GetQRCode("http://" + server_ip + ":3000/");
            using(MemoryStream memory = new MemoryStream())
            {
                im.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.StreamSource = memory;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();
                ((Image)sender).Source = bmi;
            }            
            //MessagingToolkit.QRCode.Crypt.RsEncode rE = 
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        string setting = "";

        private void allset_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow currmw = this;
            string nem = ((Rectangle)sender).Name;
            SetHotKey shk = new SetHotKey(nem, currmw);
            shk.Show();
        }
    }
}
