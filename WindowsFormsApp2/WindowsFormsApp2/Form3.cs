using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Microsoft.Win32;

namespace WindowsFormsApp2
{
    public partial class Form3 : Form
    {

        static void SendCOM(char b)
        {
            byte[] buf = new byte[1];
            buf[0] = Convert.ToByte(b);
            Data.port.Write(buf, 0, 1);
        }

        static void SpecialFrame(int a)
        {
            SendCOM(Convert.ToChar(54));
            SendCOM(Convert.ToChar(a));
            SendCOM(Convert.ToChar(54));
        }
        public Form3(string file_name)
        {
            this.label2.Text = "Вы хотите принять файл : " + file_name;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SpecialFrame(3);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
