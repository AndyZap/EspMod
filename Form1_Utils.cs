using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace EspMod
{
    public partial class Form1 : Form
    {
        // Save/Load settings -------------------------------------------------
        private bool LoadLineContainsKey(string line, string key)
        {
            return line.StartsWith(key);
        }
        private string LoadString(string line, string key)
        {
            if (!line.StartsWith(key)) { return ""; }
            return line.Remove(0, key.Length).Trim();
        }
        private int LoadInt(string line, string key)
        {
            if (!line.StartsWith(key)) { return 0; }
            string str = line.Remove(0, key.Length).Trim();

            int result = 0;
            try
            {
                result = Convert.ToInt32(str);
            }
            catch (Exception) { }
            return result;
        }
        private void LoadSettings()
        {
            string fname = ApplicationDirectory + "\\" + ApplicationNameNoExt + ".dat";
            if (File.Exists(fname))
            {
                string[] lines = File.ReadAllLines(fname);
                foreach (string s in lines)
                {
                    if (LoadLineContainsKey(s, "this.Top")) { this.Top = LoadInt(s, "this.Top"); }
                    else if (LoadLineContainsKey(s, "this.Left")) { this.Left = LoadInt(s, "this.Left"); }
                    else if (LoadLineContainsKey(s, "this.Height")) { this.Height = LoadInt(s, "this.Height"); }
                    else if (LoadLineContainsKey(s, "this.Width")) { this.Width = LoadInt(s, "this.Width"); }
                    else if (LoadLineContainsKey(s, "this.WindowState")) { this.WindowState = (FormWindowState)LoadInt(s, "this.WindowState"); }

                    else if (LoadLineContainsKey(s, "splitContainer1")) { splitContainer1.SplitterDistance = LoadInt(s, "splitContainer1"); }
                    else if (LoadLineContainsKey(s, "splitContainer2")) { splitContainer2.SplitterDistance = LoadInt(s, "splitContainer2"); }

                    else if (LoadLineContainsKey(s, "txtFilterName")) { txtFilterName.Text = LoadString(s, "txtFilterName"); }
                    else if (LoadLineContainsKey(s, "txtFilterProfile")) { txtFilterProfile.Text = LoadString(s, "txtFilterProfile"); }
                    else if (LoadLineContainsKey(s, "comboNumItemsToShow")) { comboNumItemsToShow.Text = LoadString(s, "comboNumItemsToShow"); }
                    else if (LoadLineContainsKey(s, "comboSortStyle")) { comboSortStyle.Text = LoadString(s, "comboSortStyle"); }
                    else if (LoadLineContainsKey(s, "checkShowNotes"))
                    {
                        var str = LoadString(s, "checkShowNotes");
                        checkShowNotes.Checked = str == "true";
                    }

                    else if (LoadLineContainsKey(s, "DataFolder")) { DataFolder = LoadString(s, "DataFolder"); }
                }
            }

            if(comboNumItemsToShow.SelectedIndex == -1)
                comboNumItemsToShow.SelectedIndex = 0;
            if (comboSortStyle.SelectedIndex == -1)
                comboSortStyle.SelectedIndex = 0;
        }
        private void SaveSettings()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("this.Top                 " + (this.Top < 0 ? "0" : this.Top.ToString()));
            sb.AppendLine("this.Left                " + (this.Left < 0 ? "0" : this.Left.ToString()));
            sb.AppendLine("this.Height              " + (this.Height < 200 ? "200" : this.Height.ToString()));
            sb.AppendLine("this.Width               " + (this.Width < 200 ? "200" : this.Width.ToString()));
            sb.AppendLine("this.WindowState         " + ((int)this.WindowState).ToString());

            sb.AppendLine("splitContainer1          " + splitContainer1.SplitterDistance.ToString());
            sb.AppendLine("splitContainer2          " + splitContainer2.SplitterDistance.ToString());

            sb.AppendLine("txtFilterName            " + txtFilterName.Text);
            sb.AppendLine("txtFilterProfile         " + txtFilterProfile.Text);
            sb.AppendLine("comboNumItemsToShow      " + comboNumItemsToShow.Text);
            sb.AppendLine("comboSortStyle           " + comboSortStyle.Text);
            sb.AppendLine("checkShowNotes           " + (checkShowNotes.Checked ? "true" : "false"));

            sb.AppendLine("DataFolder               " + DataFolder);

            string fname = ApplicationDirectory + "\\" + ApplicationNameNoExt + ".dat";
            File.WriteAllText(fname, sb.ToString());
        }
    }
}
