using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Numerics;

namespace WaveProject
{


    public partial class Form1 : Form
    {

        [DllImport("DFT.dll")]

        public static extern unsafe IntPtr DFTDLL();
        private WavReader reader;

        public Form1()
        {
            InitializeComponent();
            reader = new WavReader();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            chart1.Series["Magnitude"].Points.Clear();
            chart2.Series["Wave"].Points.Clear();

            int n, k, i;
            Random rnd = new Random();
            float[] x = new float[99];
            float[] Xre = new float[99];
            float[] Xim = new float[99];
            float[] P = new float[99];

            for (n = 0; n < 99; n++) x[n] = (float)(2.0 * rnd.Next(-2, 2));

            for (k = 0; k < 99; ++k)
            {
                //Real
                Xre[k] = 0;
                for (n = 0; n < 16; ++n) Xre[k] += (float)(x[n] * Math.Cos(n * k * 6.2832 / 99));

                //Imaginary
                Xim[k] = 0;
                for (n = 0; n < 16; ++n) Xim[k] -= (float)(x[n] * Math.Sin(n * k * 6.2832 / 99));

                P[k] = Xre[k] * Xre[k] + Xim[k] * Xim[k];

            }

            for (i = 1; i < P.Length; i++)
            {
                chart1.Series["Magnitude"].Points.AddXY(i, P[i]);
                chart2.Series["Wave"].Points.AddXY(i, x[i]);
            }
        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        private void menuOpenFile_Click(object sender, EventArgs e)
        { 
            Stream stream = null;
            OpenFileDialog ofd = new OpenFileDialog();
            if (!(ofd.ShowDialog() == DialogResult.Cancel))
            { 
                stream = ofd.OpenFile();
                reader.readFile(stream);
            }
            else
                ofd.Dispose();
        }
    }
}
