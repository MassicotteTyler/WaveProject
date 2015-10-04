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


    public partial class GUI : Form
    {

        [DllImport("DFT.dll")]

        public static extern unsafe IntPtr DFTDLL();
        private WavReader reader;
        private Handler handle;


        public GUI()
        {
            InitializeComponent();
            reader = new WavReader();
            handle = new Handler();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

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
            double[] real;
            double[] ima;
            byte[] samples = null;
            OpenFileDialog ofd = new OpenFileDialog();
            if (!(ofd.ShowDialog() == DialogResult.Cancel))
            { 
                stream = ofd.OpenFile();
                if (stream.CanRead)
                samples = reader.readFile(stream,out real,out ima);
            }
            else
                ofd.Dispose();
            for (int i = 1; i < samples.Length; i++)
            {
                chart1.Series["Magnitude"].Points.AddXY(i, samples[i]);
                chart2.Series["Wave"].Points.AddXY(i, samples[i]);
            }
            chart2.ChartAreas[0].AxisX.ScaleView.Size = 100;
        }
    }
}
