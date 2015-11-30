using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace WaveProject
{
    class Wav
    {

        private Handler handle;

        public class Header
        {
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

            public int fmtExtraSize;

            public byte[] dataID;
            public uint dataSize;


        }
            public uint num_samples;
            public double[] real;
            public double[] ima;
            public double[] mag;

            byte[] data;

        public Header head;

        public Wav()
        {
            head = new Header();
            real = null;
            ima = null;
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
            head.fmtBlockAlign = 1;
            head.bitDepth = 8;

            head.dataID = System.Text.Encoding.ASCII.GetBytes("data");
            head.dataSize = (uint)newData.Length;



            data = newData;

            real = dataToDouble();


        }

        private double[] dataToDouble()
        {
            handle = new Handler();
            double[] result = new double[data.Length / 2];
            for (int i = 0, pos = 0; pos < data.Length - 2; i++, pos++)
            {
                result[i] = handle.byteToDouble(data[pos], data[++pos]);
            }

            return result;
        }

        public byte[] getData()
        {
            return data;
        }

        public void setData (byte[] newData)
        {

            data = newData;
        }

        public double[] copy(int start, int end)
        {
            double[] temp = new double[(end - start)];
            byte[] byteTemp = new byte[(2* end) - (2* start)];

            for (int i = 0; i < end - start; i++)
            {
                temp[i] = real[start + i];

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
            double[] temp = new double[real.Length + pasteData.Length];
            byte[] byteTemp = new byte[data.Length + pasteData.Length * 2];
            List<byte> bTemp = data.ToList();

            int i;
            for (i = 0; i < index; i++ )
            {
                temp[i] = real[i];
            }

            pasteData.CopyTo(temp, index);



            BinaryReader read = new BinaryReader(Clipboard.GetAudioStream());
            byteTemp = read.ReadBytes(byteTemp.Length);
            bTemp.InsertRange(index * 2, byteTemp);


            updateData(bTemp.ToArray());
            real = dataToDouble();
        }

        public double[] cut(int start, int end)
        {

            double[] temp = copy(start, end);
            double[] cut = new double[real.Length - (end - start)];
            int i, k, j;
            for (i = 0; i < start; i++)
            {
                cut[i] = real[i];
            }
            for (j = i, k = 0; j < cut.Length; j++, k++)
            {
                cut[j] = real[end + k];
            }

            real = cut;
            return temp;
            
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
