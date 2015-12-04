//Tyler Massicotte A00855150 2015
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

/*****************************************************************
* GUI is the child window that gets created by the main menu.
* This window has a menu bar to open or save Wav files. Two chart
* areas inside the window draw the Wav files data.
******************************************************************/
    public partial class GUI : Form
    {

        private Handler handle;
        private Wav wav;

        //0 for Rectangle, 1 for Triangle, 2 for Hanning
        private int window_selection;

        /*****************************************************************
        * Default constructor. Initilizates a blank wav file, and creates 
        * the handler object. As well as initalize any UI components.
        ******************************************************************/
        public GUI()
        {
            wav = new Wav();
            handle = new Handler();
            InitializeComponent();
            setup_charts();
            //Default to rectangle window
            window_selection = 0;
        }


        /*****************************************************************
        * Initializes the starting chart settings. The charts cannot zoom.
        ******************************************************************/
        private void setup_charts()
        {
            chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = false;

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
        }

        /*****************************************************************
        * Gets called when chart1 gets clicked.  Doesn't do anything but 
        * had to be overridden. 
        ******************************************************************/
        private void chart1_Click(object sender, EventArgs e)
        {
        }

        /*****************************************************************
        * Gets called when chart2 gets clicked.  Doesn't do anything but 
        * had to be overridden. 
        ******************************************************************/
        private void chart2_Click(object sender, EventArgs e)
        {
        }

        /*****************************************************************
        * Gets called when the open file dialog triggers FileOk. Is empty
        * but had to be overridden.
        ******************************************************************/
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        /*****************************************************************
        * Gets called when the menuStrip gets clicked. Is empty but had to
        * be overridden.
        ******************************************************************/
        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        /*****************************************************************
        * Gets called when the tool bar button Open gets clicked. Method 
        * creates an open file dialog and prompts the user to load a file.
        * If the user selects a valid file it will be read in user the
        * WavReader class. Once the data has been loaded it draws the Wav
        * files data to the chart.
        ******************************************************************/
        private void menuOpenFile_Click(object sender, EventArgs e)
        {
            Stream stream = null;
            OpenFileDialog ofd = new OpenFileDialog();

            //Sets the default filter to display .wav files 
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

        /*****************************************************************
        * Takes in a boolean value and applies it to a list of buttons.
        * It is used to easily toggle all the buttons on or off.
        ******************************************************************/
        private void toggle_buttons(bool toggle)
        {
            playButton.Enabled = toggle;
            recordButton.Enabled = toggle;
            zoomButton.Enabled = toggle;
            selectButton.Enabled = toggle;
        }

        /*****************************************************************
        * Gets called when the save file dialog triggers FileOk. Is empty
        * but had to be overridden.
        ******************************************************************/
        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
        }

        /*****************************************************************
        * Gets called when the tool bar button Save gets clicked. Method 
        * creates a save file dialog and prompts the user to save a file.
        * If the user selects a valid file location it will be written to
        * the disk by the WavWriter.
        ******************************************************************/
        private void saveAs_Click(object sender, EventArgs e)
        {
            //If there is no data to save, don't save it.
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

        /*****************************************************************
        * Gets called when the zoom button gets clicked. Enables the
        * charts to zoom when the use makes a selection.
        ******************************************************************/
        private void zoomButton_Click(object sender, EventArgs e)
        {
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
        }

        /*****************************************************************
        * Gets called when the select button gets clicked. Disables the
        * zoom feature of the charts and enables the user to select an area.
        ******************************************************************/
        private void selectButton_Click(object sender, EventArgs e)
        {
            chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = false;
        }

        /*****************************************************************
        * Gets called when the copy menu button gets clicked. It gets the 
        * users selection and calls the corresponding handle function for
        * copying.
        ******************************************************************/
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start, end;
            getSelection(out start, out end);
            handle.copyData = wav.copy(start, end);
                
        }

        /*****************************************************************
        * Gets called when the paste tool bar button is clicked. It gets
        * the user selection and calls the Wav paste function to paste the
        * data at the selected index. It then calls drawchart to update the
        * charts data.
        ******************************************************************/
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = (int)chart2.ChartAreas[0].CursorX.SelectionEnd;

            wav.paste(index);
            drawChart(wav.data_double);
            return;

        }


        /*****************************************************************
        * Takes in a double array of Wave samples and plots them to the 
        * chart.
        ******************************************************************/
        private void drawChart(double[] data)
        {
            chart2.Series["Wave"].Points.Clear();
            for (int i = 1; i < data.Length; i+=5)
            {
                chart2.Series["Wave"].Points.AddXY(i, data[i]);
            }
            chart2.ChartAreas[0].AxisX.Minimum = 0;
        }


        /*****************************************************************
        * Takes in a double array of samples that have been passed through
        * a discrete Fourier transform. It clears any previously drawn data
        * and plots the new data.
        ******************************************************************/
        private void drawDft(double[] mag)
        {
            chart1.Series["Magnitude"].Points.Clear();
            for (int j = 1; j < mag.Length; j++)
            {
                chart1.Series["Magnitude"].Points.AddXY(j, mag[j]);
            }
        }


        /*****************************************************************
        * Gets called when the user clicked on the cut tool bar item. It
        * gets the users selection of the frequency chart and calls wav.cut
        * and storing the copied data into the clipboard and locally.
        * It then redraws the changed Wave data.
        ******************************************************************/
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start, end;

            getSelection(out start, out end);

            handle.copyData = wav.cut(start, end);

            drawChart(wav.data_double);
        }


        /*****************************************************************
        * Gets called when the user clicks the Record button. It disables
        * the record, play and stop button and sends the signal to the win32
        * class to start recording.
        ******************************************************************/
        private void recordButton_Click(object sender, EventArgs e)
        {
            recordButton.Enabled = false;
            playButton.Enabled = false;
            stopButton.Enabled = true;
            
            handle.Record();
        }

        /*****************************************************************
        * Gets called when the user clicks the stop button. It sends the 
        * message to the win32 handle to stop recording and acquires the
        * recorded samples. The buttons are then re-enabled and if data was
        * recorded a new wav file is created and drawn to the chart. If no
        * data was recorded an error message is displayed.
        ******************************************************************/
        private void stopButton_Click(object sender, EventArgs e)
        {
            handle.recordData = handle.data_stop();
            recordButton.Enabled = true;
            stopButton.Enabled = false;

            if (handle.recordData != null)
            { 
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


        /*****************************************************************
        * Gets called when the user clicks the play button. It takes the 
        * current Wav file and passes it to the win32 handle to be played.
        ******************************************************************/
        private void playButton_Click(object sender, EventArgs e)
        {
            handle.play(wav);
        }


        /*****************************************************************
        * Gets called when the user clicks the low pass filter item in the
        * menu strip. It creates the low pass filter based on the users 
        * selected of the temporal domain. It then applies the filter to
        * the Wave data and updates the data inside the wav object. It then
        * redraws the changed data to the chart.
        ******************************************************************/
        private void lowPassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[] filt = Filter.createFilter((int)chart1.ChartAreas[0].CursorX.SelectionStart, wav.mag);
            double[] fSample = Filter.apply_filter(wav.dataToDouble(), filt);

            byte[] samples = new byte[fSample.Length * 2];

            for (int i = 0, pos = 0; pos < samples.Length; i++, pos++)
            {
                handle.doubleToBytes(fSample[i], out samples[pos], out samples[++pos]);
            }
            wav.updateData(samples);
            drawChart(fSample);
        }

        /*****************************************************************
        * Gets called when the GUI object has load. It is used as an action
        * listening but its body is empty.
        ******************************************************************/
        private void GUI_Load(object sender, EventArgs e)
        {

        }


        /*****************************************************************
        * Gets called when the user clicks the hanning menu strip item.
        * It sets the window selection variable to represent hanning.
        ******************************************************************/
        private void hanningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            window_selection = 2;
        }


        /*****************************************************************
        * Gets called when the user clicks the DFT menu strip item. Checks
        * if there is data to preform DFT on. If not it returns silently.
        * It then gets the data selected by the user, applies the selected
        * window function. Then is passed through a Discrete Fourier
        * Transform. The DFT data is then drawn to the chart.
        ******************************************************************/
        private void showTemporalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wav.getData() == null)
                return;
            List<double> test = wav.dataToDouble().ToList();
            double[] nData;
            int start, end;
            getSelection(out start, out end);
            
            nData = test.GetRange(start, (end - start)).ToArray();

            wav.df = DFT.Dft(apply_window(nData));
            wav.selection = nData.ToArray();
            wav.mag = Complex.Mag(wav.df);

            drawDft(wav.mag);
        }

        private double[] apply_window(double[] samples)
        {
            switch(window_selection)
            {
                case 1:
                    return Filter.triangle_window(samples);
                case 2:
                    return Filter.hanning_window(samples);             
            }

            return Filter.rectangle_window(samples) ;
        }


        /*****************************************************************
        * Get selection gets the values of the chart the user has selected.
        * it takes in two into to assign the start and end value to. Which
        * ever value is the smallest is applied to start, and the largest
        * to end.
        ******************************************************************/
        private void getSelection(out int start, out int end)
        {
            start = (int)chart2.ChartAreas[0].CursorX.SelectionStart - 1;
            end = (int)chart2.ChartAreas[0].CursorX.SelectionEnd - 1;
            if (start < 0) start = 0;
            if (end < 0) end = 0;

            //Two variable swap
            if (start > end)
            {
                start = start + end;
                end = start - end;
                start = start - end;
            }    
        }


        /*****************************************************************
        * Gets called when the user clicks the hanning menu strip item.
        * It sets the window selection variable to represent rectangle
        * window function.
        ******************************************************************/
        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            window_selection = 0;
        }


        /*****************************************************************
        * Gets called when the user clicks the hanning menu strip item.
        * It sets the window selection variable to represent triangle
        * window function.
        ******************************************************************/
        private void triangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            window_selection = 1;
        }


        /*****************************************************************
        * Process the key presses made while this window is in focus.
        * If Control + C are pressed, Copy is called.
        * If Control + X are pressed, Cut is called.
        * If Control + V are pressed, Paste is called.
        * If Delete is pressed, Delete is called.
        ******************************************************************/
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


        /*****************************************************************
        * Gets Called when the user clicks on the delete menu item. Calls
        * the wav delete function on the data selected by the user and draws
        * the new data to the chart.
        ******************************************************************/
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start, end;
            getSelection(out start, out end);
            wav.delete(start, end);
            drawChart(wav.dataToDouble());
        }


    }
}
