using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MassBanTool.View
{
    public partial class InputDialog : Form
    {
        private InputDialog()
        {
            InitializeComponent();
        }

        string input = String.Empty;

        public static DialogResult Show(
            string text,
            string caption,
            Control inputtype,
            out string result)
        {
            var diag = new InputDialog();

            diag.Text = caption;

            diag.Icon = SystemIcons.Question;
            diag.TopMost = true;

            var parent = diag.Controls.Find("panel", false).First();

            inputtype.Width = parent.Width - 10;
            inputtype.Height = parent.Height - 10;
            
            parent.Controls.Add(inputtype);

            parent = diag.Controls.Find("panel1", false).First();

            Label Question = new Label();
            Question.Text = text;
            Question.MaximumSize = new Size(parent.Width - 10, 0);
            Question.AutoSize = true;
            parent.Controls.Add(Question);
            
            DialogResult dagResult = diag.ShowDialog();
            
            result = inputtype.Text;

            diag.Dispose();

            return dagResult;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void btn_Abort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            Hide();
        }
    }
}
