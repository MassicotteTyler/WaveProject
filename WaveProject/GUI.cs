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
        private WavWriter writer;
        private Handler handle;
        private DFT dft;
        private Wav wav;


        public GUI()
        {
            InitializeComponent();
            reader = new WavReader();
            writer = new WavWriter();
            handle = new Handler();
            dft = new DFT();
            chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = true;

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
            double[] real = null;
            double[] ima;
            byte[] samples = null;
            OpenFileDialog ofd = new OpenFileDialog();
            if (!(ofd.ShowDialog() == DialogResult.Cancel))
            {
                stream = ofd.OpenFile();
                if (stream.CanRead)
                    samples = reader.readFile(stream, out real, out ima, out wav);
            }
            else
            { 
                ofd.Dispose();
                return;
            }
            //dft._dft(real, real.Length);
            for (int i = 1; i < samples.Length; i++)
            {
                chart1.Series["Magnitude"].Points.AddXY(i, samples[i]);
                chart2.Series["Wave"].Points.AddXY(i, real[i]);
            }
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog dia = new SaveFileDialog();
            DialogResult result = new DialogResult();
            result = dia.ShowDialog();
            string fileName = dia.FileName;
            writer.writeFile(wav, fileName);
        }
    }
}
