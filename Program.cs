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
        static string Revision = "Espresso extraction model v1.3";

        public class Cell
        {
            //public int num;

            public Cell()
            {

            }

        }

        public class Grain
        {
            public List<Cell> cells = new List<Cell>();

            public double total_volume_of_these_grains_mm3;
            public double total_soluble_mass_in_these_grains_g;
            //public int    num_of_these_grains_mm3;

            public Grain(int level)
            {
                for(int i = 0; i <= level; i++)
                {
                    cells.Add(new Cell());
                }
            }

        }

        public class Layer
        {
            //public int num;
            public Dictionary<int, Grain> grains = new Dictionary<int, Grain>();

            public double layer_volume_mm3;
            public double grounds_volume_mm3;
            public double void_volume_mm3;

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
            double mass_out = 0.0;

            double modelling_time_step_sec              = 1;
            int    num_modelling_layers_in_puck         = 10;

            double puck_diameter_mm                     = 58.5;
            double grounds_density_kg_m3                = 330;

            double soluble_coffee_mass_fraction         = 0.3;
            double coffee_cell_diameter_mm              = 0.03;

            double dsv2_bean_weight                     = 18.0;

            public Puck(Config config, Log log_)
            {
                log = log_;
                log.Write("");

                // reset total volume and coffee mass in the cup
                volume_out = 0.0;
                mass_out = 0.0;

                modelling_time_step_sec = config.GetDouble("modelling_time_step_sec"); if (!Check(modelling_time_step_sec)) return;
                num_modelling_layers_in_puck = config.GetInt("num_modelling_layers_in_puck"); if (!Check(num_modelling_layers_in_puck)) return;

                puck_diameter_mm = config.GetDouble("puck_diameter_mm"); if (!Check(puck_diameter_mm)) return;
                grounds_density_kg_m3 = config.GetDouble("grounds_density_kg_m3"); if (!Check(grounds_density_kg_m3)) return;

                soluble_coffee_mass_fraction = config.GetDouble("soluble_coffee_mass_fraction"); if (!Check(soluble_coffee_mass_fraction)) return;
                coffee_cell_diameter_mm = config.GetDouble("coffee_cell_diameter_mm"); if (!Check(coffee_cell_diameter_mm)) return;

                dsv2_bean_weight = config.GetDouble("dsv2_bean_weight"); if (!Check(dsv2_bean_weight)) return;

                // load psd
                Dictionary<int, double> psd = new Dictionary<int, double>();
                double total = 0.0;
                for (int i = 0; i < 100; i++) // get the max entry index for the PSD
                {
                    if (config.HasKey("particle_size_distributution_" + i.ToString(), print_error: false))
                    {
                        var psd_value = config.GetDouble("particle_size_distributution_" + i.ToString()); if (!Check(psd_value)) return;

                        psd[i] = psd_value;
                        total += psd_value;
                    }
                }

                if(Math.Abs(total-1) > 1E-3)
                {
                    log.Write("ERROR: sum of the PSD values had to be 1");
                    return;
                }

                double puck_area_mm2 = Math.PI * puck_diameter_mm * puck_diameter_mm * 0.25;
                double grounds_density_g_mm3 = grounds_density_kg_m3 * 1E3 * 1E-9;
                double puck_volume_mm3 = dsv2_bean_weight / grounds_density_g_mm3;
                double puck_height_mm = puck_volume_mm3 / puck_area_mm2;
                log.Write("puck_height_mm " + puck_height_mm.ToString());


                double grounds_volume_fraction = config.GetDouble("grounds_volume_fraction"); if (!Check(grounds_volume_fraction)) return;
                double void_in_grounds_volume_fraction = config.GetDouble("void_in_grounds_volume_fraction"); if (!Check(void_in_grounds_volume_fraction)) return;

                double soluble_mass_per_layer = dsv2_bean_weight * soluble_coffee_mass_fraction / num_modelling_layers_in_puck;

                // setting up layers
                for (int i = 0; i < num_modelling_layers_in_puck; i++)
                {
                    Layer layer = new Layer
                    {
                        layer_volume_mm3 = puck_volume_mm3 / num_modelling_layers_in_puck
                    };

                    layer.grounds_volume_mm3 = layer.layer_volume_mm3 * grounds_volume_fraction;
                    layer.void_volume_mm3 = layer.layer_volume_mm3 * (1.0 - grounds_volume_fraction);

                    foreach(int key in psd.Keys)
                    {
                        Grain g = new Grain(key);
                        g.total_volume_of_these_grains_mm3 = layer.grounds_volume_mm3 * psd[key];
                        g.total_soluble_mass_in_these_grains_g = soluble_mass_per_layer * psd[key];

                        /* check!
                        var initial_concentration = g.total_soluble_mass_in_these_grains_g / 
                            (g.total_volume_of_these_grains_mm3 * void_in_grounds_volume_fraction);

                        log.Write("initial_concentration inside grain " + initial_concentration.ToString());

                        initial_concentration 0.000185839830844456
                        good: 185 kg/m3

                        if (key == 0)
                        {
                            var initial_concentration_in_void = g.total_soluble_mass_in_these_grains_g / layer.void_volume_mm3;
                            log.Write("initial_concentration between grains " + initial_concentration_in_void.ToString());
                        }

                        initial_concentration between grains 8.59375E-05
                        0.08E-3 g per 1E-3 g (this is 1 mm3 weight) => 8% TDS in the void. Looks correct.
                        */


                        layer.grains[key] = g;
                    }

                    layers.Add(layer);
                }
             }                

            bool Check(double x)
            {
                if(x == double.MinValue)
                {
                    has_errors = true;
                    return false;
                }
                return true;
            }
            bool Check(int x)
            {
                if (x == int.MinValue)
                {
                    has_errors = true;
                    return false;
                }
                return true;
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

            public bool HasKey(string key, bool print_error = true)
            {
                if(!settings.ContainsKey(key))
                {
                    if(print_error)
                        log.Write("ERROR: cannot required entry in the input files:  " + key);
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
                    return double.MinValue;

                try
                {
                    return Convert.ToDouble(settings[key]);
                }
                catch (Exception)
                {
                    log.Write("ERROR: reading double value for:  " + key);
                    return double.MinValue;
                }
            }
            public int GetInt(string key)
            {
                if (!HasKey(key))
                    return int.MinValue;

                try
                {
                    return Convert.ToInt32(settings[key]);
                }
                catch (Exception)
                {
                    log.Write("ERROR: reading int value for:  " + key);
                    return int.MinValue;
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
