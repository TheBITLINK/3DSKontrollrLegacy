using InputManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DSKontrollr
{
    public partial class SetHotKey : Form
    {
        string Namee;
        MainWindow SParent;
        public SetHotKey(string name, MainWindow parent)
        {
            InitializeComponent();
            Namee = name;
            SParent = parent;
        }

        private void SetHotKey_Load(object sender, EventArgs e)
        {
            
        }

        private void SetHotKey_KeyUp(object sender, KeyEventArgs e)
        {
            switch (Namee)
            {
                case "aset":
                    SParent.VK_RETURN = e.KeyValue;
                    break;
                case "bset":
                    SParent.VK_BACK = e.KeyValue;
                    break;
                case "upset":
                    SParent.VK_UP = e.KeyValue;
                    break;
                case "dowset":
                    SParent.VK_DOWN = e.KeyValue;
                    break;
                case "leftset":
                    SParent.VK_LEFT = e.KeyValue;
                    break;
                case "rightset":
                    SParent.VK_RIGHT = e.KeyValue;
                    break;
            }
            this.Close();
        }
    }
}
