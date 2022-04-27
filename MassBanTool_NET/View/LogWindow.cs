using System;
using System.IO;
using System.Windows.Forms;

namespace MassBanTool
{
    public partial class LogWindow : System.Windows.Forms.Form
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        public void Log(string line)
        {
            if (txt_log.InvokeRequired)
            {
                txt_log.Invoke(new Action(() => { txt_log.AppendText(line + Environment.NewLine); }));
            }
            else
            {
                txt_log.AppendText(line + Environment.NewLine);
            }
        }

        private void saveLogAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckFileExists = true;
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|" +
                                     "log files (*.log)|*.log|" +
                                     "All files (*.*)|*.*";

            saveFileDialog1.FileName = $"MassBanTool_Log_{DateTime.Now:s}";

            var diag = saveFileDialog1.ShowDialog();

            if (diag == DialogResult.OK)
            {
                saveLog(saveFileDialog1.FileName);
            }
        }

        private void saveLog(string fileName)
        {
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }

            File.WriteAllText(fileName, txt_log.Text + Environment.NewLine);
        }
    }
}