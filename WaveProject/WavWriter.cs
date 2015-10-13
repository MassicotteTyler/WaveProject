using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace WaveProject
{
    class WavWriter
    {
        public void writeFile(Wav file, string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(fileStream);

            //Write the header
            writer.Write(file.head.chunkID);
            writer.Write(file.head.fileSize);
            writer.Write(file.head.riffType);

            //Write the format chunk
            writer.Write(file.head.fmtID);
            writer.Write(file.head.fmtSize);
            writer.Write(file.head.fmtCode);
            writer.Write(file.head.channels);
            writer.Write(file.head.sampleRate);
            writer.Write(file.head.fmtAvgBPS);
            writer.Write(file.head.fmtBlockAlign);
            writer.Write(file.head.bitDepth);

            //Write the data chunk
            writer.Write(file.head.dataID);
            writer.Write(file.head.dataSize);


            byte[] data = file.getData();

            for (int i = 0; i < file.head.dataSize / file.head.fmtBlockAlign; i++)
            {
                if (i < data.Length)
                    writer.Write((ushort)data[i]);
                else
                    writer.Write(0);
            }
            //writer.Seek(4, SeekOrigin.Begin);
            //uint filesize = (uint)writer.BaseStream.Length;
            //writer.Write(filesize - 8);

            writer.Close();
            fileStream.Close();


        }
    }
}
