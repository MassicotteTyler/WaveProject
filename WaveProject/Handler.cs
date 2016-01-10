//Tyler Massicotte A00855150 2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;



namespace WaveProject
{
    /*****************************************************************
    * The handler class contains all the imported Win32 functions. The 
    * handler class is used to do all win32 recording and playing. It
    * contains Waveformat and Waveheader structs to be used with win32
    * wave functions. The class also handles data processing and
    * algorithms for converting 16bit byte samples to 1 double/sample.
    * Also handles any data processing for copying from the GUI.
    ******************************************************************/
    class Handler
    {
        //Store the values copied from the GUI
        public double[] copyData;

        //Temporay store the data for checking.
        public byte[] recordData;

        //Class to store win32 messages
        private static class Win32_msg
        {
            public const int MMSYSERR_NOERROR = 0;
            public const int MM_WIM_DATA = 0x3c0;
            public const uint WAVE_MAPPER = 4294967295;
            public const int CALLBACK_FUNCTION = 0x0030000;
            public const int WAVE_FORMAT_PCM = 1;
            public const uint WHDR_BEGINLOOP = 0x00000004;
            public const uint WHDR_ENDLOOP = 0x00000008;
        }

        /*****************************************************************
        * WAVEFORMAT is a struct recreated to match the win32 struct of the
        * same name. It's specified in this class so it can be created and
        * passed to the appropriate win32 functions. It represents the first
        * 44 bytes of a wave file, it specifies the format of the wav file
        * for recording and playing.
        ******************************************************************/
        [StructLayout(LayoutKind.Sequential)]
        public struct WAVEFORMAT
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitPerSample;
            public ushort cbSize;
        }

        /*****************************************************************
        * WAVEHDR is a recreation of the WAVEHDR struct in win32. Its a 
        * header for the wave file to be used with recording and playing.
        * InPtr replace the need for pointers to be passed into the function.
        ******************************************************************/
        [StructLayout(LayoutKind.Sequential)]
        public struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public uint dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }


        //Creates the layout of the delegate function. Is used as a callback function pointer for waveIn/waveOut.
        public delegate void RecordingDelegate(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2);

        //Win32 function imports.
        /*****************************************************************
        * The following are all the functions imported from the Win32 library. 
        * IntPtr objects are used to replace the need for pointers. Entry
        * points are used to be able to call the unmanaged code.
        ******************************************************************/
        //Functions to record
        //Add a cleared buffer to be filled with recorded data
        [DllImport("winmm.dll")]
        public static extern int waveInAddBuffer(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint cWaveHdrSize);

        //Add a Waveheader to specify recording/playing flags and data
        [DllImport("winmm.dll")]
        public static extern int waveInPrepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);

        //Function to start recording wave data on the given waveform-audio input device
        [DllImport("winmm.dll")]
        public static extern int waveInStart(IntPtr hWaveIn);

        //The waveInOpen function opens the given waveform-audio input device for recording.
        [DllImport("winmm.dll", EntryPoint = "waveInOpen", SetLastError = true)]
        public static extern int waveInOpen(ref IntPtr t, uint id, ref WAVEFORMAT pwfx, IntPtr dwCallback, int dwInstance, int fdwOpen);

        //This function cleans up the preparation performed by waveInPrepareHeader. Must be called after the device fills up the buffer.
        [DllImport("winmm.dll", EntryPoint = "waveInUnprepareHeader", SetLastError = true)]
        public static extern int waveInUnprepareHeader(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);
        //The waveInStop function stops waveform-audio input.
        [DllImport("winmm.dll", EntryPoint = "waveInStop", SetLastError = true)]
        static extern uint waveInStop(IntPtr hwi);

        //The waveInClose function closes the given waveform-audio input device.
        [DllImport("winmm.dll", EntryPoint = "waveInClose", SetLastError = true)]
        public static extern uint waveInClose(IntPtr hwnd);

        //The waveInReset function stops input on the given waveform-audio input device and resets the current position to zero.
        //Marks the buffer as done.
        [DllImport("winmm.dll", EntryPoint = "waveInReset", SetLastError = true)]
        static extern uint waveInReset(IntPtr hwi);

        //Functions for playing wav files
        //The waveOutOpen function opens the given waveform-audio output device for playback.
        [DllImport("winmm.dll", EntryPoint = "waveOutOpen", SetLastError = true)]
        public static extern int waveOutOpen(ref IntPtr t, uint id, ref WAVEFORMAT pwfx, IntPtr dwCallback, int dwInstance, int fdwOpen);

        //The waveOutPrepareHeader function prepares a waveform-audio data block for playback.
        [DllImport("winmm.dll", EntryPoint = "waveOutPrepareHeader", SetLastError = true)]
        public static extern int waveOutPrepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);

        //The waveOutWrite function sends a data block to the given waveform-audio output device.
        [DllImport("winmm.dll", EntryPoint = "waveOutWrite", SetLastError = true)]
        public static extern int waveOutWrite(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);

        //The waveOutUnprepareHeader function cleans up the preparation performed by the waveOutPrepareHeader function. 
        [DllImport("winmm.dll", EntryPoint = "waveOutUnprepareHeader", SetLastError = true)]
        public static extern int waveOutUnprepareHeader(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);

        //The waveOutClose function closes the given waveform-audio output device.
        [DllImport("winmm.dll", EntryPoint = "waveOutClose", SetLastError = true)]
        public static extern uint waveOutClose(IntPtr hwnd);

        //The waveInStart function starts output on the given waveform-audio input device.
        [DllImport("winmm.dll", EntryPoint = "waveOutStart", SetLastError = true)]
        public static extern int waveOutStart(IntPtr hWaveIn);

        //The waveOutStop function stops output on the given waveform-audio input device.
        [DllImport("winmm.dll", EntryPoint = "waveOutStop", SetLastError = true)]
        static extern uint waveOutStop(IntPtr hwi);

        //The waveOutReset function stops playback on the given waveform-audio output device and resets the current position to zero/
        [DllImport("winmm.dll", EntryPoint = "waveOutReset", SetLastError = true)]
        static extern uint waveOutReset(IntPtr hwi);

        //Delegate to represent a function pointer.
        private Handler.RecordingDelegate FunctionPointer;
        //Replaces the need for hWAveIn
        private IntPtr handle;
        //Pointer to get the indentifier for hWaveOut
        private IntPtr hWaveOut;
        private uint bufferLength;
        private WAVEHDR header;
        private WAVEHDR Outheader;
        //Provides the unmanaged code access to a managed object 
        private GCHandle headerPin;
        private GCHandle bufferPin;
        private GCHandle savePin;
        //Buffer to store recorded data.
        private byte[] buffer;
        //Buffer to store all recoreded data.
        private byte[] save;

        //Calls setupWaveIn() which starts the waveIn procedure.
        public void Record()
        {
            setupWaveIn();
        }


        /*****************************************************************
        * Sets up the header struct and calls in WaveInPrepareHeader. Calls
        * waveInAddBuffer and adds a buffer to store recorded data.
        ******************************************************************/
        private void setupBuffer()
        {
            header.lpData = bufferPin.AddrOfPinnedObject();
            header.dwBufferLength = bufferLength;
            header.dwFlags = 0;
            header.dwBytesRecorded = 0;
            header.dwLoops = 0;
            header.dwUser = IntPtr.Zero;
            header.lpNext = IntPtr.Zero;
            header.reserved = IntPtr.Zero;
            headerPin = GCHandle.Alloc(header, GCHandleType.Pinned);

            int i = Handler.waveInPrepareHeader(this.handle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
            if (i != Win32_msg.MMSYSERR_NOERROR)
                return;

            i = Handler.waveInAddBuffer(handle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
            if (i != Win32_msg.MMSYSERR_NOERROR)
                return;



        }

        /*****************************************************************
        * Setup wave in setups the format struct as well as prepares the
        * buffer. It clears the handle calls waveInOpen to start the recording
        * procedure. The buffer is setup and finally waveInStart is called 
        * to start receiving input on the specified audio device.
        ******************************************************************/
        private void setupWaveIn()
        {
            handle = new IntPtr();
            FunctionPointer = this.callbackWaveIn;
            Handler.WAVEFORMAT format;
            format.wFormatTag = 1; //WAVE_FORMAT_PCM
            format.nChannels = 1;
            format.nSamplesPerSec = 11025;
            format.wBitPerSample = 16;
            //format.nBlockAlign = Convert.ToUInt16(format.nChannels *(format.wBitPerSample >> 3));
            format.nBlockAlign = 2;
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;
            bufferLength = 11025; //format.nAvgBytesPerSec /800 16384
            buffer = new byte[bufferLength];
            save = null;
            bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            format.cbSize = 0;
            //WAVE_MAPPER

            int i = Handler.waveInOpen(ref handle, Win32_msg.WAVE_MAPPER, ref format, Marshal.GetFunctionPointerForDelegate(FunctionPointer),
                                        0, Win32_msg.CALLBACK_FUNCTION);
            if (i != Win32_msg.MMSYSERR_NOERROR)
            {
                //Error
                return;
            }

            setupBuffer();
            i = Handler.waveInStart(handle);
            if (i != Win32_msg.MMSYSERR_NOERROR)
            {
                //Error
                return;
            }

        }

        /*****************************************************************
        * Play takes in a wav object to output. The save buffer is assigned
        * the data of the wav object. The hWaveOut handle is cleared. The
        * format header is setup from the wav header values. waveOutOpen
        * is then called to start the audio output procedure. The output
        * buffer is then setup where waveOutWrite is called.
        ******************************************************************/
        public void play(Wav wav)
        {
            save = wav.getData() ;
            savePin = GCHandle.Alloc(save, GCHandleType.Pinned);
            hWaveOut = new IntPtr();
            FunctionPointer = this.callbackWaveOut;
            Handler.WAVEFORMAT format;
            format.wFormatTag = 1; //WAVE_FORMAT_PCM
            format.nChannels = wav.head.channels;
            format.nSamplesPerSec = wav.head.sampleRate;
            format.wBitPerSample = wav.head.bitDepth;
            format.nBlockAlign = Convert.ToUInt16(format.nChannels * (format.wBitPerSample >> 3));
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;
            format.cbSize = 0;

            int i = Handler.waveOutOpen(ref hWaveOut, Win32_msg.WAVE_MAPPER, ref format, Marshal.GetFunctionPointerForDelegate(FunctionPointer),
                                        0, Win32_msg.CALLBACK_FUNCTION);

            if (i != Win32_msg.MMSYSERR_NOERROR)
                return;

            setupOutbuffer();

        }

        /*****************************************************************
        * Sets up the header to be passed into waveout. Then waveOutWrite
        * is called to start output of the audio data.
        ******************************************************************/
        private void setupOutbuffer()
        {
            Outheader.lpData = savePin.AddrOfPinnedObject();
            Outheader.dwBufferLength = (uint)save.Length;
            Outheader.dwFlags = Win32_msg.WHDR_BEGINLOOP | Win32_msg.WHDR_ENDLOOP;
            Outheader.dwBytesRecorded = 0;
            Outheader.dwLoops = 1;
            Outheader.lpNext = IntPtr.Zero;
            Outheader.reserved = IntPtr.Zero;

            int i = Handler.waveOutPrepareHeader(hWaveOut, ref Outheader, Convert.ToUInt32(Marshal.SizeOf(Outheader)));
            if (i != Win32_msg.MMSYSERR_NOERROR)
                return;

            i = Handler.waveOutWrite(hWaveOut, ref Outheader, Convert.ToUInt32(Marshal.SizeOf(Outheader)));
            if (i != Win32_msg.MMSYSERR_NOERROR)
                return;
        }

        /*****************************************************************
        * callbackWaveIn is used as the callback function for the waveIn
        * procedure. The only message handled is MM_WIM_DATA. When that 
        * message is sent all the data recored is saved to the save buffer.
        * The GCHandles are then reallocated and the recording buffered is 
        * cleared. The header is unprepared and setupBuffer() is called to
        * setup a new header and to continue recording.
        ******************************************************************/
        private void callbackWaveIn(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2)
        {

            if (message == Win32_msg.MM_WIM_DATA)
            {
                if (save != null)
                {
                    List<byte> temp = save.ToList();
                    temp.AddRange(buffer.ToList().GetRange(0, (int)wavehdr.dwBytesRecorded));
                    save = temp.ToArray();
                }
                else
                    save = buffer;

                savePin = GCHandle.Alloc(save, GCHandleType.Pinned);
                buffer = new byte[bufferLength];
                bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                int i = waveInUnprepareHeader(deviceHandle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
                if (i != Win32_msg.MMSYSERR_NOERROR)
                {
                    //Error
                    return;
                }

                setupBuffer();
            }
        }

        /*****************************************************************
        * callbackWaveOut is used as the callback function for the waveOut
        * procedure. The only message handled is MM_WIM_DATA. When that
        * message is sent the header is unprepared and setupOutBuffer is 
        * called to prepare a new header and to continue output.
        ******************************************************************/
        private void callbackWaveOut(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2)
        {

            if (message == Win32_msg.MM_WIM_DATA)
            {
                int i = waveInUnprepareHeader(deviceHandle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
                if (i != Win32_msg.MMSYSERR_NOERROR)
                    return;
                setupOutbuffer();
            }
        }


        /*****************************************************************
        * data_stop stops the wavein procedure from receiving any further
        * input. It then closes the audio device it received input from.
        * It checks if any data was recorded and returns null if no data was.
        * If data was recorded it frees the GCHandle pins and returns the
        * save buffer of recorded data.
        ******************************************************************/
        public byte[] data_stop()
        {
            waveInStop(handle);
            waveInClose(handle);

            if (save == null)
            {
                return null;
            }
            bufferPin.Free();
            savePin.Free();
            

            return save;
        }

        /*****************************************************************
        * byteToDouble converts recorded wave bytes to a double. 
        * This function is to be used with 16 bit samples. This is because
        * with 16 bit samples you need to convert 2 bytes to a double to
        * keep the ration the same. 2 bytes to 1 sample, 1 double to 1 sample.
        * The way this is done is by bit shifting the second bit to the right
        * by eight and then bitwise OR it with the first byte to create a 
        * 16 bit int of the sample. That short is then divided by its range
        * 32768 to get a value between 1 and -1. Byte2 has to be bit shifted
        * because of the Endian.
        * Example of the conversion
        * Byte1 91 = 10011111 Byte2 159 = 10110011
        * right shift Byte2 = 10110011 00000000
        * bitwise or with Byte1 = 1011011 10011111 = 23455
        * divide by 32768 to make the value a double between 1, -1
        * Final result = 0.715789794921875
        ******************************************************************/
        public double byteToDouble(byte firstByte, byte SecondByte)
        {
            short s = (short)((SecondByte << 8) | firstByte);
            return s / 32768.0;
        }

        /************************************************************************
        * doubleToBytes converts from 1 double/sample to 2 bytes (16bit sample).
        * It just reverses the algorithm used in byteToDouble but takes advantage 
        * of truncating data when getting the first byte value. It takes in the
        * double sample and two our parameters for the bytes to return.
        ************************************************************************/
        public void doubleToBytes(double sample, out byte b1, out byte b2)
        {
            short s = (short) (sample * 32768);
            b1 = (byte) s;
            b2 =  (byte) (s >> 8);
        }
    }
}
