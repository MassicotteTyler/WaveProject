//Tyler Massicotte A00855150 2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WaveProject
{
    class DFT
    {


        public static Complex[] Dft(double[] samples)
        {
            int N = samples.Length;
            Thread thread1, thread2;
            //Setup threadpool que
            Complex[] output = new Complex[N];
            Complex[] temp1 = new Complex[N / 2];
            Complex[] temp2 = new Complex[N / 2];

            var subDft1 = new Action<double[]>(samp => 
            {
                for (int k1 = 0; k1 < N / 2 + 1; k1++)
                {
                    double sumreal = 0;
                    double sumimag = 0;
                    for (int t2 = 0; t2 < N; t2++)
                    {
                        sumreal += samples[t2] * Math.Cos(t2 * ((2 * Math.PI) * k1 / N));
                        sumimag -= samples[t2] * Math.Sin(t2 * ((2 * Math.PI) * k1 / N));
                    }
                    output[k1] = new Complex(sumreal, sumimag);
                }
            });
            
            var subDft2 = new Action<double[]>(samp =>
            {
                for (int k = N / 2 + 1; k < N; k++)
                {
                    double sumreal = 0;
                    double sumimag = 0;
                    for (int t = 0; t < N; t++)
                    {
                        sumreal += samples[t] * Math.Cos(t * ((2 * Math.PI) * k / N));
                        sumimag -= samples[t] * Math.Sin(t * ((2 * Math.PI) * k / N));
                    }
                    output[k] = new Complex(sumreal, sumimag);
                }
            });


            thread1 = new Thread(() => subDft1(samples), 1024 * 1024);
            thread2 = new Thread(() => subDft2(samples), 1024 * 1024);

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();

            return output;

        }


        public static double[] iDft(Complex[] input)
        {
            int N = input.Length;

            double[] output = new double[N];

            double arg = 2.0 * Math.PI / (double)N;

            var subDft1 = new Action(() =>
            {
                for (int k = 0; k < N / 2 + 1; k++)
                {
                    double sumreal = 0;
                    double sumimag = 0;
                    for (int t = 0; t < N; t++)
                    {
                        sumreal += input[t].real * Math.Cos(t  * (2 * Math.PI) * k / N);
                        sumimag += input[t].ima * Math.Sin(t * (2 * Math.PI) * k / N);
                    }
                    output[k] = (sumreal - sumimag) / N;
                }
            });

            var subDft2 = new Action(() =>
            {
                for (int k = N / 2 + 1; k < N; k++)
                {
                    double sumreal = 0;
                    double sumimag = 0;
                    for (int t = 0; t < N; t++)
                    {
                        sumreal += input[t].real * Math.Cos(t * (2 * Math.PI) * k / N);
                        sumimag += input[t].ima * Math.Sin(t * (2 * Math.PI) * k / N);
                    }
                    output[k] = (sumreal - sumimag) / N;
                }
            });

            Thread t1 = new Thread(() => subDft1(), 1024 * 1024);
            Thread t2 = new Thread(() => subDft2(), 1024 * 1024);

            t1.Start();
            t2.Start();



            t1.Join();
            t2.Join();

            return output;
        }





    }
}
