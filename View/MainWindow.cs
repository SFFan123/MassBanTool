using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CredentialManagement;
using MassBanTool.Models;
using MassBanTool.View;
using Newtonsoft.Json.Linq;
using DialogResult = System.Windows.Forms.DialogResult;

namespace MassBanTool
{
    public partial class MainWindow : Form
    {
        public const string version = "v.0.4.5.1";
        private string channel;
        private Mutex channelMutex;
        private Regex channelRegex = new Regex(@"^\w{4,26}$");
        private bool connected;
        private bool checkForUpdatesOnStartUp;
        private bool includePrereleasesUpdates;


        private ListType inputListType = default;
        private string oauth;
        TwitchChatClient twitchChat = null;
        private string uname;


        public MainWindow()
        {
            InitializeComponent();
            getLogin();
            LoadData();
            Shown += delegate(object sender, EventArgs args) { Task.Run( () => checkForUpdates() ); };
        }

        private void checkForUpdates(bool manual = false)
        {
            if (!checkForUpdatesOnStartUp && !manual)
            {
                return;
            }

            string url = "https://api.github.com/repos/SFFan123/MassBanTool/releases";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);

                client.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");//Set the User Agent to "request"

                HttpResponseMessage response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body.
                    string result = response.Content.ReadAsStringAsync().Result;
                    JArray jArray = JArray.Parse(result);

                    List<JObject> releases = new List<JObject>();
                    foreach (var item in jArray)
                    {
                        releases.Add(JObject.Parse(item.ToString()));
                    }

                    if (!includePrereleasesUpdates)
                        releases = releases.Where(x => !(bool)x["prerelease"]).ToList();

                    
                    string remoteversion = Regex.Match( (string)releases[0]["tag_name"], @"^v\.(\d+\.)+\d+").Value;

                    var current = Version.Parse(version.Replace("v.", ""));
                    var remote = Version.Parse(remoteversion.Replace("v.", ""));

                    if (current.CompareTo(remote) < 1)
                    {
                        var diag_result = MessageBox.Show($"Version {releases[0]["name"]} is available on Github.\n" +
                                                          $"Released on: {DateTime.Parse(releases[0]["published_at"].ToString()):D}\n" +
                                                          "Click Yes to open the release page, no to discard this dialog.", "New Version Avaibale", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (diag_result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("https://github.com/SFFan123/MassBanTool/releases/latest");
                        }
                    }

                }
            }
        }

        public HashSet<string> lastVisitedChannels { get; private set; }

        public List<string> usernameOrCommandList
        {
            get => listBox_toBan.Items.Cast<string>().ToList();
        }

        public static LogWindow logwindow { get; private set; }

        public string Listinfo { get; set; } = "<none>";


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_showOauth.Checked)
            {
                txt_oauth.PasswordChar = '\0';
            }
            else
            {
                txt_oauth.PasswordChar = '\u25CF';
            }
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            string channel = comboBox_channel.Text.Trim().ToLower();
            if (channel == string.Empty)
            {
                ThrowError("Invalid Channel", false);
                return;
            }

            List<string> channelList = channel.Split(',').Select(x => x.Trim()).ToList();


            foreach (var chl in channelList)
            {
                if (!channelRegex.IsMatch(chl))
                {
                    ShowWarning($"Invalid Channel name '{chl}'.");
                    return;
                }
            }


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
                            twitchChat.switchChannel(channelList);
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
                    twitchChat.switchChannel(channelList);
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

            if (!CheckMutex())
            {
                ShowWarning(
                    "There is already an instance running with that username.\nIf you just closed the other instance wait 2 secs and try again.");
                return;
            }

            twitchChat = new TwitchChatClient(username, oauth, channelList, this);

            twitchChat.PropertyChanged += ClientPropChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if there is no mutex for the desired username.</returns>
        private bool CheckMutex()
        {
            try
            {
                channelMutex = new Mutex(true, "MassBanTool_" + uname);
                return channelMutex.WaitOne(TimeSpan.Zero, true);
            }
            catch (Exception)
            {
                return false;
            }
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
                            lbl_listType.ForeColor = DefaultForeColor;
                            break;

                        case ListType.Mixed:
                            btn_run_regex.Enabled = true;
                            btnRemovePrefixes.Enabled = true;

                            btn_RunReadfile.Enabled = false;
                            btn_run_unban.Enabled = false;
                            btn_action_run.Enabled = false;
                            txt_actions_ban_reason.Enabled = false;
                            lbl_listType.ForeColor = DefaultForeColor;
                            break;

                        case ListType.UserList:
                            btn_run_regex.Enabled = true;
                            btn_action_run.Enabled = true;
                            btn_run_unban.Enabled = true;
                            txt_actions_ban_reason.Enabled = true;

                            btnRemovePrefixes.Enabled = false;
                            btn_RunReadfile.Enabled = false;
                            lbl_listType.ForeColor = DefaultForeColor;
                            break;

                        case ListType.Malformed:
                            btn_run_regex.Enabled = false;
                            btn_action_run.Enabled = false;
                            btn_run_unban.Enabled = false;
                            txt_actions_ban_reason.Enabled = false;

                            btnRemovePrefixes.Enabled = false;
                            btn_RunReadfile.Enabled = false;

                            lbl_listType.ForeColor = Color.Red;
                            break;

                        default:
                            btn_run_regex.Enabled = false;
                            btn_action_run.Enabled = false;
                            btn_run_unban.Enabled = false;
                            txt_actions_ban_reason.Enabled = false;

                            btnRemovePrefixes.Enabled = false;
                            btn_RunReadfile.Enabled = false;

                            lbl_listType.ForeColor = Color.Red;
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

                if (mod)
                {
                    toolStripStatusMod.Image = Properties.Resources.moderator2;
                }
                else if (broadcaster)
                {
                    toolStripStatusMod.Image = Properties.Resources.broadcaster2;
                }
            }
        }

        public void setInfo(object sender, string channel)
        {
            connected = true;
            this.channel = channel;
            toolStripStatusLabel_Channel.Text = channel;

            if (lastVisitedChannels == null)
            {
                lastVisitedChannels = new HashSet<string>();
            }

            lastVisitedChannels.Add(channel);


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

            if (saveSettingsToolStripMenuItem.GetCurrentParent().InvokeRequired)
            {
                saveSettingsToolStripMenuItem.GetCurrentParent().Invoke(new Action(() =>
                {
                    saveSettingsToolStripMenuItem.Enabled = true;
                }));

                saveLoginToolStripMenuItem.GetCurrentParent().Invoke(new Action(() =>
                {
                    saveLoginToolStripMenuItem.Enabled = true;
                }));
            }
            else
            {
                saveSettingsToolStripMenuItem.Enabled = true;
                saveLoginToolStripMenuItem.Enabled = true;
            }


            if (InvokeRequired)
            {
                btn_connect.Invoke(new Action(() => { btn_connect.Text = "Switch Channel"; }));
                txt_oauth.Invoke(new Action(() => { txt_oauth.ReadOnly = true; }));
                txt_username.Invoke(new Action(() => { txt_username.ReadOnly = true; }));
                comboBox_channel.Invoke(new Action(() =>
                {
                    comboBox_channel.Items.Add(channel);
                    comboBox_channel.Refresh();
                }));
            }
            else
            {
                btn_connect.Text = "Switch Channel";
                txt_oauth.ReadOnly = true;
                txt_username.ReadOnly = true;

                comboBox_channel.Items.Add(channel);
                comboBox_channel.Refresh();
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

            if (TwitchChatClient.mt_pause)
            {
                twitchChat.TargetStatus_for_pause = twitchChat.CurrentStatus;
                twitchChat.CurrentStatus = ToolStatus.Paused;
            }
            else
            {
                twitchChat.CurrentStatus = twitchChat.TargetStatus_for_pause;
            }

            pauseButtonToggleText();

            twitchChat.NotifyPropertyChanged(nameof(twitchChat.CurrentStatus));
        }

        private void pauseButtonToggleText()
        {
            string resume = "RESUME";
            string pause = "PAUSE";
            if (TwitchChatClient.mt_pause)
            {
                btn_actions_Stop.Text = resume;
                btn_Pause_Readfile.Text = resume;
                button3.Text = resume;
            }
            else
            {
                btn_actions_Stop.Text = pause;
                btn_Pause_Readfile.Text = pause;
                button3.Text = pause;
            }
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

        private void setListBoxRows(IEnumerable<object> input)
        {
            listBox_toBan.Items.Clear();

            var items = input.AsParallel().Select(x => new ListViewItem(x.ToString())).ToArray();

            listBox_toBan.Items.AddRange(items);
        }

        private void btn_run_regex_Click(object sender, EventArgs e)
        {
            string sregex = txt_uname_regex.Text;
            Regex rgx = new Regex(sregex);
            List<string> newToBan = new List<string>();
            progresBar_BanProgress.Maximum = usernameOrCommandList.Count;
            
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
            setListBoxRows(usernameOrCommandList);
            progresBar_BanProgress.Maximum = 100;
            progresBar_BanProgress.Value = 100;
            twitchChat.CurrentStatus = ToolStatus.Ready;
            twitchChat.NotifyPropertyChanged(nameof(twitchChat.CurrentStatus));
        }

        private void getLogin()
        {
            using (var cred = new Credential())
            {
                cred.Target = "MassBanTool";
                cred.Load();
                if (cred.Exists())
                {
                    txt_username.Text = cred.Username;
                    txt_oauth.Text = cred.Password;
                }

                string fileName = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolLogin.txt");

                if (!cred.Exists() && File.Exists(fileName))
                {
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
                        log(e.ToString());
                    }
                }

                if (File.Exists(fileName))
                {
                    Shown += movelogin;
                }
            }
        }

        private void movelogin(object sender, EventArgs eventArgs)
        {
            var tsk = Task.Run(() =>
            {
                saveLogin();
                Shown -= movelogin;
            });
        }

        private void LoadData()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolData.json");
            try
            {
                string a = File.ReadAllText(fileName);
                a = a.Trim();
                var data = DataWrapper.fromJson(a);

                if (data.message_delay != default)
                {
                    in_cooldown.Text = data.message_delay.ToString();
                }

                if (data.lastVisitedChannel != null)
                {
                    lastVisitedChannels = data.lastVisitedChannel;
                }
                else
                {
                    lastVisitedChannels = new HashSet<string>();
                }

                if (data.allowedActions != null)
                {
                    textBoxAllowedActions.Lines = data.allowedActions.ToArray();
                }

                comboBox_channel.Items.AddRange(lastVisitedChannels.ToArray<object>());

                checkForUpdatesOnStartUp = data.checkForUpdates;
                checkForUpdatesOnStartupToolStripMenuItem.Checked = data.checkForUpdates;

                includePrereleasesUpdates = data.includePrereleases;
                includePrereleasesToolStripMenuItem.Checked = data.includePrereleases;
            }
            catch (Exception)
            {
                // ignored
            }
        }


        private void saveLogin()
        {
            if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(oauth))
            {
                uname = txt_username.Text.Trim();
                oauth = txt_oauth.Text.Trim();

                if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(oauth))
                    return;
            }

            string fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolLogin.txt");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }


            using (var cred = new Credential())
            {
                cred.Password = oauth;
                cred.Target = "MassBanTool";
                cred.Username = uname;
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.LocalComputer;
                cred.Save();
            }
        }

        private void saveData()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolData.json");
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

            int delay;
            if (twitchChat.cooldown == 301)
            {
                if (!int.TryParse(in_cooldown.Text, out delay))
                {
                    delay = 301;
                }
            }
            else
            {
                delay = twitchChat.cooldown;
            }

            var data = new DataWrapper()
            {
                lastVisitedChannel = lastVisitedChannels,
                allowedActions = textBoxAllowedActions.Lines
                    .Select(x => x.Trim())
                    .Where(x => x != string.Empty)
                    .ToHashSet(),
                message_delay = delay,
                checkForUpdates = checkForUpdatesOnStartUp,
                includePrereleases = includePrereleasesUpdates
            };

            string result = data.toJSON();


            File.WriteAllText(fileName, result);
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

        public void ThrowError(string message, bool exitonThrow = true, bool hardError = false)
        {
            if (hardError)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { this.Enabled = false; }));
                }
                else
                {
                    Enabled = false;
                }
            }

            MessageBox.Show(message, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (exitonThrow)
                Environment.Exit(-1);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            setListBoxRows(result.ToArray());
            checkListType();
        }

        private void btn_RunReadfile_Click(object sender, EventArgs e)
        {
            bool showWarning = radio_Readfile_WarnAndAbort.Checked;
            bool protectVIPMods = checkBox_readfile_protectVIPMods.Checked;


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

            // TODO
            // username regex?
            Regex readfileParseRegex = new Regex(@"^(?:.|\/)([a-zA-Z]+) (\w{4,25})( .+)?$", RegexOptions.Compiled);

            List<string> filteredCommands = new List<string>();
            foreach (string line in usernameOrCommandList)
            {
                var match = readfileParseRegex.Match(line);

                if (!match.Success)
                {
                    if (showWarning)
                    {
                        ShowWarning(
                            "Cannot parse Readfile. Reasons for this can be invalid usernames or invalid command syntax.");
                        return;
                    }

                    continue;
                }

                // check if command is allowed.
                if (allowList.Contains(match.Groups[1].Value))
                {
                    if (protectVIPMods)
                    {
                        if (twitchChat.allSpecialChannelUser.Contains(match.Groups[2].Value))
                        {
                            if (showWarning)
                            {
                                ShowWarning($"Protected user '{match.Groups[2].Value}' targeted in the file.");
                                return;
                            }

                            continue;
                        }

                        filteredCommands.Add(line);
                    }
                    else
                    {
                        filteredCommands.Add(line);
                    }
                }
                else
                {
                    if (showWarning)
                    {
                        ShowWarning("Mismatch between allowed commands and commands used in the file.");
                        return;
                    }

                    continue;
                }
            }

            // Put in Queue
            twitchChat.addRawMessages(filteredCommands);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SFFan123/MassBanTool/releases");
        }

        private void checkListType()
        {
            inputListType = ListType.None;
            int listEnumerator = 0;
            foreach (ListViewItem line in listBox_toBan.Items)
            {
                if (line.Text.StartsWith("/") || line.Text.StartsWith("."))
                {
                    if (inputListType == ListType.None || inputListType == ListType.ReadFile)
                    {
                        inputListType = ListType.ReadFile;
                    }
                    else
                    {
                        log($"INFO: Line {listEnumerator} -> '{line.Text}' --- triggered Listtype Mixed");
                        inputListType = ListType.Mixed;
                        listBox_toBan.Items[listEnumerator].BackColor = Color.Yellow;
                    }

                    if (inputListType == ListType.Mixed)
                    {
                        twitchChat.NotifyPropertyChanged(nameof(inputListType));
                        return;
                    }
                }
                else if (line.Text.Contains(" "))
                {
                    log(
                        $"INFO: Line {listEnumerator} -> '{line.Text}' --- triggered Listtype Malformed");
                    inputListType = ListType.Malformed;
                    twitchChat.NotifyPropertyChanged(nameof(inputListType));
                    listBox_toBan.Items[listEnumerator].BackColor = Color.Red;
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
                setListBoxRows(fileContent);

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

            var input_field = new NumericUpDown();
            input_field.Minimum = 100;
            input_field.Maximum = 5000;
            input_field.Increment = 100;

            var diag = InputDialog.Show("Amount of Follows? must be between 100 and 5000 - in steps of 100",
                "Fetch amount", input_field, out string input);

            if (diag != DialogResult.OK)
            {
                return;
            }

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
            setListBoxRows(c);

            Listinfo = $"{channel} - last {follows} @{DateTime.Now.ToLocalTime():T}";
            lbl_list.Text = Listinfo;

            tabControl.SelectedTab = tabPageFiltering;

            toolStripStatusLabel.Text = "Ready";

            setEnableForControl(true);

            inputListType = ListType.UserList;
        }

        private void OpenListFromURL()
        {
            var diag = InputDialog.Show("URL of the file.", "Destination", new TextBox(), out string URL);

            if (diag != DialogResult.OK)
            {
                return;
            }

            if (string.Empty == URL)
            {
                ShowWarning("Invalid URL");
                return;
            }

            toolStripStatusLabel.Text = "Fetching List ...";

            setEnableForControl(true);

            using (var client = new HttpClient())
            {
                try
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

                        if (result.Length == 0)
                        {
                            toolStripStatusLabel.Text = "Ready";
                            return;
                        }

                        setListBoxRows(result);

                        lbl_list.Text = URL + " @ " + DateTime.Now.ToString("T");

                        checkListType();

                        toolStripStatusLabel.Text = "Ready";
                    }
                    else
                    {
                        ThrowError("Fetching of the File Failed.", false);
                    }
                }
                catch (UriFormatException)
                {
                    ShowWarning("Invalid URL");
                    toolStripStatusLabel.Text = "Ready";
                    return;
                }
                catch (HttpRequestException e)
                {
                    ThrowError("Fetching of the File Failed.\n" + e.Message, false);
                    toolStripStatusLabel.Text = "Ready";
                    return;
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

        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveData();
        }

        private void saveLoginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveLogin();
        }

        private void comboBox_channel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_connect_Click(null, null);
            }
        }

        private void in_cooldown_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_applyDelay_Click(null, null);
            }
        }

        private void hELPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SFFan123/MassBanTool/wiki/ELP");
        }

        private void licenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SFFan123/MassBanTool/blob/master/LICENSE");
        }

        public void increaseDelay(int cooldown)
        {
            if (in_cooldown.InvokeRequired)
            {
                in_cooldown.Invoke(new Action(() => { in_cooldown.Text = cooldown.ToString(); }));
            }
            else
            {
                in_cooldown.Text = cooldown.ToString();
            }
        }

        private void btn_showConsole_Click(object sender, EventArgs e)
        {
            if (logwindow == null || logwindow.IsDisposed)
            {
                logwindow = new LogWindow();
                logwindow.Show();
            }
            else
            {
                logwindow.WindowState = FormWindowState.Normal;
                logwindow.Top = this.Top;
                logwindow.Left = this.Left;
                logwindow.TopMost = true;
                logwindow.TopMost = false;
            }
        }

        private void log(string line)
        {
            if (logwindow != null && !logwindow.IsDisposed)
                logwindow.Log(line);
        }

        private void linkLabel_CooldownInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel_CooldownInfo.LinkVisited = true;
            System.Diagnostics.Process.Start("https://github.com/SFFan123/MassBanTool/wiki/Cooldown-between-Messages");
        }

        private void listBox_toBan_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                object[] itemsToRemove = new object[listBox_toBan.SelectedItems.Count];
                listBox_toBan.SelectedItems.CopyTo(itemsToRemove, 0);
                foreach (var objecttoRemove in itemsToRemove)
                {
                    listBox_toBan.Items.Remove((ListViewItem)objecttoRemove);
                }
                listBox_toBan.Select();
                GC.Collect();
            }
        }

        private void checkForUpdatesOnStartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkForUpdatesOnStartUp = !checkForUpdatesOnStartUp;
            UpdatesToolStripMenuItem.Checked = checkForUpdatesOnStartUp;
        }

        private void includePrereleasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            includePrereleasesUpdates = !includePrereleasesUpdates;
            includePrereleasesToolStripMenuItem.Checked = includePrereleasesUpdates;
        }

        private void checkForUpdatesNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkForUpdates(true);
        }
    }
}