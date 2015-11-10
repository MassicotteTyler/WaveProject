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

        public Byte[] readFile(Stream stream, out double[] left, out double[] right, out Wav file)
        { 
            Wav waveFile = new Wav();
            Handler hand = new Handler();
            left = null;
            right = null;
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
            //waveFile.head.subChunk2id = reader.ReadInt16();
            //waveFile.head.subChunk2Size = reader.ReadInt16();
            waveFile.head.bitDepth = reader.ReadUInt16();
            waveFile.head.dataID = reader.ReadBytes(4);
            waveFile.head.dataSize = reader.ReadUInt32();


            if (waveFile.head.fmtSize != 16) //only fmt to read for now
            {
                file = null;
                return null; 
            }


            //waveFile.head.dataID = reader.ReadInt32();
            //waveFile.head.dataSize = reader.ReadInt32();
            waveFile.num_samples = (uint)(waveFile.head.dataSize /
                         (waveFile.head.channels * waveFile.head.bitDepth) / 8);


            // Store the audio data of the wave file to a byte array. 

            //for (int j = 0; j < waveFile.head.dataSize / waveFile.head.fmtBlockAlign; j++)
            //{
            //    lData.Add((short)reader.ReadUInt16());
            //    rData.Add((short)reader.ReadUInt16());
            //}

            waveFile.setData(reader.ReadBytes((int)waveFile.head.dataSize/ waveFile.head.fmtBlockAlign));

            //After this you have to split that byte array for each channel (Left, Right)
            //only worrying about dual channel for now
            if (waveFile.head.channels == 1)
            {
                left = new double[waveFile.getData().Length];
            }
            else if (waveFile.head.channels == 2)
            {
                left = new double[waveFile.num_samples/2];
                right = new double[waveFile.num_samples/2];
            }
            // for one channel
            for (int i = 0, pos = 0; pos < waveFile.getData().Length - 2; i++, pos++)
            {
                left[i] = hand.byteToDouble(waveFile.getData()[pos], waveFile.getData()[++pos]);
            }

            file = waveFile;
            //Wav supports many channels, so you have to read channel from header
            return waveFile.getData();
        }

    }

    
}
