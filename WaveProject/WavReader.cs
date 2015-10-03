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
        public Byte[] readFile(Stream stream)
        {
            Byte[] wav;
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


            // Store the audio data of the wave file to a byte array. 

            wav = reader.ReadBytes(dataSize);

            //After this you have to split that byte array for each channel (Left, Right)
            //Wav supports many channels, so you have to read channel from header
            return wav;
        }
    }
}
