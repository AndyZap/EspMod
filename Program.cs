using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspMod
{
    class Program
    {
        static string Revision = "Espresso extraction model v1.2";

        public class Cell
        {
            //public int num;

            public Cell()
            {

            }

        }

        public class Grain
        {
            //public int num;
            public List<Cell> cells = new List<Cell>();


            public Grain()
            {

            }

        }

        public class Layer
        {
            //public int num;
            public List<Grain> cells = new List<Grain>();


            public Layer()
            {

            }

        }

        public class Puck
        {
            public bool has_errors = false;

            Log             log;

            //public int num;
            List<Layer> layers = new List<Layer>();

            double volume_out = 0.0; // total volume and coffee mass in the cup
            double coffee_out = 0.0;


            public Puck(Config config, Log log_)
            {
                log = log_;

                // reset total volume and coffee mass in the cup
                volume_out = 0.0; 
                coffee_out = 0.0;




            }

            public void Simulate()
            { 

            }
        }

        public class Config
        {
            public bool has_errors = false;
            Dictionary<string, string> settings = new Dictionary<string, string>();
            Log log;

            public Config(string fname, Log log_)
            {
                log = log_;

                if (!LoadFile(fname))
                {
                    has_errors = true;
                    return;
                }

                var shot_file_name = GetString("shot_file_name");
                if(shot_file_name != "")
                {
                    if (!LoadFile(shot_file_name))
                    {
                        has_errors = true;
                        return;
                    }
                }
            }


            public bool LoadFile(string fname)
            {
                if (!File.Exists(fname))
                {
                    log.Write("ERROR: cannot find the input file " + fname);
                    return false;
                }
                log.Write("Loading data from input file " + fname);


                var lines = File.ReadAllLines(fname);
                foreach(string s in lines)
                {
                    string line = s.Trim().Replace("\t", " ");  // replace tabs with spaces and trim

                    if (line.StartsWith("#") || String.IsNullOrEmpty(line))
                        continue;

                    var words = line.Split(' ');
                    var key = words[0];
                    var value = line.Remove(0, key.Length).Trim();

                    settings[key] = value;
                }

                return true;
            }

            public bool HasKey(string key)
            {
                if(!settings.ContainsKey(key))
                {
                    Console.WriteLine("ERROR: cannot find settings entry " + key);
                    return false;
                }

                return true;
            }

            public string GetString(string key)
            {
                if (!HasKey(key))
                    return "";

                return settings[key];
            }

            public double GetDouble(string key)
            {
                if (!HasKey(key))
                    return double.NaN;

                try
                {
                    return Convert.ToDouble(settings[key]);
                }
                catch (Exception)
                {
                    return double.NaN;
                }
            }

        }

        public class Log
        {
            string log_name;

            public Log(string name, string init_string)
            {
                log_name = name;
                File.WriteAllText(log_name, init_string + "\r\n");
            }

            public void Write(string s)
            {
                File.AppendAllText(log_name, s + "\r\n");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(Revision);

            if(args.Length < 1)
            {
                Console.WriteLine("ERROR: please supply the config file name to process");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("ERROR: cannot find the input file " + args[0]);
                return;
            }

            string log_file_name = Path.GetFileNameWithoutExtension(args[0]) + ".log";
            Log log = new Log(log_file_name, Revision);

            Config config = new Config(args[0], log);
            if(config.has_errors)
                return;

            Puck puck = new Puck(config, log);
            if (puck.has_errors)
                return;

            puck.Simulate();

            Console.WriteLine("Finished");
        }
    }
}
