namespace Beacon
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            labelHostStatus = new Label();
            labelPName = new Label();
            labelDifficulty = new Label();
            labelPCount = new Label();
            labelTCPAddress = new Label();
            label10 = new Label();
            labelLCount = new Label();
            label6 = new Label();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            openToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            label7 = new Label();
            labelPMode = new Label();
            gameList = new DataGridView();
            label5 = new Label();
            listLog = new RichTextBox();
            label8 = new Label();
            labelModName = new Label();
            listBox1 = new ListBox();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gameList).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label1.ForeColor = Color.BurlyWood;
            label1.Location = new Point(12, 11);
            label1.Name = "label1";
            label1.Size = new Size(114, 19);
            label1.TabIndex = 0;
            label1.Text = "Hosting Now?";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label2.ForeColor = Color.BurlyWood;
            label2.Location = new Point(12, 71);
            label2.Name = "label2";
            label2.Size = new Size(142, 19);
            label2.TabIndex = 1;
            label2.Text = "Character Name:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label3.ForeColor = Color.BurlyWood;
            label3.Location = new Point(12, 101);
            label3.Name = "label3";
            label3.Size = new Size(133, 19);
            label3.TabIndex = 2;
            label3.Text = "Game Difficulty:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label4.ForeColor = Color.BurlyWood;
            label4.Location = new Point(12, 131);
            label4.Name = "label4";
            label4.Size = new Size(113, 19);
            label4.TabIndex = 3;
            label4.Text = "Player Count:";
            // 
            // labelHostStatus
            // 
            labelHostStatus.AutoSize = true;
            labelHostStatus.BackColor = Color.Black;
            labelHostStatus.Font = new Font("Formal436 BT", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelHostStatus.ForeColor = Color.Gainsboro;
            labelHostStatus.Location = new Point(160, 11);
            labelHostStatus.Name = "labelHostStatus";
            labelHostStatus.Size = new Size(0, 18);
            labelHostStatus.TabIndex = 4;
            // 
            // labelPName
            // 
            labelPName.AutoSize = true;
            labelPName.BackColor = Color.Black;
            labelPName.Font = new Font("Formal436 BT", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelPName.ForeColor = Color.Gainsboro;
            labelPName.Location = new Point(160, 71);
            labelPName.Name = "labelPName";
            labelPName.Size = new Size(0, 18);
            labelPName.TabIndex = 5;
            // 
            // labelDifficulty
            // 
            labelDifficulty.AutoSize = true;
            labelDifficulty.BackColor = Color.Black;
            labelDifficulty.Font = new Font("Formal436 BT", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelDifficulty.ForeColor = Color.Gainsboro;
            labelDifficulty.Location = new Point(160, 101);
            labelDifficulty.Name = "labelDifficulty";
            labelDifficulty.Size = new Size(0, 18);
            labelDifficulty.TabIndex = 6;
            // 
            // labelPCount
            // 
            labelPCount.AutoSize = true;
            labelPCount.BackColor = Color.Black;
            labelPCount.Font = new Font("Formal436 BT", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelPCount.ForeColor = Color.Gainsboro;
            labelPCount.Location = new Point(160, 131);
            labelPCount.Name = "labelPCount";
            labelPCount.Size = new Size(0, 18);
            labelPCount.TabIndex = 7;
            // 
            // labelTCPAddress
            // 
            labelTCPAddress.AutoSize = true;
            labelTCPAddress.BackColor = Color.Black;
            labelTCPAddress.Font = new Font("Formal436 BT", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelTCPAddress.ForeColor = Color.Gainsboro;
            labelTCPAddress.Location = new Point(160, 41);
            labelTCPAddress.Name = "labelTCPAddress";
            labelTCPAddress.Size = new Size(0, 18);
            labelTCPAddress.TabIndex = 9;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label10.ForeColor = Color.BurlyWood;
            label10.Location = new Point(12, 41);
            label10.Name = "label10";
            label10.Size = new Size(95, 19);
            label10.TabIndex = 8;
            label10.Text = "IP Address:";
            // 
            // labelLCount
            // 
            labelLCount.AutoSize = true;
            labelLCount.BackColor = Color.Black;
            labelLCount.Font = new Font("Formal436 BT", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelLCount.ForeColor = Color.Gainsboro;
            labelLCount.Location = new Point(160, 161);
            labelLCount.Name = "labelLCount";
            labelLCount.Size = new Size(0, 18);
            labelLCount.TabIndex = 12;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label6.ForeColor = Color.BurlyWood;
            label6.Location = new Point(12, 161);
            label6.Name = "label6";
            label6.Size = new Size(101, 19);
            label6.TabIndex = 11;
            label6.Text = "Game Slots:";
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { openToolStripMenuItem, exitToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(104, 48);
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(103, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(103, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label7.ForeColor = Color.BurlyWood;
            label7.Location = new Point(12, 190);
            label7.Name = "label7";
            label7.Size = new Size(93, 19);
            label7.TabIndex = 13;
            label7.Text = "Play Mode:";
            // 
            // labelPMode
            // 
            labelPMode.AutoSize = true;
            labelPMode.BackColor = Color.Black;
            labelPMode.Font = new Font("Formal436 BT", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelPMode.ForeColor = Color.Gainsboro;
            labelPMode.Location = new Point(160, 191);
            labelPMode.Name = "labelPMode";
            labelPMode.Size = new Size(0, 18);
            labelPMode.TabIndex = 14;
            // 
            // gameList
            // 
            gameList.AllowUserToAddRows = false;
            gameList.AllowUserToDeleteRows = false;
            gameList.AllowUserToResizeColumns = false;
            gameList.AllowUserToResizeRows = false;
            gameList.BackgroundColor = Color.Black;
            gameList.BorderStyle = BorderStyle.None;
            gameList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = Color.DarkOrange;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            gameList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            gameList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.WhiteSmoke;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            gameList.DefaultCellStyle = dataGridViewCellStyle2;
            gameList.EnableHeadersVisualStyles = false;
            gameList.GridColor = Color.Black;
            gameList.Location = new Point(12, 281);
            gameList.Name = "gameList";
            gameList.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            gameList.RowTemplate.Height = 25;
            gameList.ScrollBars = ScrollBars.Vertical;
            gameList.Size = new Size(729, 253);
            gameList.TabIndex = 15;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label5.ForeColor = Color.CornflowerBlue;
            label5.Location = new Point(267, 255);
            label5.Name = "label5";
            label5.Size = new Size(175, 19);
            label5.TabIndex = 16;
            label5.Text = "-- Active Games List --";
            // 
            // listLog
            // 
            listLog.BackColor = Color.Black;
            listLog.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            listLog.Location = new Point(347, 16);
            listLog.Name = "listLog";
            listLog.Size = new Size(394, 164);
            listLog.TabIndex = 17;
            listLog.Text = "";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Formal436 BT", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label8.ForeColor = Color.BurlyWood;
            label8.Location = new Point(12, 220);
            label8.Name = "label8";
            label8.Size = new Size(98, 19);
            label8.TabIndex = 18;
            label8.Text = "Mod Name:";
            // 
            // labelModName
            // 
            labelModName.AutoSize = true;
            labelModName.BackColor = Color.Black;
            labelModName.Font = new Font("Formal436 BT", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            labelModName.ForeColor = Color.Gainsboro;
            labelModName.Location = new Point(160, 220);
            labelModName.Name = "labelModName";
            labelModName.Size = new Size(0, 18);
            labelModName.TabIndex = 19;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(799, 24);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(697, 499);
            listBox1.TabIndex = 20;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1523, 557);
            Controls.Add(listBox1);
            Controls.Add(labelModName);
            Controls.Add(label8);
            Controls.Add(listLog);
            Controls.Add(label5);
            Controls.Add(gameList);
            Controls.Add(labelPMode);
            Controls.Add(label7);
            Controls.Add(labelLCount);
            Controls.Add(label6);
            Controls.Add(labelTCPAddress);
            Controls.Add(label10);
            Controls.Add(labelPCount);
            Controls.Add(labelDifficulty);
            Controls.Add(labelPName);
            Controls.Add(labelHostStatus);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Beacon";
            FormClosing += Form1_FormClosing;
            Resize += Form1_Resize;
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gameList).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label labelHostStatus;
        private Label labelPName;
        private Label labelDifficulty;
        private Label labelPCount;
        private Label labelTCPAddress;
        private Label label10;
        private Label labelLCount;
        private Label label6;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private Label label7;
        private Label labelPMode;
        private DataGridView gameList;
        private Label label5;
        private RichTextBox listLog;
        private Label label8;
        private Label labelModName;
        private ListBox listBox1;
    }
}
