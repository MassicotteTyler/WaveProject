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
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
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
            byte[] control = wav.getData();
            byte[] test = handle.doubleToBytes(real);
            drawChart(real);
            
            wav.real = real;
            wav.ima = ima;
            


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
            int start;
            int end;
            getSelection(out start, out end);

            if ((start - end) == 0)
                return;
           handle.copyData = wav.copy(start, end);
                
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {


            int index = (int)chart2.ChartAreas[0].CursorX.SelectionStart;

            if (handle.copyData == null)
            {
                //Handle if Clipboard is )
                wav.paste(index);
                drawChart(wav.real);
                return;
            }

            wav.paste(handle.copyData, index);
            drawChart(wav.real);
        }

        private void drawChart(double[] data)
        {
            chart2.Series["Wave"].Points.Clear();
            for (int i = 1; i < data.Length; i+=5)
            {
                chart2.Series["Wave"].Points.AddXY(i, data[i]);
            }
            chart2.ChartAreas[0].AxisX.Minimum = 0;
        }

        private void drawChart(byte[] data)
        {
            chart2.Series["Wave"].Points.Clear();
            for (int i = 1; i < data.Length; i+=100)
            {
                chart2.Series["Wave"].Points.AddXY(i, data[i] - 128);
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start;
            int end;

            getSelection(out start, out end);

            if ((start - end) == 0)
                return;

            handle.copyData = wav.cut(start, end);

            drawChart(wav.real);
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            recordButton.Enabled = false;
            stopButton.Enabled = true;
            //t1 = new Thread(new ThreadStart(call_record));
            //t1.Start();
            handle.Record();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            handle.recordData = handle.data_stop();
            recordButton.Enabled = true;
            stopButton.Enabled = false;
            if (handle.recordData != null)
            {
                wav.setData(handle.recordData);
                Wav nWav = new Wav(handle.recordData);
                wav = nWav;
                //wav.trimData();
                //drawChart(wav.getData());
                drawChart(wav.dataToDouble());

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
            //MemoryStream stream = new MemoryStream(data);
            //SoundPlayer player = new SoundPlayer(stream);
            //player.Play();

            handle.play(wav);



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

        private void hanningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Complex[] result = dft.Dft(filter.hanning_window(dft.iDft(wav.df)));
            int max = (int)chart1.ChartAreas[0].AxisY.Maximum;
            Complex[] result = dft.Dft(filter.hanning_window(wav.ima));
            double[] mag = Complex.Mag(result);

            chart1.Series["Magnitude"].Points.Clear();
            for (int j = 1; j < mag.Length ; j++)
            {
                chart1.Series["Magnitude"].Points.AddXY(j, mag[j]);
            }

            chart1.ChartAreas[0].AxisY.Maximum = max;
            //chart1.ChartAreas[0].AxisX.Minimum = 1;
            
        }

        private void showTemporalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wav.getData() == null)
                return;
            List<double> test = wav.real.ToList();
            List<double> nData;
            int start, end;
            getSelection(out start, out end);
            

            if (start < end)
                nData = test.GetRange(start, (end - start));
            else
                nData = test.GetRange(end, (start - end));

            wav.df = dft.Dft(nData.ToArray());
            wav.ima = nData.ToArray();
            wav.mag = Complex.Mag(wav.df);

            chart1.Series["Magnitude"].Points.Clear();
            for (int j = 1; j < wav.mag.Length; j++)
            {
                chart1.Series["Magnitude"].Points.AddXY(j, wav.mag[j]);
            }
        }

        private void getSelection(out int start, out int end)
        {
            start = (int)chart2.ChartAreas[0].CursorX.SelectionStart - 1;
            end = (int)chart2.ChartAreas[0].CursorX.SelectionEnd - 1;
            if (start < 0) start = 0;
            if (end < 0) end = 0;

            if (start > end)
            {
                start = start + end;
                end = start - end;
                start = start - end;
            }    
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Complex[] result = dft.Dft(wav.ima);
            double[] mag = Complex.Mag(result);

            chart1.Series["Magnitude"].Points.Clear();
            for (int j = 1; j < mag.Length; j++)
            {
                chart1.Series["Magnitude"].Points.AddXY(j, mag[j]);
            }

            //chart1.ChartAreas[0].AxisY.Maximum = 600;
            //chart1.ChartAreas[0].AxisX.Minimum = 1;
        }
    }
}
