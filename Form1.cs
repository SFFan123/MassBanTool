using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MassBanTool
{
    public partial class Form : System.Windows.Forms.Form
    {
        private bool Broadcaster = false;
        private string channel;
        private bool connected;
        private string defaultStatus = "Ready";
        TwitchChatClient twitchChat = null;
        Thread clientThread = null;
        private bool Moderator = false;
        private string oauth;
        private List<string> toBan = new List<string>();
        private string uname;


        public Form()
        {
            InitializeComponent();
            getLogin();
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
            string channel = txt_channel.Text.Trim().ToLower();
            if (connected)
            {
                twitchChat.switchChannel(channel);
                return;
            }

            string username = txt_username.Text.Trim().ToLower();
            oauth = txt_oauth.Text.Trim().ToLower();

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


            clientThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                twitchChat = new TwitchChatClient(username, oauth, channel, this);
            });
            clientThread.Start();
        }

        public void setMod(object sender, bool mod, bool broadcaster)
        {
            this.Moderator = mod;
            this.Broadcaster = broadcaster;
            if (InvokeRequired)
            {
                toolStripStatusMod.Visible = mod || broadcaster;
                if (broadcaster)
                {
                    toolStripStatusMod.Image = Properties.Resources.broadcaster2;
                }

                if (mod)
                {
                    toolStripStatusMod.Image = Properties.Resources.moderator2;
                }

                pbModerator.Invoke(new Action(() =>
                {
                    pbModerator.Visible = mod || broadcaster;
                    if (broadcaster)
                    {
                        pbModerator.Image = Properties.Resources.broadcaster2;
                    }

                    if (mod)
                    {
                        pbModerator.Image = Properties.Resources.moderator2;
                    }
                }));
            }
        }

        public void setInfo(object sender, string channel, string displayname)
        {
            connected = true;
            this.channel = channel;
            this.uname = displayname;
            toolStripStatusLabel_Channel.Text = channel;
            toolStripStatusLabel_Username.Text = displayname;

            toolStripStatusLabel.Text = "Connected/Ready";
            if (InvokeRequired)
            {
                btn_connect.Invoke(new Action(() => { btn_connect.Text = "Switch Channel"; }));
                btn_OpenList.Invoke(new Action(() => { btn_OpenList.Enabled = true; }));
                btn_saveLogin.Invoke(new Action(() => { btn_saveLogin.Visible = true; }));
                btn_getFollows.Invoke(new Action(() => { btn_getFollows.Enabled = true; }));
                txt_oauth.Invoke(new Action(() => { txt_oauth.ReadOnly = true; }));
                txt_username.Invoke(new Action(() => { txt_username.ReadOnly = true; }));
            }
        }

        public void setBanProgress(object sender, int index, int max)
        {
            if (InvokeRequired)
            {
                progresBar_BanProgress.Invoke(new Action(() =>
                {
                    progresBar_BanProgress.Maximum = max;
                    progresBar_BanProgress.Value = index;
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

        public void setETA(object sender, string eta)
        {
            if (InvokeRequired)
            {
                toolstripETA.Text = "ETA: " + eta;
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
                    txt_ToBan.Lines = content;
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
                if (MessageBox.Show("You have not provided a reason for the bans. Do you want to continue?",
                    "Confirm Ban without reason", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    twitchChat.addToBann(toBan, "");
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (twitchChat.cooldown == default)
                    btn_applyDelay_Click(null, null);

                twitchChat.addToBann(toBan, ban_reason);
            }
        }

        private void btn_actions_Stop_Click(object sender, EventArgs e)
        {
            TwitchChatClient.mt_pause = !TwitchChatClient.mt_pause;

            if (TwitchChatClient.mt_pause)
            {
                toolStripStatusLabel.Text = "Paused! / Ready";
            }
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
            foreach (var item in c)
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
            progresBar_BanProgress.Maximum = toBan.Count;
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

        private void getLogin()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolLogin.txt");
            try
            {
                string[] a = File.ReadAllLines(fileName);
                if (a.Length == 2)
                {
                    txt_username.Text = a[0];
                    txt_oauth.Text = a[1];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void saveLogin()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolLogin.txt");
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MassBanTool");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }

            File.WriteAllText(fileName, uname + Environment.NewLine + oauth);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveLogin();
        }

        private void btn_applyDelay_Click(object sender, EventArgs e)
        {
            if (int.TryParse(in_cooldown.Text, out int result))
            {
                twitchChat.cooldown = result;
            }
            else
            {
                MessageBox.Show("Invalid Cooldown.");
            }
            
        }

        public void ThrowError(string message, bool exitonThrow = true)
        {
            MessageBox.Show(message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if(exitonThrow)
                Environment.Exit(-1);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "WARNING",MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btn_showconsole_Click(object sender, EventArgs e)
        {
            var a  = Program.AllocConsole();
            btn_showconsole.Enabled = false;
        }
    }
}