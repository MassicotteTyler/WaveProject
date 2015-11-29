﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Media;

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
        private Filter filter;
        Thread t1;

        public GUI()
        {
            wav = new Wav();
            InitializeComponent();
            reader = new WavReader();
            writer = new WavWriter();
            handle = new Handler();
            t1 = new Thread(new ThreadStart(call_record));
            filter = new Filter();
            dft = new DFT();
            chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = false;

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
            stopButton.Enabled = false;
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
            playButton.Enabled = false;
            recordButton.Enabled = false;
            zoomButton.Enabled = false;
            selectButton.Enabled = false;

            double[] temp = handle.bufferByteToDouble(wav.getData());
            Complex[] samp = dft.Dft(real);
            drawChart(real);
            mag = Complex.Mag(samp);
            wav.real = real;
            wav.ima = ima;
            wav.mag = mag;

            for (int j = 1; j < mag.Length/2; j++)
            {
                chart1.Series["Magnitude"].Points.AddXY(j, mag[j]);
            }
            playButton.Enabled = true;
            recordButton.Enabled = true;
            zoomButton.Enabled = true;
            selectButton.Enabled = true;

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
            wav = new Wav(handle.doubleToBytes(wav.real));
            //drawChart(wav.real);
            drawChart(wav.getData());

        }

        private void drawChart(double[] data)
        {
            chart2.Series["Wave"].Points.Clear();
            for (int i = 1; i < data.Length/ 2; i++)
            {
                chart2.Series["Wave"].Points.AddXY(i, data[i]);
            }
        }

        private void drawChart(byte[] data)
        {
            chart2.Series["Wave"].Points.Clear();
            for (int i = 1; i < data.Length/2; i+=100)
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
            recordButton.Enabled = false;
            stopButton.Enabled = true;
            t1 = new Thread(new ThreadStart(call_record));
            t1.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            handle.stop();
            recordButton.Enabled = true;
            stopButton.Enabled = false;
            if (handle.recordData != null)
            {
                wav.setData(handle.recordData);
                Wav nWav = new Wav(handle.recordData);
                wav = nWav;
                wav.trimData();
                drawChart(wav.getData());

            }
            else
                Console.Write("Record data null");
            
        }

        private void call_record()
        {
            byte[] temp = handle.Record();
            handle.recordData = temp;
        }

        private void playData(Byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            SoundPlayer player = new SoundPlayer(stream);
            //SoundPlayer player = new SoundPlayer("C:\\Users\\a00855150\\Desktop\\Wave_files\\sample15.wav");
            player.Play();



        }

        private void playButton_Click(object sender, EventArgs e)
        { 
                byte[] data = wav.toArray();
                playData(data);

        }

        private void lowPassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[] filt = dft.createFilter((int)chart1.ChartAreas[0].CursorX.SelectionStart, wav.mag);
            //for (int i = 0, k = 0; i < temp.Length - 2; i++, k++)
            //{
            //    dTemp[k] = handle.byteToDouble(temp[i], temp[++i]);
            //}
            double[] fSample = filter.apply_filter(wav.real, filt);
            //temp = handle.doubleToBytes(fSample);
            drawChart(fSample);
        }

        private void GUI_Load(object sender, EventArgs e)
        {

        }
    }
}
