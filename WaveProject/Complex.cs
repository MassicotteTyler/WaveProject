using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveProject
{
    public class Complex
    {
        public double real;
        public double ima;

        public Complex()
        {
            this.real = 0;
            this.ima = 0;
        }

        public Complex(double r, double i)
        {
            this.real = r;
            this.ima = i;
        }

        public static Complex Polar(double r, double i)
        {
            double pythag = Math.Sqrt(Math.Pow(r, 2) + (Math.Pow(i, 2)));
            double angle = Math.Atan(i / r);
            return new Complex(pythag * Math.Cos(angle), pythag * Math.Sin(angle));
        }

        public static double[] Mag(Complex[] values)
        {
            double[] temp = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                temp[i] = Math.Sqrt(Math.Pow(values[i].real, 2) + Math.Pow(values[i].ima, 2)) / values.Length;
            }
            return temp;
        }
    }
}
