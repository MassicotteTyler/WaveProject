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
            left = null;
            right = null;
            BinaryReader reader = new BinaryReader(stream);
            //Read the wave file header from the buffer

            waveFile.head.chunkID = reader.ReadInt32();
            waveFile.head.fileSize = reader.ReadInt32();
            waveFile.head.riffType = reader.ReadInt32();
            waveFile.head.fmtID = reader.ReadInt32();
            waveFile.head.fmtSize = reader.ReadInt32();
            waveFile.head.fmtCode = reader.ReadInt16();
            waveFile.head.channels = reader.ReadInt16();
            waveFile.head.sampleRate = reader.ReadInt32();
            waveFile.head.fmtAvgBPS = reader.ReadInt32();
            waveFile.head.fmtBlockAlign = reader.ReadInt16();
            waveFile.head.subChunk2id = reader.ReadInt16();
            waveFile.head.subChunk2Size = reader.ReadInt16();
            waveFile.head.bitDepth = reader.ReadInt16();

            if (waveFile.head.fmtSize == 18)
            {
                // Read any extra values
                waveFile.head.fmtExtraSize = reader.ReadInt16();
                reader.ReadBytes(waveFile.head.fmtExtraSize);
            }

            waveFile.head.dataID = reader.ReadInt32();
            waveFile.head.dataSize = reader.ReadInt32();
            waveFile.num_samples = (waveFile.head.dataSize / 
                         (waveFile.head.channels * waveFile.head.bitDepth) / 8);

            // Store the audio data of the wave file to a byte array. 

            waveFile.setData(reader.ReadBytes(waveFile.head.dataSize));

            //After this you have to split that byte array for each channel (Left, Right)
            //only worrying about dual channel for now
            if (waveFile.head.channels == 1)
            {
                left = new double[waveFile.num_samples];
            }
            else if (waveFile.head.channels == 2)
            {
                left = new double[waveFile.num_samples/2];
                right = new double[waveFile.num_samples/2];
            }
            int i = 0;
            int pos = 0;
            while (pos < waveFile.getData().Length-1)
            {
                left[i] = byteToDouble(waveFile.getData()[pos], waveFile.getData()[pos + 1]);
                pos += 2;
                if (waveFile.head.channels == 2)
                {
                    right[i] = byteToDouble(waveFile.getData()[pos], waveFile.getData()[pos + 1]);
                    pos += 2;
                }
                i++;

            }

            file = waveFile;
            //Wav supports many channels, so you have to read channel from header
            return waveFile.getData();
        }

        static double byteToDouble(byte firstByte, byte secondByte)
        {
            short s = (short)((secondByte << 8) | firstByte);
            return s / 32768.0;
        }
    }

    
}
