//Tyler Massicotte A00855150 2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace WaveProject
{
    /********************************************************
    * The WavWriter class handles the writing of Wav files
    * to the specified file location. It uses a file stream
    * as well as a binary writer to write the data to the disk
    **********************************************************/
    class WavWriter
    {
        /********************************************************
        * Takes in a Wav object and string depicting the file path.
        * A binary writer is used to write the wav file to the disk.
        * THe header file is written first, and then sample data 
        * portion of the wav object.
        **********************************************************/
        public static void writeFile(Wav file, string filePath)
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
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(file.head.dataSize);

            //Write the samples
            writer.Write(file.getData());

            //Close the writer and filestream
            writer.Close();
            fileStream.Close();


        }
    }
}
