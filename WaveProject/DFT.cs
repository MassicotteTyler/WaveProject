//Tyler Massicotte A00855150 2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WaveProject
{
    /********************************************************
    * The DFT classes houses functions used to apply the 
    * Discrete Fourier Transform onto wave samples. As well as
    * preform inverse Discrete Fourier Transform.
    **********************************************************/
    class DFT
    {

        /********************************************************
        * DFT takes in an array of samples and applies the Discrete
        * Fourier Transform to them. The output is rendered as an
        * array of complex numbers. The method splits the array 
        * into 2 parts which a thead to work on each.
        **********************************************************/
        public static Complex[] Dft(double[] samples)
        {
            int N = samples.Length;
            Thread thread1, thread2;

            Complex[] output = new Complex[N];

            //First half of dft. Uses a lambda expression to
            //only run the first half.
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
            
            //Second half of the DFT function. Also uses
            // a lambda expression to calculate the second half.
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

            //Start both threads.
            thread1.Start();
            thread2.Start();

            //Wait for both threads to finish. thread2 still runs
            //while thread1's join method as been called.
            thread1.Join();
            thread2.Join();

            return output;

        }


        /********************************************************
        * iDft takes in an array of complex numbers and applies the
        * inverse Discrete Fourier Transform algorithm to them. The
        * output is renered as an array of doubles. The method splits
        * up the main algorithm into 2 halfs and threads each half 
        **********************************************************/
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
