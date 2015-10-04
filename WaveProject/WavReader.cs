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
        public Byte[] readFile(Stream stream, out double[] left, out double[] right)
        {
            Byte[] wav;
            left = null;
            right = null;
            BinaryReader reader = new BinaryReader(stream);

            //Read the wave file header from the buffer. 

            int chunkID = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            int fmtID = reader.ReadInt32();
            int fmtSize = reader.ReadInt32();
            int fmtCode = reader.ReadInt16();
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int fmtAvgBPS = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();

            if (fmtSize == 18)
            {
                // Read any extra values
                int fmtExtraSize = reader.ReadInt16();
                reader.ReadBytes(fmtExtraSize);
            }

            int dataID = reader.ReadInt32();
            int dataSize = reader.ReadInt32();
            int num_samples = dataSize / (channels * bitDepth / 8);

            // Store the audio data of the wave file to a byte array. 

            wav = reader.ReadBytes(dataSize);

            //After this you have to split that byte array for each channel (Left, Right)
            //only worrying about dual channel for now
            if (channels == 1)
            {
                left = new double[num_samples];
            }
            else if (channels == 2)
            {
                left = new double[num_samples/2];
                right = new double[num_samples/2];
            }
            int i = 0;
            int pos = 0;
            while (pos < wav.Length)
            {
                left[i] = byteToDouble(wav[pos], wav[pos + 1]);
                pos += 2;
                if (channels == 2)
                {
                    right[i] = byteToDouble(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                i++;

            }


            //Wav supports many channels, so you have to read channel from header
            return wav;
        }

        static double byteToDouble(byte firstByte, byte secondByte)
        {
            short s = (short)((secondByte << 8) | firstByte);
            return s / 32768.0;
        }
    }

    
}
