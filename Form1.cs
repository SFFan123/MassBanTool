using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MassBanTool
{
    public partial class Form : System.Windows.Forms.Form
    {
        private string uname;
        private string channel;
        private bool Moderator = false;
        IRCClient iRC = null;
        Thread ircThread = null;
        private List<string> toBan = new List<string>();
        private string defaultStatus = "Ready";

        public Form()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_showOauth.Checked)
            {
                txt_oauth.PasswordChar = '\0';
            }
            else
            {
                txt_oauth.PasswordChar = '*';
            }
        }
        private void btn_connect_Click(object sender, EventArgs e)
        {
            string username = txt_username.Text.Trim().ToLower();
            string channel = txt_channel.Text.Trim().ToLower();
            string oauth = txt_oauth.Text.Trim().ToLower();

            if (username.Trim().Equals(String.Empty))
            {
                MessageBox.Show("No username given!");
                return;
            }
            if (channel.Trim().Equals(String.Empty))
            {
                MessageBox.Show("No channel given!");
                return;
            }
            if (oauth.Trim().Equals(String.Empty))
            {
                MessageBox.Show("No oauth given!");
                return;
            }
            uname = username;
            this.channel = channel;


            ircThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                iRC = new IRCClient("irc.chat.twitch.tv", 6697, username, channel, oauth, this);
                iRC.Start();
            });
            ircThread.Start();
            btn_OpenList.Enabled = true;
            btn_getFollows.Enabled = true;
        }

        public void setMod(object sender, bool mod)
        {
            this.Moderator = mod;
            if (InvokeRequired)
            {
                toolStripStatusMod.Visible = mod;
                pbModerator.Invoke(new Action(() =>
                {
                    pbModerator.Visible = mod;
                }));
            }
        }
        public void setInfo(object sender, string channel, string displayname)
        {
            toolStripStatusLabel_Channel.Text = channel;
            toolStripStatusLabel_Username.Text = displayname;
            toolStripStatusLabel.Text = "Connected/Ready";

        }

        public void setBanProgress(object sender, int index, int max)
        {
            if (InvokeRequired)
            {
                progresBar_BanProgress.Invoke(new Action(() =>
                {
                    progresBar_BanProgress.Value = index;
                    progresBar_BanProgress.Maximum = max;
                    progresBar_BanProgress.Refresh();
                }));

                if (index == max)
                {
                    toolStripStatusLabel.Text = "Done";
                }
                else
                {
                    toolStripStatusLabel.Text = "banning ...";
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://twitchapps.com/tmi/");
        }

        private void btn_OpenList_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;


            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel.Text = "Importing ...";
                progresBar_BanProgress.Value = 0;
                //Get the path of specified file
                filePath = openFileDialog1.FileName;

                lbl_list.Text = filePath;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog1.OpenFile();


                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                    string[] content = fileContent.Split('\n');
                    int count = content.Length;
                    progresBar_BanProgress.Maximum = count;
                    for (int i = 0; i < count; i++)
                    {
                        txt_ToBan.AppendText($"{content[i]}\r\n");
                        if (i % 5 == 0)
                        {
                            progresBar_BanProgress.Value = i + 1;
                        }


                    }
                    toBan.AddRange(content);
                }
                toolStripStatusLabel.Text = defaultStatus;
                progresBar_BanProgress.Value = progresBar_BanProgress.Maximum;
                progresBar_BanProgress.Refresh();

                foreach (Control item in tabPageBanning.Controls)
                {
                    item.Enabled = true;
                }
            }

        }

        private void btn_action_run_Click(object sender, EventArgs e)
        {
            string ban_reason = txt_actions_ban_reason.Text.Trim();
            if (ban_reason.Equals(string.Empty))
            {
                if (MessageBox.Show("You have not provided a reason for the bans. Do you want to continue?", "Confirm Ban without reason", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    iRC.addToBann(toBan, "");
                }
                else
                {
                    return;
                }
            }
            else
            {
                iRC.addToBann(toBan, ban_reason);
            }
        }

        private void btn_actions_Stop_Click(object sender, EventArgs e)
        {
            iRC.StopQueue();
            toolStripStatusLabel.Text = "Aborted! / Ready";
        }

        private void btn_getFollows_Click(object sender, EventArgs e)
        {
            if (channel.Equals(string.Empty))
            {
                MessageBox.Show("No Channel given can't fetch follows!");
                return;
            }
            toolStripStatusLabel.Text = "Fetching Last 1000 Follows from API...";
            string[] c = getFollowsFromAPI();
            if (c.Length == 0)
            {
                MessageBox.Show("Fetching Follows failed.");
                toolStripStatusLabel.Text = defaultStatus;
                return;
            }
            toBan = c.ToList<string>();
            txt_ToBan.Text = "";
            foreach(var item in c)
            {
                txt_ToBan.AppendText(item.Trim() + "\r\n");
            }
            tabControl.SelectedTab = tabPageFiltering;

            /* foreach (Control item in tabPageFiltering.Controls)
             {
                 item.Enabled = true;
             }
             */
        }

        private string[] getFollowsFromAPI()
        {
            string URL = "https://cactus.tools/twitch/followers";
            string urlParameters = $"?channel={channel}&max=1000";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                string[] result = response.Content.ReadAsStringAsync().Result.Split('\n');
                client.Dispose();
                return result;
            }
            else
            {
                client.Dispose();
                toolStripStatusLabel.Text = "API Call failed.";
                return Array.Empty<string>();
            }
        }

        private void btn_run_regex_Click(object sender, EventArgs e)
        {
            string sregex = txt_uname_regex.Text;
            Regex rgx = new Regex(sregex);
            List<string> newToBan = new List<string>();
            progresBar_BanProgress.Maximum = newToBan.Count;
            txt_ToBan.Text = "";
            for (int i = 0; i < toBan.Count; i++)
            {
                if (i % 10 == 0)
                {
                    progresBar_BanProgress.Value = i;
                    toolStripStatusLabel.Text = "Applying Regex";
                }

                if (rgx.IsMatch(toBan[i]))
                {
                    newToBan.Add(toBan[i]);
                }
            }
            toBan = newToBan;
            txt_ToBan.Lines = toBan.ToArray();
            progresBar_BanProgress.Maximum = 100;
            progresBar_BanProgress.Value = 100;
            toolStripStatusLabel.Text = defaultStatus;
        }
    }
}
