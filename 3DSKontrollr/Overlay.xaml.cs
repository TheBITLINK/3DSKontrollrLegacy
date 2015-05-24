using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace _3DSKontrollr
{
    /// <summary>
    /// Lógica de interacción para Overlay.xaml
    /// </summary>
    public partial class Overlay : Window
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

        DispatcherTimer dpT2 = new DispatcherTimer();

        public bool PopupStat { get; private set; }
        public Overlay()
        {
            InitializeComponent();
            DispatcherTimer dpT = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            dpT.Tick += dpT_Tick;
            dpT.Start();
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
            dpT2.Tick += dpT2_Tick;
        }

        void dpT2_Tick(object sender, EventArgs e)
        {
            HidePopup();
            dpT2.Stop();
        }

        public enum PopupStatus {
            Wait,
            NDS,
            Warn,
            No
        }

        public void ShowPopup()
        {
            dpT2.Stop();
            popup.Visibility = System.Windows.Visibility.Visible;
            DoubleAnimation dA = new DoubleAnimation() { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200) };
            popup.BeginAnimation(Grid.OpacityProperty, dA);
            popupscale.BeginAnimation(ScaleTransform.ScaleXProperty, dA);
            popupscale.BeginAnimation(ScaleTransform.ScaleYProperty, dA);
            PopupStat = true;
        }

        public void ShowPopup(double Timeout)
        {
            dpT2.Stop();
            popup.Visibility = System.Windows.Visibility.Visible;
            DoubleAnimation dA = new DoubleAnimation() { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200) };
            popup.BeginAnimation(Grid.OpacityProperty, dA);
            popupscale.BeginAnimation(ScaleTransform.ScaleXProperty, dA);
            popupscale.BeginAnimation(ScaleTransform.ScaleYProperty, dA);
            dpT2.Interval = TimeSpan.FromMilliseconds(Timeout);
            dpT2.Start();
        }

        public void PopupTimeout(double Timeout)
        {
            dpT2.Stop();
            dpT2.Interval = TimeSpan.FromMilliseconds(Timeout);
            dpT2.Start();
        }

        public void HidePopup()
        {
            popup.Visibility = System.Windows.Visibility.Visible;
            DoubleAnimation dA = new DoubleAnimation() { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(200) };
            popup.BeginAnimation(Grid.OpacityProperty, dA);
            popupscale.BeginAnimation(ScaleTransform.ScaleXProperty, dA);
            popupscale.BeginAnimation(ScaleTransform.ScaleYProperty, dA);
            PopupStat = false;
        }

        public async void SetPopup(string Text, PopupStatus Status, int Timeout)
        {
            await System.Threading.Tasks.Task.Delay(Timeout);
            SetPopup(Text, Status);
        }

        public void SetPopup(string Text, PopupStatus Status)
        {
            popuptext.Text = Text;
            switch (Status)
            {
                case PopupStatus.NDS:
                    popupring.Visibility = System.Windows.Visibility.Hidden;
                    popup3ds.Visibility = System.Windows.Visibility.Visible;
                    popupwarn.Visibility = System.Windows.Visibility.Hidden;
                    popuptext.Margin = new Thickness() { Right = 24 };
                    break;
                case PopupStatus.Wait:
                    popupring.Visibility = System.Windows.Visibility.Visible;
                    popup3ds.Visibility = System.Windows.Visibility.Hidden;
                    popupwarn.Visibility = System.Windows.Visibility.Hidden;
                    popuptext.Margin = new Thickness() { Right = 24 };
                    break;
                case PopupStatus.Warn:
                    popupring.Visibility = System.Windows.Visibility.Hidden;
                    popup3ds.Visibility = System.Windows.Visibility.Hidden;
                    popupwarn.Visibility = System.Windows.Visibility.Visible;
                    popuptext.Margin = new Thickness() { Right = 24 };
                    break;
                case PopupStatus.No:
                    popupring.Visibility = System.Windows.Visibility.Hidden;
                    popup3ds.Visibility = System.Windows.Visibility.Hidden;
                    popupwarn.Visibility = System.Windows.Visibility.Hidden;
                    popuptext.Margin = new Thickness() { Right = 0};
                    break;
            }
        }

        void dpT_Tick(object sender, EventArgs e)
        {
            RECT rct;
            int handle = GetForegroundWindow();
            var thhh = System.Diagnostics.Process.GetCurrentProcess().Id;
            uint pidd;
            System.IntPtr hann = new IntPtr(handle);
            GetWindowThreadProcessId(hann, out pidd);
            var occ = Convert.ToInt32(pidd);

            if(!GetWindowRect(handle, out rct ))
            {
                return;
            }

            this.Left = rct.Left;
            this.Top = rct.Top;
            this.Width = rct.Right - rct.Left + 1;
            this.Height = rct.Bottom - rct.Top + 1;

            if(thhh != occ)
            {
                bottomgrid.Opacity = 1;
                
            }
            else
            {
                bottomgrid.Opacity = 0;
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
    }

    public static class WindowsServices
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
