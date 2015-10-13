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

            byte[] data;

        public Header head;

        public Wav()
        {
            head = new Header();
        }

        public byte[] getData()
        {
            return data;
        }

        public void setData (byte[] newData)
        {
            data = newData;
        }

    }
}
