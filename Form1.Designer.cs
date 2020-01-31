namespace EspMod
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.labelTopR = new System.Windows.Forms.Label();
            this.labelTopL = new System.Windows.Forms.Label();
            this.labelBotR = new System.Windows.Forms.Label();
            this.labelBotL = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.txtCopy = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.checkShowNotes = new System.Windows.Forms.CheckBox();
            this.comboSortStyle = new System.Windows.Forms.ComboBox();
            this.comboNumItemsToShow = new System.Windows.Forms.ComboBox();
            this.txtFilterProfile = new System.Windows.Forms.TextBox();
            this.btnSaveNotes = new System.Windows.Forms.Button();
            this.txtFilterName = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.listData = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labDate = new System.Windows.Forms.Label();
            this.labVideo = new System.Windows.Forms.Label();
            this.labBeanWeight = new System.Windows.Forms.Label();
            this.labDaysSinceRoast = new System.Windows.Forms.Label();
            this.labKpi = new System.Windows.Forms.Label();
            this.labAvFlow = new System.Windows.Forms.Label();
            this.labPI = new System.Windows.Forms.Label();
            this.labRatio = new System.Windows.Forms.Label();
            this.labGrind = new System.Windows.Forms.Label();
            this.labProfile = new System.Windows.Forms.Label();
            this.labName = new System.Windows.Forms.Label();
            this.labHasPlot = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnMenu = new System.Windows.Forms.Button();
            this.btnImportData = new System.Windows.Forms.Button();
            this.btnRefPlot = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectImageToDigitiseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.digitiserModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel4);
            this.splitContainer1.Panel2.Controls.Add(this.panel3);
            this.splitContainer1.Panel2.Controls.Add(this.panel5);
            this.splitContainer1.Panel2.Controls.Add(this.listData);
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Resize += new System.EventHandler(this.splitContainer1_Panel2_Resize);
            this.splitContainer1.Size = new System.Drawing.Size(1109, 739);
            this.splitContainer1.SplitterDistance = 166;
            this.splitContainer1.SplitterWidth = 7;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.labelTopR);
            this.splitContainer2.Panel1.Controls.Add(this.labelTopL);
            this.splitContainer2.Panel1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.splitContainer2.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel1_Paint);
            this.splitContainer2.Panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitContainer2_Panel1_MouseMove);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.pictureBox1);
            this.splitContainer2.Panel2.Controls.Add(this.labelBotR);
            this.splitContainer2.Panel2.Controls.Add(this.labelBotL);
            this.splitContainer2.Panel2.Cursor = System.Windows.Forms.Cursors.Cross;
            this.splitContainer2.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel2_Paint);
            this.splitContainer2.Panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitContainer2_Panel2_MouseMove);
            this.splitContainer2.Size = new System.Drawing.Size(166, 739);
            this.splitContainer2.SplitterDistance = 374;
            this.splitContainer2.TabIndex = 0;
            // 
            // labelTopR
            // 
            this.labelTopR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTopR.BackColor = System.Drawing.SystemColors.Window;
            this.labelTopR.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTopR.Location = new System.Drawing.Point(89, 0);
            this.labelTopR.Name = "labelTopR";
            this.labelTopR.Size = new System.Drawing.Size(74, 18);
            this.labelTopR.TabIndex = 1;
            this.labelTopR.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelTopL
            // 
            this.labelTopL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTopL.BackColor = System.Drawing.SystemColors.Window;
            this.labelTopL.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTopL.Location = new System.Drawing.Point(0, 0);
            this.labelTopL.Name = "labelTopL";
            this.labelTopL.Size = new System.Drawing.Size(83, 18);
            this.labelTopL.TabIndex = 0;
            // 
            // labelBotR
            // 
            this.labelBotR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBotR.BackColor = System.Drawing.SystemColors.Window;
            this.labelBotR.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBotR.Location = new System.Drawing.Point(90, 0);
            this.labelBotR.Name = "labelBotR";
            this.labelBotR.Size = new System.Drawing.Size(74, 18);
            this.labelBotR.TabIndex = 3;
            this.labelBotR.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelBotL
            // 
            this.labelBotL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBotL.BackColor = System.Drawing.SystemColors.Window;
            this.labelBotL.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBotL.Location = new System.Drawing.Point(1, 0);
            this.labelBotL.Name = "labelBotL";
            this.labelBotL.Size = new System.Drawing.Size(82, 18);
            this.labelBotL.TabIndex = 2;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.txtCopy);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 695);
            this.panel4.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(936, 44);
            this.panel4.TabIndex = 6;
            // 
            // txtCopy
            // 
            this.txtCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCopy.Location = new System.Drawing.Point(894, 11);
            this.txtCopy.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.txtCopy.Name = "txtCopy";
            this.txtCopy.Size = new System.Drawing.Size(43, 26);
            this.txtCopy.TabIndex = 38;
            this.txtCopy.Visible = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.checkShowNotes);
            this.panel3.Controls.Add(this.comboSortStyle);
            this.panel3.Controls.Add(this.comboNumItemsToShow);
            this.panel3.Controls.Add(this.txtFilterProfile);
            this.panel3.Controls.Add(this.btnSaveNotes);
            this.panel3.Controls.Add(this.txtFilterName);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 625);
            this.panel3.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(936, 44);
            this.panel3.TabIndex = 5;
            // 
            // checkShowNotes
            // 
            this.checkShowNotes.AutoSize = true;
            this.checkShowNotes.Location = new System.Drawing.Point(479, 10);
            this.checkShowNotes.Name = "checkShowNotes";
            this.checkShowNotes.Size = new System.Drawing.Size(64, 22);
            this.checkShowNotes.TabIndex = 43;
            this.checkShowNotes.Text = "Notes";
            this.checkShowNotes.UseVisualStyleBackColor = true;
            this.checkShowNotes.CheckedChanged += new System.EventHandler(this.checkShowNotes_CheckedChanged);
            // 
            // comboSortStyle
            // 
            this.comboSortStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSortStyle.FormattingEnabled = true;
            this.comboSortStyle.Items.AddRange(new object[] {
            "Sort by ID",
            "Sort Smart"});
            this.comboSortStyle.Location = new System.Drawing.Point(362, 8);
            this.comboSortStyle.Name = "comboSortStyle";
            this.comboSortStyle.Size = new System.Drawing.Size(113, 26);
            this.comboSortStyle.TabIndex = 42;
            this.comboSortStyle.SelectedIndexChanged += new System.EventHandler(this.comboSortStyle_SelectedIndexChanged);
            // 
            // comboNumItemsToShow
            // 
            this.comboNumItemsToShow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboNumItemsToShow.FormattingEnabled = true;
            this.comboNumItemsToShow.Items.AddRange(new object[] {
            "Show last 31 days",
            "Show last 90 days",
            "Show all"});
            this.comboNumItemsToShow.Location = new System.Drawing.Point(214, 8);
            this.comboNumItemsToShow.Name = "comboNumItemsToShow";
            this.comboNumItemsToShow.Size = new System.Drawing.Size(146, 26);
            this.comboNumItemsToShow.TabIndex = 41;
            this.comboNumItemsToShow.SelectedIndexChanged += new System.EventHandler(this.comboNumItemsToShow_SelectedIndexChanged);
            // 
            // txtFilterProfile
            // 
            this.txtFilterProfile.Location = new System.Drawing.Point(87, 8);
            this.txtFilterProfile.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.txtFilterProfile.Name = "txtFilterProfile";
            this.txtFilterProfile.Size = new System.Drawing.Size(124, 26);
            this.txtFilterProfile.TabIndex = 40;
            this.txtFilterProfile.TextChanged += new System.EventHandler(this.txtFilterProfile_TextChanged);
            this.txtFilterProfile.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFilterProfile_KeyDown);
            // 
            // btnSaveNotes
            // 
            this.btnSaveNotes.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSaveNotes.Location = new System.Drawing.Point(848, 0);
            this.btnSaveNotes.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnSaveNotes.Name = "btnSaveNotes";
            this.btnSaveNotes.Size = new System.Drawing.Size(88, 44);
            this.btnSaveNotes.TabIndex = 39;
            this.btnSaveNotes.TabStop = false;
            this.btnSaveNotes.Text = "Save notes";
            this.btnSaveNotes.UseVisualStyleBackColor = true;
            this.btnSaveNotes.Click += new System.EventHandler(this.btnSaveNotes_Click);
            // 
            // txtFilterName
            // 
            this.txtFilterName.Location = new System.Drawing.Point(13, 8);
            this.txtFilterName.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.txtFilterName.Name = "txtFilterName";
            this.txtFilterName.Size = new System.Drawing.Size(71, 26);
            this.txtFilterName.TabIndex = 33;
            this.txtFilterName.TextChanged += new System.EventHandler(this.txtFilterName_TextChanged);
            this.txtFilterName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFilterName_KeyDown);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.txtNotes);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 565);
            this.panel5.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(936, 60);
            this.panel5.TabIndex = 4;
            // 
            // txtNotes
            // 
            this.txtNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNotes.Location = new System.Drawing.Point(0, 0);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new System.Drawing.Size(936, 60);
            this.txtNotes.TabIndex = 0;
            this.txtNotes.Text = "Han 18.6 -> 43.0 in 63 sec, ratio 2.31 grind 3.50 ";
            // 
            // listData
            // 
            this.listData.Dock = System.Windows.Forms.DockStyle.Top;
            this.listData.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listData.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listData.FormattingEnabled = true;
            this.listData.IntegralHeight = false;
            this.listData.ItemHeight = 24;
            this.listData.Items.AddRange(new object[] {
            "x",
            "y"});
            this.listData.Location = new System.Drawing.Point(0, 87);
            this.listData.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.listData.Name = "listData";
            this.listData.Size = new System.Drawing.Size(936, 478);
            this.listData.TabIndex = 3;
            this.listData.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listData_DrawItem);
            this.listData.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.listData_MeasureItem);
            this.listData.SelectedIndexChanged += new System.EventHandler(this.listData_SelectedIndexChanged);
            this.listData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listData_MouseDown);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.labDate);
            this.panel2.Controls.Add(this.labVideo);
            this.panel2.Controls.Add(this.labBeanWeight);
            this.panel2.Controls.Add(this.labDaysSinceRoast);
            this.panel2.Controls.Add(this.labKpi);
            this.panel2.Controls.Add(this.labAvFlow);
            this.panel2.Controls.Add(this.labPI);
            this.panel2.Controls.Add(this.labRatio);
            this.panel2.Controls.Add(this.labGrind);
            this.panel2.Controls.Add(this.labProfile);
            this.panel2.Controls.Add(this.labName);
            this.panel2.Controls.Add(this.labHasPlot);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 53);
            this.panel2.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(936, 34);
            this.panel2.TabIndex = 2;
            // 
            // labDate
            // 
            this.labDate.Dock = System.Windows.Forms.DockStyle.Left;
            this.labDate.Location = new System.Drawing.Point(578, 0);
            this.labDate.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labDate.Name = "labDate";
            this.labDate.Size = new System.Drawing.Size(99, 34);
            this.labDate.TabIndex = 0;
            this.labDate.Text = "Date";
            this.labDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labVideo
            // 
            this.labVideo.Dock = System.Windows.Forms.DockStyle.Left;
            this.labVideo.Location = new System.Drawing.Point(559, 0);
            this.labVideo.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labVideo.Name = "labVideo";
            this.labVideo.Size = new System.Drawing.Size(19, 34);
            this.labVideo.TabIndex = 13;
            this.labVideo.Text = "V";
            this.labVideo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labBeanWeight
            // 
            this.labBeanWeight.Dock = System.Windows.Forms.DockStyle.Left;
            this.labBeanWeight.Location = new System.Drawing.Point(514, 0);
            this.labBeanWeight.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labBeanWeight.Name = "labBeanWeight";
            this.labBeanWeight.Size = new System.Drawing.Size(45, 34);
            this.labBeanWeight.TabIndex = 1;
            this.labBeanWeight.Text = "Bean";
            this.labBeanWeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labDaysSinceRoast
            // 
            this.labDaysSinceRoast.Dock = System.Windows.Forms.DockStyle.Left;
            this.labDaysSinceRoast.Location = new System.Drawing.Point(469, 0);
            this.labDaysSinceRoast.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labDaysSinceRoast.Name = "labDaysSinceRoast";
            this.labDaysSinceRoast.Size = new System.Drawing.Size(45, 34);
            this.labDaysSinceRoast.TabIndex = 12;
            this.labDaysSinceRoast.Text = "Age";
            this.labDaysSinceRoast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labKpi
            // 
            this.labKpi.Dock = System.Windows.Forms.DockStyle.Left;
            this.labKpi.Location = new System.Drawing.Point(423, 0);
            this.labKpi.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labKpi.Name = "labKpi";
            this.labKpi.Size = new System.Drawing.Size(46, 34);
            this.labKpi.TabIndex = 3;
            this.labKpi.Text = "KPI";
            this.labKpi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labAvFlow
            // 
            this.labAvFlow.Dock = System.Windows.Forms.DockStyle.Left;
            this.labAvFlow.Location = new System.Drawing.Point(371, 0);
            this.labAvFlow.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labAvFlow.Name = "labAvFlow";
            this.labAvFlow.Size = new System.Drawing.Size(52, 34);
            this.labAvFlow.TabIndex = 5;
            this.labAvFlow.Text = "AvFL";
            this.labAvFlow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labPI
            // 
            this.labPI.Dock = System.Windows.Forms.DockStyle.Left;
            this.labPI.Location = new System.Drawing.Point(334, 0);
            this.labPI.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labPI.Name = "labPI";
            this.labPI.Size = new System.Drawing.Size(37, 34);
            this.labPI.TabIndex = 11;
            this.labPI.Text = "Pi";
            this.labPI.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labRatio
            // 
            this.labRatio.Dock = System.Windows.Forms.DockStyle.Left;
            this.labRatio.Location = new System.Drawing.Point(278, 0);
            this.labRatio.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labRatio.Name = "labRatio";
            this.labRatio.Size = new System.Drawing.Size(56, 34);
            this.labRatio.TabIndex = 6;
            this.labRatio.Text = "Ratio";
            this.labRatio.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labGrind
            // 
            this.labGrind.Dock = System.Windows.Forms.DockStyle.Left;
            this.labGrind.Location = new System.Drawing.Point(228, 0);
            this.labGrind.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labGrind.Name = "labGrind";
            this.labGrind.Size = new System.Drawing.Size(50, 34);
            this.labGrind.TabIndex = 4;
            this.labGrind.Text = "Grind";
            this.labGrind.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labProfile
            // 
            this.labProfile.Dock = System.Windows.Forms.DockStyle.Left;
            this.labProfile.Location = new System.Drawing.Point(101, 0);
            this.labProfile.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labProfile.Name = "labProfile";
            this.labProfile.Size = new System.Drawing.Size(127, 34);
            this.labProfile.TabIndex = 7;
            this.labProfile.Text = "Profile";
            this.labProfile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labName
            // 
            this.labName.Dock = System.Windows.Forms.DockStyle.Left;
            this.labName.Location = new System.Drawing.Point(13, 0);
            this.labName.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(88, 34);
            this.labName.TabIndex = 2;
            this.labName.Text = "Name";
            this.labName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labHasPlot
            // 
            this.labHasPlot.Dock = System.Windows.Forms.DockStyle.Left;
            this.labHasPlot.Location = new System.Drawing.Point(0, 0);
            this.labHasPlot.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labHasPlot.Name = "labHasPlot";
            this.labHasPlot.Size = new System.Drawing.Size(13, 34);
            this.labHasPlot.TabIndex = 8;
            this.labHasPlot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnMenu);
            this.panel1.Controls.Add(this.btnImportData);
            this.panel1.Controls.Add(this.btnRefPlot);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(936, 53);
            this.panel1.TabIndex = 1;
            // 
            // btnMenu
            // 
            this.btnMenu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMenu.Location = new System.Drawing.Point(844, 0);
            this.btnMenu.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnMenu.Name = "btnMenu";
            this.btnMenu.Size = new System.Drawing.Size(88, 50);
            this.btnMenu.TabIndex = 15;
            this.btnMenu.TabStop = false;
            this.btnMenu.Text = ". . .";
            this.btnMenu.UseVisualStyleBackColor = true;
            this.btnMenu.Click += new System.EventHandler(this.BtnMenu_Click);
            // 
            // btnImportData
            // 
            this.btnImportData.Location = new System.Drawing.Point(5, 0);
            this.btnImportData.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnImportData.Name = "btnImportData";
            this.btnImportData.Size = new System.Drawing.Size(88, 50);
            this.btnImportData.TabIndex = 14;
            this.btnImportData.TabStop = false;
            this.btnImportData.Text = "Import";
            this.btnImportData.UseVisualStyleBackColor = true;
            this.btnImportData.Click += new System.EventHandler(this.btnImportData_Click);
            // 
            // btnRefPlot
            // 
            this.btnRefPlot.Location = new System.Drawing.Point(103, 0);
            this.btnRefPlot.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
            this.btnRefPlot.Name = "btnRefPlot";
            this.btnRefPlot.Size = new System.Drawing.Size(91, 50);
            this.btnRefPlot.TabIndex = 11;
            this.btnRefPlot.TabStop = false;
            this.btnRefPlot.Text = "Ref plot";
            this.btnRefPlot.UseVisualStyleBackColor = true;
            this.btnRefPlot.Click += new System.EventHandler(this.btnRefPlot_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.digitiserModeToolStripMenuItem,
            this.selectImageToDigitiseToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(197, 60);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(193, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(193, 6);
            // 
            // selectImageToDigitiseToolStripMenuItem
            // 
            this.selectImageToDigitiseToolStripMenuItem.Enabled = false;
            this.selectImageToDigitiseToolStripMenuItem.Name = "selectImageToDigitiseToolStripMenuItem";
            this.selectImageToDigitiseToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.selectImageToDigitiseToolStripMenuItem.Text = "Select image to digitise";
            this.selectImageToDigitiseToolStripMenuItem.Click += new System.EventHandler(this.selectImageToDigitiseToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(75, 176);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(76, 27);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
            // 
            // digitiserModeToolStripMenuItem
            // 
            this.digitiserModeToolStripMenuItem.CheckOnClick = true;
            this.digitiserModeToolStripMenuItem.Name = "digitiserModeToolStripMenuItem";
            this.digitiserModeToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.digitiserModeToolStripMenuItem.Text = "Digitiser mode";
            this.digitiserModeToolStripMenuItem.Click += new System.EventHandler(this.digitiserModeToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1109, 739);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "Form1";
            this.Text = "DE";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ListBox listData;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labAvFlow;
        private System.Windows.Forms.Label labGrind;
        private System.Windows.Forms.Label labKpi;
        private System.Windows.Forms.Label labBeanWeight;
        private System.Windows.Forms.Label labDate;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labRatio;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox txtFilterName;
        private System.Windows.Forms.Label labProfile;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label labHasPlot;
        private System.Windows.Forms.Button btnRefPlot;
        private System.Windows.Forms.Button btnImportData;
        private System.Windows.Forms.TextBox txtCopy;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label labPI;
        private System.Windows.Forms.Label labelTopL;
        private System.Windows.Forms.Label labelTopR;
        private System.Windows.Forms.Button btnSaveNotes;
        private System.Windows.Forms.Label labelBotR;
        private System.Windows.Forms.Label labelBotL;
        private System.Windows.Forms.Button btnMenu;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.TextBox txtNotes;
        private System.Windows.Forms.Label labDaysSinceRoast;
        private System.Windows.Forms.ComboBox comboSortStyle;
        private System.Windows.Forms.ComboBox comboNumItemsToShow;
        private System.Windows.Forms.TextBox txtFilterProfile;
        private System.Windows.Forms.CheckBox checkShowNotes;
        private System.Windows.Forms.Label labVideo;
        private System.Windows.Forms.ToolStripMenuItem selectImageToDigitiseToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem digitiserModeToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}
