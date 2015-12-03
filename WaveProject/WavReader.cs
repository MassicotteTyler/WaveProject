/* WavReader reads in wave file data and converts it to a Wav class file. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WaveProject
{
    class WavReader
    {

        public static void readFile(Stream stream, out Wav file)
        { 
            Wav waveFile = new Wav();
            Handler hand = new Handler();
            BinaryReader reader = new BinaryReader(stream);
            //Read the wave file header from the buffer

            waveFile.head.chunkID = reader.ReadBytes(4);
            waveFile.head.fileSize = reader.ReadUInt32();
            waveFile.head.riffType = reader.ReadBytes(4);
            waveFile.head.fmtID = reader.ReadBytes(4);
            waveFile.head.fmtSize = reader.ReadUInt32();
            waveFile.head.fmtCode = reader.ReadUInt16();
            waveFile.head.channels = reader.ReadUInt16();
            waveFile.head.sampleRate = reader.ReadUInt32();
            waveFile.head.fmtAvgBPS = reader.ReadUInt32();
            waveFile.head.fmtBlockAlign = reader.ReadUInt16();
            waveFile.head.bitDepth = reader.ReadUInt16();
            waveFile.head.dataID = reader.ReadBytes(4);
            waveFile.head.dataSize = reader.ReadUInt32();


            if (waveFile.head.fmtSize != 16) //only fmt to read for now
            {
                file = null;
                return; 
            }


            waveFile.num_samples = (uint)(waveFile.head.dataSize /
                         (waveFile.head.channels * waveFile.head.bitDepth) / 8);

            waveFile.setData(reader.ReadBytes((int)waveFile.head.dataSize));
            file = waveFile;
        }

    }

    
}
