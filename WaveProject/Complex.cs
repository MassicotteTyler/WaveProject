//Tyler Massicotte A00855150 2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveProject
{

    /********************************************************
    * The Complex class is a representation of a complex number.
    * Complex numbers have a Real part and an imaginary part.
    * Eg. 5 + i3 
    * Discrete Fourier Transform renders the out as complex numbers
    * and to properly display the data it needs to use pythagoras
    * formula to calculate it.
    **********************************************************/
    public class Complex
    {
        //The real value of the number
        public double real;
        //The value of the number that contains i
        public double ima;


        /********************************************************
        * Default constructor. Initializes the real and imaginary
        * part to 0.
        **********************************************************/
        public Complex()
        {
            this.real = 0;
            this.ima = 0;
        }

        /********************************************************
        * Constructor, takes in two double values, one for real
        * and one for imaginary.
        **********************************************************/
        public Complex(double r, double i)
        {
            this.real = r;
            this.ima = i;
        }

        /********************************************************
        * Calculates the magnitude of the Dft bin. It does this 
        * by applying pythagoras theorem to the bin values. Returns
        * the newly calculated values.
        **********************************************************/
        public static double[] Mag(Complex[] values)
        {
            double[] temp = new double[values.Length];

            //SquareRoot(real^2 + imaginary^2) divided by number of bins
            for (int i = 0; i < values.Length; i++)
            {
                temp[i] = Math.Sqrt(Math.Pow(values[i].real, 2) + Math.Pow(values[i].ima, 2)) / values.Length;
            }
            return temp;
        }
    }
}
