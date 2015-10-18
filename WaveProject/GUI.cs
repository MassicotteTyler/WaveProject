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

        [DllImport("Win32.dll")]

        public static extern unsafe short* RECORDDLL();
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
            double[] mag = null;
            double[] ima = null;
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
            if (wav == null)
            {
                MessageBox.Show("Only 16bit PCM wav files", "Wav format error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            real = dft._dft(real, real.Length,out mag);
            drawChart(real);
            //drawChart(wav.getData());
            wav.real = real;
            wav.ima = ima;
            wav.mag = mag;

            for (int j = 1; j < mag.Length/2; j++)
            {
                chart1.Series["Magnitude"].Points.AddXY(j, mag[j]);
            }
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog dia = new SaveFileDialog();
            DialogResult result = new DialogResult();
            if ((result = dia.ShowDialog()) == DialogResult.Cancel)
                return;
            string fileName = dia.FileName;
            writer.writeFile(wav, fileName);
        }

        private void zoomButton_Click(object sender, EventArgs e)
        {
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = (int)chart2.ChartAreas[0].CursorX.SelectionStart;
            int end = (int)chart2.ChartAreas[0].CursorX.SelectionEnd;
            if ((start - end) == 0)
                return;
            if (start < end)
                handle.copyData = wav.copy(start, end);
            else
                handle.copyData = wav.copy(end, start);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (handle.copyData == null)
                return;

            int index = (int)chart2.ChartAreas[0].CursorX.SelectionStart;

            wav.paste(handle.copyData, index);
            
            drawChart(wav.real);

        }

        private void drawChart(double[] data)
        {
            chart2.Series["Wave"].Points.Clear();
            for (int i = 1; i < data.Length / 2; i++)
            {
                chart2.Series["Wave"].Points.AddXY(i, data[i]);
            }
        }

        private void drawChart(byte[] data)
        {
            chart2.Series["Wave"].Points.Clear();
            for (int i = 1; i < data.Length / 2; i++)
            {
                chart2.Series["Wave"].Points.AddXY(i, data[i]);
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = (int)chart2.ChartAreas[0].CursorX.SelectionStart;
            int end = (int)chart2.ChartAreas[0].CursorX.SelectionEnd;

            if ((start - end) == 0)
                return;

            if (start < end)
                handle.copyData = wav.cut(start, end);
            else
                handle.copyData = wav.cut(end, start);
            drawChart(wav.real);
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            unsafe
            {
                short* data;
                data = RECORDDLL();
            }
        }
    }
}
