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
        static string Revision = "Espresso extraction model v1.8";

        public class Cell
        {
            public static double diameter_mm;
            public static double volume_mm3;
            public static double void_volume_mm3;
            public static double K_the_coefficient;
            public static double modelling_time_step_sec;

            public double        num_of_these_cells;

            public Cell(double n)
            {
                num_of_these_cells = n;
            }
        }

        public class Grain
        {
            public List<Cell> cells = new List<Cell>();

            public double[] mass_g;
            public double[] delta_mass_g;
            public double[] cell_count_coeff;

            public double total_volume_of_these_grains_mm3;
            public double total_soluble_mass_in_these_grains_g;

            public double diameter_of_one_grain_mm;
            public double volume_of_one_grain_mm3;
            public double soluble_mass_in_one_grain_g;
            public double num_of_these_grains;

            public Grain(int level)
            {
                mass_g = new double[level];
                delta_mass_g = new double[level];
                cell_count_coeff = new double[level];

                // for the calc we assume one center cell is surrounded by layers of cells
                double num_cell_at_prev_size = 0;
                for (int i = 0; i < level; i++)
                {
                    // calc per layer
                    var grain_diam = Cell.diameter_mm + Cell.diameter_mm * 2 * i;
                    var grain_vol = Math.PI * grain_diam * grain_diam * grain_diam / 6;
                    var num_cells_per_grain = grain_vol / Cell.volume_mm3;

                    var num_cells_per_level = num_cells_per_grain - num_cell_at_prev_size;

                    cells.Add(new Cell(num_cells_per_level));

                    num_cell_at_prev_size = num_cells_per_grain;

                    if(i == level-1)
                    {
                        diameter_of_one_grain_mm = grain_diam;
                        volume_of_one_grain_mm3 = grain_vol;
                    }

                    delta_mass_g[i] = 0.0;
                }
                num_of_these_grains = total_volume_of_these_grains_mm3 / volume_of_one_grain_mm3;


                cell_count_coeff[0] = 1.0;
                for (int i = 1; i < level; i++)
                    cell_count_coeff[i] = cells[i - 1].num_of_these_cells / cells[i].num_of_these_cells;
            }

            public double GetTotalNumCells()
            {
                double output = 0;
                foreach(var cell in cells)
                {
                    output += cell.num_of_these_cells;
                }
                return output;
            }
            public void SetInitalMassPerCell(double mass)
            {
                for (int i = 0; i < cells.Count; i++)
                    mass_g[i] = mass;
            }

            public void Print(Log log, int index)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("   Grains with " + index.ToString() + " cell layers; Per layer TDS=");
                for(int i = cells.Count-1; i>=0; i--)
                {
                    var tds = 1E5 * mass_g[i] / Cell.void_volume_mm3;
                    sb.Append(tds.ToString("0.00").PadLeft(6));
                    if(i != 0)
                        sb.Append(", ");
                }
                log.Write(sb.ToString());
            }

            public void SimulateCell2CellDiffusion()
            {
                double cell_2_cell_coeff = Cell.K_the_coefficient * Cell.modelling_time_step_sec / Cell.void_volume_mm3;

                // reset delta mass
                for (int i = 0; i < cells.Count; i++)
                    delta_mass_g[i] = 0.0;

                // cell 2 cell tranfer
                for (int i = 0; i < cells.Count-1; i++)
                {
                    var delta_m          = cell_2_cell_coeff * (mass_g[i] - mass_g[i + 1]);
                    delta_mass_g[i]     -= delta_m;
                    delta_mass_g[i + 1] += delta_m * cell_count_coeff[i + 1];
                }
            }
            public void UpdateMass()
            {
                for (int i = 0; i < cells.Count; i++)
                    mass_g[i] += delta_mass_g[i];
            }
        }

        public class Layer
        {
            //public int num;
            public Dictionary<int, Grain> grains = new Dictionary<int, Grain>();

            public double layer_volume_mm3;
            public double grounds_volume_mm3;
            public double void_volume_mm3;
            public double mass_g;
            public double delta_mass_g;

            public Layer()
            {
            }

            public void Print(Log log, int index)
            {
                var void_tds = 1E5 * mass_g / void_volume_mm3;
                log.Write("Puck layer=" + index.ToString() + "                            TDS=" + void_tds.ToString("0.00").PadLeft(6));
                foreach (int key in grains.Keys)
                    grains[key].Print(log, key);

                log.Write("");
            }

            public void SimulateGrainIntoVoidDiffusion()
            {
                delta_mass_g = 0;
                foreach (int key in grains.Keys)
                {
                    var grain = grains[key];

                    // cell 2 cell
                    grain.SimulateCell2CellDiffusion();

                    // outer cell to void transfer
                    var outer_cell_index = grain.cells.Count - 1;
                    var delta_m_out = Cell.K_the_coefficient * Cell.modelling_time_step_sec *
                        (grain.mass_g[outer_cell_index] / Cell.void_volume_mm3 - mass_g / void_volume_mm3);

                    grain.delta_mass_g[outer_cell_index] -= delta_m_out;

                    delta_mass_g += delta_m_out * grain.num_of_these_grains * grain.cells[outer_cell_index].num_of_these_cells;
                }
            }
        }

        public class Puck
        {
            Log    log;
            public bool has_errors = false;

            List<Layer> layers = new List<Layer>();

            double volume_in_cup_mm3                    = 0.0;
            double mass_in_cup_g                        = 0.0;

            double modelling_time_step_sec              = 1;
            double modelling_total_time_sec             = 2;
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

                // load vars
                modelling_time_step_sec                 = config.GetDouble("modelling_time_step_sec");          if (!Check(modelling_time_step_sec)) return;
                modelling_total_time_sec                = config.GetInt("modelling_total_time_sec");            if (!Check(modelling_total_time_sec)) return;
                num_modelling_layers_in_puck            = config.GetInt("num_modelling_layers_in_puck");        if (!Check(num_modelling_layers_in_puck)) return;

                puck_diameter_mm                        = config.GetDouble("puck_diameter_mm");                 if (!Check(puck_diameter_mm)) return;
                grounds_density_kg_m3                   = config.GetDouble("grounds_density_kg_m3");            if (!Check(grounds_density_kg_m3)) return;

                soluble_coffee_mass_fraction            = config.GetDouble("soluble_coffee_mass_fraction");     if (!Check(soluble_coffee_mass_fraction)) return;
                coffee_cell_diameter_mm                 = config.GetDouble("coffee_cell_diameter_mm");          if (!Check(coffee_cell_diameter_mm)) return;

                dsv2_bean_weight                        = config.GetDouble("dsv2_bean_weight");                 if (!Check(dsv2_bean_weight)) return;

                double grounds_volume_fraction          = config.GetDouble("grounds_volume_fraction");          if (!Check(grounds_volume_fraction)) return;
                double void_in_grounds_volume_fraction  = config.GetDouble("void_in_grounds_volume_fraction");  if (!Check(void_in_grounds_volume_fraction)) return;

                // load PSD
                Dictionary<int, double> psd = new Dictionary<int, double>();
                double total = 0.0;
                for (int i = 0; i < 100; i++) // support up to 100 layers
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


                // calculate the basic vars
                double puck_area_mm2            = Math.PI * puck_diameter_mm * puck_diameter_mm * 0.25;
                double grounds_density_g_mm3    = grounds_density_kg_m3 * 1E3 * 1E-9;
                double puck_volume_mm3          = dsv2_bean_weight / grounds_density_g_mm3;
                double puck_height_mm           = puck_volume_mm3 / puck_area_mm2;
                double soluble_mass_per_layer   = dsv2_bean_weight * soluble_coffee_mass_fraction / num_modelling_layers_in_puck;
                log.Write("puck_height_mm " + puck_height_mm.ToString("0.000"));


                // setting up static vars for Cell. Assume cells are little cubes when calculating the volume
                Cell.diameter_mm = coffee_cell_diameter_mm;
                Cell.volume_mm3 = Math.PI * coffee_cell_diameter_mm * coffee_cell_diameter_mm * coffee_cell_diameter_mm / 6;
                Cell.void_volume_mm3 = Cell.volume_mm3 * void_in_grounds_volume_fraction;

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
                        if (key == 0)
                        {
                            layer.mass_g = soluble_mass_per_layer * psd[key];

                            var initial_concentration_in_void = layer.mass_g / layer.void_volume_mm3;
                            log.Write("initial_concentration between grains " + initial_concentration_in_void.ToString("0.0000000"));

                            //initial_concentration between grains 8.59375E-05
                            //0.08E-3 g per 1E-3 g(this is 1 mm3 weight) => 8 % TDS in the void.Looks correct.
                        }
                        else
                        {
                            Grain g = new Grain(key);
                            g.total_volume_of_these_grains_mm3 = layer.grounds_volume_mm3 * psd[key];
                            g.total_soluble_mass_in_these_grains_g = soluble_mass_per_layer * psd[key];
                            g.num_of_these_grains = g.total_volume_of_these_grains_mm3 / g.volume_of_one_grain_mm3;
                            g.soluble_mass_in_one_grain_g = g.total_soluble_mass_in_these_grains_g / g.num_of_these_grains;

                            double mass_per_cell = g.soluble_mass_in_one_grain_g / g.GetTotalNumCells();
                            g.SetInitalMassPerCell(mass_per_cell);

                            // checks!
                            var initial_concentration = g.total_soluble_mass_in_these_grains_g / 
                                (g.total_volume_of_these_grains_mm3 * void_in_grounds_volume_fraction);
                            log.Write("initial_concentration inside grain   " + initial_concentration.ToString("0.0000000"));

                            initial_concentration = mass_per_cell / Cell.void_volume_mm3;
                            log.Write("initial_concentration inside g_alt   " + initial_concentration.ToString("0.0000000"));

                            //initial_concentration 0.000185839830844456
                            //good: 185 kg/m3 => TDS 18.5% in cells

                            layer.grains[key] = g;
                        }
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

            public void Print(double timestamp)
            {
                log.Write("");
                log.Write("-------------------------------------------------------------------------------");
                log.Write("Time=" + timestamp.ToString("0.00") + " sec");
                log.Write("");

                if (layers.Count == 1)
                    layers[0].Print(log, 0);
                else if (layers.Count == 2)
                {
                    layers[0].Print(log, 0);
                    layers[1].Print(log, 1);
                }
                else if (layers.Count > 2)
                {
                    layers[0].Print(log, 0);
                    layers[layers.Count / 2].Print(log, layers.Count / 2);
                    layers[layers.Count - 1].Print(log, layers.Count - 1);
                }

                var tds = volume_in_cup_mm3 == 0 ? 0.0 : 1E5 * mass_in_cup_g / volume_in_cup_mm3;
                var ey = 100.0 * mass_in_cup_g / dsv2_bean_weight;
                log.Write("In the cup: Volume_ml=" + (1E-3* volume_in_cup_mm3).ToString("0.000") +
                          " Mass_g=" + mass_in_cup_g.ToString("0.00") +
                          " TDS%=" + tds.ToString("0.00") + 
                          " EY%=" + ey.ToString("0.0"));
            }

            public void Simulate()
            {
                double timestamp = 0.0;

                var fresh_water_mm3 = 1.0E3 * modelling_time_step_sec;  // 1 ml
                Cell.K_the_coefficient = 0.2 * Cell.void_volume_mm3;
                Cell.modelling_time_step_sec = modelling_time_step_sec;

                if(fresh_water_mm3 > layers[0].void_volume_mm3 * 0.3)
                {
                    log.Write("ERROR: Please decrease the time step, fresh water takes more than 30% of the layer volume");
                    has_errors = true;
                    return;
                }

                // assume for a start that water moves at constant 1 ml/sec speed, i.e. fresh_water_ml = 1
                do
                {
                    timestamp += modelling_time_step_sec;

                    // diffusion from grains
                    foreach (Layer layer in layers)
                        layer.SimulateGrainIntoVoidDiffusion();


                    // flow of the fresh water
                    
                    var layer_0 = layers[0];

                    var prev_delta_layer_mass_g = layer_0.mass_g * (fresh_water_mm3 / layer_0.void_volume_mm3);
                    layer_0.delta_mass_g -= prev_delta_layer_mass_g;

                    for (int i = 1; i < layers.Count; i++)
                    {
                        var layer_i = layers[i];
                        var current_delta_layer_mass_g = layer_i.mass_g * (fresh_water_mm3 / layer_i.void_volume_mm3);

                        layer_i.delta_mass_g += prev_delta_layer_mass_g;
                        layer_i.delta_mass_g -= current_delta_layer_mass_g;

                        prev_delta_layer_mass_g = current_delta_layer_mass_g;
                    }

                    // update in the cup values
                    volume_in_cup_mm3 += fresh_water_mm3;
                    mass_in_cup_g += prev_delta_layer_mass_g;
                    

                    // update mass values
                    for (int i = 0; i < layers.Count; i++)
                    {
                        var layer_i = layers[i];
                        layer_i.mass_g += layer_i.delta_mass_g;

                        foreach(int key in layer_i.grains.Keys)
                        {
                            layer_i.grains[key].UpdateMass();
                        }
                    }


                    Print(timestamp);
                }
                while (timestamp < modelling_total_time_sec);
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

            puck.Print(timestamp: 0.0);

            puck.Simulate();

            Console.WriteLine("Finished");
        }
    }
}
