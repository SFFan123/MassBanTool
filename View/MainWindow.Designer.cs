namespace MassBanTool
{
    partial class MainWindow
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.txt_ToBan = new System.Windows.Forms.TextBox();
            this.progresBar_BanProgress = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_connect = new System.Windows.Forms.Button();
            this.txt_username = new System.Windows.Forms.TextBox();
            this.txt_oauth = new System.Windows.Forms.TextBox();
            this.chk_showOauth = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.lbl_list = new System.Windows.Forms.Label();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelSeperator = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_Channel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusMod = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripWarning = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolstripETA = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_BanIndex = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageBanning = new System.Windows.Forms.TabPage();
            this.btn_Abort = new System.Windows.Forms.Button();
            this.btn_actions_Stop = new System.Windows.Forms.Button();
            this.txt_actions_ban_reason = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_action_run = new System.Windows.Forms.Button();
            this.tabPageFiltering = new System.Windows.Forms.TabPage();
            this.btn_run_regex = new System.Windows.Forms.Button();
            this.txt_uname_regex = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabMisc = new System.Windows.Forms.TabPage();
            this.btn_showConsole = new System.Windows.Forms.Button();
            this.btnRemovePrefixes = new System.Windows.Forms.Button();
            this.tabUnban = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.btn_run_unban = new System.Windows.Forms.Button();
            this.tab_ReadFile = new System.Windows.Forms.TabPage();
            this.radio_Readfile_Ignore = new System.Windows.Forms.RadioButton();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxAllowedActions = new System.Windows.Forms.TextBox();
            this.radio_Readfile_WarnAndAbort = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.btn_AbortReadfile = new System.Windows.Forms.Button();
            this.btn_Pause_Readfile = new System.Windows.Forms.Button();
            this.btn_RunReadfile = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.in_cooldown = new System.Windows.Forms.NumericUpDown();
            this.btn_applyDelay = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openListFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openListURLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fetchLastFollowersOfChannelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLoginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_About = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Releases = new System.Windows.Forms.ToolStripMenuItem();
            this.hELPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.licenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbl_listType = new System.Windows.Forms.Label();
            this.comboBox_channel = new System.Windows.Forms.ComboBox();
            this.linkLabel_CooldownInfo = new System.Windows.Forms.LinkLabel();
            this.checkBox_readfile_protectVIPMods = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageBanning.SuspendLayout();
            this.tabPageFiltering.SuspendLayout();
            this.tabMisc.SuspendLayout();
            this.tabUnban.SuspendLayout();
            this.tab_ReadFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.in_cooldown)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txt_ToBan
            // 
            this.txt_ToBan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txt_ToBan.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_ToBan.Location = new System.Drawing.Point(13, 55);
            this.txt_ToBan.Multiline = true;
            this.txt_ToBan.Name = "txt_ToBan";
            this.txt_ToBan.ReadOnly = true;
            this.txt_ToBan.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_ToBan.Size = new System.Drawing.Size(315, 407);
            this.txt_ToBan.TabIndex = 10000;
            // 
            // progresBar_BanProgress
            // 
            this.progresBar_BanProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progresBar_BanProgress.Location = new System.Drawing.Point(13, 468);
            this.progresBar_BanProgress.Name = "progresBar_BanProgress";
            this.progresBar_BanProgress.Size = new System.Drawing.Size(315, 23);
            this.progresBar_BanProgress.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(419, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Username";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(404, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Oauth Token";
            // 
            // btn_connect
            // 
            this.btn_connect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_connect.Location = new System.Drawing.Point(471, 110);
            this.btn_connect.Name = "btn_connect";
            this.btn_connect.Size = new System.Drawing.Size(109, 23);
            this.btn_connect.TabIndex = 4;
            this.btn_connect.Text = "Connect to Twitch";
            this.btn_connect.UseVisualStyleBackColor = true;
            this.btn_connect.Click += new System.EventHandler(this.btn_connect_Click);
            // 
            // txt_username
            // 
            this.txt_username.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_username.Location = new System.Drawing.Point(480, 31);
            this.txt_username.Name = "txt_username";
            this.txt_username.Size = new System.Drawing.Size(100, 20);
            this.txt_username.TabIndex = 0;
            // 
            // txt_oauth
            // 
            this.txt_oauth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_oauth.Location = new System.Drawing.Point(480, 58);
            this.txt_oauth.Name = "txt_oauth";
            this.txt_oauth.PasswordChar = '*';
            this.txt_oauth.Size = new System.Drawing.Size(100, 20);
            this.txt_oauth.TabIndex = 2;
            // 
            // chk_showOauth
            // 
            this.chk_showOauth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chk_showOauth.AutoSize = true;
            this.chk_showOauth.Location = new System.Drawing.Point(587, 60);
            this.chk_showOauth.Name = "chk_showOauth";
            this.chk_showOauth.Size = new System.Drawing.Size(86, 17);
            this.chk_showOauth.TabIndex = 7;
            this.chk_showOauth.Text = "Show OAuth";
            this.toolTip1.SetToolTip(this.chk_showOauth, "Toggles the hiding of the oauth token, since its a password.");
            this.chk_showOauth.UseVisualStyleBackColor = true;
            this.chk_showOauth.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(423, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Channel";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(679, 55);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(66, 25);
            this.button1.TabIndex = 13;
            this.button1.Tag = "";
            this.button1.Text = "Get OAuth";
            this.toolTip1.SetToolTip(this.button1, "Opens your default browser and goes to https://twitchapps.com/tmi/\r\nwhere you can" +
        " create a oauth token for the twitch chat.\r\nPaste it in the oauth field.");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lbl_list
            // 
            this.lbl_list.AutoSize = true;
            this.lbl_list.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_list.Location = new System.Drawing.Point(12, 29);
            this.lbl_list.Name = "lbl_list";
            this.lbl_list.Size = new System.Drawing.Size(49, 15);
            this.lbl_list.TabIndex = 14;
            this.lbl_list.Text = "<none>";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(12, 17);
            this.toolStripStatusLabel.Text = "-";
            // 
            // toolStripStatusLabelSeperator
            // 
            this.toolStripStatusLabelSeperator.Name = "toolStripStatusLabelSeperator";
            this.toolStripStatusLabelSeperator.Size = new System.Drawing.Size(10, 17);
            this.toolStripStatusLabelSeperator.Text = "|";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(54, 17);
            this.toolStripStatusLabel3.Text = "Channel:";
            // 
            // toolStripStatusLabel_Channel
            // 
            this.toolStripStatusLabel_Channel.Name = "toolStripStatusLabel_Channel";
            this.toolStripStatusLabel_Channel.Size = new System.Drawing.Size(12, 17);
            this.toolStripStatusLabel_Channel.Text = "-";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusLabelSeperator,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel_Channel,
            this.toolStripStatusMod,
            this.toolStripWarning,
            this.toolstripETA,
            this.toolStripStatusLabel_BanIndex});
            this.statusStrip1.Location = new System.Drawing.Point(0, 499);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(754, 22);
            this.statusStrip1.TabIndex = 17;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusMod
            // 
            this.toolStripStatusMod.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripStatusMod.Image = global::MassBanTool.Properties.Resources.moderator2;
            this.toolStripStatusMod.Name = "toolStripStatusMod";
            this.toolStripStatusMod.Size = new System.Drawing.Size(16, 17);
            this.toolStripStatusMod.Visible = false;
            // 
            // toolStripWarning
            // 
            this.toolStripWarning.Name = "toolStripWarning";
            this.toolStripWarning.Size = new System.Drawing.Size(0, 17);
            // 
            // toolstripETA
            // 
            this.toolstripETA.Name = "toolstripETA";
            this.toolstripETA.Size = new System.Drawing.Size(37, 17);
            this.toolstripETA.Text = "ETA: -";
            // 
            // toolStripStatusLabel_BanIndex
            // 
            this.toolStripStatusLabel_BanIndex.Name = "toolStripStatusLabel_BanIndex";
            this.toolStripStatusLabel_BanIndex.Size = new System.Drawing.Size(0, 17);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageBanning);
            this.tabControl.Controls.Add(this.tabPageFiltering);
            this.tabControl.Controls.Add(this.tabMisc);
            this.tabControl.Controls.Add(this.tabUnban);
            this.tabControl.Controls.Add(this.tab_ReadFile);
            this.tabControl.Location = new System.Drawing.Point(334, 228);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(420, 263);
            this.tabControl.TabIndex = 19;
            // 
            // tabPageBanning
            // 
            this.tabPageBanning.Controls.Add(this.btn_Abort);
            this.tabPageBanning.Controls.Add(this.btn_actions_Stop);
            this.tabPageBanning.Controls.Add(this.txt_actions_ban_reason);
            this.tabPageBanning.Controls.Add(this.label4);
            this.tabPageBanning.Controls.Add(this.btn_action_run);
            this.tabPageBanning.Location = new System.Drawing.Point(4, 22);
            this.tabPageBanning.Name = "tabPageBanning";
            this.tabPageBanning.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBanning.Size = new System.Drawing.Size(412, 237);
            this.tabPageBanning.TabIndex = 0;
            this.tabPageBanning.Text = "Banning";
            this.tabPageBanning.UseVisualStyleBackColor = true;
            // 
            // btn_Abort
            // 
            this.btn_Abort.Enabled = false;
            this.btn_Abort.Location = new System.Drawing.Point(87, 208);
            this.btn_Abort.Name = "btn_Abort";
            this.btn_Abort.Size = new System.Drawing.Size(75, 23);
            this.btn_Abort.TabIndex = 8;
            this.btn_Abort.Text = "Abort";
            this.toolTip1.SetToolTip(this.btn_Abort, "Stops and clears the queue.");
            this.btn_Abort.UseVisualStyleBackColor = true;
            this.btn_Abort.Click += new System.EventHandler(this.btn_Abort_Click);
            // 
            // btn_actions_Stop
            // 
            this.btn_actions_Stop.Enabled = false;
            this.btn_actions_Stop.Location = new System.Drawing.Point(6, 208);
            this.btn_actions_Stop.Name = "btn_actions_Stop";
            this.btn_actions_Stop.Size = new System.Drawing.Size(75, 23);
            this.btn_actions_Stop.TabIndex = 7;
            this.btn_actions_Stop.Text = "PAUSE";
            this.toolTip1.SetToolTip(this.btn_actions_Stop, "Pauses/resumes the execution of the banning.");
            this.btn_actions_Stop.UseVisualStyleBackColor = true;
            this.btn_actions_Stop.Click += new System.EventHandler(this.btn_actions_Stop_Click);
            // 
            // txt_actions_ban_reason
            // 
            this.txt_actions_ban_reason.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_actions_ban_reason.Enabled = false;
            this.txt_actions_ban_reason.Location = new System.Drawing.Point(87, 8);
            this.txt_actions_ban_reason.Name = "txt_actions_ban_reason";
            this.txt_actions_ban_reason.Size = new System.Drawing.Size(215, 20);
            this.txt_actions_ban_reason.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Ban Reason: ";
            // 
            // btn_action_run
            // 
            this.btn_action_run.Enabled = false;
            this.btn_action_run.Location = new System.Drawing.Point(329, 208);
            this.btn_action_run.Name = "btn_action_run";
            this.btn_action_run.Size = new System.Drawing.Size(75, 23);
            this.btn_action_run.TabIndex = 4;
            this.btn_action_run.Text = "RUN";
            this.toolTip1.SetToolTip(this.btn_action_run, "Adds all entries to the queue and starts banning.");
            this.btn_action_run.UseVisualStyleBackColor = true;
            this.btn_action_run.Click += new System.EventHandler(this.btn_action_run_Click);
            // 
            // tabPageFiltering
            // 
            this.tabPageFiltering.Controls.Add(this.btn_run_regex);
            this.tabPageFiltering.Controls.Add(this.txt_uname_regex);
            this.tabPageFiltering.Controls.Add(this.label5);
            this.tabPageFiltering.Location = new System.Drawing.Point(4, 22);
            this.tabPageFiltering.Name = "tabPageFiltering";
            this.tabPageFiltering.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFiltering.Size = new System.Drawing.Size(412, 237);
            this.tabPageFiltering.TabIndex = 1;
            this.tabPageFiltering.Text = "Listfilter";
            this.tabPageFiltering.UseVisualStyleBackColor = true;
            // 
            // btn_run_regex
            // 
            this.btn_run_regex.Enabled = false;
            this.btn_run_regex.Location = new System.Drawing.Point(329, 33);
            this.btn_run_regex.Name = "btn_run_regex";
            this.btn_run_regex.Size = new System.Drawing.Size(75, 23);
            this.btn_run_regex.TabIndex = 4;
            this.btn_run_regex.Text = "Run Regex";
            this.toolTip1.SetToolTip(this.btn_run_regex, "Filters the list through the given regex only entries that matches the given patt" +
        "ern will remain in the list.");
            this.btn_run_regex.UseVisualStyleBackColor = true;
            this.btn_run_regex.Click += new System.EventHandler(this.btn_run_regex_Click);
            // 
            // txt_uname_regex
            // 
            this.txt_uname_regex.Location = new System.Drawing.Point(52, 7);
            this.txt_uname_regex.Name = "txt_uname_regex";
            this.txt_uname_regex.Size = new System.Drawing.Size(352, 20);
            this.txt_uname_regex.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Regex";
            // 
            // tabMisc
            // 
            this.tabMisc.Controls.Add(this.btn_showConsole);
            this.tabMisc.Controls.Add(this.btnRemovePrefixes);
            this.tabMisc.Location = new System.Drawing.Point(4, 22);
            this.tabMisc.Name = "tabMisc";
            this.tabMisc.Padding = new System.Windows.Forms.Padding(3);
            this.tabMisc.Size = new System.Drawing.Size(412, 237);
            this.tabMisc.TabIndex = 2;
            this.tabMisc.Text = "Misc";
            this.tabMisc.UseVisualStyleBackColor = true;
            // 
            // btn_showConsole
            // 
            this.btn_showConsole.Location = new System.Drawing.Point(6, 35);
            this.btn_showConsole.Name = "btn_showConsole";
            this.btn_showConsole.Size = new System.Drawing.Size(102, 23);
            this.btn_showConsole.TabIndex = 2;
            this.btn_showConsole.Text = "Show Logwindow";
            this.btn_showConsole.UseVisualStyleBackColor = true;
            this.btn_showConsole.Click += new System.EventHandler(this.btn_showConsole_Click);
            // 
            // btnRemovePrefixes
            // 
            this.btnRemovePrefixes.Enabled = false;
            this.btnRemovePrefixes.Location = new System.Drawing.Point(6, 6);
            this.btnRemovePrefixes.Name = "btnRemovePrefixes";
            this.btnRemovePrefixes.Size = new System.Drawing.Size(102, 23);
            this.btnRemovePrefixes.TabIndex = 1;
            this.btnRemovePrefixes.Text = "Remove clutter";
            this.toolTip1.SetToolTip(this.btnRemovePrefixes, "Removes .ban /ban .mod /ban /somecommand form the nameslist.");
            this.btnRemovePrefixes.UseVisualStyleBackColor = true;
            this.btnRemovePrefixes.Click += new System.EventHandler(this.btnRemovePrefixes_Click);
            // 
            // tabUnban
            // 
            this.tabUnban.Controls.Add(this.button2);
            this.tabUnban.Controls.Add(this.button3);
            this.tabUnban.Controls.Add(this.btn_run_unban);
            this.tabUnban.Location = new System.Drawing.Point(4, 22);
            this.tabUnban.Name = "tabUnban";
            this.tabUnban.Padding = new System.Windows.Forms.Padding(3);
            this.tabUnban.Size = new System.Drawing.Size(412, 237);
            this.tabUnban.TabIndex = 3;
            this.tabUnban.Text = "Unban";
            this.tabUnban.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(87, 208);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "Abort";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btn_Abort_Click);
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.Location = new System.Drawing.Point(6, 208);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 10;
            this.button3.Text = "PAUSE";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btn_actions_Stop_Click);
            // 
            // btn_run_unban
            // 
            this.btn_run_unban.Enabled = false;
            this.btn_run_unban.Location = new System.Drawing.Point(329, 208);
            this.btn_run_unban.Name = "btn_run_unban";
            this.btn_run_unban.Size = new System.Drawing.Size(75, 23);
            this.btn_run_unban.TabIndex = 9;
            this.btn_run_unban.Text = "RUN";
            this.btn_run_unban.UseVisualStyleBackColor = true;
            this.btn_run_unban.Click += new System.EventHandler(this.button4_Click);
            // 
            // tab_ReadFile
            // 
            this.tab_ReadFile.Controls.Add(this.checkBox_readfile_protectVIPMods);
            this.tab_ReadFile.Controls.Add(this.radio_Readfile_Ignore);
            this.tab_ReadFile.Controls.Add(this.label8);
            this.tab_ReadFile.Controls.Add(this.textBoxAllowedActions);
            this.tab_ReadFile.Controls.Add(this.radio_Readfile_WarnAndAbort);
            this.tab_ReadFile.Controls.Add(this.label7);
            this.tab_ReadFile.Controls.Add(this.btn_AbortReadfile);
            this.tab_ReadFile.Controls.Add(this.btn_Pause_Readfile);
            this.tab_ReadFile.Controls.Add(this.btn_RunReadfile);
            this.tab_ReadFile.Location = new System.Drawing.Point(4, 22);
            this.tab_ReadFile.Name = "tab_ReadFile";
            this.tab_ReadFile.Padding = new System.Windows.Forms.Padding(3);
            this.tab_ReadFile.Size = new System.Drawing.Size(412, 237);
            this.tab_ReadFile.TabIndex = 4;
            this.tab_ReadFile.Text = "Readfile";
            this.tab_ReadFile.UseVisualStyleBackColor = true;
            // 
            // radio_Readfile_Ignore
            // 
            this.radio_Readfile_Ignore.AutoSize = true;
            this.radio_Readfile_Ignore.Location = new System.Drawing.Point(141, 43);
            this.radio_Readfile_Ignore.Name = "radio_Readfile_Ignore";
            this.radio_Readfile_Ignore.Size = new System.Drawing.Size(55, 17);
            this.radio_Readfile_Ignore.TabIndex = 18;
            this.radio_Readfile_Ignore.Text = "Ignore";
            this.radio_Readfile_Ignore.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(138, 3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(167, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Behaviour on command Mismatch";
            // 
            // textBoxAllowedActions
            // 
            this.textBoxAllowedActions.Location = new System.Drawing.Point(9, 19);
            this.textBoxAllowedActions.Multiline = true;
            this.textBoxAllowedActions.Name = "textBoxAllowedActions";
            this.textBoxAllowedActions.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxAllowedActions.Size = new System.Drawing.Size(121, 183);
            this.textBoxAllowedActions.TabIndex = 16;
            this.textBoxAllowedActions.Text = "ban\r\nunban\r\nmod\r\nunmod\r\nblock\r\nunblock\r\nvip\r\nunvip\r\ntimeout\r\nslow\r\nslowoff\r\nfollo" +
    "wers\r\nfollowersoff\r\nsubscribers\r\nsubscribersoff\r\nclear\r\nuniquechat\r\nuniquechatof" +
    "f\r\nemoteonly\r\nemoteonlyoff";
            this.toolTip1.SetToolTip(this.textBoxAllowedActions, "Allowed Chat commands for Readfile.\r\none per line.\r\nwithout . or /");
            // 
            // radio_Readfile_WarnAndAbort
            // 
            this.radio_Readfile_WarnAndAbort.AutoSize = true;
            this.radio_Readfile_WarnAndAbort.Checked = true;
            this.radio_Readfile_WarnAndAbort.Location = new System.Drawing.Point(141, 20);
            this.radio_Readfile_WarnAndAbort.Name = "radio_Readfile_WarnAndAbort";
            this.radio_Readfile_WarnAndAbort.Size = new System.Drawing.Size(100, 17);
            this.radio_Readfile_WarnAndAbort.TabIndex = 17;
            this.radio_Readfile_WarnAndAbort.TabStop = true;
            this.radio_Readfile_WarnAndAbort.Text = "Warn and Abort";
            this.radio_Readfile_WarnAndAbort.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(85, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Allowed Actions.";
            // 
            // btn_AbortReadfile
            // 
            this.btn_AbortReadfile.Enabled = false;
            this.btn_AbortReadfile.Location = new System.Drawing.Point(87, 208);
            this.btn_AbortReadfile.Name = "btn_AbortReadfile";
            this.btn_AbortReadfile.Size = new System.Drawing.Size(75, 23);
            this.btn_AbortReadfile.TabIndex = 14;
            this.btn_AbortReadfile.Text = "Abort";
            this.btn_AbortReadfile.UseVisualStyleBackColor = true;
            this.btn_AbortReadfile.Click += new System.EventHandler(this.btn_Abort_Click);
            // 
            // btn_Pause_Readfile
            // 
            this.btn_Pause_Readfile.Enabled = false;
            this.btn_Pause_Readfile.Location = new System.Drawing.Point(6, 208);
            this.btn_Pause_Readfile.Name = "btn_Pause_Readfile";
            this.btn_Pause_Readfile.Size = new System.Drawing.Size(75, 23);
            this.btn_Pause_Readfile.TabIndex = 13;
            this.btn_Pause_Readfile.Text = "PAUSE";
            this.btn_Pause_Readfile.UseVisualStyleBackColor = true;
            this.btn_Pause_Readfile.Click += new System.EventHandler(this.btn_actions_Stop_Click);
            // 
            // btn_RunReadfile
            // 
            this.btn_RunReadfile.Enabled = false;
            this.btn_RunReadfile.Location = new System.Drawing.Point(329, 208);
            this.btn_RunReadfile.Name = "btn_RunReadfile";
            this.btn_RunReadfile.Size = new System.Drawing.Size(75, 23);
            this.btn_RunReadfile.TabIndex = 12;
            this.btn_RunReadfile.Text = "RUN";
            this.btn_RunReadfile.UseVisualStyleBackColor = true;
            this.btn_RunReadfile.Click += new System.EventHandler(this.btn_RunReadfile_Click);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(599, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(151, 26);
            this.label6.TabIndex = 10002;
            this.label6.Text = "Cooldown between messages \r\nin ms";
            // 
            // in_cooldown
            // 
            this.in_cooldown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.in_cooldown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.in_cooldown.Location = new System.Drawing.Point(587, 142);
            this.in_cooldown.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.in_cooldown.Minimum = new decimal(new int[] {
            301,
            0,
            0,
            0});
            this.in_cooldown.Name = "in_cooldown";
            this.in_cooldown.Size = new System.Drawing.Size(79, 20);
            this.in_cooldown.TabIndex = 10003;
            this.toolTip1.SetToolTip(this.in_cooldown, "The Delay between messages sent to Twitch in ms.\r\nThe value can\'t be blow 301.\r\nT" +
        "o apply the changes hit the apply button to the right.");
            this.in_cooldown.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.in_cooldown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.in_cooldown_KeyUp);
            // 
            // btn_applyDelay
            // 
            this.btn_applyDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_applyDelay.Location = new System.Drawing.Point(670, 139);
            this.btn_applyDelay.Name = "btn_applyDelay";
            this.btn_applyDelay.Size = new System.Drawing.Size(75, 23);
            this.btn_applyDelay.TabIndex = 10004;
            this.btn_applyDelay.Text = "Apply";
            this.toolTip1.SetToolTip(this.btn_applyDelay, "Applies the Delay between the messages.");
            this.btn_applyDelay.UseVisualStyleBackColor = true;
            this.btn_applyDelay.Click += new System.EventHandler(this.btn_applyDelay_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.toolStripMenuItem_About});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(754, 25);
            this.menuStrip1.TabIndex = 10005;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openListFileToolStripMenuItem,
            this.openListURLToolStripMenuItem,
            this.fetchLastFollowersOfChannelToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(39, 21);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openListFileToolStripMenuItem
            // 
            this.openListFileToolStripMenuItem.Enabled = false;
            this.openListFileToolStripMenuItem.Name = "openListFileToolStripMenuItem";
            this.openListFileToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.openListFileToolStripMenuItem.Text = "Open List <File>";
            this.openListFileToolStripMenuItem.Click += new System.EventHandler(this.openListFileToolStripMenuItem_Click);
            // 
            // openListURLToolStripMenuItem
            // 
            this.openListURLToolStripMenuItem.Enabled = false;
            this.openListURLToolStripMenuItem.Name = "openListURLToolStripMenuItem";
            this.openListURLToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.openListURLToolStripMenuItem.Text = "Open List <URL>";
            this.openListURLToolStripMenuItem.Click += new System.EventHandler(this.openListURLToolStripMenuItem_Click);
            // 
            // fetchLastFollowersOfChannelToolStripMenuItem
            // 
            this.fetchLastFollowersOfChannelToolStripMenuItem.Enabled = false;
            this.fetchLastFollowersOfChannelToolStripMenuItem.Name = "fetchLastFollowersOfChannelToolStripMenuItem";
            this.fetchLastFollowersOfChannelToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.fetchLastFollowersOfChannelToolStripMenuItem.Text = "Fetch Last Followers of Channel";
            this.fetchLastFollowersOfChannelToolStripMenuItem.Click += new System.EventHandler(this.fetchLastFollowersOfChannelToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveSettingsToolStripMenuItem,
            this.saveLoginToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(66, 21);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // saveSettingsToolStripMenuItem
            // 
            this.saveSettingsToolStripMenuItem.Enabled = false;
            this.saveSettingsToolStripMenuItem.Name = "saveSettingsToolStripMenuItem";
            this.saveSettingsToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.saveSettingsToolStripMenuItem.Text = "Save Settings";
            this.saveSettingsToolStripMenuItem.ToolTipText = "Saves Cooldown, Allowed Actions in Readfile,... to the Appdata to load it later.";
            this.saveSettingsToolStripMenuItem.Click += new System.EventHandler(this.saveSettingsToolStripMenuItem_Click);
            // 
            // saveLoginToolStripMenuItem
            // 
            this.saveLoginToolStripMenuItem.Enabled = false;
            this.saveLoginToolStripMenuItem.Name = "saveLoginToolStripMenuItem";
            this.saveLoginToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.saveLoginToolStripMenuItem.Text = "Save Login";
            this.saveLoginToolStripMenuItem.ToolTipText = "Saves the username and oauth combo to the Windows Credential Manager.";
            this.saveLoginToolStripMenuItem.Click += new System.EventHandler(this.saveLoginToolStripMenuItem_Click);
            // 
            // toolStripMenuItem_About
            // 
            this.toolStripMenuItem_About.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Releases,
            this.hELPToolStripMenuItem,
            this.licenseToolStripMenuItem});
            this.toolStripMenuItem_About.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem_About.Name = "toolStripMenuItem_About";
            this.toolStripMenuItem_About.Size = new System.Drawing.Size(52, 21);
            this.toolStripMenuItem_About.Text = "About";
            // 
            // toolStripMenuItem_Releases
            // 
            this.toolStripMenuItem_Releases.Name = "toolStripMenuItem_Releases";
            this.toolStripMenuItem_Releases.Size = new System.Drawing.Size(118, 22);
            this.toolStripMenuItem_Releases.Text = "Releases";
            this.toolStripMenuItem_Releases.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // hELPToolStripMenuItem
            // 
            this.hELPToolStripMenuItem.Name = "hELPToolStripMenuItem";
            this.hELPToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.hELPToolStripMenuItem.Text = "HELP";
            this.hELPToolStripMenuItem.Click += new System.EventHandler(this.hELPToolStripMenuItem_Click);
            // 
            // licenseToolStripMenuItem
            // 
            this.licenseToolStripMenuItem.Name = "licenseToolStripMenuItem";
            this.licenseToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.licenseToolStripMenuItem.Text = "License";
            this.licenseToolStripMenuItem.Click += new System.EventHandler(this.licenseToolStripMenuItem_Click);
            // 
            // lbl_listType
            // 
            this.lbl_listType.AutoSize = true;
            this.lbl_listType.Location = new System.Drawing.Point(335, 211);
            this.lbl_listType.Name = "lbl_listType";
            this.lbl_listType.Size = new System.Drawing.Size(0, 13);
            this.lbl_listType.TabIndex = 10006;
            // 
            // comboBox_channel
            // 
            this.comboBox_channel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_channel.FormattingEnabled = true;
            this.comboBox_channel.Location = new System.Drawing.Point(480, 84);
            this.comboBox_channel.Name = "comboBox_channel";
            this.comboBox_channel.Size = new System.Drawing.Size(121, 21);
            this.comboBox_channel.TabIndex = 10007;
            this.comboBox_channel.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBox_channel_KeyUp);
            // 
            // linkLabel_CooldownInfo
            // 
            this.linkLabel_CooldownInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel_CooldownInfo.AutoSize = true;
            this.linkLabel_CooldownInfo.Location = new System.Drawing.Point(676, 178);
            this.linkLabel_CooldownInfo.Name = "linkLabel_CooldownInfo";
            this.linkLabel_CooldownInfo.Size = new System.Drawing.Size(74, 13);
            this.linkLabel_CooldownInfo.TabIndex = 10008;
            this.linkLabel_CooldownInfo.TabStop = true;
            this.linkLabel_CooldownInfo.Text = "Cooldown info";
            this.linkLabel_CooldownInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_CooldownInfo_LinkClicked);
            // 
            // checkBox_readfile_protectVIPMods
            // 
            this.checkBox_readfile_protectVIPMods.AutoSize = true;
            this.checkBox_readfile_protectVIPMods.Checked = true;
            this.checkBox_readfile_protectVIPMods.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_readfile_protectVIPMods.Location = new System.Drawing.Point(142, 66);
            this.checkBox_readfile_protectVIPMods.Name = "checkBox_readfile_protectVIPMods";
            this.checkBox_readfile_protectVIPMods.Size = new System.Drawing.Size(193, 17);
            this.checkBox_readfile_protectVIPMods.TabIndex = 20;
            this.checkBox_readfile_protectVIPMods.Text = "Protect Mods/VIPs from commands";
            this.checkBox_readfile_protectVIPMods.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 521);
            this.Controls.Add(this.linkLabel_CooldownInfo);
            this.Controls.Add(this.comboBox_channel);
            this.Controls.Add(this.lbl_listType);
            this.Controls.Add(this.btn_applyDelay);
            this.Controls.Add(this.in_cooldown);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.lbl_list);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chk_showOauth);
            this.Controls.Add(this.txt_oauth);
            this.Controls.Add(this.txt_username);
            this.Controls.Add(this.btn_connect);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progresBar_BanProgress);
            this.Controls.Add(this.txt_ToBan);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(770, 560);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MassBanTool v. 0.4.5 Beta";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageBanning.ResumeLayout(false);
            this.tabPageBanning.PerformLayout();
            this.tabPageFiltering.ResumeLayout(false);
            this.tabPageFiltering.PerformLayout();
            this.tabMisc.ResumeLayout(false);
            this.tabUnban.ResumeLayout(false);
            this.tab_ReadFile.ResumeLayout(false);
            this.tab_ReadFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.in_cooldown)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_ToBan;
        private System.Windows.Forms.ProgressBar progresBar_BanProgress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_connect;
        private System.Windows.Forms.TextBox txt_username;
        private System.Windows.Forms.TextBox txt_oauth;
        private System.Windows.Forms.CheckBox chk_showOauth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lbl_list;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSeperator;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Channel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusMod;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageBanning;
        private System.Windows.Forms.Button btn_actions_Stop;
        private System.Windows.Forms.TextBox txt_actions_ban_reason;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_action_run;
        private System.Windows.Forms.TabPage tabPageFiltering;
        private System.Windows.Forms.Button btn_run_regex;
        private System.Windows.Forms.TextBox txt_uname_regex;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripWarning;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown in_cooldown;
        private System.Windows.Forms.Button btn_applyDelay;
        private System.Windows.Forms.ToolStripStatusLabel toolstripETA;
        private System.Windows.Forms.TabPage tabMisc;
        private System.Windows.Forms.Button btn_Abort;
        private System.Windows.Forms.TabPage tabUnban;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btn_run_unban;
        private System.Windows.Forms.Button btnRemovePrefixes;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabPage tab_ReadFile;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btn_AbortReadfile;
        private System.Windows.Forms.Button btn_Pause_Readfile;
        private System.Windows.Forms.Button btn_RunReadfile;
        private System.Windows.Forms.TextBox textBoxAllowedActions;
        private System.Windows.Forms.RadioButton radio_Readfile_Ignore;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RadioButton radio_Readfile_WarnAndAbort;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_About;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Releases;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_BanIndex;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openListFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openListURLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fetchLastFollowersOfChannelToolStripMenuItem;
        private System.Windows.Forms.Label lbl_listType;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLoginToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBox_channel;
        private System.Windows.Forms.ToolStripMenuItem hELPToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem licenseToolStripMenuItem;
        private System.Windows.Forms.Button btn_showConsole;
        private System.Windows.Forms.LinkLabel linkLabel_CooldownInfo;
        private System.Windows.Forms.CheckBox checkBox_readfile_protectVIPMods;
    }
}

