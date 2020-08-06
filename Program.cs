using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EspMod
{
    class Program
    {
        static string Revision = "Espresso extraction model v1.22";

        public static  class Cell
        {
            public static double size_mm;
            public static double volume_mm3;
            public static double void_volume_mm3;
        }

        public class CellLayer
        {
            public double num_cells;

            public CellLayer(double n)
            {
                num_cells = n;
            }
        }

        public class Particle
        {
            public List<CellLayer> cell_layers = new List<CellLayer>();

            public double[] cell_count_ratio;

            public double num_of_these_particles;

            public Particle(int num_layers, double total_volume_of_these_particles_mm3)
            {
                cell_count_ratio = new double[num_layers];

                // for the calc we assume one center cell is surrounded by layers of cells
                double num_cell_at_prev_size = 0;
                for (int i = 0; i < num_layers; i++)
                {
                    // calc per layer
                    var particle_diam = Cell.size_mm + Cell.size_mm * 2 * i;
                    var particle_vol = Math.PI * particle_diam * particle_diam * particle_diam / 6;
                    var num_cells_per_particle = particle_vol / Cell.volume_mm3;

                    var num_cells_per_layer = num_cells_per_particle - num_cell_at_prev_size;

                    cell_layers.Add(new CellLayer(num_cells_per_layer));

                    num_cell_at_prev_size = num_cells_per_particle;

                    if(i == num_layers-1)
                        num_of_these_particles = total_volume_of_these_particles_mm3 / particle_vol;
                }

                cell_count_ratio[0] = 1.0;
                for (int i = 1; i < num_layers; i++)
                    cell_count_ratio[i] = cell_layers[i - 1].num_cells / cell_layers[i].num_cells;
            }
        }


        public class Puck
        {
            Log log;
            public bool has_errors = false;

            public List<Particle> particles = new List<Particle>();

            double volume_in_cup_mm3 = 0.0;
            double mass_in_cup_g = 0.0;

            double modelling_time_step_sec = 1;
            double modelling_print_step_sec = 2;
            int    num_modelling_slices_in_puck = 10;

            double grounds_density_kg_m3 = 330;
            double water_density_kg_m3 = 997;

            double soluble_coffee_mass_fraction = 0.3;
            double coffee_cell_size_mm = 0.03;

            double bean_weight_g = 18.0;
            double measured_ey = 20;

            public enum ModellingMode { Calibrate, Simulate };
            public ModellingMode modelling_mode = ModellingMode.Simulate;

            // shot data
            List<double> espresso_elapsed = new List<double>();
            List<double> espresso_weight = new List<double>();
            int    espresso_last_index = 0;
            double espresso_last_weight = 0;
            double espresso_weight_trim_front_sec = 5;
            double espresso_weight_trim_back_g = 1;

            // plots
            int    plots_enable = 1;
            string plots_tag = "";

            // mass matrix and mass delta for each cell and between particles
            public double[][][] mass_g;
            public double[][][] delta_mass_g;

            public double[] mass_btw_g;
            public double[] delta_mass_btw_g;

            public double volume_between_particles_per_slice_mm3;
            public double inital_soluble_mass_per_slice;
            public double inital_soluble_mass_between_particles;
            public double inital_soluble_mass_inside_cells;

            public double[][] cell_count_ratio;
            public double[] num_cells_in_outer_layer;

            public double K_the_coefficient;

            // var for the plots
            List<double> plot_time = new List<double>();
            List<double> plot_ey = new List<double>();
            List<double> plot_input_weight = new List<double>();
            List<double> plot_remain_mass_top = new List<double>();
            List<double> plot_remain_mass_middle = new List<double>();
            List<double> plot_remain_mass_bottom = new List<double>();

            // check the max percent of water replaced at each time step
            public double max_percent_water_replaced_each_time_step = 0.0;

            public Puck(Config config, Log log_)
            {
                log = log_;
                log.Write("");

                // load vars
                modelling_time_step_sec = config.GetDouble("modelling_time_step_sec"); if (!Check(modelling_time_step_sec)) return;
                modelling_print_step_sec = config.GetDouble("modelling_print_step_sec"); if (!Check(modelling_print_step_sec)) return;
                num_modelling_slices_in_puck = config.GetInt("num_modelling_slices_in_puck"); if (!Check(num_modelling_slices_in_puck)) return;

                plots_enable = config.GetInt("plots_enable"); if (!Check(plots_enable)) return;
                plots_tag = config.GetString("plots_tag");

                grounds_density_kg_m3 = config.GetDouble("grounds_density_kg_m3"); if (!Check(grounds_density_kg_m3)) return;
                water_density_kg_m3 = config.GetDouble("water_density_kg_m3"); if (!Check(water_density_kg_m3)) return;

                soluble_coffee_mass_fraction = config.GetDouble("soluble_coffee_mass_fraction"); if (!Check(soluble_coffee_mass_fraction)) return;
                coffee_cell_size_mm = config.GetDouble("coffee_cell_size_mm"); if (!Check(coffee_cell_size_mm)) return;

                bean_weight_g = config.GetDouble("dsv2_bean_weight"); if (!Check(bean_weight_g)) return;
                measured_ey = config.GetDouble("measured_ey"); if (!Check(measured_ey)) return;

                espresso_elapsed = config.GetList("espresso_elapsed"); if (!Check(espresso_elapsed)) return;
                espresso_weight = config.GetList("espresso_weight"); if (!Check(espresso_weight)) return;

                if (espresso_elapsed.Count != espresso_weight.Count)
                {
                    log.Write("ERROR: espresso_elapsed array size is different from espresso_weight array size");
                    has_errors = true;
                    return;
                }

                espresso_weight_trim_front_sec = config.GetDouble("espresso_weight_trim_front_sec"); if (!Check(espresso_weight_trim_front_sec)) return;
                espresso_weight_trim_back_g = config.GetDouble("espresso_weight_trim_back_g"); if (!Check(espresso_weight_trim_back_g)) return;

                EspressoWeightTrimBack();

                double grounds_volume_fraction = config.GetDouble("grounds_volume_fraction"); if (!Check(grounds_volume_fraction)) return;
                double void_in_grounds_volume_fraction = config.GetDouble("void_in_grounds_volume_fraction"); if (!Check(void_in_grounds_volume_fraction)) return;

                string modelling_mode_str = config.GetString("modelling_mode"); if (!Check(modelling_mode_str)) return;
                if(modelling_mode_str == "simulate") modelling_mode = ModellingMode.Simulate;
                else if (modelling_mode_str == "calibrate") modelling_mode = ModellingMode.Calibrate;
                else
                {
                    log.Write("ERROR: do not understand modelling_mode, please use simulate or calibrate");
                    has_errors = true;
                    return;
                }

                // load PSD
                List<int> psd_particle_sizes = new List<int>();
                List<double> psd_values = new List<double>();
                double psd0 = 0;
                double total_non_fines = 0;
                for (int i = 0; i < 100; i++) // support up to 100 layers
                {
                    if (config.HasKey("particle_size_distributution_" + i.ToString(), print_error: false))
                    {
                        var psd_value = config.GetDouble("particle_size_distributution_" + i.ToString()); if (!Check(psd_value)) return;

                        if (i == 0)
                            psd0 = psd_value;
                        else
                        {
                            psd_particle_sizes.Add(i);
                            psd_values.Add(psd_value);
                            total_non_fines += psd_value;
                        }
                    }
                }
                if (Math.Abs(total_non_fines + psd0 - 1) > 1E-3)
                {
                    log.Write("ERROR: sum of the PSD values had to be 1");
                    has_errors = true;
                    return;
                }
                if (Math.Abs(psd0 - 1) < 1E-3)
                {
                    log.Write("ERROR: PSD contains fines only, please add at least one particle size");
                    has_errors = true;
                    return;
                }


                // calculate the basic vars
                double grounds_density_g_mm3 = grounds_density_kg_m3 * 1E-6;
                double puck_volume_mm3 = bean_weight_g / grounds_density_g_mm3;
                double soluble_mass_per_slice = bean_weight_g * soluble_coffee_mass_fraction / num_modelling_slices_in_puck;

                // setting up static vars for Cell. Assume cells are little spheres when calculating the volume
                Cell.size_mm = coffee_cell_size_mm;
                Cell.volume_mm3 = Math.PI * coffee_cell_size_mm * coffee_cell_size_mm * coffee_cell_size_mm / 6;
                Cell.void_volume_mm3 = Cell.volume_mm3 * void_in_grounds_volume_fraction;

                // setting up slice vars
                double slice_volume_mm3 = puck_volume_mm3 / num_modelling_slices_in_puck;
                double particles_volume_per_slice_mm3 = slice_volume_mm3 * grounds_volume_fraction;

                volume_between_particles_per_slice_mm3 = slice_volume_mm3 * (1.0 - grounds_volume_fraction);
                inital_soluble_mass_per_slice = soluble_mass_per_slice;

                double mass_in_particles_per_slice_g = soluble_mass_per_slice * (1 - psd0);
                double num_of_cells_per_slice = particles_volume_per_slice_mm3 / Cell.volume_mm3;
                double mass_per_cell = mass_in_particles_per_slice_g / num_of_cells_per_slice;

                for (int i = 0; i < psd_particle_sizes.Count; i++)
                {
                    // careful with using PSD fractions in the next line - we take fraction of non-fines
                    double total_volume_of_these_particles_mm3 = particles_volume_per_slice_mm3 * (psd_values[i] / total_non_fines);

                    particles.Add(new Particle(psd_particle_sizes[i], total_volume_of_these_particles_mm3));
                }

                // setting up mass matrix and mass delta, now per slice
                int num_particle_sizes = psd_particle_sizes.Count;

                mass_g = new double[num_modelling_slices_in_puck][][];
                delta_mass_g = new double[num_modelling_slices_in_puck][][];
                for (int i_slice = 0; i_slice < num_modelling_slices_in_puck; i_slice++)
                {
                    mass_g[i_slice] = new double[num_particle_sizes][];
                    delta_mass_g[i_slice] = new double[num_particle_sizes][];
                    cell_count_ratio = new double[num_particle_sizes][];
                    num_cells_in_outer_layer = new double[num_particle_sizes];

                    for (int i_particle = 0; i_particle < num_particle_sizes; i_particle++)
                    {
                        int num_layers = particles[i_particle].cell_layers.Count;
                        mass_g[i_slice][i_particle] = new double[num_layers];
                        delta_mass_g[i_slice][i_particle] = new double[num_layers];
                        cell_count_ratio[i_particle] = new double[num_layers];

                        var m = mass_g[i_slice][i_particle];
                        var dm = delta_mass_g[i_slice][i_particle];

                        for (int i_layer = 0; i_layer < num_layers; i_layer++)
                        {
                            m[i_layer] = mass_per_cell;
                            dm[i_layer] = 0;

                            cell_count_ratio[i_particle][i_layer] = particles[i_particle].cell_count_ratio[i_layer];
                        }

                        num_cells_in_outer_layer[i_particle] = particles[i_particle].num_of_these_particles * particles[i_particle].cell_layers[num_layers - 1].num_cells;
                    }
                }

                mass_btw_g = new double[num_modelling_slices_in_puck];
                delta_mass_btw_g = new double[num_modelling_slices_in_puck];
                for (int i_slice = 0; i_slice < num_modelling_slices_in_puck; i_slice++)
                {
                    mass_btw_g[i_slice] = soluble_mass_per_slice * psd0;
                    delta_mass_btw_g[i_slice] = 0;
                }

                inital_soluble_mass_between_particles = soluble_mass_per_slice * psd0;
                inital_soluble_mass_inside_cells = mass_per_cell;
            }

            bool Check(string x)
            {
                if (x == "")
                {
                    has_errors = true;
                    return false;
                }
                return true;
            }
            bool Check(double x)
            {
                if (x == double.MinValue)
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
            bool Check(List<double> x)
            {
                if (x.Count == 0)
                {
                    has_errors = true;
                    return false;
                }
                return true;
            }

            public void PrintParticle(int[] slice_list, int i_particle)
            {
                StringBuilder sb = new StringBuilder();

                int num_cells = particles[i_particle].cell_layers.Count;

                sb.Append("   Particles with " + num_cells.ToString() + " cell layers; Per layer     Concent_kg_m3=");
                for (int i_layer = num_cells - 1; i_layer >= 0; i_layer--)
                {
                    double concentration_kg_m3 = 0.0;
                    foreach (var i_slice in slice_list)
                        concentration_kg_m3 += 1E6 * mass_g[i_slice][i_particle][i_layer] / Cell.void_volume_mm3;
                    concentration_kg_m3 /= slice_list.Length;

                    sb.Append(concentration_kg_m3.ToString("0").PadLeft(4));
                    if (i_layer != 0)
                        sb.Append(", ");
                }
                log.Write(sb.ToString());
            }
            public double PrintParticleMass(int[] slice_list, int i_particle)
            {
                StringBuilder sb = new StringBuilder();

                int num_cells = particles[i_particle].cell_layers.Count;

                sb.Append("   Particles with " + num_cells.ToString() + " cell layers; Per layer % of initial mass=");

                double sum_all = 0;
                for (int i_layer = num_cells - 1; i_layer >= 0; i_layer--)
                {
                    double percent_mass = 0.0;
                    foreach (var i_slice in slice_list)
                        percent_mass += mass_g[i_slice][i_particle][i_layer]
                                        * particles[i_particle].num_of_these_particles * particles[i_particle].cell_layers[i_layer].num_cells;
                    percent_mass /= slice_list.Length;

                    percent_mass *= 1E2 / inital_soluble_mass_per_slice;

                    sb.Append(percent_mass.ToString("0").PadLeft(4));
                    if (i_layer != 0)
                        sb.Append(", ");

                    sum_all += percent_mass;
                }
                log.Write(sb.ToString());

                return sum_all;
            }
            public void CheckTotalMass()
            {
                double result = 0;
                for (int i_slice = 0; i_slice < num_modelling_slices_in_puck; i_slice++)
                {
                    for (int i_particle = 0; i_particle < particles.Count; i_particle++)
                    {
                        int num_cells = particles[i_particle].cell_layers.Count;

                        for (int i_layer = num_cells - 1; i_layer >= 0; i_layer--)
                        {
                            result += mass_g[i_slice][i_particle][i_layer]
                                    * particles[i_particle].num_of_these_particles * particles[i_particle].cell_layers[i_layer].num_cells;
                        }
                    }

                    result += mass_btw_g[i_slice];
                }

                result += mass_in_cup_g;

                var check = Math.Abs(result - inital_soluble_mass_per_slice * num_modelling_slices_in_puck);
                if (check > 1E-8)
                    log.Write("Mass check failed " + check.ToString());
            }

            public double PrintSlice(int[] slice_list, string caption)
            {
                double void_concentration_kg_m3 = 0.0;
                double percent_mass = 0.0;

                foreach (var i_slice in slice_list)
                {
                    void_concentration_kg_m3 += 1E6 * mass_btw_g[i_slice] / volume_between_particles_per_slice_mm3;
                    percent_mass += 1E2 * mass_btw_g[i_slice] / inital_soluble_mass_per_slice;
                }
                void_concentration_kg_m3 /= slice_list.Length;
                percent_mass /= slice_list.Length;

                log.Write(caption.PadRight(30) +
                               "   Between       Concent_kg_m3=" + void_concentration_kg_m3.ToString("0").PadLeft(4));

                for (int i_particle = 0; i_particle < particles.Count; i_particle++)
                    PrintParticle(slice_list, i_particle);

                log.Write("              " +
                            "                   Between   % of initial mass=" + percent_mass.ToString("0").PadLeft(4));

                double sum_all = 0;
                for (int i_particle = 0; i_particle < particles.Count; i_particle++)
                    sum_all += PrintParticleMass(slice_list, i_particle);

                log.Write("                    Sum over all particles % of initial mass=" + sum_all.ToString("0").PadLeft(4));

                log.Write("");

                return sum_all;
            }
            public void Print(double timestamp)
            {
                log.Write("");
                log.Write("-------------------------------------------------------------------------------");
                log.Write("Time=" + timestamp.ToString("0.00") + " sec");

                double top_sum_all = 0.0;
                double middle_sum_all = 0.0;
                double bottom_sum_all = 0.0;

                if (num_modelling_slices_in_puck == 1)
                {
                    log.Write("");
                    log.Write("Initial soluble mass in the puck= " + inital_soluble_mass_per_slice.ToString("0.00").PadLeft(3) +
                              " g. The percentage of the initial mass below is given relative to this value");

                    log.Write("");

                    PrintSlice(new int[] { 0 }, "Whole puck");
                }
                else if (num_modelling_slices_in_puck == 2)
                {
                    log.Write("");
                    log.Write("Initial soluble mass in half of the puck= " + inital_soluble_mass_per_slice.ToString("0.00").PadLeft(3) +
                              " g. The percentage of the initial mass below is given relative to this value");

                    log.Write("");

                    PrintSlice(new int[] { 0 }, "Top half of the puck");
                    PrintSlice(new int[] { 1 }, "Bottom half of the puck");
                }
                else if (num_modelling_slices_in_puck > 2)
                {
                    log.Write("");
                    log.Write("Initial soluble mass in third of the puck= " +
                              (inital_soluble_mass_per_slice * num_modelling_slices_in_puck / 3).ToString("0.00").PadLeft(3) +
                              " g. The percentage of the initial mass below is given relative to this value");

                    log.Write("");

                    int slice_counter = 0;

                    List<int> slice_list = new List<int>();
                    for (int i = 0; i < num_modelling_slices_in_puck / 3; i++)
                    {
                        slice_list.Add(slice_counter);
                        slice_counter++;
                    }
                    top_sum_all = PrintSlice(slice_list.ToArray(), "Top third of the puck");

                    slice_list.Clear();
                    for (int i = 0; i < num_modelling_slices_in_puck / 3; i++)
                    {
                        slice_list.Add(slice_counter);
                        slice_counter++;
                    }
                    middle_sum_all = PrintSlice(slice_list.ToArray(), "Middle third of the puck");


                    slice_list.Clear();
                    for (int i = 0; i < num_modelling_slices_in_puck / 3; i++)
                    {
                        slice_list.Add(slice_counter);
                        slice_counter++;
                    }
                    bottom_sum_all = PrintSlice(slice_list.ToArray(), "Bottom third of the puck");
                }

                var concentration = volume_in_cup_mm3 == 0 ? 0.0 : 1E6 * mass_in_cup_g / volume_in_cup_mm3;
                var ey = 100.0 * mass_in_cup_g / bean_weight_g;
                var water_weight_g = volume_in_cup_mm3 * water_density_kg_m3 * 1E-6;
                var total_weight_g = water_weight_g + mass_in_cup_g;
                var ratio = total_weight_g / bean_weight_g;


                log.Write("In the cup: Volume_mL=" + (1E-3 * volume_in_cup_mm3).ToString("0.0") +
                          " Coffee_mass_g=" + mass_in_cup_g.ToString("0.00") +
                          " Total_mass_g=" + total_weight_g.ToString("0.00") +
                          " Ratio=1:" + ratio.ToString("0.0") +
                          " Concent_kg_m3=" + concentration.ToString("0") +
                          " EY%=" + ey.ToString("0.00"));

                CheckTotalMass();

                plot_time.Add(timestamp);
                plot_ey.Add(ey);
                plot_input_weight.Add(espresso_last_weight);
                plot_remain_mass_top.Add(top_sum_all);
                plot_remain_mass_middle.Add(middle_sum_all);
                plot_remain_mass_bottom.Add(bottom_sum_all);
            }

            void SaveList(string fname, List<double> x, List<double> y)
            {
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < x.Count; i++)
                {
                    sb.AppendLine(x[i].ToString() + "\t" + y[i].ToString());
                }

                File.WriteAllText(fname, sb.ToString());
            }
            public void SavePlots(string folder_name)
            {
                if(plots_enable != 1) return;

                string plots_folder_name = folder_name == "" ? "plots" : folder_name + "\\plots";

                if (!Directory.Exists(plots_folder_name))
                    Directory.CreateDirectory(plots_folder_name);

                SaveList(plots_folder_name + "\\plot_ey" + plots_tag + ".txt", plot_time, plot_ey);
                SaveList(plots_folder_name + "\\plot_input_weight" + plots_tag + ".txt", plot_time, plot_input_weight);
                SaveList(plots_folder_name + "\\plot_remain_mass_top" + plots_tag + ".txt", plot_time, plot_remain_mass_top);
                SaveList(plots_folder_name + "\\plot_remain_mass_middle" + plots_tag + ".txt", plot_time, plot_remain_mass_middle);
                SaveList(plots_folder_name + "\\plot_remain_mass_bottom" + plots_tag + ".txt", plot_time, plot_remain_mass_bottom);
            }

            public void SaveExtra()
            {
                if (has_errors)
                    return;

                log.Write("");
                log.Write("Max percent of water replaced at each time step per slice=" + max_percent_water_replaced_each_time_step.ToString("0.0") + "%");
            }

            public double GetEspressoWeight(double timestamp)
            {
                int max_index = espresso_elapsed.Count - 1;
                if (timestamp <= espresso_elapsed[0])
                    return espresso_weight[0];

                if (timestamp >= espresso_elapsed[max_index])
                    return espresso_weight[max_index];


                double res = 0.0;
                while (timestamp < espresso_elapsed[espresso_last_index])
                    espresso_last_index--;
                while (timestamp >= espresso_elapsed[espresso_last_index+1])
                    espresso_last_index++;

                res = espresso_weight[espresso_last_index]
                     + (espresso_weight[espresso_last_index + 1] - espresso_weight[espresso_last_index])
                     * (timestamp - espresso_elapsed[espresso_last_index])
                     / (espresso_elapsed[espresso_last_index + 1] - espresso_elapsed[espresso_last_index]);

                return Math.Max(0.0, res);
            }

            void EspressoWeightTrimBack()
            {
                int max_index = espresso_elapsed.Count - 1;
                double max_value = espresso_weight[max_index];

                int new_index = max_index;
                while(max_value - espresso_weight[new_index] < espresso_weight_trim_back_g)
                {
                    new_index--;
                    if (new_index == 0)
                        break;
                }

                for(int i = 0; i < max_index- new_index; i++)
                {
                    espresso_elapsed.RemoveAt(espresso_elapsed.Count - 1);
                    espresso_weight.RemoveAt(espresso_weight.Count - 1);
                }
            }

            public void SimulateParticleDiffusion(int i_slice)
            {
                double cell_2_cell_coeff = K_the_coefficient * modelling_time_step_sec;
                double cell_2_void_coeff = K_the_coefficient * Cell.void_volume_mm3 * modelling_time_step_sec;

                for (int i_particle = 0; i_particle < particles.Count; i_particle++)
                {
                    int num_layers = particles[i_particle].cell_layers.Count;

                    var m = mass_g[i_slice][i_particle];
                    var dm = delta_mass_g[i_slice][i_particle];

                    // cell 2 cell tranfer
                    for (int i_layer = 0; i_layer < num_layers - 1; i_layer++)
                    {
                        var delta_m = cell_2_cell_coeff * (m[i_layer] - m[i_layer + 1]);
                        dm[i_layer] -= delta_m;
                        dm[i_layer + 1] += delta_m * cell_count_ratio[i_particle][i_layer + 1];
                    }

                    // outer cell to void transfer
                    var delta_m_out = cell_2_void_coeff * (m[num_layers - 1] / Cell.void_volume_mm3 - mass_btw_g[i_slice] / volume_between_particles_per_slice_mm3);

                    dm[num_layers - 1] -= delta_m_out;

                    delta_mass_btw_g[i_slice] += delta_m_out * num_cells_in_outer_layer[i_particle];
                }
            }

            public double Simulate(double k_the_coefficient, bool print_to_log = false)
            {
                double timestamp = espresso_weight_trim_front_sec;
                double last_print_time = espresso_weight_trim_front_sec;
                double delta_mass_in_cup_g = 0.0;
                double ey;

                volume_in_cup_mm3 = 0.0;
                mass_in_cup_g = 0.0;
                max_percent_water_replaced_each_time_step = 0.0;

                for (int i_slice = 0; i_slice < num_modelling_slices_in_puck; i_slice++)
                {
                    for (int i_particle = 0; i_particle < particles.Count; i_particle++)
                    {
                        int num_layers = particles[i_particle].cell_layers.Count;

                        var m = mass_g[i_slice][i_particle];
                        var dm = delta_mass_g[i_slice][i_particle];

                        for (int i_layer = 0; i_layer < num_layers; i_layer++)
                        {
                            m[i_layer] = inital_soluble_mass_inside_cells;
                            dm[i_layer] = 0;
                        }
                    }
                }

                for (int i_slice = 0; i_slice < num_modelling_slices_in_puck; i_slice++)
                {
                    mass_btw_g[i_slice] = inital_soluble_mass_between_particles;
                    delta_mass_btw_g[i_slice] = 0;
                }


                K_the_coefficient = k_the_coefficient;

                espresso_last_weight = GetEspressoWeight(timestamp);

                var modelling_total_time_sec = espresso_elapsed[espresso_elapsed.Count - 1];

                if (print_to_log)
                    Print(timestamp);

                do
                {
                    timestamp += modelling_time_step_sec;

                    var espresso_current_weight = GetEspressoWeight(timestamp);
                    var water_weight_g = espresso_current_weight - espresso_last_weight - delta_mass_in_cup_g;
                    var fresh_water_mm3 = water_weight_g / (water_density_kg_m3 * 1E-6);
                    espresso_last_weight = espresso_current_weight;

                    var percent_water_replaced_each_time_step = 100.0 * fresh_water_mm3 / volume_between_particles_per_slice_mm3;
                    if (percent_water_replaced_each_time_step > 20.0)
                    {
                        log.Write("");
                        log.Write("ERROR: Please decrease the time step, fresh water takes more than 20% of the layer volume");
                        has_errors = true;
                        return 0.0;
                    }
                    max_percent_water_replaced_each_time_step = Math.Max(percent_water_replaced_each_time_step, max_percent_water_replaced_each_time_step);


                    // diffusion from particles
                    for (int i_slice = 0; i_slice < num_modelling_slices_in_puck; i_slice++)
                        SimulateParticleDiffusion(i_slice);

                    // flow of the fresh water
                    var mass_passed_to_next_layer_g = mass_btw_g[0] * (fresh_water_mm3 / volume_between_particles_per_slice_mm3);
                    delta_mass_btw_g[0] -= mass_passed_to_next_layer_g;

                    for (int i = 1; i < num_modelling_slices_in_puck; i++)
                    {
                        var current_delta_layer_mass_g = mass_btw_g[i] * (fresh_water_mm3 / volume_between_particles_per_slice_mm3);

                        delta_mass_btw_g[i] += mass_passed_to_next_layer_g;
                        delta_mass_btw_g[i] -= current_delta_layer_mass_g;

                        mass_passed_to_next_layer_g = current_delta_layer_mass_g;
                    }
                    delta_mass_in_cup_g = mass_passed_to_next_layer_g;

                    // Print
                    if ((timestamp - last_print_time + 1E-5) > modelling_print_step_sec) // bump the time delta for robust interval comparison
                    {
                        if (print_to_log)
                            Print(timestamp);
                        last_print_time = timestamp;
                    }

                    ey = 100.0 * mass_in_cup_g / bean_weight_g;

                    // Finally update mass values for the next step
                    for (int i_slice = 0; i_slice < num_modelling_slices_in_puck; i_slice++)
                    {
                        mass_btw_g[i_slice] += delta_mass_btw_g[i_slice];
                        delta_mass_btw_g[i_slice] = 0;

                        for (int i_particle = 0; i_particle < particles.Count; i_particle++)
                        {
                            int num_layers = particles[i_particle].cell_layers.Count;

                            var m = mass_g[i_slice][i_particle];
                            var dm = delta_mass_g[i_slice][i_particle];

                            for (int i_layer = 0; i_layer < num_layers; i_layer++)
                            {
                                m[i_layer] += dm[i_layer];
                                dm[i_layer] = 0;
                            }
                        }
                    }

                    // update in the cup values
                    volume_in_cup_mm3 += fresh_water_mm3;
                    mass_in_cup_g += delta_mass_in_cup_g;
                }
                while (timestamp < modelling_total_time_sec);

                var result = ey - measured_ey;

                if (modelling_mode == ModellingMode.Calibrate && print_to_log == false)
                    log.Write("k_the_coefficient=" + k_the_coefficient.ToString("0.000000") 
                        + "  measured EY=" + measured_ey.ToString("0.000000") 
                        + "  calculated EY=" + ey.ToString("0.000000"));

                return result;
            }

            // This is the famous Zeroin from Forsythe book. Initially had it on punchcards in Fortran 
            public double Zeroin(double Ax, double Bx,  // search interval
                          double Tol)            // answer tolerance
            {
                double A, B, C, D, E, Eps, FA, FB, FC, Toll, Xm, P, Q, R, S;

                // Calculation Of comp. Epsilon
                Eps = 1.0;
                while ((1.0 + Eps / 2.0) != 1.0)
                {
                    Eps /= 2.0;
                }

                // Start Values
                A = Ax; B = Bx;
                FA = Simulate(A);
                FB = Simulate(B);

                // Start Step
                do
                {
                    C = A; FC = FA;
                    D = B - A; E = D;

                Lab30:
                    if (Math.Abs(FC) < Math.Abs(FB))
                    {
                        A = B; B = C; C = A;
                        FA = FB; FB = FC; FC = FA;
                    }
                    // Test Of Collapsing
                    Toll = 2.0 * Eps * Math.Abs(B) + Tol / 2.0;
                    Xm = (C - B) / 2.0;

                    if ((Math.Abs(Xm) <= Toll) || (FB == 0.0))
                    {
                        // End of Job and exit
                        return B;
                    }

                    // Is Bisection Needed ??
                    if ((Math.Abs(E) < Toll) || (Math.Abs(FA) <= Math.Abs(FB)))
                    {
                        // Bisection
                        D = Xm; E = D;
                    }
                    else
                    //Is Square interpolation possible ??
                    {
                        if (A == C)
                        {
                            // Line Interpolation}
                            S = FB / FA;
                            P = 2.0 * Xm * S;
                            Q = 1.0 - S;
                        }
                        else
                        // Inverse Square Interpolation
                        {
                            Q = FA / FC;
                            R = FB / FC;
                            S = FB / FA;
                            P = S * (2.0 * Xm * Q * (Q - R) - (B - A) * (R - 1.0));
                            Q = (Q - 1.0) * (R - 1.0) * (S - 1.0);
                        }
                        // Choose the Signs
                        if (P > 0) Q = -Q;
                        P = Math.Abs(P);

                        // Is Square interpolation possible ??
                        // Is Interpolation possible ??
                        if ((2.0 * P < (3.0 * Xm * Q - Math.Abs(Toll) * Q)) && (P < Math.Abs(E * Q / 2.0)))
                        {
                            E = D; D = P / Q;
                        }
                        else
                        // Bisection
                        {
                            D = Xm; E = D;
                        }
                    }

                    // End of step
                    A = B;
                    FA = FB;
                    if (Math.Abs(D) > Toll)
                    {
                        B += D;
                    }
                    else
                    {
                        if (Xm < 0.0)
                        {
                            B -= Math.Abs(Toll);
                        }
                        else
                        {
                            B += Math.Abs(Toll);
                        }
                    }
                    FB = Simulate (B);
                    if (FB * (FC / Math.Abs(FC)) <= 0.0) goto Lab30;
                }
                while (true);
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

                // presets
                settings["wait_for_keystroke"] = "1";

                if (!LoadFile(fname))
                {
                    has_errors = true;
                    return;
                }

                if (HasKey("shot_file_name", print_error: false))
                {
                    var shot_file_name = GetString("shot_file_name");
                    if (shot_file_name != "")
                    {
                        if (!LoadFile(shot_file_name))
                        {
                            has_errors = true;
                            return;
                        }
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
                        log.Write("ERROR: cannot find required entry in the input files:  " + key);
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

            public List<double> GetList(string key)
            {
                List<double> res = new List<double>();

                if (!HasKey(key))
                    return res;

                var str = settings[key];

                str = str.Replace("{", "").Replace("}", "").Trim();

                if (String.IsNullOrEmpty(str))
                    return res;

                var words = str.Split(' ');
                foreach (var w in words)
                {
                    var x = Convert.ToDouble(w.Trim());
                    res.Add(x);
                }

                return res;
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

        static void WaitForKey()
        {
            Console.WriteLine("");
            Console.WriteLine("Press any key to close this window ...");
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Console.WriteLine(Revision);

            if(args.Length < 1)
            {
                Console.WriteLine("ERROR: please supply the config file name to process");
                WaitForKey();
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("ERROR: cannot find the input file " + args[0]);
                WaitForKey();
                return;
            }

            string log_file_name = Path.GetFileNameWithoutExtension(args[0]) + ".log";
            Log log = new Log(log_file_name, Revision);

            Config config = new Config(args[0], log);
            if (config.has_errors)
            {
                Console.WriteLine("ERRORS: please check the log for details");
                WaitForKey();
                return;
            }

            Puck puck = new Puck(config, log);
            if (puck.has_errors)
            {
                Console.WriteLine("ERRORS: please check the log for details");
                WaitForKey();
                return;
            }

            

            double k_the_coefficient = config.GetDouble("k_the_coefficient");
            if(k_the_coefficient == double.MinValue)
            {
                Console.WriteLine("ERROR: cannot find entry k_the_coefficient in the input file");
                WaitForKey();
                return;
            }

            if (puck.modelling_mode == Puck.ModellingMode.Simulate)
            {
                puck.Simulate(k_the_coefficient, print_to_log: true);
                puck.SaveExtra();
                puck.SavePlots(Path.GetDirectoryName(args[0]));
            }
            else if (puck.modelling_mode == Puck.ModellingMode.Calibrate)
            {
                log.Write("Running the root finding algorithm ...");
                double fitted_k = puck.Zeroin(0.01, 0.35, 1E-5);
                double result = puck.Simulate(fitted_k, print_to_log: false);
                if (Math.Abs(result) < 1E-4)
                {
                    log.Write("");
                    log.Write("==> This is the value for k_the_coefficient = " + fitted_k.ToString("0.000000"));
                    log.Write("");

                    puck.Simulate(fitted_k, print_to_log: true);
                    puck.SaveExtra();
                    puck.SavePlots(Path.GetDirectoryName(args[0]));
                }
                else
                {
                    log.Write("");
                    log.Write("ERROR: cannot find k_the_coefficient to match the measured EY. Review the inputs, specially the PSD values");
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Finished");
            if(config.GetString("wait_for_keystroke") == "1")
                WaitForKey();
        }
    }
}
