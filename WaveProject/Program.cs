//Tyler Massicotte A00855150 2015
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaveProject
{
    //This is the starting point of the program. It creates and runs the main
    //window GUI.
    static class Program
    {
        [STAThread]
        static  void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainWindow mainWin = new MainWindow();

            Application.Run(mainWin);
        }
    }
}
