using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MassBanTool
{
    public static class Program
    {
        // TODO add mutex
        // TODO check if multi channel support
        // TODO show count
        // TODO give feedback who has been banned
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
