using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveProject
{
    class Wav
    {
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
            for (int i = 0; i < end - start; i++)
            {
                temp[i] = real[start + i];

            }

            return temp;
        }

        public void paste(double[] pasteData, int index)
        {
            double[] temp = new double[real.Length + pasteData.Length];
            int i;
            for (i = 0; i < index; i++ )
            {
                temp[i] = real[i];
            }

            pasteData.CopyTo(temp, index);

            for (int j = index + pasteData.Length; j < real.Length; j++)
            {
                temp[j] = real[i++];
            }
            real = temp;
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


    }
}
