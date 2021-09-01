namespace MassBanTool
{
    partial class Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form));
            this.txt_ToBan = new System.Windows.Forms.TextBox();
            this.progresBar_BanProgress = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_connect = new System.Windows.Forms.Button();
            this.txt_username = new System.Windows.Forms.TextBox();
            this.txt_oauth = new System.Windows.Forms.TextBox();
            this.chk_showOauth = new System.Windows.Forms.CheckBox();
            this.txt_channel = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_OpenList = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.pbModerator = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lbl_list = new System.Windows.Forms.Label();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelSeperator = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_Channel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_Username = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusMod = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripWarning = new System.Windows.Forms.ToolStripStatusLabel();
            this.btn_getFollows = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageBanning = new System.Windows.Forms.TabPage();
            this.btn_actions_Stop = new System.Windows.Forms.Button();
            this.txt_actions_ban_reason = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_action_run = new System.Windows.Forms.Button();
            this.tabPageFiltering = new System.Windows.Forms.TabPage();
            this.btn_run_regex = new System.Windows.Forms.Button();
            this.txt_uname_regex = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_saveLogin = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.in_cooldown = new System.Windows.Forms.NumericUpDown();
            this.btn_applyDelay = new System.Windows.Forms.Button();
            this.toolstripETA = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pbModerator)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageBanning.SuspendLayout();
            this.tabPageFiltering.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.in_cooldown)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_ToBan
            // 
            this.txt_ToBan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txt_ToBan.Location = new System.Drawing.Point(13, 38);
            this.txt_ToBan.Multiline = true;
            this.txt_ToBan.Name = "txt_ToBan";
            this.txt_ToBan.ReadOnly = true;
            this.txt_ToBan.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_ToBan.Size = new System.Drawing.Size(315, 385);
            this.txt_ToBan.TabIndex = 10000;
            // 
            // progresBar_BanProgress
            // 
            this.progresBar_BanProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progresBar_BanProgress.Location = new System.Drawing.Point(13, 429);
            this.progresBar_BanProgress.Name = "progresBar_BanProgress";
            this.progresBar_BanProgress.Size = new System.Drawing.Size(315, 23);
            this.progresBar_BanProgress.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(358, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Username (lower case)";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(403, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Oauth Token";
            // 
            // btn_connect
            // 
            this.btn_connect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_connect.Location = new System.Drawing.Point(470, 107);
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
            this.txt_username.Location = new System.Drawing.Point(479, 28);
            this.txt_username.Name = "txt_username";
            this.txt_username.Size = new System.Drawing.Size(100, 20);
            this.txt_username.TabIndex = 0;
            // 
            // txt_oauth
            // 
            this.txt_oauth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_oauth.Location = new System.Drawing.Point(479, 55);
            this.txt_oauth.Name = "txt_oauth";
            this.txt_oauth.PasswordChar = '*';
            this.txt_oauth.Size = new System.Drawing.Size(100, 20);
            this.txt_oauth.TabIndex = 2;
            // 
            // chk_showOauth
            // 
            this.chk_showOauth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chk_showOauth.AutoSize = true;
            this.chk_showOauth.Location = new System.Drawing.Point(586, 57);
            this.chk_showOauth.Name = "chk_showOauth";
            this.chk_showOauth.Size = new System.Drawing.Size(86, 17);
            this.chk_showOauth.TabIndex = 7;
            this.chk_showOauth.Text = "Show OAuth";
            this.chk_showOauth.UseVisualStyleBackColor = true;
            this.chk_showOauth.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // txt_channel
            // 
            this.txt_channel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_channel.Location = new System.Drawing.Point(479, 81);
            this.txt_channel.Name = "txt_channel";
            this.txt_channel.Size = new System.Drawing.Size(100, 20);
            this.txt_channel.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(370, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Channel (lowercase)";
            // 
            // btn_OpenList
            // 
            this.btn_OpenList.Enabled = false;
            this.btn_OpenList.Location = new System.Drawing.Point(13, 9);
            this.btn_OpenList.Name = "btn_OpenList";
            this.btn_OpenList.Size = new System.Drawing.Size(75, 23);
            this.btn_OpenList.TabIndex = 10;
            this.btn_OpenList.Text = "Open List";
            this.btn_OpenList.UseVisualStyleBackColor = true;
            this.btn_OpenList.Click += new System.EventHandler(this.btn_OpenList_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog";
            // 
            // pbModerator
            // 
            this.pbModerator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbModerator.Image = global::MassBanTool.Properties.Resources.moderator2;
            this.pbModerator.Location = new System.Drawing.Point(586, 104);
            this.pbModerator.Name = "pbModerator";
            this.pbModerator.Size = new System.Drawing.Size(27, 26);
            this.pbModerator.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbModerator.TabIndex = 12;
            this.pbModerator.TabStop = false;
            this.pbModerator.Visible = false;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(678, 52);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(66, 25);
            this.button1.TabIndex = 13;
            this.button1.Text = "Get OAuth";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lbl_list
            // 
            this.lbl_list.AutoSize = true;
            this.lbl_list.Location = new System.Drawing.Point(175, 14);
            this.lbl_list.Name = "lbl_list";
            this.lbl_list.Size = new System.Drawing.Size(43, 13);
            this.lbl_list.TabIndex = 14;
            this.lbl_list.Text = "<none>";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel.Text = "Ready";
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
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(63, 17);
            this.toolStripStatusLabel2.Text = "Username:";
            // 
            // toolStripStatusLabel_Username
            // 
            this.toolStripStatusLabel_Username.Name = "toolStripStatusLabel_Username";
            this.toolStripStatusLabel_Username.Size = new System.Drawing.Size(12, 17);
            this.toolStripStatusLabel_Username.Text = "-";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusLabelSeperator,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel_Channel,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel_Username,
            this.toolStripStatusMod,
            this.toolStripWarning,
            this.toolstripETA});
            this.statusStrip1.Location = new System.Drawing.Point(0, 460);
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
            // btn_getFollows
            // 
            this.btn_getFollows.Enabled = false;
            this.btn_getFollows.Location = new System.Drawing.Point(94, 9);
            this.btn_getFollows.Name = "btn_getFollows";
            this.btn_getFollows.Size = new System.Drawing.Size(75, 23);
            this.btn_getFollows.TabIndex = 18;
            this.btn_getFollows.Text = "Get Follows";
            this.btn_getFollows.UseVisualStyleBackColor = true;
            this.btn_getFollows.Click += new System.EventHandler(this.btn_getFollows_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageBanning);
            this.tabControl.Controls.Add(this.tabPageFiltering);
            this.tabControl.Location = new System.Drawing.Point(334, 189);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(420, 263);
            this.tabControl.TabIndex = 19;
            // 
            // tabPageBanning
            // 
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
            // btn_actions_Stop
            // 
            this.btn_actions_Stop.Enabled = false;
            this.btn_actions_Stop.Location = new System.Drawing.Point(6, 208);
            this.btn_actions_Stop.Name = "btn_actions_Stop";
            this.btn_actions_Stop.Size = new System.Drawing.Size(75, 23);
            this.btn_actions_Stop.TabIndex = 7;
            this.btn_actions_Stop.Text = "Abort";
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
            this.tabPageFiltering.Text = "Follow filter";
            this.tabPageFiltering.UseVisualStyleBackColor = true;
            // 
            // btn_run_regex
            // 
            this.btn_run_regex.Location = new System.Drawing.Point(329, 33);
            this.btn_run_regex.Name = "btn_run_regex";
            this.btn_run_regex.Size = new System.Drawing.Size(75, 23);
            this.btn_run_regex.TabIndex = 4;
            this.btn_run_regex.Text = "Run Regex";
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
            // btn_saveLogin
            // 
            this.btn_saveLogin.Location = new System.Drawing.Point(470, 136);
            this.btn_saveLogin.Name = "btn_saveLogin";
            this.btn_saveLogin.Size = new System.Drawing.Size(109, 23);
            this.btn_saveLogin.TabIndex = 10001;
            this.btn_saveLogin.Text = "Save Login*";
            this.btn_saveLogin.UseVisualStyleBackColor = true;
            this.btn_saveLogin.Visible = false;
            this.btn_saveLogin.Click += new System.EventHandler(this.button2_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(596, 162);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(148, 13);
            this.label6.TabIndex = 10002;
            this.label6.Text = "Cooldown between messages";
            // 
            // in_cooldown
            // 
            this.in_cooldown.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.in_cooldown.Location = new System.Drawing.Point(599, 139);
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
            this.in_cooldown.Size = new System.Drawing.Size(66, 20);
            this.in_cooldown.TabIndex = 10003;
            this.in_cooldown.Value = new decimal(new int[] {
            301,
            0,
            0,
            0});
            // 
            // btn_applyDelay
            // 
            this.btn_applyDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_applyDelay.Location = new System.Drawing.Point(669, 136);
            this.btn_applyDelay.Name = "btn_applyDelay";
            this.btn_applyDelay.Size = new System.Drawing.Size(75, 23);
            this.btn_applyDelay.TabIndex = 10004;
            this.btn_applyDelay.Text = "Apply";
            this.btn_applyDelay.UseVisualStyleBackColor = true;
            this.btn_applyDelay.Click += new System.EventHandler(this.btn_applyDelay_Click);
            // 
            // toolstripETA
            // 
            this.toolstripETA.Name = "toolstripETA";
            this.toolstripETA.Size = new System.Drawing.Size(37, 17);
            this.toolstripETA.Text = "ETA: -";
            // 
            // Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 482);
            this.Controls.Add(this.btn_applyDelay);
            this.Controls.Add(this.in_cooldown);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btn_saveLogin);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btn_getFollows);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lbl_list);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pbModerator);
            this.Controls.Add(this.btn_OpenList);
            this.Controls.Add(this.txt_channel);
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
            this.MinimumSize = new System.Drawing.Size(770, 521);
            this.Name = "Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MassBanTool v. 0.2.0";
            ((System.ComponentModel.ISupportInitialize)(this.pbModerator)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageBanning.ResumeLayout(false);
            this.tabPageBanning.PerformLayout();
            this.tabPageFiltering.ResumeLayout(false);
            this.tabPageFiltering.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.in_cooldown)).EndInit();
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
        private System.Windows.Forms.TextBox txt_channel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_OpenList;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.PictureBox pbModerator;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lbl_list;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSeperator;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Channel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_Username;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusMod;
        private System.Windows.Forms.Button btn_getFollows;
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
        private System.Windows.Forms.Button btn_saveLogin;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown in_cooldown;
        private System.Windows.Forms.Button btn_applyDelay;
        private System.Windows.Forms.ToolStripStatusLabel toolstripETA;
    }
}

