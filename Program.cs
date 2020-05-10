﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EspMod
{
    class Program
    {
        static string Revision = "Espresso extraction model v1.11";

        public class Cell
        {
            public static double size_mm;
            public static double volume_mm3;
            public static double void_volume_mm3;
            public static double Kstar_the_coefficient;
            public static double modelling_time_step_sec;

            public double        num_of_these_cells;

            public Cell(double n)
            {
                num_of_these_cells = n;
            }
        }

        public class Particle
        {
            public List<Cell> cells = new List<Cell>();

            public double[] mass_g;
            public double[] delta_mass_g;
            public double[] cell_count_coeff;

            public double diameter_of_one_particle_mm;
            public double volume_of_one_particle_mm3;
            public double num_of_these_particles;

            public Particle(int level, double total_volume_of_these_particles_mm3)
            {
                mass_g = new double[level];
                delta_mass_g = new double[level];
                cell_count_coeff = new double[level];

                // for the calc we assume one center cell is surrounded by layers of cells
                double num_cell_at_prev_size = 0;
                for (int i = 0; i < level; i++)
                {
                    // calc per layer
                    var particle_diam = Cell.size_mm + Cell.size_mm * 2 * i;
                    var particle_vol = Math.PI * particle_diam * particle_diam * particle_diam / 6;
                    var num_cells_per_particle = particle_vol / Cell.volume_mm3;

                    var num_cells_per_level = num_cells_per_particle - num_cell_at_prev_size;

                    cells.Add(new Cell(num_cells_per_level));

                    num_cell_at_prev_size = num_cells_per_particle;

                    if(i == level-1)
                    {
                        diameter_of_one_particle_mm = particle_diam;
                        volume_of_one_particle_mm3 = particle_vol;
                    }

                    delta_mass_g[i] = 0.0;
                }
                num_of_these_particles = total_volume_of_these_particles_mm3 / volume_of_one_particle_mm3;


                cell_count_coeff[0] = 1.0;
                for (int i = 1; i < level; i++)
                    cell_count_coeff[i] = cells[i - 1].num_of_these_cells / cells[i].num_of_these_cells;
            }
            public void SetInitalMassPerCell(double mass)
            {
                for (int i = 0; i < cells.Count; i++)
                    mass_g[i] = mass;
            }

            public void Print(Log log, int index)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("   Particles with " + index.ToString() + " cell layers; Per layer Concentration=");
                for(int i = cells.Count-1; i>=0; i--)
                {
                    var concentration_kg_m3 = 1E6 * mass_g[i] / Cell.void_volume_mm3;
                    sb.Append(concentration_kg_m3.ToString("0").PadLeft(4));
                    if(i != 0)
                        sb.Append(", ");
                }
                log.Write(sb.ToString());
            }

            public void PrintMass(Log log, int index, double inital_total_mass)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("   Particles with " + index.ToString() + " cell layers; Per layer     % of mass=");
                for (int i = cells.Count - 1; i >= 0; i--)
                {
                    var percent_mass = 1E2 * mass_g[i] * num_of_these_particles * cells[i].num_of_these_cells / inital_total_mass;
                    sb.Append(percent_mass.ToString("0").PadLeft(4));
                    if (i != 0)
                        sb.Append(", ");
                }
                log.Write(sb.ToString());
            }

            public void SimulateCell2CellDiffusion()
            {
                double cell_2_cell_coeff = Cell.Kstar_the_coefficient * Cell.modelling_time_step_sec / Cell.void_volume_mm3;

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

        public class Slice
        {
            public Dictionary<int, Particle> particles = new Dictionary<int, Particle>();

            public double volume_between_particles_mm3;
            public double mass_between_particles_g;
            public double delta_mass_g;
            public double inital_soluble_mass_per_layer;

            public Slice()
            {
            }

            public void Print(Log log, int index)
            {
                var void_concentration_kg_m3 = 1E6 * mass_between_particles_g / volume_between_particles_mm3;
                var percent_mass = 1E2 * mass_between_particles_g / inital_soluble_mass_per_layer;
                log.Write("Puck slice=" + (index+1).ToString().PadLeft(3) + 
                          "                             Concentration=" + void_concentration_kg_m3.ToString("0").PadLeft(4));

                foreach (int key in particles.Keys)
                    particles[key].Print(log, key);

                log.Write("");

                log.Write("              " +
                          "                                 % of mass=" + percent_mass.ToString("0").PadLeft(4));

                foreach (int key in particles.Keys)
                    particles[key].PrintMass(log, key, inital_soluble_mass_per_layer);

                log.Write("");
            }

            public void SimulateParticleIntoVoidDiffusion()
            {
                delta_mass_g = 0;
                foreach (int key in particles.Keys)
                {
                    var particle = particles[key];

                    // cell 2 cell
                    particle.SimulateCell2CellDiffusion();

                    // outer cell to void transfer
                    var outer_cell_index = particle.cells.Count - 1;
                    var delta_m_out = Cell.Kstar_the_coefficient * Cell.modelling_time_step_sec *
                        (particle.mass_g[outer_cell_index] / Cell.void_volume_mm3 - mass_between_particles_g / volume_between_particles_mm3);

                    particle.delta_mass_g[outer_cell_index] -= delta_m_out;

                    delta_mass_g += delta_m_out * particle.num_of_these_particles * particle.cells[outer_cell_index].num_of_these_cells;
                }
            }
        }

        public class Puck
        {
            Log    log;
            public bool has_errors = false;

            List<Slice> slices = new List<Slice>();

            double volume_in_cup_mm3                    = 0.0;
            double mass_in_cup_g                        = 0.0;

            double modelling_time_step_sec              = 1;
            double modelling_print_step_sec             = 2;
            double modelling_total_time_sec             = 2;
            int    num_modelling_slices_in_puck         = 10;

            double grounds_density_kg_m3                = 330;
            double water_density_kg_m3                  = 997;

            double soluble_coffee_mass_fraction         = 0.3;
            double coffee_cell_size_mm                  = 0.03;

            double bean_weight_g                        = 18.0;

            public Puck(Config config, Log log_)
            {
                log = log_;
                log.Write("");

                // load vars
                modelling_time_step_sec                 = config.GetDouble("modelling_time_step_sec");          if (!Check(modelling_time_step_sec)) return;
                modelling_print_step_sec                = config.GetDouble("modelling_print_step_sec");         if (!Check(modelling_print_step_sec)) return;
                modelling_total_time_sec                = config.GetInt("modelling_total_time_sec");            if (!Check(modelling_total_time_sec)) return;
                num_modelling_slices_in_puck            = config.GetInt("num_modelling_slices_in_puck");        if (!Check(num_modelling_slices_in_puck)) return;

                grounds_density_kg_m3                   = config.GetDouble("grounds_density_kg_m3");            if (!Check(grounds_density_kg_m3)) return;
                water_density_kg_m3                     = config.GetDouble("water_density_kg_m3");              if (!Check(water_density_kg_m3)) return;

                soluble_coffee_mass_fraction            = config.GetDouble("soluble_coffee_mass_fraction");     if (!Check(soluble_coffee_mass_fraction)) return;
                coffee_cell_size_mm                     = config.GetDouble("coffee_cell_size_mm");              if (!Check(coffee_cell_size_mm)) return;

                bean_weight_g                           = config.GetDouble("dsv2_bean_weight");                 if (!Check(bean_weight_g)) return;

                double grounds_volume_fraction          = config.GetDouble("grounds_volume_fraction");          if (!Check(grounds_volume_fraction)) return;
                double void_in_grounds_volume_fraction  = config.GetDouble("void_in_grounds_volume_fraction");  if (!Check(void_in_grounds_volume_fraction)) return;

                // load PSD
                Dictionary<int, double> psd = new Dictionary<int, double>();
                double                  psd0 = 0;
                double                  total_non_fines = 0;
                for (int i = 0; i < 100; i++) // support up to 100 layers
                {
                    if (config.HasKey("particle_size_distributution_" + i.ToString(), print_error: false))
                    {
                        var psd_value = config.GetDouble("particle_size_distributution_" + i.ToString()); if (!Check(psd_value)) return;

                        if (i == 0)
                            psd0 = psd_value;
                        else
                        {
                            psd[i] = psd_value;
                            total_non_fines += psd_value;
                        }
                    }
                }
                if(Math.Abs(total_non_fines + psd0 - 1) > 1E-3)
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
                double grounds_density_g_mm3    = grounds_density_kg_m3 * 1E-6;
                double puck_volume_mm3          = bean_weight_g / grounds_density_g_mm3;
                double soluble_mass_per_layer   = bean_weight_g * soluble_coffee_mass_fraction / num_modelling_slices_in_puck;

                // setting up static vars for Cell. Assume cells are little spheres when calculating the volume
                Cell.size_mm = coffee_cell_size_mm;
                Cell.volume_mm3 = Math.PI * coffee_cell_size_mm * coffee_cell_size_mm * coffee_cell_size_mm / 6;
                Cell.void_volume_mm3 = Cell.volume_mm3 * void_in_grounds_volume_fraction;

                // setting up layers
                for (int i = 0; i < num_modelling_slices_in_puck; i++)
                {
                    Slice slice = new Slice();

                    double slice_volume_mm3 = puck_volume_mm3 / num_modelling_slices_in_puck;
                    double particles_volume_mm3 = slice_volume_mm3 * grounds_volume_fraction;

                    slice.volume_between_particles_mm3 = slice_volume_mm3 * (1.0 - grounds_volume_fraction);
                    slice.mass_between_particles_g = soluble_mass_per_layer * psd0;
                    slice.inital_soluble_mass_per_layer = soluble_mass_per_layer;

                    double mass_in_particles_g = soluble_mass_per_layer * (1 - psd0);
                    double num_of_cells_per_layer = particles_volume_mm3 / Cell.volume_mm3;
                    double mass_per_cell = mass_in_particles_g / num_of_cells_per_layer;

                    foreach (int key in psd.Keys)
                    {
                        // careful with using PSD fractions in the next line - we take fraction of non-fines
                        double total_volume_of_these_particles_mm3 = particles_volume_mm3 * (psd[key] / total_non_fines);

                        Particle g = new Particle(key, total_volume_of_these_particles_mm3);
                        g.SetInitalMassPerCell(mass_per_cell);

                        slice.particles[key] = g;
                    }

                    slices.Add(slice);
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

                if (slices.Count == 1)
                    slices[0].Print(log, 0);
                else if (slices.Count == 2)
                {
                    slices[0].Print(log, 0);
                    slices[1].Print(log, 1);
                }
                else if (slices.Count > 2)
                {
                    slices[0].Print(log, 0);
                    slices[slices.Count / 2].Print(log, slices.Count / 2);
                    slices[slices.Count - 1].Print(log, slices.Count - 1);
                }

                var concentration = volume_in_cup_mm3 == 0 ? 0.0 : 1E6 * mass_in_cup_g / volume_in_cup_mm3;
                var ey = 100.0 * mass_in_cup_g / bean_weight_g;
                log.Write("In the cup: Volume_mL=" + (1E-3* volume_in_cup_mm3).ToString("0.0") +
                          " Mass_g=" + mass_in_cup_g.ToString("0.00") +
                          " Concentration=" + concentration.ToString("0") + 
                          " EY%=" + ey.ToString("0.0"));
            }

            public double Simulate(double k_the_coefficient, bool print_to_log = false)
            {
                double timestamp = 0.0;
                double last_print_time = 0.0;

                var fresh_water_mm3 = 1.0E3 * modelling_time_step_sec;  // 1 ml
                Cell.Kstar_the_coefficient = k_the_coefficient * Cell.void_volume_mm3;
                Cell.modelling_time_step_sec = modelling_time_step_sec;

                if(fresh_water_mm3 > slices[0].volume_between_particles_mm3 * 0.3)
                {
                    log.Write("ERROR: Please decrease the time step, fresh water takes more than 30% of the layer volume");
                    has_errors = true;
                    return 0.0;
                }

                // assume for a start that water moves at constant 1 ml/sec speed, i.e. fresh_water_ml = 1
                do
                {
                    timestamp += modelling_time_step_sec;

                    // diffusion from grains
                    foreach (Slice layer in slices)
                        layer.SimulateParticleIntoVoidDiffusion();

                    // flow of the fresh water
                    var layer_0 = slices[0];

                    var prev_delta_layer_mass_g = layer_0.mass_between_particles_g * (fresh_water_mm3 / layer_0.volume_between_particles_mm3);
                    layer_0.delta_mass_g -= prev_delta_layer_mass_g;

                    for (int i = 1; i < slices.Count; i++)
                    {
                        var layer_i = slices[i];
                        var current_delta_layer_mass_g = layer_i.mass_between_particles_g * (fresh_water_mm3 / layer_i.volume_between_particles_mm3);

                        layer_i.delta_mass_g += prev_delta_layer_mass_g;
                        layer_i.delta_mass_g -= current_delta_layer_mass_g;

                        prev_delta_layer_mass_g = current_delta_layer_mass_g;
                    }

                    // update in the cup values
                    volume_in_cup_mm3 += fresh_water_mm3;
                    mass_in_cup_g += prev_delta_layer_mass_g;
                    

                    // update mass values
                    for (int i = 0; i < slices.Count; i++)
                    {
                        var layer_i = slices[i];
                        layer_i.mass_between_particles_g += layer_i.delta_mass_g;

                        foreach(int key in layer_i.particles.Keys)
                        {
                            layer_i.particles[key].UpdateMass();
                        }
                    }

                    if ((timestamp - last_print_time + 0.001) > modelling_print_step_sec) // bump the time delta for robust interval comparison
                    {
                        if(print_to_log)
                            Print(timestamp);
                        last_print_time = timestamp;
                    }
                }
                while (timestamp < modelling_total_time_sec);

                return 0.0; // TODO
            }

            // This is the famous Zeroin from Forthyte book. Initially had it on punchcards in Fortran 
            double Zeroin(double Ax, double Bx,  // search interval
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

            double k_the_coefficient = config.GetDouble("k_the_coefficient");
            if(k_the_coefficient == double.MinValue)
            {
                Console.WriteLine("ERROR: cannot find entry k_the_coefficient in the input file");
                return;
            }

            puck.Simulate(k_the_coefficient, print_to_log: true);

            Console.WriteLine("Finished");

        }
    }
}
