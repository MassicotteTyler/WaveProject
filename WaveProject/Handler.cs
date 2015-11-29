using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;



namespace WaveProject
{

    public class Handler
    {
        private DFT dft;
        public double[] copyData;
        public byte[] recordData;

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

        public delegate void RecordingDelegate(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2);

        [DllImport("winmm.dll")]
        public static extern int waveInAddBuffer(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint cWaveHdrSize);
        [DllImport("winmm.dll")]
        public static extern int waveInPrepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);
        [DllImport("winmm.dll")]
        public static extern int waveInStart(IntPtr hWaveIn);

        [DllImport("winmm.dll", EntryPoint = "waveInOpen", SetLastError = true)]
        public static extern int waveInOpen(ref IntPtr t, uint id, ref WAVEFORMAT pwfx, IntPtr dwCallback, int dwInstance, int fdwOpen);
        [DllImport("winmm.dll", EntryPoint = "waveInUnprepareHeader", SetLastError = true)]
        public static extern int waveInUnprepareHeader(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);
        [DllImport("winmm.dll", EntryPoint = "waveInStop", SetLastError = true)]
        static extern uint waveInStop(IntPtr hwi);
        [DllImport("winmm.dll", EntryPoint = "waveInClose", SetLastError = true)]
        public static extern uint waveInClose(IntPtr hwnd);
        [DllImport("winmm.dll", EntryPoint = "waveInReset", SetLastError = true)]
        static extern uint waveInReset(IntPtr hwi);

        private Handler.RecordingDelegate waveIn;
        private IntPtr handle;
        private uint bufferLength;
        private WAVEHDR header;
        private GCHandle headerPin;
        private GCHandle bufferPin;
        private byte[] buffer;

        public byte[] Record()
        {
            setupWaveIn();
            return buffer;
        }

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
            if (i != 0)
            {
                //Error in waveIn
                return;
            }

            i = Handler.waveInAddBuffer(handle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
            if (i != 0)
            {
                //Error om waveInAdd
                return;

            }

        }

        private void setupWaveIn()
        {
            handle = new IntPtr();
            waveIn = this.callbackWaveIn;
            Handler.WAVEFORMAT format;
            format.wFormatTag = 1; //WAVE_FORMAT_PCM
            format.nChannels = 1;
            format.nSamplesPerSec = 11025;
            format.wBitPerSample = 8;
            format.nBlockAlign = Convert.ToUInt16(format.nChannels *(format.wBitPerSample >> 3));
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;
            bufferLength = 40000; //format.nAvgBytesPerSec /800 16384
            buffer = new byte[bufferLength];
            bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            format.cbSize = 0;
            //WAVE_MAPPER

            int i = Handler.waveInOpen(ref handle, 4294967295, ref format, Marshal.GetFunctionPointerForDelegate(waveIn), 0, 0x0030000);
            if (i != 0)
            {
                //Error
                return;
            }

            setupBuffer();
            i = Handler.waveInStart(handle);
            if (i != 0)
            {
                //stuff
            }

        }


        private void callbackWaveIn(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2)
        {

            if (message == 0x3BF) //WIM_DATA
            {
                //if (wavehdr.dwBytesRecorded > 0)
                //{
                //    foreach (byte buf in buffer)
                //    {
                //        //do stuff
                //    }
                //}

                int i = waveInUnprepareHeader(deviceHandle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
                if (i != 0) //MMSYSERR_NOERROR
                {
                    //Error
                    return;
                }
                setupBuffer();
            }
        }


        public void stop()
        {
            waveInStop(handle);
            waveInReset(handle);
            waveInClose(handle);
        }




        public Handler()
        {

        }

        public double byteToDouble(byte firstByte, byte SecondByte)
        {
            short s = (short)((SecondByte << 8) | firstByte);
            return s / 32768.0;
        }

        public double[] bufferByteToDouble(byte[] values)
        {
            double[] result = new double[values.Length / 8];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }

        public byte[] doubleToBytes(double[] values)
        {

            byte[] result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }

        
    }
}
