using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MassBanTool
{
    
    public partial class LogWindow : System.Windows.Forms.Form
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        public void Log (string line)
        {
            if (txt_log.InvokeRequired)
            {
                txt_log.Invoke(new Action(() =>
                {
                    txt_log.AppendText(line + Environment.NewLine);
                }));
            }
            else
            {
                txt_log.AppendText(line + Environment.NewLine);
            }
        }
    }
}
