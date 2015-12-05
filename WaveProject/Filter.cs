using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveProject
{
    /********************************************************
    * The Filtering class houses the methods for creating and 
    * applying filters. As well as the methods that apply window
    * functions to samples.
    **********************************************************/
    class Filter
    {

        /********************************************************
        * Create filter takes in a double array of samples and an
        * int to specify where to make the cut off point for the
        * lowpass filter. It then creates an array of complex numbers
        * that have values of 1's or 0's. One to specify to keep that
        * corresponding frequency. 0 to remove the frequency. The 
        * array of complex numbers are then passed through iDft to get
        * the weights of the filter. The filter is then returned.
        **********************************************************/
        public static double[] createFilter(int selection, double[] samples)
        {
            double[] filter;
            Complex[] temp = new Complex[samples.Length];

            //Always keep the DC component
            temp[0] = new Complex(1, 1);
            int front, end;
            for (front = 1, end = temp.Length - 1; front < selection; front++, end--)
            {
                temp[front] = new Complex(1, 1);
                temp[end] = new Complex(1, 1);
            }
            for (front = selection; front < temp.Length / 2 + 1; front++, end--)
            {
                temp[front] = new Complex(0, 0);
                temp[end] = new Complex(0, 0);
            }

            //Get the weights
            filter = DFT.iDft(temp);

            return filter;
        }


        /*****************************************************************
        * Takes in 2 double arrays, one being wave samples and the other
        * being a filter of weights to apply. it then applies the weights to
        * the samples and returns the newly filtered sample.
        ******************************************************************/
        public static double[] apply_filter(double[] samples, double[] filter)
        {
            double[] result = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                double temp = 0;
                for (int j = 0; j < filter.Length; j++)
                {
                    if ((i + j) < (samples.Length))
                        temp += samples[i + j] * filter[j];
                    else
                        temp += 0;
                }
                result[i] = temp;
            }
            return result;
        }

        /*****************************************************************
        * Takes in a double array of wave samples and applies the Hanning
        * windowing function to it. It returns the newly weighted samples.
        ******************************************************************/
        public static double[] hanning_window(double[] samples)
        {
            double[] output = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                output[i] = 0.54 + (0.46 * Math.Cos(Math.PI * samples[i]));
            }

            return output;
        }

        /*****************************************************************
        * Takes in a double array of wave samples and applies the Rectangle
        * windowing function to it. It returns the newly weighted samples.
        ******************************************************************/
        public static double[] rectangle_window(double[] samples)
        {
            double[] output = new double[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                output[i] = samples[i] * 1;
            }

            return output;
        }

        /*****************************************************************
        * Takes in a double array of wave samples and applies the Triangle
        * windowing function to it. It returns the newly weighted samples.
        ******************************************************************/
        public static double[] triangle_window(double[] samples)
        {
            double[] output = new double[samples.Length];
            int N = samples.Length;
            for (int i = 0; i < samples.Length; i++)
            {
                output[i] = 1 - Math.Abs(((samples[i] - ((N - 1) / 2)) /
                                 (N / 2)));
            }
            return output;
        }
    }
}
