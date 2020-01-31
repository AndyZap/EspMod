using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace EspMod
{
    public partial class Form1 : Form
    {
        string Revision = "Espresso extraction model v1.1";
        string ApplicationDirectory = "";
        string ApplicationNameNoExt = "";

        GraphPainter GraphTop = null;
        GraphPainter GraphBot = null;

        public string MainPlotKey = "";
        public string RefPlotKey = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = Revision;
            
            GraphTop = new GraphPainter(splitContainer2.Panel1, this.Font);
            GraphBot = new GraphPainter(splitContainer2.Panel2, this.Font);

            ApplicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
            ApplicationNameNoExt = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);

            LoadSettings();

            //string data_fname = (Directory.Exists(DataFolder) ? DataFolder : ApplicationDirectory) + "\\" + ApplicationNameNoExt + ".csv";
            //    ReadAllRecords(data_fname, ApplicationDirectory);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void listData_SelectedIndexChanged(object sender, EventArgs e)
        {
            listData.Refresh();

            if (listData.SelectedIndex < 0 || listData.SelectedIndex >= listData.Items.Count)
                return;

            string key = (string)listData.Items[listData.SelectedIndex];

            if (!Data.ContainsKey(key))
            {
                MainPlotKey = "";
                return;
            }

            MainPlotKey = key;

            txtNotes.Text = String.IsNullOrEmpty(Data[key].notes) ? "" : Data[key].notes;

            PlotDataRec(GraphTop, Data[key]);
        }

        public void PlotDataRec(GraphPainter gp, DataStruct ds)
        {
            if (gp == GraphBot)
            {
                labelBotL.Text = "";
                labelBotR.Text = "";
            }
            else if (gp == GraphTop)
            {
                labelTopL.Text = "";
                labelTopR.Text = "";
            }

            gp.SetAxisTitles("", "");

            gp.data.Clear();

            gp.SetData(0, ds.elapsed, ds.flow_goal, Color.Blue, 2, DashStyle.Dash);
            gp.SetData(1, ds.elapsed, ds.pressure_goal, Color.LimeGreen, 2, DashStyle.Dash);

            gp.SetData(2, ds.elapsed, ds.flow, Color.Blue, 3, DashStyle.Solid);
            gp.SetData(3, ds.elapsed, ds.pressure, Color.LimeGreen, 3, DashStyle.Solid);

            gp.SetData(4, ds.elapsed, ds.flow_weight, Color.Brown, 3, DashStyle.Solid);

            List<double> temperature_scaled = new List<double>();
            List<double> temperature_target_scaled = new List<double>();
            foreach (var t in ds.temperature_basket)
                temperature_scaled.Add(t / 10.0);
            foreach (var t in ds.temperature_goal)
                temperature_target_scaled.Add(t / 10.0);

            gp.SetData(5, ds.elapsed, temperature_target_scaled, Color.Red, 2, DashStyle.Dash);
            gp.SetData(6, ds.elapsed, temperature_scaled, Color.Red, 3, DashStyle.Solid);

            var pi = ds.getPreinfTime();
            List<double> x_pi = new List<double>(); x_pi.Add(pi); x_pi.Add(pi);
            List<double> y_pi = new List<double>(); y_pi.Add(0); y_pi.Add(1);
            gp.SetData(7, x_pi, y_pi, Color.Brown, 2, DashStyle.Solid);

            gp.SetAutoLimits();

            gp.panel.Refresh();
        }

        private void listData_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= listData.Items.Count)
                return;

            string key = (string)listData.Items[e.Index];

            if (!Data.ContainsKey(key))
                return;

            DataStruct d = Data[key];

            if (checkShowNotes.Checked)
            {
                if (d.notes != "")
                    e.ItemHeight = (int)(e.ItemHeight * 1.7);
            }
        }

        private string TrimStringToDraw(string s, Graphics g, Font font, int width)
        {
            string out_str = s;

            var x = g.MeasureString(out_str, font).ToSize().Width;

            while (x >= width)
            {
                out_str = out_str.Remove(out_str.Length - 1);
                x = g.MeasureString(out_str, font).ToSize().Width;
            }

            return out_str;
        }

        private void listData_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= listData.Items.Count)
                return;

            string key = (string)listData.Items[e.Index];

            if (!Data.ContainsKey(key))
                return;

            DataStruct d = Data[key];

            Brush myBrush = Brushes.Black;
            if (e.Index % 2 == 0)
            {
                var brush = new SolidBrush(Color.FromArgb(255, 240, 240, 240));
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            else
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);

            Rectangle myrec = e.Bounds;

            // plot color bar.  blue is the current one, RefPlotKey is red
            myrec.X = labHasPlot.Left + 2; myrec.Width = labHasPlot.Width - 5;
            if (listData.GetSelected(e.Index))
                e.Graphics.FillRectangle(Brushes.Blue, myrec);
            else
                e.Graphics.FillRectangle(key == RefPlotKey ? Brushes.Red : Brushes.White, myrec);

            // Text. Move the text a bit down
            myrec.Y += 2;

            myrec.X = labName.Left; myrec.Width = labName.Width;
            e.Graphics.DrawString(d.name, e.Font, myBrush, myrec, StringFormat.GenericTypographic);

            myrec.X = labProfile.Left; myrec.Width = labProfile.Width;
            e.Graphics.DrawString(d.profile, e.Font, myBrush, myrec, StringFormat.GenericTypographic);

            myrec.X = labGrind.Left; myrec.Width = labGrind.Width;
            e.Graphics.DrawString(d.grind, e.Font, myBrush, myrec, StringFormat.GenericTypographic);

            myrec.X = labBeanWeight.Left; myrec.Width = labBeanWeight.Width;
            e.Graphics.DrawString(d.bean_weight.ToString("0.0"), e.Font, myBrush, myrec, StringFormat.GenericTypographic);


            myrec.X = labRatio.Left; myrec.Width = labRatio.Width;
            e.Graphics.DrawString(d.getRatio().ToString("0.0"), e.Font, myBrush, myrec, StringFormat.GenericTypographic);

            myrec.X = labAvFlow.Left; myrec.Width = labAvFlow.Width;
            e.Graphics.DrawString(d.getAverageWeightFlow().ToString("0.0"), e.Font, myBrush, myrec, StringFormat.GenericTypographic);

            myrec.X = labPI.Left; myrec.Width = labPI.Width;
            e.Graphics.DrawString(d.getPreinfTime().ToString("0"), e.Font, myBrush, myrec, StringFormat.GenericTypographic);

            myrec.X = labDate.Left; myrec.Width = labDate.Width;
            e.Graphics.DrawString(d.getNiceDateStr(DateTime.Now), e.Font, myBrush, myrec, StringFormat.GenericTypographic);

            myrec.X = labVideo.Left; myrec.Width = labVideo.Width;
            e.Graphics.DrawString(d.has_video ? "v": "", e.Font, myBrush, myrec, StringFormat.GenericTypographic);

            if (checkShowNotes.Checked)
            {
                if (d.notes != "") // notes, on a separate line
                {
                    myrec.X = labGrind.Left + 5; myrec.Width = e.Bounds.Width - labName.Left - 10;
                    myrec.Y += e.Bounds.Height / 2;

                    var notes_str = TrimStringToDraw(d.notes, e.Graphics, e.Font, myrec.Width);
                    if (notes_str.StartsWith("*"))
                        myrec.X -= 20;

                    Font font1 = new Font(e.Font.FontFamily, (float)(e.Font.Size * 0.7), FontStyle.Regular);
                    e.Graphics.DrawString(notes_str, font1, myBrush, myrec, StringFormat.GenericTypographic);
                }
            }
        }

        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {
            if (GraphTop != null)
                GraphTop.Plot(e.Graphics);
        }
        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {
            if (digitiserModeToolStripMenuItem.Checked)
            {
                if(BmpToDigitise != null)
                    e.Graphics.DrawImage(BmpToDigitise, 0, 0, splitContainer2.Panel2.Width, splitContainer2.Panel2.Height);
            }
            else
            {
                if (GraphBot != null)
                    GraphBot.Plot(e.Graphics);
            }
        }

        List<string> SmartOutputSort(List<string> input)
        {
            List<DataStruct> list = new List<DataStruct>();
            foreach (var i in input)
                list.Add(Data[i]);

            list.Sort(delegate (DataStruct a1, DataStruct a2)
            {
                if (a1.name != a2.name) { return a2.name.CompareTo(a1.name); }
                else if (a1.profile != a2.profile) { return a2.profile.CompareTo(a1.profile); }
                else if (a1.grind != a2.grind) { return a2.grind.CompareTo(a1.grind); }
                else if (a1.bean_weight != a2.bean_weight) { return a2.bean_weight.CompareTo(a1.bean_weight); }
                else
                {
                    if (Math.Abs(a1.getRatio() - a2.getRatio()) < 0.2) return 0;
                    else return a1.getRatio().CompareTo(a2.getRatio());
                }
            });

            List<string> output_list = new List<string>();
            foreach (var x in list)
                output_list.Add(x.date_str);

            return output_list;
        }

        private void FilterData()
        {
            List<string> sorted_keys = new List<string>();

            var flt_name = txtFilterName.Text.Trim().ToLower();
            var flt_profile = txtFilterProfile.Text.Trim().ToLower();

            int max_days = int.MaxValue;
            if (comboNumItemsToShow.Text == "Show last 31 days")
                max_days = 31;
            else if (comboNumItemsToShow.Text == "Show last 90 days")
                max_days = 90;

            foreach (var key in Data.Keys)
            {
                if (!String.IsNullOrEmpty(flt_name) && Data[key].name.ToLower().Contains(flt_name) == false)
                    continue;

                if (!String.IsNullOrEmpty(flt_profile) && Data[key].profile.ToLower().Contains(flt_profile) == false)
                    continue;

                if (!Data[key].enabled)
                    continue;

                if ((DateTime.Now - Data[key].date).TotalDays > max_days)
                    continue;

                if (Data[key].name == "steam") // always remove steam
                    continue;

                sorted_keys.Add(key);
            }

            if (comboSortStyle.Text == "Sort by ID")
                sorted_keys.Sort();
            else
                sorted_keys = SmartOutputSort(sorted_keys);

            string saved_key = "";
            if (listData.SelectedIndex != -1)
                saved_key = (string)listData.Items[listData.SelectedIndex];

            listData.Items.Clear();
            bool saved_key_set = false;
            for (int i = sorted_keys.Count - 1; i >= 0; i--)
            {
                listData.Items.Add(sorted_keys[i]);
                if (sorted_keys[i] == saved_key)
                {
                    listData.SelectedItem = sorted_keys[i];
                    saved_key_set = true;
                }
            }
            if (!saved_key_set && listData.Items.Count != 0)
                listData.SelectedIndex = 0;

            listData.Refresh();
        }

        private void splitContainer1_Panel2_Resize(object sender, EventArgs e)
        {
            listData.Height = splitContainer1.Panel2.Height - panel1.Height - panel2.Height - panel3.Height
            - panel4.Height - panel5.Height - 5;
        }

        private void btnRefPlot_Click(object sender, EventArgs e)
        {
            if (listData.SelectedIndex != -1)
                RefPlot(listData.SelectedIndex);
        }

        private void RefPlot(int index)
        {
            string key = (string)listData.Items[index];

            if (!Data.ContainsKey(key))
            {
                RefPlotKey = "";
                return;
            }

            if (RefPlotKey == key)
            {
                RefPlotKey = "";
                GraphBot.data.Clear();
                splitContainer2.Panel2.Refresh();
                labelBotL.Text = "";
                listData.Refresh();
                return;
            }

            RefPlotKey = key;

            PlotDataRec(GraphBot, Data[key]);

            listData.Focus();
            listData.Refresh();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void listData_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = listData.IndexFromPoint(e.Location);
                if (index != -1)
                    RefPlot(index);
            }
        }

        private void BtnMenu_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(btnMenu, new Point(0, btnMenu.Size.Height), ToolStripDropDownDirection.BelowLeft);
        }

        private void btnSaveNotes_Click(object sender, EventArgs e)
        {
            if (listData.SelectedIndex < 0 || listData.SelectedIndex >= listData.Items.Count)
                return;

            string key = (string)listData.Items[listData.SelectedIndex];

            if (!Data.ContainsKey(key))
                return;

            Data[key].notes = txtNotes.Text.Replace(",", " ");

            FilterData();
        }
        void btnImportData_Click(object sender, EventArgs e)  // this comes from button
        {
            if (!Directory.Exists(ShotsFolder))
            {
                MessageBox.Show("ERROR: ShotsFolder location is not set");
                return;
            }
            var old_count = Data.Count;

            var files = Directory.GetFiles(ShotsFolder, "*.shot", SearchOption.TopDirectoryOnly);
            foreach (var f in files)
            {
                if (Path.GetFileNameWithoutExtension(f) == "0") // skip 0.shot, this is a config file for DE1Win10
                    continue;

                var key = ReadDateFromShotFile(f);
                if (Data.ContainsKey(key))
                    continue;

                if (key == "")
                {
                    MessageBox.Show("ERROR: when reading date from shot file " + f);
                    return;
                }

                if (!ImportShotFile(f))
                {
                    MessageBox.Show("ERROR: when reading shot file " + f);
                    return;
                }
            }
            FilterData();

            MessageBox.Show("Loaded " + (Data.Count - old_count).ToString() + " shot files");
        }

        private void splitContainer2_Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            var x = GraphTop.ToDataX(e.X);
            var y = GraphTop.ToDataY(e.Y);

            labelTopR.Text = x.ToString("0.0") + ", " + y.ToString("0.0");
        }

        private void splitContainer2_Panel2_MouseMove(object sender, MouseEventArgs e)
        {
            var x = GraphBot.ToDataX(e.X);
            var y = GraphBot.ToDataY(e.Y);

            labelBotR.Text = x.ToString("0.0") + ", " + y.ToString("0.0");
        }

        private void txtFilterName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 27)
            {
                if (txtFilterName.Text == "")
                    txtFilterProfile.Text = "";
                else
                    txtFilterName.Text = "";
            }
        }
        private void txtFilterProfile_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 27)
            {
                if (txtFilterProfile.Text == "")
                    txtFilterName.Text = "";
                else
                    txtFilterProfile.Text = "";
            }
        }
        private void txtFilterName_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }
        private void txtFilterProfile_TextChanged(object sender, EventArgs e)
        {
            FilterData();
        }
        private void comboNumItemsToShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }
        private void comboSortStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterData();
        }
        private void checkShowNotes_CheckedChanged(object sender, EventArgs e)
        {
            FilterData();
        }

        private void digitiserModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (digitiserModeToolStripMenuItem.Checked)
            {
                selectImageToDigitiseToolStripMenuItem.Enabled = true;
                BmpToDigitise = null;
            }
            else
            {
                selectImageToDigitiseToolStripMenuItem.Enabled = false;
                BmpToDigitise = null;
            }

            splitContainer2.Panel2.Refresh();
        }

        Bitmap BmpToDigitise = null;

        private void selectImageToDigitiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            BmpToDigitise = new Bitmap(openFileDialog1.FileName);

            splitContainer2.Panel2.Refresh();
        }
    }
}