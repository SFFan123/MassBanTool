using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MassBanTool
{
    static class Program
    {
        // TODO add mutex
        // TODO check if multi channel support
        // TODO show count
        // TODO give feedback who has been banned
        // TODO PAUSE option.
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form());
        }
    }
}
