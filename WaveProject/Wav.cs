//Tyler Massicotte A00855150 2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace WaveProject
{
    /************************************************************************************************************
    * The Wav class is a reimplementation of the format structure wav files use. A Wav object represents wav data
    * either recorded or read from a file. The class has methods to manipulate the sample data of the wav while 
    * updating the header values accordingly.
    ************************************************************************************************************/
    class Wav
    {

        private Handler handle;

        /* Inner class Header represents the first 44 bytes of a wav file. All the format data is contained inside. */
        public class Header
        {
            /*The header bytes of the wav file */
            public byte[] chunkID;
            public uint fileSize;
            public byte[] riffType;
            public byte[] fmtID;
            public uint fmtSize;
            public ushort fmtCode;
            public ushort channels;
            public uint sampleRate;
            public uint fmtAvgBPS;
            public ushort fmtBlockAlign;
            public ushort bitDepth;

            public byte[] dataID;
            public uint dataSize;


        }

        public double[] mag;
        public double[] selection;
        public double[] data_double;
        public Complex[] df;

        //The wav samples
        private byte[] data;

        //The header file to store the format values.
        public Header head;

        /********************************************************
        * Default constructor. Initializes the header and the 
        * handler objects.
        **********************************************************/
        public Wav()
        {
            head = new Header();
            handle = new Handler();

        }

        /********************************************************
        * Constructor: Takes in a byte array of new samples and
        * creates the corresponding header file. Uses preset values
        * for the format, channels and sample rate as this is the 
        * default for recording.
        **********************************************************/
        public Wav(byte[] newData)
        {
            head = new Header();

            head.chunkID = System.Text.Encoding.ASCII.GetBytes("RIFF");
            head.fileSize = 36 + (uint)newData.Length;
            head.riffType = System.Text.Encoding.ASCII.GetBytes("WAVE");
            head.fmtID = System.Text.Encoding.ASCII.GetBytes("fmt ");
            head.fmtSize = 16;
            head.fmtCode = 1;
            head.channels = 1;
            head.sampleRate = 11025;
            head.fmtAvgBPS = 22050;
            head.fmtBlockAlign = 2;
            head.bitDepth = 16;

            head.dataID = System.Text.Encoding.ASCII.GetBytes("data");
            head.dataSize = (uint)newData.Length;

            data = newData;

            data_double = dataToDouble();


        }

        /*
        * Converts the samples from bytes to doubles. This function is for use with 16bit samples.
        * Because we are working with 16bit samples there are two bytes for sample. Therefore we
        * convert two bytes into one double.
        * Returns the double array of converted values.
        */
        public double[] dataToDouble()
        {
            handle = new Handler();
            double[] result = new double[data.Length / 2];
            for (int i = 0, pos = 0; pos < data.Length - 2; i++, pos++)
            {
                result[i] = handle.byteToDouble(data[pos], data[++pos]); //make method static
            }

            return result;
        }

        //Returns the wav files samples in bytes.
        public byte[] getData()
        {
            return data;
        }

        /********************************************************
        * Gets called when a wav file is being read from a disk.
        * sets the assigns newData to the private memeber data. 
        * Then generates the double version of the byte array.
        **********************************************************/
        public void setData (byte[] newData)
        {

            data = newData;
            data_double = dataToDouble();
        }


        /********************************************************
        * Copy saves all the data from the start and until the end
        * index from the original byte array and stores it in the
        * Clipboard.
        **********************************************************/
        public double[] copy(int start, int end)
        {
            double[] temp = new double[(end - start)];
            byte[] byteTemp = new byte[(2* end) - (2* start)];

            for (int i = 0; i < end - start; i++)
            {
                temp[i] = data_double[start + i];

            }

            for (int i = 0; i < (2 * end) - (2 * start); i++)
            {
                byteTemp[i] = data[(2 * start) + i];
            }
            Clipboard.SetAudio(byteTemp);
            return temp;
        }

        /********************************************************
        * Paste grabs the data from the Clipboard and inserts it
        * at the specified index and updates the header filesize.
        **********************************************************/
        public void paste(int index)
        {
            //If there is no data to paste
            if (Clipboard.GetAudioStream() == null)
                return;

            BinaryReader read = new BinaryReader(Clipboard.GetAudioStream());
            byte[] byteTemp = read.ReadBytes((int)Clipboard.GetAudioStream().Length);
            List<byte> bTemp = data.ToList();
            bTemp.InsertRange(index * 2, byteTemp);
            updateData(bTemp.ToArray());
            data_double = dataToDouble();
        }

        /********************************************************
        * Cut calls copy on the data selected between start and end.
        * It then removes the data selected and updates the header
        * format accordingly.
        **********************************************************/
        public double[] cut(int start, int end)
        { 
            double[] temp = copy(start, end);
            List<byte> lData = data.ToList();
            lData.RemoveRange(2 * start, (2 * end - 2 * start));
            updateData(lData.ToArray());
            data_double = dataToDouble();
            return temp;
            
        }

        /********************************************************
        * Delete removes all data specified in the range between
        * start and end. Then it updates the header information
        * accordingly.
        **********************************************************/
        public void delete(int start, int end)
        {
            double[] cut = new double[data_double.Length - (end - start)];
            List<byte> lData = data.ToList();
            lData.RemoveRange(2 * start, (2 * end - 2 * start));
            updateData(lData.ToArray());
            data_double = dataToDouble();
        }

        /********************************************************
        * Takes in an array of bytes representing the new wav data.
        * it assigns the new data to the data member while also 
        * updating the file and data size in the format header.
        **********************************************************/
        public void updateData(byte[] newData)
        {
            head.fileSize = 36 + (uint)newData.Length;
            head.dataSize = (uint)newData.Length;
            data = newData;
        }


        /********************************************************
        * toArray takes all the data from the header and adds it 
        * to a byte list. It then adds all the sample data to the
        * list. The list is then converted to an array. This array
        * will be a properly formatted .wav file ready to save or 
        * play. It returns the .wav byte array created by the list.
        **********************************************************/
        public byte[] toArray()
        {
            List<byte> arr = new List<byte>();
            arr.AddRange(head.chunkID);
            arr.AddRange(BitConverter.GetBytes(head.fileSize));
            arr.AddRange(head.riffType);
            arr.AddRange(head.fmtID);
            arr.AddRange(BitConverter.GetBytes(head.fmtSize));
            arr.AddRange(BitConverter.GetBytes(head.fmtCode));
            arr.AddRange(BitConverter.GetBytes(head.channels));
            arr.AddRange(BitConverter.GetBytes(head.sampleRate));
            arr.AddRange(BitConverter.GetBytes(head.fmtAvgBPS));
            arr.AddRange(BitConverter.GetBytes(head.fmtBlockAlign));
            arr.AddRange(BitConverter.GetBytes(head.bitDepth));

            arr.AddRange(head.dataID);
            arr.AddRange(BitConverter.GetBytes(head.dataSize));
            arr.AddRange(data);

            return arr.ToArray();
        }
    }
}
