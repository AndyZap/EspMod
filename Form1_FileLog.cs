using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EspMod
{
    public partial class Form1 : Form
    {
        string ShotsFolder = "";
        string DataFolder = "";

        public Dictionary<string, DataStruct> Data = new Dictionary<string, DataStruct>();
        public class DataStruct
        {
            public string date_str = "";
            public DateTime date = DateTime.MinValue;
            public bool enabled = true;
            public int id = 0;
            public string name = "";
            public double bean_weight = 0;
            public double coffee_weight = 0;
            public string grind = "";
            public double shot_time = 0;
            public string notes = "";
            public string profile = "";
            public bool has_video = false;
            public double retained_volume = 0;

            public List<double> elapsed = new List<double>();
            public List<double> pressure = new List<double>();
            public List<double> weight = new List<double>();
            public List<double> flow = new List<double>();
            public List<double> flow_weight = new List<double>();
            public List<double> temperature_basket = new List<double>();
            public List<double> temperature_mix = new List<double>();
            public List<double> pressure_goal = new List<double>();
            public List<double> flow_goal = new List<double>();
            public List<double> temperature_goal = new List<double>();

            public DataStruct() { }

            public void WriteRecord(StringBuilder sb)
            {
                sb.AppendLine("clock " + date_str);
                sb.AppendLine("enabled " + (enabled ? "1" : "0"));
                sb.AppendLine("record_id " + id.ToString());
                sb.AppendLine("name " + name);
                sb.AppendLine("bean_weight " + bean_weight.ToString());
                sb.AppendLine("coffee_weight " + coffee_weight.ToString());
                sb.AppendLine("grind " + grind);
                sb.AppendLine("shot_time " + shot_time.ToString());
                sb.AppendLine("notes " + notes);
                sb.AppendLine("profile " + profile);

                sb.AppendLine(WriteList(elapsed, "elapsed", "0.0##"));
                sb.AppendLine(WriteList(pressure, "pressure", "0.0#"));
                sb.AppendLine(WriteList(weight, "weight", "0.0"));
                sb.AppendLine(WriteList(flow, "flow", "0.0#"));
                sb.AppendLine(WriteList(flow_weight, "flow_weight", "0.0#"));
                sb.AppendLine(WriteList(temperature_basket, "temperature_basket", "0.0"));
                sb.AppendLine(WriteList(temperature_mix, "temperature_mix", "0.0"));
                sb.AppendLine(WriteList(pressure_goal, "pressure_goal", "0.0"));
                sb.AppendLine(WriteList(flow_goal, "flow_goal", "0.0"));
                sb.AppendLine(WriteList(temperature_goal, "temperature_goal", "0.0"));
                sb.AppendLine();
            }

            public string WriteList(List<double> list, string key, string format)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(key + " {");
                foreach (var x in list)
                    sb.Append(x.ToString(format) + " ");

                sb.Append("}");

                return sb.ToString().Replace(" }", "}");
            }

            // These are functions to work with the old format --------------------------

            public DataStruct(string s) // init from the csv file line
            {
                var words = s.Replace("\"", "").Split(',');

                date_str = words[0];
                name = words[1].ToLower();
                bean_weight = Convert.ToDouble(words[2]);
                coffee_weight = Convert.ToDouble(words[3]);
                grind = words[4];
                shot_time = Convert.ToDouble(words[5]);
                notes = words[6].Trim();

                var vector_data = words[7].Split(';');
                foreach (var v in vector_data)
                {
                    if (String.IsNullOrEmpty(v.Trim()))
                        continue;

                    weight.Add(Convert.ToDouble(v));
                }

                if (words.Length > 8)
                {
                    vector_data = words[8].Split(';');
                    foreach (var v in vector_data)
                    {
                        if (String.IsNullOrEmpty(v.Trim()))
                            continue;

                        pressure.Add(Convert.ToDouble(v));
                    }
                }

                SetupIncreasingAndFlowArrays();

                SetupNoPreinfArrays();

                date = DateTime.Parse(date_str);
                date_str = date.ToString("yyyy MM dd ddd HH:mm");

                if (bean_weight < 0)
                    enabled = false;
            }
            public void SetupIncreasingAndFlowArrays()
            {
                List<double> tmp = new List<double>();
                int counter = 0;
                foreach (var d in weight)
                {
                    elapsed.Add(counter);
                    flow_weight.Add(0.0);
                    tmp.Add(0.0);
                    if (pressure.Count < weight.Count)
                        pressure.Add(0.0);

                    counter++;
                }

                if (weight.Count < 3)  // no processing, simply copy the data
                    return;

                // chop the possible bump in the weight graphs at the beginning - e.g. after tare
                if (weight.Count > 5)
                {
                    for (int i = 5; i >= 1; i--)
                    {
                        if (weight[i] < weight[i - 1])
                            weight[i - 1] = weight[i];
                    }
                }

                // chop the bump in the pressure graphs at the beginning
                if (pressure.Count > 5)
                {
                    for (int i = 5; i >= 1; i--)
                    {
                        if (pressure[i] < pressure[i - 1])
                            pressure[i - 1] = pressure[i];
                    }
                }

                for (int i = 1; i < weight.Count; i++)
                    tmp[i] = weight[i] - weight[i - 1];

                for (int i = 1; i < weight.Count - 1; i++)  // average over 3 sec
                    flow_weight[i] = (tmp[i] + tmp[i - 1] + tmp[i + 1]) / 3.0;

                flow_weight[weight.Count - 1] = flow_weight[weight.Count - 2];


            }
            public void SetupNoPreinfArrays()
            {
                // copy data
                foreach (var d in weight)
                    data_increasing_nopi.Add(d);

                foreach (var d in pressure)
                    pressure_nopi.Add(d);

                foreach (var d in flow_weight)
                    flow_nopi.Add(d);

                // find first point when the flow_weight is about 1 g/s
                for (int i = 0; i < flow_weight.Count; i++)
                {
                    if (flow_weight[i] > _GOOD_FLOW_VALUE)
                    {
                        good_flow_index = i;
                        break;
                    }
                }

                // now find the starting index to chop the pre-infusion/bloom part
                int tokeep_index = 0;
                for (int i = good_flow_index; i >= 0; i--)
                {
                    if (good_flow_index > _BLOOM_TIME_INDEX)
                    {
                        if (flow_weight[i] < _GOOD_FLOW_VALUE / 2) // Bloom algo - get the half-point
                        {
                            tokeep_index = i;
                            break;
                        }
                    }
                    else
                    {
                        if (flow_weight[i] < _GOOD_FLOW_SMALL_VALUE) // all the rest algo - check against small value
                        {
                            tokeep_index = i;
                            break;
                        }
                    }
                }
                if (good_flow_index > _BLOOM_TIME_INDEX)  // extra adjustment for Bloom algo
                    tokeep_index -= good_flow_index - tokeep_index;

                data_increasing_nopi.RemoveRange(0, tokeep_index);
                pressure_nopi.RemoveRange(0, tokeep_index);
                flow_nopi.RemoveRange(0, tokeep_index);
            }
            public List<double> getWeightPoints(List<int> points)
            {
                if (points.Count != 2)
                    throw new Exception("WeightPoints not set correctly");

                List<double> output = new List<double>(points.Count + 1);

                // make an array which extends after the last point
                List<double> values = new List<double>();
                foreach (var d in weight)
                    values.Add(d);

                while (values.Count < points[1] + 10)
                    values.Add(coffee_weight);


                // here are the 3 bars:
                output.Add(values[points[0]] / coffee_weight);

                output.Add(values[points[1]] / coffee_weight - output[0]);

                output.Add(1.0 - (output[1] + output[0]));

                return output;
            }

            // derived vars
            readonly double _GOOD_FLOW_VALUE = 1;
            readonly double _BLOOM_TIME_INDEX = 30;
            readonly double _GOOD_FLOW_SMALL_VALUE = 0.1;

            private int good_flow_index = 0;
            public List<double> data_increasing_nopi = new List<double>();
            public List<double> flow_nopi = new List<double>();
            public List<double> pressure_nopi = new List<double>();
            public int saved_plot_index = -1;  // -1 means this has not been saved in the data struct

            private double getFirstDropTime()
            {
                for (int i = 0; i < weight.Count; i++)
                {
                    if (weight[i] > 0.1)
                        return elapsed[i];
                }

                return 0.0;
            }

            private double flowTimeAdjustment(double f, double first_drop)
            {
                double flow_time = f;

                // remove 0 flow points
                for (int i = 1; i < elapsed.Count; i++)
                {
                    if (elapsed[i] < first_drop)
                        continue;

                    if (elapsed[i] > shot_time - 8.0) // skip last 8 sec
                        break;

                    if ((flow.Count != 0 && flow[i] < 0.2) || (flow_weight.Count != 0 && flow_weight[i] < 0.07))
                        flow_time -= elapsed[i] - elapsed[i - 1];
                }

                return flow_time;
            }

            public double getAverageWeightFlow()
            {
                var first_drop = getFirstDropTime();
                double flow_time = shot_time - first_drop;

                flow_time = flowTimeAdjustment(flow_time, first_drop);

                return coffee_weight / flow_time;
            }
            public double getPreinfTime()
            {
                var first_drop = getFirstDropTime();
                double flow_time = shot_time - first_drop;

                flow_time = flowTimeAdjustment(flow_time, first_drop);

                return shot_time - flow_time;
            }

            public double getRatio()
            {
                return coffee_weight / bean_weight;
            }

            public string getNiceDateStr(DateTime dt)
            {
                var dt1 = new DateTime(dt.Year, dt.Month, dt.Day, date.Hour, date.Minute, date.Second);

                TimeSpan ts = dt1 - date;
                string nice_d = "";
                if (ts.TotalDays < 1)
                    nice_d = "T0 " + date.ToString("HH:mm");
                else if (ts.TotalDays > 28)
                    nice_d = date.ToString("dd/MM/yy");
                else
                    nice_d += date.ToString("dd/MM") + " " + date.ToString("HH:mm");

                return nice_d;
            }

            public static int getMaxId(Dictionary<string, DataStruct> data)
            {
                int max_id = -1;

                foreach (var value in data.Values)
                    max_id = Math.Max(value.id, max_id);

                return max_id + 1;
            }

            public List<double> getTotalWaterVolume()
            {
                List<double> total_list = new List<double>();

                if (flow.Count == 0)
                    return total_list;

                double total = 0;
                total_list.Add(total);
                for (int i = 1; i < elapsed.Count; i++)
                {
                    total += flow[i] * (elapsed[i] - elapsed[i - 1]);  // method 1 (rectangular)
                    //total += 0.5 * (flow[i] + flow[i-1]) * (elapsed[i] - elapsed[i - 1]);  // method 2 (trapeziodal)

                    total_list.Add(total);
                }

                retained_volume = total_list[total_list.Count - 1] - weight[weight.Count - 1];

                return total_list;
            }
        }

        static List<double> ReadList(string line, string key, double min_limit = 0.0)
        {
            var str = line.Remove(0, key.Length);

            str = str.Replace("{", "").Replace("}", "").Trim();

            List<double> res = new List<double>();

            if (String.IsNullOrEmpty(str))
                return res;

            var words = str.Split(' ');
            foreach (var w in words)
            {
                var x = Convert.ToDouble(w.Trim());
                x = Math.Max(x, min_limit);
                res.Add(x);
            }

            return res;
        }
        static string ReadString(string line, string key)
        {
            var str = line.Remove(0, key.Length);
            return str.Replace("{", "").Replace("}", "").Trim();
        }
        static double ReadDouble(string line, string key)
        {
            return Convert.ToDouble(ReadString(line, key));
        }

        bool ImportShotFile(string fname)
        {
            DataStruct d = new DataStruct();

            var lines = File.ReadAllLines(fname);
            foreach (var s in lines)
            {
                var line = s.Trim().Replace("  ", " ").Replace("  ", " ").Replace("  ", " "); // trim and remove double spaces
                if (String.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("clock "))
                {
                    string clock_str = line.Replace("clock ", "").Trim();
                    d.date = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(clock_str)).LocalDateTime;
                    d.date_str = d.date.ToString("yyyy MM dd ddd HH:mm");

                }
                else if (line.StartsWith("espresso_elapsed {"))
                {
                    d.elapsed = ReadList(line, "espresso_elapsed ");
                }
                else if (line.StartsWith("espresso_pressure {"))
                {
                    d.pressure = ReadList(line, "espresso_pressure ");
                }
                else if (line.StartsWith("espresso_weight {"))
                {
                    d.weight = ReadList(line, "espresso_weight ");
                }
                else if (line.StartsWith("espresso_flow {"))
                {
                    d.flow = ReadList(line, "espresso_flow ");
                }
                else if (line.StartsWith("espresso_flow_weight {"))
                {
                    d.flow_weight = ReadList(line, "espresso_flow_weight ");
                }
                else if (line.StartsWith("espresso_temperature_basket {"))
                {
                    d.temperature_basket = ReadList(line, "espresso_temperature_basket ");
                }
                else if (line.StartsWith("espresso_temperature_mix {"))
                {
                    d.temperature_mix = ReadList(line, "espresso_temperature_mix ");
                }
                else if (line.StartsWith("espresso_pressure_goal {"))
                {
                    d.pressure_goal = ReadList(line, "espresso_pressure_goal ");
                }
                else if (line.StartsWith("espresso_flow_goal {"))
                {
                    d.flow_goal = ReadList(line, "espresso_flow_goal ");
                }
                else if (line.StartsWith("espresso_temperature_goal {"))
                {
                    d.temperature_goal = ReadList(line, "espresso_temperature_goal ");
                }
                else if (line.StartsWith("drink_weight "))
                {
                    d.coffee_weight = ReadDouble(line, "drink_weight ");
                }
                else if (line.StartsWith("dsv2_bean_weight "))
                {
                    d.bean_weight = ReadDouble(line, "dsv2_bean_weight ");
                }
                else if (line.StartsWith("grinder_setting {"))
                {
                    d.grind = ReadString(line, "grinder_setting ");
                }
                else if (line.StartsWith("bean_brand {"))
                {
                    d.name = ReadString(line, "bean_brand ");
                }
                else if (line.StartsWith("espresso_notes {"))
                {
                    d.notes = ReadString(line, "espresso_notes ");
                }
                else if (line.StartsWith("profile_title {"))
                {
                    d.profile = ReadString(line, "profile_title ");
                }
            }

            if (d.elapsed.Count == 0)
                return false;

            // trim data at the end of the shot if the weight does not change. Do not do for steam!
            int last = d.weight.Count - 1;
            while (d.name != "steam" && d.weight[last] == d.weight[last - 1])
            {
                d.weight.RemoveAt(last);

                while (d.elapsed.Count != d.weight.Count)
                {
                    d.elapsed.RemoveAt(last);
                    d.pressure.RemoveAt(last);
                    d.flow.RemoveAt(last);
                    d.flow_weight.RemoveAt(last);
                    d.temperature_basket.RemoveAt(last);
                    d.temperature_mix.RemoveAt(last);
                    d.pressure_goal.RemoveAt(last);
                    d.flow_goal.RemoveAt(last);
                    d.temperature_goal.RemoveAt(last);
                }

                last = d.weight.Count - 1;
            }

            // setup the fields which are not saved in the file
            d.shot_time = d.elapsed[d.elapsed.Count - 1];
            d.id = DataStruct.getMaxId(Data);

            if (d.name != "steam" && (d.weight[d.weight.Count - 1] == 0.0 || d.bean_weight == 0.0))
                d.enabled = false;

            // finally add to the master list
            Data.Add(d.date_str, d);

            return true;
        }
        string ReadDateFromShotFile(string fname)
        {
            var lines = File.ReadAllLines(fname);
            foreach (var s in lines)
            {
                var line = s.Trim();
                if (String.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("clock "))
                {
                    string clock_str = line.Replace("clock ", "").Trim();
                    DateTime dt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(clock_str)).LocalDateTime;

                    return dt.ToString("yyyy MM dd ddd HH:mm");
                }
            }

            return "";
        }
        private DataStruct ReadRecord(List<string> lines)
        {
            DataStruct d = new DataStruct();

            try
            {
                foreach (var line in lines)
                {
                    if (line.StartsWith("clock "))
                    {
                        d.date_str = line.Replace("clock ", "").Trim();
                        d.date = DateTime.Parse(d.date_str);
                    }
                    else if (line.StartsWith("enabled "))
                    {
                        d.enabled = 1 == (int)ReadDouble(line, "enabled ");
                    }
                    else if (line.StartsWith("record_id "))
                    {
                        d.id = (int)ReadDouble(line, "record_id ");
                    }
                    else if (line.StartsWith("name "))
                    {
                        d.name = ReadString(line, "name ");
                    }
                    else if (line.StartsWith("bean_weight "))
                    {
                        d.bean_weight = ReadDouble(line, "bean_weight ");
                    }
                    else if (line.StartsWith("coffee_weight "))
                    {
                        d.coffee_weight = ReadDouble(line, "coffee_weight ");
                    }
                    else if (line.StartsWith("grind "))
                    {
                        d.grind = ReadString(line, "grind ");
                    }
                    else if (line.StartsWith("shot_time "))
                    {
                        d.shot_time = ReadDouble(line, "shot_time ");
                    }
                    else if (line.StartsWith("notes "))
                    {
                        d.notes = ReadString(line, "notes ");
                    }
                    else if (line.StartsWith("profile "))
                    {
                        d.profile = ReadString(line, "profile ");
                    }

                    else if (line.StartsWith("elapsed {"))
                    {
                        d.elapsed = ReadList(line, "elapsed {");
                    }
                    else if (line.StartsWith("pressure {"))
                    {
                        d.pressure = ReadList(line, "pressure {");
                    }
                    else if (line.StartsWith("weight {"))
                    {
                        d.weight = ReadList(line, "weight {");
                    }
                    else if (line.StartsWith("flow {"))
                    {
                        d.flow = ReadList(line, "flow {");
                    }
                    else if (line.StartsWith("flow_weight {"))
                    {
                        d.flow_weight = ReadList(line, "flow_weight {");
                    }
                    else if (line.StartsWith("temperature_basket {"))
                    {
                        d.temperature_basket = ReadList(line, "temperature_basket {");
                    }
                    else if (line.StartsWith("temperature_mix {"))
                    {
                        d.temperature_mix = ReadList(line, "temperature_mix {");
                    }
                    else if (line.StartsWith("pressure_goal {"))
                    {
                        d.pressure_goal = ReadList(line, "pressure_goal {");
                    }
                    else if (line.StartsWith("flow_goal {"))
                    {
                        d.flow_goal = ReadList(line, "flow_goal {");
                    }
                    else if (line.StartsWith("temperature_goal {"))
                    {
                        d.temperature_goal = ReadList(line, "temperature_goal {");
                    }
                    else
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            // fixes for the steam record ------------------------------------
            if (d.name == "steam")
            {
                int target_length = d.elapsed.Count;
                for (int i = 0; i < d.elapsed.Count; i++)
                {
                    if (d.flow[i] < 0.3 && d.pressure[i] < 0.5)
                    {
                        target_length = i;
                        break;
                    }
                }

                while (d.elapsed.Count > target_length)
                {
                    var last = d.elapsed.Count - 1;
                    d.weight.RemoveAt(last);
                    d.elapsed.RemoveAt(last);
                    d.pressure.RemoveAt(last);
                    d.flow.RemoveAt(last);
                    d.flow_weight.RemoveAt(last);
                    d.temperature_basket.RemoveAt(last);
                    d.temperature_mix.RemoveAt(last);
                    d.pressure_goal.RemoveAt(last);
                    d.flow_goal.RemoveAt(last);
                    d.temperature_goal.RemoveAt(last);
                }

                for (int i = 0; i < d.elapsed.Count; i++)
                {
                    d.weight[i] = 0.0;
                    d.flow_weight[i] = 0.0;
                    d.pressure_goal[i] = 0.0;
                    d.flow_goal[i] = 0.0;
                    d.temperature_goal[i] = 0.0;
                }

                d.bean_weight = 1;
                d.coffee_weight = 1;
                d.grind = "";
                d.shot_time = 1;
                d.notes = "";
                d.profile = "";
                d.has_video = false;
                d.retained_volume = 1;
            }


            return d;
        }
        private void ReadAllRecords(string fname, string video_folder = "")
        {
            List<string> record_lines = new List<string>();

            var lines = File.ReadAllLines(fname);
            var counter = 0;
            foreach (var s in lines)
            {
                counter++;
                var line = s.TrimStart();
                if (String.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("clock "))
                {
                    if (record_lines.Count != 0)
                    {
                        DataStruct d = ReadRecord(record_lines);
                        if (d == null)
                        {
                            MessageBox.Show("ERROR reading DE1LogView.csv file, see record which ends at line " + (counter - 1).ToString());
                            return;
                        }

                        // flag if the video exists
                        string video_file = video_folder + "\\" + d.id.ToString() + "-1.m4v";
                        d.has_video = File.Exists(video_file);

                        Data.Add(d.date_str, d);
                    }
                    record_lines.Clear();
                }

                record_lines.Add(line);
            }

            if (record_lines.Count != 0)
            {
                DataStruct d = ReadRecord(record_lines);

                if (d == null)
                {
                    MessageBox.Show("ERROR reading DE1LogView.csv file, see record which ends at line " + (counter - 1).ToString());
                    return;
                }

                // flag if the video exists
                string video_file = video_folder + "\\" + d.id.ToString() + "-1.m4v";
                d.has_video = File.Exists(video_file);

                Data.Add(d.date_str, d);
            }

            FilterData();
        }

    }
}
