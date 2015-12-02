﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;



namespace WaveProject
{

    class Handler
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

        [DllImport("winmm.dll", EntryPoint = "waveOutOpen", SetLastError = true)]
        public static extern int waveOutOpen(ref IntPtr t, uint id, ref WAVEFORMAT pwfx, IntPtr dwCallback, int dwInstance, int fdwOpen);
        [DllImport("winmm.dll", EntryPoint = "waveOutPrepareHeader", SetLastError = true)]
        public static extern int waveOutPrepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);
        [DllImport("winmm.dll", EntryPoint = "waveOutWrite", SetLastError = true)]
        public static extern int waveOutWrite(IntPtr hWaveIn, ref WAVEHDR lpWaveHdr, uint Size);
        [DllImport("winmm.dll", EntryPoint = "waveOutUnprepareHeader", SetLastError = true)]
        public static extern int waveOutUnprepareHeader(IntPtr hwi, ref WAVEHDR pwh, uint cbwh);
        [DllImport("winmm.dll", EntryPoint = "waveOutClose", SetLastError = true)]
        public static extern uint waveOutClose(IntPtr hwnd);
        [DllImport("winmm.dll", EntryPoint = "waveOutStart", SetLastError = true)]
        public static extern int waveOutStart(IntPtr hWaveIn);
        [DllImport("winmm.dll", EntryPoint = "waveOutStop", SetLastError = true)]
        static extern uint waveOutStop(IntPtr hwi);
        [DllImport("winmm.dll", EntryPoint = "waveOutReset", SetLastError = true)]
        static extern uint waveOutReset(IntPtr hwi);

        private Handler.RecordingDelegate waveIn;
        private IntPtr handle;
        private IntPtr hWaveOut;
        private uint bufferLength;
        private WAVEHDR header;
        private WAVEHDR Outheader;
        private GCHandle headerPin;
        private GCHandle bufferPin;
        private GCHandle savePin;
        private byte[] buffer;
        private byte[] save;

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
            format.wBitPerSample = 16;
            //format.nBlockAlign = Convert.ToUInt16(format.nChannels *(format.wBitPerSample >> 3));
            format.nBlockAlign = 2;
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;
            bufferLength = 22050; //format.nAvgBytesPerSec /800 16384
            buffer = new byte[bufferLength];
            save = null;
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

        public void play(Wav wav)
        {
            save = wav.getData() ;
            savePin = GCHandle.Alloc(save, GCHandleType.Pinned);
            hWaveOut = new IntPtr();
            waveIn = this.callbackWaveOut;
            Handler.WAVEFORMAT format;
            format.wFormatTag = 1; //WAVE_FORMAT_PCM
            format.nChannels = wav.head.channels;
            format.nSamplesPerSec = wav.head.sampleRate;
            format.wBitPerSample = wav.head.bitDepth;
            format.nBlockAlign = Convert.ToUInt16(format.nChannels * (format.wBitPerSample >> 3));
            format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;
            savePin = GCHandle.Alloc(save, GCHandleType.Pinned);
            format.cbSize = 0;
            //WAVE_MAPPER

            //new test
            //byte[] header = new byte[44];
            //Array.Copy(wav.getData(), 0, header, 0, 44);

            int i = Handler.waveOutOpen(ref hWaveOut, 4294967295, ref format, Marshal.GetFunctionPointerForDelegate(waveIn), 0, 0x0030000);
            if (i != 0)
            {
                //Error
                return;
            }

            //int i = Handler.waveOutOpen(ref hWaveOut, 4294967295, ref format, Marshal.GetFunctionPointerForDelegate(waveIn), 0, 0x0030000);
            //if (i != 0)
            //{
            //    //Error
            //    return;
            //}
            setupOutbuffer();

        }

        private void setupOutbuffer()
        {
            Outheader.lpData = savePin.AddrOfPinnedObject();
            Outheader.dwBufferLength = (uint)save.Length;
            Outheader.dwFlags = 0x00000004 | 0x00000008;
            Outheader.dwBytesRecorded = 0;
            Outheader.dwLoops = 1;
            Outheader.lpNext = IntPtr.Zero;
            Outheader.reserved = IntPtr.Zero;

            int i = Handler.waveOutPrepareHeader(hWaveOut, ref Outheader, Convert.ToUInt32(Marshal.SizeOf(Outheader)));
            if (i != 0)
                return;

            i = Handler.waveOutWrite(hWaveOut, ref Outheader, Convert.ToUInt32(Marshal.SizeOf(Outheader)));
            if (i != 0)
                return;



        }


        private void callbackWaveIn(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2)
        {

            if (message == 0x3c0) //WIM_DATA
            {
                if (save != null)
                {
                    List<byte> temp = save.ToList();
                    temp.AddRange(buffer.ToList());
                    save = temp.ToArray();
                }
                else
                    save = buffer;

                savePin = GCHandle.Alloc(save, GCHandleType.Pinned);
                buffer = new byte[bufferLength];
                bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                int i = waveInUnprepareHeader(deviceHandle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
                if (i != 0) //MMSYSERR_NOERROR
                {
                    //Error
                    return;
                }

                setupBuffer();
            }
        }

        private void callbackWaveOut(IntPtr deviceHandle, uint message, IntPtr instance, ref WAVEHDR wavehdr, IntPtr reserved2)
        {

            if (message == 0x3c0) //WIM_DATA
            {
                //List<byte> temp = save.ToList();
                //temp.AddRange(buffer.ToList());
                //temp.RemoveAll(delegate (byte a) { return a == 0; });

                //save = temp.ToArray();

                //savePin = GCHandle.Alloc(save, GCHandleType.Pinned);

                int i = waveInUnprepareHeader(deviceHandle, ref header, Convert.ToUInt32(Marshal.SizeOf(header)));
                if (i != 0) //MMSYSERR_NOERROR
                {
                    //Error
                    return;
                }
                setupOutbuffer();
            }
        }


        public void stop()
        {
            List<byte> temp = buffer.ToList();
            temp.RemoveAll(delegate (byte a) { return a == 0; });

            buffer = temp.ToArray();

            bufferPin.Free();
            bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            waveInStop(handle);
            waveInReset(handle);
            waveInClose(handle);
        }


        public byte[] data_stop()
        {
            waveInStop(handle);
            //Thread.Sleep(200);
            //waveInReset(handle);
            waveInClose(handle);


            //buffer = temp.ToArray();

            bufferPin.Free();
            savePin.Free();
            
            //bufferPin = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            return save;
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

            List<byte> result = new List<byte>();
            for (int i = 0; i < values.Length; i++)
            {
                result.AddRange(doubleConvert(values[i]));
            }
            return result.ToArray();
        }

        private byte[] doubleConvert(double value)
        {
            byte[] output = new byte[8];
            long lng = BitConverter.DoubleToInt64Bits(value);
            for (int i = 0; i < 8; i++)
                output[i] = (byte)((lng >> ((7 - i) * 8)) & 0xff);
            return output;
        }

        
    }
}
