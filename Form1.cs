using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace MassBanTool
{
    public partial class Form : System.Windows.Forms.Form
    {
        private string channel;
        private bool connected;
        private bool consoleIsShown = false;
        private string oauth;
        TwitchChatClient twitchChat = null;
        private string uname;

        private ListType inputListType = default;


        public Form()
        {
            InitializeComponent();
            getLogin();
        }

        public List<string> usernameOrCommandList
        {
            get => txt_ToBan.Lines.ToList();
        }

        public string Listinfo { get; set; } = "<none>";


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
                if (twitchChat.MessagesQueue.Count > 0)
                {
                    bool wasrunning = TwitchChatClient.mt_pause;
                    TwitchChatClient.mt_pause = true;
                    var result = MessageBox.Show(
                        "You are currently running an action, do you want to abort that action and change channels?",
                        "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        twitchChat.Abort();
                        try
                        {
                            twitchChat.switchChannel(channel);
                        }
                        catch (ArgumentException exception)
                        {
                            ThrowError(exception.Message, false);
                        }
                    }
                    TwitchChatClient.mt_pause = wasrunning;
                    return;
                    
                }
                try
                {
                    twitchChat.switchChannel(channel);
                }
                catch (ArgumentException exception)
                {
                    ThrowError(exception.Message, false);
                }

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

            twitchChat = new TwitchChatClient(username, oauth, channel, this);

            twitchChat.PropertyChanged += ClientPropChanged;
        }

        private void ClientPropChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(twitchChat.CurrentStatus):
                    switch (twitchChat.CurrentStatus)
                    {
                        case ToolStatus.Ready:
                            toolStripStatusLabel.Text = "Ready";
                            break;
                        case ToolStatus.Aborted:
                            toolStripStatusLabel.Text = "Aborted";
                            break;
                        case ToolStatus.Banning:
                            toolStripStatusLabel.Text = "Banning";
                            break;
                        case ToolStatus.Disconnected:
                            toolStripStatusLabel.Text = "Disconnected";
                            break;
                        case ToolStatus.ReadFile:
                            toolStripStatusLabel.Text = "Executing Readfile";
                            break;
                        case ToolStatus.UnBanning:
                            toolStripStatusLabel.Text = "Unbanning";
                            break;
                        case ToolStatus.Paused:
                            toolStripStatusLabel.Text = "Paused";
                            break;
                        case ToolStatus.Done:
                            toolStripStatusLabel.Text = "Done";
                            break;
                    }
                    break;
                case nameof(inputListType):
                    lbl_listType.Text = $"Listtype: {inputListType}";
                    switch (inputListType)
                    {
                        case ListType.ReadFile:
                            btn_RunReadfile.Enabled = true;
                            btn_run_regex.Enabled = true;
                            btnRemovePrefixes.Enabled = true;

                            btn_run_unban.Enabled = false;
                            btn_action_run.Enabled = false;
                            txt_actions_ban_reason.Enabled = false;
                            break;

                        case ListType.Mixed:
                            btn_run_regex.Enabled = true;
                            btnRemovePrefixes.Enabled = true;

                            btn_RunReadfile.Enabled = false;
                            btn_run_unban.Enabled = false;
                            btn_action_run.Enabled = false;
                            txt_actions_ban_reason.Enabled = false;
                            break;

                        case ListType.UserList:
                            btn_run_regex.Enabled = true;
                            btn_action_run.Enabled = true;
                            btn_run_unban.Enabled = true;
                            txt_actions_ban_reason.Enabled = true;

                            btnRemovePrefixes.Enabled = false;
                            btn_RunReadfile.Enabled = false;
                            break;
                    }
                    break;
            }
        }


        public void setMod(object sender, bool mod, bool broadcaster)
        {
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

        public void setInfo(object sender, string channel)
        {
            connected = true;
            this.channel = channel;
            toolStripStatusLabel_Channel.Text = channel;

            if (openListFileToolStripMenuItem.GetCurrentParent().InvokeRequired)
            {
                openListFileToolStripMenuItem.GetCurrentParent().Invoke(new Action(() =>
                {
                    openListFileToolStripMenuItem.Enabled = true;
                }));

                openListURLToolStripMenuItem.GetCurrentParent().Invoke(new Action(() =>
                {
                    openListURLToolStripMenuItem.Enabled = true;
                }));

                fetchLastFollowersOfChannelToolStripMenuItem.GetCurrentParent().Invoke(new Action(() =>
                {
                    fetchLastFollowersOfChannelToolStripMenuItem.Enabled = true;
                }));
            }
            else
            {
                openListFileToolStripMenuItem.Enabled = true;
                openListURLToolStripMenuItem.Enabled = true;
                fetchLastFollowersOfChannelToolStripMenuItem.Enabled = true;
            }

            if (InvokeRequired)
            {
                btn_connect.Invoke(new Action(() => { btn_connect.Text = "Switch Channel"; }));
                btn_saveLogin.Invoke(new Action(() => { btn_saveLogin.Visible = true; }));
                txt_oauth.Invoke(new Action(() => { txt_oauth.ReadOnly = true; }));
                txt_username.Invoke(new Action(() => { txt_username.ReadOnly = true; }));
            }
            else
            {
                btn_connect.Text = "Switch Channel";
                btn_saveLogin.Visible = true;
                txt_oauth.ReadOnly = true;
                txt_username.ReadOnly = true;
            }
        }

        public void setBanProgress(object sender, int index, int max)
        {
            if (InvokeRequired)
            {
                progresBar_BanProgress.Invoke(new Action(() =>
                {
                    double percent = (double)index / (double)max;
                    toolStripStatusLabel_BanIndex.Text = $"{index}/{max} - {string.Format("{0:P1}", percent)}";
                    progresBar_BanProgress.Maximum = max;
                    progresBar_BanProgress.Value = index;
                    progresBar_BanProgress.Refresh();
                }));


                if (index == max)
                {
                    twitchChat.CurrentStatus = ToolStatus.Done;
                    twitchChat.NotifyPropertyChanged(nameof(twitchChat.CurrentStatus));
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

        private void setEnableForControl(bool enabled = true)
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                foreach (Control item in tab.Controls)
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
                    twitchChat.setToBann(usernameOrCommandList, "");
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

                twitchChat.setToBann(usernameOrCommandList, ban_reason);
            }
        }

        private void btn_actions_Stop_Click(object sender, EventArgs e)
        {
            TwitchChatClient.mt_pause = !TwitchChatClient.mt_pause;

            twitchChat.TargetStatus_for_pause = twitchChat.CurrentStatus;
            if (TwitchChatClient.mt_pause)
            {
                btn_actions_Stop.Text = "RESUME";
                twitchChat.CurrentStatus = ToolStatus.Paused;
            }
            else
            {
                btn_actions_Stop.Text = "PAUSE";
                twitchChat.CurrentStatus = twitchChat.TargetStatus_for_pause;
            }

            twitchChat.NotifyPropertyChanged(nameof(twitchChat.CurrentStatus));
        }

        private string[] getFollowsFromAPI(int amount = 1000)
        {
            string URL = "https://cactus.tools/twitch/followers";
            string urlParameters = $"?channel={channel}&max={amount}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);

                HttpResponseMessage response = client.GetAsync(urlParameters).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body.
                    string[] result = response.Content.ReadAsStringAsync().Result.Split('\n');
                    return result;
                }
                else
                {
                    toolStripStatusLabel.Text = "API Call failed.";
                    return Array.Empty<string>();
                }
            }
        }

        private void btn_run_regex_Click(object sender, EventArgs e)
        {
            string sregex = txt_uname_regex.Text;
            Regex rgx = new Regex(sregex);
            List<string> newToBan = new List<string>();
            progresBar_BanProgress.Maximum = usernameOrCommandList.Count;
            txt_ToBan.Text = "";
            for (int i = 0; i < usernameOrCommandList.Count; i++)
            {
                if (i % 10 == 0)
                {
                    progresBar_BanProgress.Value = i;
                    toolStripStatusLabel.Text = "Applying Regex";
                }

                if (rgx.IsMatch(usernameOrCommandList[i]))
                {
                    newToBan.Add(usernameOrCommandList[i]);
                }
            }

            //usernameOrCommandList = newToBan;
            txt_ToBan.Lines = usernameOrCommandList.ToArray();
            progresBar_BanProgress.Maximum = 100;
            progresBar_BanProgress.Value = 100;
            twitchChat.CurrentStatus = ToolStatus.Ready;
            twitchChat.NotifyPropertyChanged(nameof(twitchChat.CurrentStatus));
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
            if (exitonThrow)
                Environment.Exit(-1);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btn_showconsole_Click(object sender, EventArgs e)
        {
            if (consoleIsShown)
            {
                consoleIsShown = Program.FreeConsole();
                btn_showconsole.Text = "Show Console";
            }
            else
            {
                consoleIsShown = Program.AllocConsole();
                btn_showconsole.Text = "Hide Console";
            }
        }

        private void btn_Abort_Click(object sender, EventArgs e)
        {
            twitchChat.Abort();
            toolStripStatusLabel.Text = "Aborted! / Ready";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (twitchChat.cooldown == default)
                btn_applyDelay_Click(null, null);

            // run unban ...
            twitchChat.setToUNBann(usernameOrCommandList);
            TwitchChatClient.mt_pause = false;
        }

        private void btnRemovePrefixes_Click(object sender, EventArgs e)
        {
            List<string> result = new List<string>();
            Regex regex = new Regex(@"^(?:(?:\/|\.)[^\s]+ )?(\w+)(?: .+)?$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (string user in usernameOrCommandList)
            {
                string _user = user.Trim();
                _user = regex.Replace(_user, @"$1");
                _user = _user.Trim();

                result.Add(_user);
            }
            txt_ToBan.Lines = result.ToArray();
            checkListType();
        }

        private void btn_RunReadfile_Click(object sender, EventArgs e)
        {
            // get allow list

            var allowList = textBoxAllowedActions
                .Lines
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x) && x != string.Empty)
                .ToArray();

            if (allowList.Length == 0)
            {
                MessageBox.Show("No allowed action.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            allowList = allowList.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // Filter list
            // broken.
            var commandList = usernameOrCommandList.Where(x => x != String.Empty && allowList.Contains(
                //TODO check if command prefix
                x.Substring(1, x.IndexOf(" ") - 1))
            ).ToList();

            if (radio_Readfile_WarnAndAbort.Checked && commandList.Count != usernameOrCommandList.Count)
            {
                MessageBox.Show("Missmatch between allowed commands and commands used in the file.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Put in Queue
            twitchChat.addRawMessages(usernameOrCommandList);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SFFan123/MassBanTool/releases");
        }

        private void checkListType()
        {
            inputListType = ListType.none;
            foreach (var line in txt_ToBan.Lines)
            {
                if (line.StartsWith("/") || line.StartsWith("."))
                {
                    if (inputListType == ListType.none || inputListType == ListType.ReadFile)
                    {
                        inputListType = ListType.ReadFile;
                    }
                    else
                    {
                        inputListType = ListType.Mixed;
                    }

                    if (inputListType == ListType.Mixed)
                    {
                        twitchChat.NotifyPropertyChanged(nameof(inputListType));
                        return;
                    }
                        
                }
                else if(line.Contains(" "))
                {
                    inputListType = ListType.Mixed;
                    twitchChat.NotifyPropertyChanged(nameof(inputListType));
                    return;
                }
            }

            inputListType = inputListType == ListType.ReadFile ? ListType.ReadFile : ListType.UserList;

            twitchChat.NotifyPropertyChanged(nameof(inputListType));
        }


        private void OpenListFromFile()
        {
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                twitchChat.CurrentStatus = ToolStatus.Importing;
                twitchChat.NotifyPropertyChanged(nameof(twitchChat.CurrentStatus));


                progresBar_BanProgress.Value = 0;
                //Get the path of specified file
                var filePath = openFileDialog1.FileName;

                Listinfo = filePath;

                lbl_list.Text = Listinfo;

                var fileContent = File.ReadLines(filePath).ToArray();
                fileContent = fileContent.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                txt_ToBan.Lines = fileContent;

                twitchChat.CurrentStatus = ToolStatus.Ready;
                twitchChat.NotifyPropertyChanged(nameof(twitchChat.CurrentStatus));
                progresBar_BanProgress.Value = progresBar_BanProgress.Maximum;
                progresBar_BanProgress.Refresh();

                setEnableForControl(true);

                checkListType();
            }
        }

        private void FetchLastFollowers()
        {
            if (channel.Equals(string.Empty))
            {
                MessageBox.Show("No Channel given can't fetch follows!");
                return;
            }
            
            string input = Interaction.InputBox("Amount of Follows? must be between 100 and 5000 - in steps of 100", "Fetch amount");

            if (input == string.Empty)
                return;

            if (!int.TryParse(input, out int follows) && follows < 5000 && follows >= 100)
            {
                MessageBox.Show("Invalid Follow amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            toolStripStatusLabel.Text = $"Fetching Last {follows} Follows from API...";
            string[] c = getFollowsFromAPI(follows);
            if (c.Length == 0)
            {
                MessageBox.Show("Fetching Follows failed.");
                twitchChat.CurrentStatus = ToolStatus.Ready;
                twitchChat.NotifyPropertyChanged(nameof(twitchChat.CurrentStatus));
                return;
            }

            //usernameOrCommandList = c.ToList<string>();
            txt_ToBan.Lines = c;

            Listinfo = $"{channel} - last {follows} @{DateTime.Now.ToLocalTime():T}";
            lbl_list.Text = Listinfo;

            tabControl.SelectedTab = tabPageFiltering;

            toolStripStatusLabel.Text = "Ready";

            setEnableForControl(true);

            inputListType = ListType.UserList;
        }

        private void OpenListFromURL()
        {
            string URL = Interaction.InputBox("URL of the file.", "Destination");

            toolStripStatusLabel.Text = "Fetching List ...";

            setEnableForControl(true);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(URL);

                HttpResponseMessage response = client.GetAsync(URL).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body.
                    string[] result = response.Content.ReadAsStringAsync()
                        .Result
                        .Split('\n')
                        .Select(x => x.Trim())
                        .Where(x => x != string.Empty)
                        .ToArray();

                    txt_ToBan.Lines = result;

                    lbl_list.Text = URL + " @ " + DateTime.Now.ToString("T");

                    checkListType();

                    toolStripStatusLabel.Text = "Ready";
                }
                else
                {
                    ThrowError("Fetching of the File Failed.", false);
                }
            }
        }

        private void openListURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenListFromURL();
        }

        private void fetchLastFollowersOfChannelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FetchLastFollowers();
        }

        private void openListFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenListFromFile();
        }
    }
}