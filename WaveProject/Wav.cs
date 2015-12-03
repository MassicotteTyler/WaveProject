using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace WaveProject
{
    /*
    * The Wav class is a reimplementation of the format structure wav files use. A Wav object represents wav data
    * either recordered or opened from a file. The class has methods to manipulate the sample data of the wav while 
    * updating the header values accordingly.
    */
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
            public uint num_samples;
            public double[] mag;
            public double[] selection;
            public double[] data_double;
            public Complex[] df;

            //The wav samples
            private byte[] data;

        public Header head;

        public Wav()
        {
            head = new Header();
            mag = null;
            handle = new Handler();

        }

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
            head.fmtAvgBPS = 11025;
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

        //replaced by updateData. Remove.
        public void setData (byte[] newData)
        {

            data = newData;
            data_double = dataToDouble();
        }


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

        public void paste(double[] pasteData, int index)
        {

            if (pasteData.Length == 0 || Clipboard.GetAudioStream() == null)
                return;

            byte[] byteTemp = new byte[data.Length + pasteData.Length * 2];
            List<byte> bTemp = data.ToList();
            List<double> dTemp = dataToDouble().ToList();

            dTemp.InsertRange(index, pasteData);


            BinaryReader read = new BinaryReader(Clipboard.GetAudioStream());
            byteTemp = read.ReadBytes(byteTemp.Length);
            bTemp.InsertRange(index * 2, byteTemp);


            updateData(bTemp.ToArray());
            data_double = dataToDouble();
        }

        public void paste(int index)
        {

            if (Clipboard.GetAudioStream() == null)
                return;

            BinaryReader read = new BinaryReader(Clipboard.GetAudioStream());
            byte[] byteTemp = read.ReadBytes((int)Clipboard.GetAudioStream().Length);
            List<byte> bTemp = data.ToList();
            bTemp.InsertRange(index * 2, byteTemp);
            updateData(bTemp.ToArray());
            data_double = dataToDouble();
        }

        public double[] cut(int start, int end)
        { 
            double[] temp = copy(start, end);
            double[] cut = new double[data_double.Length - (end - start)];
            List<double> test = data_double.ToList();
            List<byte> lData = data.ToList();
            lData.RemoveRange(2 * start, (2 * end - 2 * start));
            test.RemoveRange(start, end - start);
            updateData(lData.ToArray());
            cut = test.ToArray();

            data_double = cut;
            return temp;
            
        }

        public void delete(int start, int end)
        {
            double[] cut = new double[data_double.Length - (end - start)];
            List<double> test = data_double.ToList();
            List<byte> lData = data.ToList();
            lData.RemoveRange(2 * start, (2 * end - 2 * start));
            test.RemoveRange(start, end - start);
            updateData(lData.ToArray());
        }

        public void updateData(byte[] newData)
        {

            head.fileSize = 36 + (uint)newData.Length;
            head.dataSize = (uint)newData.Length;

            data = newData;
        }
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

        public void trimData()
        {
            int i;
            
            for (i= 0; i < data.Length && !(data[i] == (byte)0); i++);

            byte[] temp = new byte[i];
            Array.Copy(data, temp, i);
            data = temp;
            head.fileSize = (uint)(36 + data.Length);
            head.dataSize = (uint)data.Length;

        }




    }
}
