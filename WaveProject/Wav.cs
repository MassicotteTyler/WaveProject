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
            public int chunkID;
            public int fileSize;
            public int riffType;
            public int fmtID;
            public int fmtSize;
            public int fmtCode;
            public int channels;
            public int sampleRate;
            public int fmtAvgBPS;
            public int fmtBlockAlign;
            public int subChunk2id;
            public int subChunk2Size;
            public int bitDepth;

            public int fmtExtraSize;

            public int dataID;
            public int dataSize;


        }
            public int num_samples;

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
