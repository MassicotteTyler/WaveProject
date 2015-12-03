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

        private Handler handle;
        private Wav wav;

        public GUI()
        {
            wav = new Wav();
            InitializeComponent();
            handle = new Handler();
            setup_charts();
        }

        private void setup_charts()
        {
            chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = false;

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Wav files | *.wav";
            if (!(ofd.ShowDialog() == DialogResult.Cancel))
            {
                stream = ofd.OpenFile();
                if (stream.CanRead)
                    WavReader.readFile(stream, out wav);
            }
            else
            { 
                ofd.Dispose();
                return;
            }

            if (wav == null)
            {
                MessageBox.Show("Only FMT 16 PCM wav files", "Wav format error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            toggle_buttons(false);

            drawChart(wav.dataToDouble());

            toggle_buttons(true);

        }

        private void toggle_buttons(bool toggle)
        {
            playButton.Enabled = toggle;
            recordButton.Enabled = toggle;
            zoomButton.Enabled = toggle;
            selectButton.Enabled = toggle;
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
        }

        private void saveAs_Click(object sender, EventArgs e)
        {
            if (wav.getData() == null)
            {
                MessageBox.Show("No data to save", "Save error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog dia = new SaveFileDialog();
            dia.Filter = "Wav file | *.wav";

            if ((dia.ShowDialog()) == DialogResult.Cancel)
                return;
            string fileName = dia.FileName;
            WavWriter.writeFile(wav, fileName);

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
            int start, end;
            getSelection(out start, out end);
            handle.copyData = wav.copy(start, end);
                
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = (int)chart2.ChartAreas[0].CursorX.SelectionEnd;

            wav.paste(index);
            drawChart(wav.data_double);
            return;

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

        private void drawDft(double[] mag)
        {
            chart1.Series["Magnitude"].Points.Clear();
            for (int j = 1; j < mag.Length; j++)
            {
                chart1.Series["Magnitude"].Points.AddXY(j, mag[j]);
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start, end;

            getSelection(out start, out end);

            handle.copyData = wav.cut(start, end);

            drawChart(wav.data_double);
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            recordButton.Enabled = false;
            playButton.Enabled = false;
            stopButton.Enabled = true;
            
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
                wav = new Wav(handle.recordData);
                drawChart(wav.dataToDouble());
                playButton.Enabled = true;

            }
            else
            {
                MessageBox.Show("No data was recorded", "Recording error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }         
        }

        private void call_record()
        {
            byte[] temp = handle.Record();
            handle.recordData = temp;
        }

        private void playData(Byte[] data)
        {
            handle.play(wav);
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            byte[] data = wav.toArray();
            playData(data);

        }

        private void lowPassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[] filt = Filter.createFilter((int)chart1.ChartAreas[0].CursorX.SelectionStart, wav.mag);
            double[] fSample = Filter.apply_filter(wav.dataToDouble(), filt);
            drawChart(fSample);
        }

        private void GUI_Load(object sender, EventArgs e)
        {

        }

        private void hanningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Complex[] result = dft.Dft(filter.hanning_window(dft.iDft(wav.df)));
            Complex[] result = DFT.Dft(Filter.hanning_window(wav.selection));
            double[] mag = Complex.Mag(result);

            drawDft(mag);
            
        }

        private void showTemporalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wav.getData() == null)
                return;
            List<double> test = wav.dataToDouble().ToList();
            List<double> nData;
            int start, end;
            getSelection(out start, out end);
            
            nData = test.GetRange(start, (end - start));

            wav.df = DFT.Dft(nData.ToArray());
            wav.selection = nData.ToArray();
            wav.mag = Complex.Mag(wav.df);

            drawDft(wav.mag);
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
            Complex[] result = DFT.Dft(wav.data_double);
            double[] mag = Complex.Mag(result);

            drawDft(mag);

        }



        private void triangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Complex[] result = DFT.Dft(wav.selection);
            double[] mag = Complex.Mag(result);

            drawDft(mag);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C))
            {
                copyToolStripMenuItem_Click(this, null);
                return true;
            }

            if (keyData == (Keys.Control | Keys.X))
            {
                cutToolStripMenuItem_Click(this, null);
                return true;
            }
            if (keyData == (Keys.Control | Keys.V))
            {
                pasteToolStripMenuItem_Click(this, null);
                return true;
            }

            if (keyData == (Keys.Delete))
            {
                deleteToolStripMenuItem_Click(this, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start, end;
            getSelection(out start, out end);
            wav.delete(start, end);
            drawChart(wav.dataToDouble());
        }


    }
}
