//Tyler Massicotte A00855150 2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WaveProject
{
    /********************************************************
    * The WavReader class is responsible for reading a wav file
    * from a specified location. It creates a Wav object and stores
    * the data read into it.
    **********************************************************/
    class WavReader
    {

        /********************************************************
        * readFile takes in a filestream and has an out parameter
        * wav object. It reads in the specified wav file. Allocating
        * the first 44 bytes to as the format header. It checks the
        * fmtsize as this program only handles FMT 16 and PCM.
        * once all the data has been read the wav file is assigned.
        **********************************************************/
        public static void readFile(Stream stream, out Wav file)
        { 
            Wav waveFile = new Wav();
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

            //Read the sample data and add it to the wav object.
            waveFile.setData(reader.ReadBytes((int)waveFile.head.dataSize));

            //Assign the wav object to the out parameter.
            file = waveFile;
        }

    }

    
}
