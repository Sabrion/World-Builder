using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace World_Builder
{
    public partial class Form1 : Form
    {
        private readonly Random d = new Random(); // For all your randomizer purposes.
        private readonly List<Bitmap> bip = new List<Bitmap>(); // For Undo/Redo functions
        private int bipDex = 0; // For keeping track of where we are in the Undo/Redo
        readonly Bitmap b = new Bitmap(@"C:\Users\[Username]\Documents\Biome.bmp"); //Direct link to biome image 20x20 px image map
        // Bitmap b = new Bitmap(20, 20); // Old b used with the graphics functions in the main startup.

        public Form1()
        {
            InitializeComponent();
            bip.Add(new Bitmap(600, 600)); // The first basic bip
            Graphics.FromImage(bip[0]).Clear(Color.Black); // See a blank map, paint it black.
            pbTop.Image = bip[0]; // Put the blank map on the form.
            /* using (Graphics g = Graphics.FromImage(b)) // Draws the biome map, now I just load a bmp for that.
            {
                g.FillRectangle(new SolidBrush(Color.White),         0,  0, 20,  4); // Ice Desert
                g.FillRectangle(new SolidBrush(Color.Thistle),       0,  4,  4,  4); // Tundra
                g.FillRectangle(new SolidBrush(Color.SpringGreen),   4,  4,  6,  4); // Taiga
                g.FillRectangle(new SolidBrush(Color.Olive),        10,  4, 10,  9); // Swampland
                g.FillRectangle(new SolidBrush(Color.Goldenrod),     0,  8,  4, 10); // Savanna
                g.FillRectangle(new SolidBrush(Color.GreenYellow),   4,  8,  3, 10); // Shrubland
                g.FillRectangle(new SolidBrush(Color.Green),         7,  8,  3, 10); // Forest
                g.FillRectangle(new SolidBrush(Color.ForestGreen),  10, 13,  8,  6); // Seasonal Forest
                g.FillRectangle(new SolidBrush(Color.DarkGreen),    18, 13,  2,  6); // Rain Forest
                g.FillRectangle(new SolidBrush(Color.PeachPuff),     0, 19,  4,  1); // Desert
                g.FillRectangle(new SolidBrush(Color.LawnGreen),     4, 19,  6,  1); // Plains
                
            }*/
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            DateTime n = DateTime.Now; // This is mostly used for timing how long an operation takes
            this.Text = "World Builder: Working!"; // Let the user know we're doing things.
            switch (e.Modifiers.ToString() + e.KeyCode.ToString()) // Detect Keypress
            {
                case "Control, AltBack": Undo(true); break; // Undo all.
                case "NoneF2": bip.Add(new Bitmap(bip[bipDex])); bipDex++; Geometry(new SolidBrush(Color.White)); bip.RemoveRange(bipDex + 1, bip.Count() - bipDex - 1); break; // Adds a landmass mask
                case "NoneF3": bip.Add(new Bitmap(bip[bipDex])); bipDex++; Geometry(new SolidBrush(Color.Black)); break; // Adds water mask
                case "NoneF4": bip.Add(new Bitmap(bip[bipDex])); bipDex++; VaryAll(); break; // Varies each pixel by +/- 20/10/5/0 to each of R, G, & B (may not alter them by the same values)
                case "NoneF5": bip.Add(new Bitmap(bip[bipDex])); bipDex++; NoiseAll(); break; // Applies random noise to each pixel random R, G, and B for land (white) pixels, and random B for water (black) pixels.
                case "NoneF6": bip.Add(new Bitmap(bip[bipDex])); bipDex++; AverageAll(); break; // Applies an average value to all pixels based on their surrounding pixels.
                case "NoneF7": bip.Add(new Bitmap(bip[bipDex])); bipDex++; ConvertAll(); break; // Converts the noisy data into biome colors.
                case "NoneF8": bip.Add(new Bitmap(bip[bipDex])); bipDex++; Voronoi(VoronoiSeed()); break; // Generates a Voronoi tile map using random colors.
                case "NoneF9": bip.Add(new Bitmap(bip[bipDex])); bipDex++; Voronoi(VoronoiSeed(false)); break; // Generates a Voronoi tile map of land masses (white) and water tiles (black)
                case "NoneF11": bip.Add(new Bitmap(bip[bipDex])); bipDex++; TotalCreation(); break; // Voronoi(Seed(False)), OSimplex, VaryAll x4, AvergeAll x4, ConvertAll automated process
                case "NoneTab": bip.Add(new Bitmap(bip[bipDex])); bipDex++; OSimplexAll(); break; // Applies u/KdotJRPG's OpenSimplex Noise (2D version) to the map
                case "NoneReturn": Clipboard.SetImage(bip[bipDex]); break; // Quick map screenshot
                case "ControlY": Redo(); break; // Redo function, not fully functional yet. TODO: I guess. XD
                case "ControlZ": Undo(); break; // Basic undo function, not fully functional yet, also TODO.
                case "NoneD5": bip.Add(new Bitmap(bip[bipDex])); bipDex++; bip[bipDex] = b; break; // Display the biome map image, kinda breaks map generation, even with the current Undo/Redo.
            }
            pbTop.Image = bip[bipDex]; // Show the resulting map.
            this.Text = e.Modifiers.ToString() + e.KeyCode.ToString() + " Complete! " + (DateTime.Now - n).ToString(); // Alert the user that the process is complete, and show how long it took.
        }

        #region Other Code; // This is so I can just quick squish my code and jump to the noise sections when I was working on it.

        private void AverageAll() // You'll see several of these all functions, these are just to iterate through each pixel and apply the associated function.
        {
            for (int i = 20; i < bip[bipDex].Width - 20; i++) for (int j = 20; j < bip[bipDex].Height - 20; j++)
                {
                    bip[bipDex].SetPixel(i, j, Averager(new Point(i, j)));
                }
        }

        private Color Averager(Point p) // Replaces the selected pixel with the average value of the selected pixel and its surrounding pixels.
        {
            List<Point> q = new List<Point>();
            for (int i = -1; i < 2; i++) for(int j = -1; j < 2; j++) // Generate the list of points to average together.
                {
                    q.Add(new Point(p.X + i, p.Y + j)); // There's probably an easier/faster/more optimal way of doing this, this was just what I thought of first.
                }
            double r = 0.0; // Set the starting values for R/G/B
            double g = 0.0;
            double b = 0.0;
            for (int i = 0; i < 9; i++) // Sum the values
            {
                        r += bip[bipDex].GetPixel(q[i].X, q[i].Y).R;
                        g += bip[bipDex].GetPixel(q[i].X, q[i].Y).G;
                        b += bip[bipDex].GetPixel(q[i].X, q[i].Y).B;
            }
            return Color.FromArgb((int)r / 9, (int)g / 9, (int)b / 9); // Return the average
        }

        private void ConvertAll() // Another iterator
        {
            for (int i = 0; i < bip[bipDex].Width; i++) for (int j = 0; j < bip[bipDex].Height; j++)
                {
                    Converter(new Point(i, j));
                }
        }

        private void Converter(Point l) // Converts the noise data into biome colors TODO: two-tone map version
        {
            Color c = bip[bipDex].GetPixel(l.X, l.Y); // Get the map noise data, saved as 0-255 in RGB
            double t = (c.G / 255.0 + (299.5 - Math.Abs(299.5 - l.Y)) / 300) / 2.0; // Set the Temperature value
            double p = t * c.B / 255.0; // Set the precipitation value;
            if (c.R > 230) bip[bipDex].SetPixel(l.X, l.Y, Color.White); // Check to see if we're at the peak of a mountain.
            else if (c.R > 190) bip[bipDex].SetPixel(l.X, l.Y, b.GetPixel(Math.Min((int)(p * 20), 20), Math.Min((int)(t * 20), 20))); // Check for land biome
            else if (c.R > 169) bip[bipDex].SetPixel(l.X, l.Y, Color.PeachPuff); // Check for beach
            else bip[bipDex].SetPixel(l.X, l.Y, Color.FromArgb(0,0,(int)(c.B / 4.0 + 127))); // Otherwise water.
        }

        private double Dist(Tuple<int, int, int> a, Tuple<int, int, int> b) // Distance function for Voronoi
        {
            return Math.Sqrt(Math.Pow((double) (a.Item1 - b.Item1), 2) + Math.Pow((double) (a.Item2 - b.Item2), 2) + Math.Pow((double)(a.Item3 - b.Item3), 2));
        }

        private void Geometry(Brush b) // Basic geometry function
        {
            PointF q = new PointF(d.Next(40, bip[bipDex].Width - 40), d.Next(40, bip[bipDex].Height - 40)); // Basic Starting point
            PointF r = new PointF(Math.Min(Math.Max(q.X + d.Next(-60, 60), 20), bip[bipDex].Width - 40), Math.Min(Math.Max(q.Y + d.Next(-60, 60), 20), bip[bipDex].Height - 40)); // Chained points for closed curves
            PointF s = new PointF(Math.Min(Math.Max(r.X + d.Next(-60, 60), 20), bip[bipDex].Width - 40), Math.Min(Math.Max(r.Y + d.Next(-60, 60), 20), bip[bipDex].Height - 40)); // The limits keep the chains reasonable
            PointF[] p = new PointF[3] { q, r, s};

            using (Graphics g = Graphics.FromImage(bip[bipDex]))
            {
                switch (d.Next(1, 100)) // Random chance for...
                {
                    case int n when n < 76: // Closed curves
                        for (int i = d.Next(3, 8); i > 0; i--) // Creates a chain of closed curves (1-6)
                        {
                            g.FillClosedCurve(b, p); // Draw the first closed curve
                            q = new PointF(r.X, r.Y); // Generate the next set of points in the chain by dumping the last two points into the first two points and then generating a new point
                            r = new PointF(s.X, s.Y);
                            s = new PointF(Math.Min(Math.Max(r.X + d.Next(-60, 60), 20), bip[bipDex].Width - 40), Math.Min(Math.Max(r.Y + d.Next(-60, 60), 20), bip[bipDex].Height - 40));
                            p = new PointF[3] { q, r, s };
                        }
                        break;
                    default:
                        g.FillEllipse(b, new Rectangle((int)p[0].X, (int)p[0].Y, d.Next(20,60), d.Next(20, 60))); // Draw a circle at the first point
                        break;
                        // TODO: More shapes, maybe hex- and pentagons if there's quick code for it.
                }
            }

        }

        private void NoiseAll() // Iterator
        {
            for (int i = 20; i < bip[bipDex].Width - 20; i++) for (int j = 20; j < bip[bipDex].Height - 20; j++)
                {
                    bip[bipDex].SetPixel(i, j, Noisy(new Point(i, j)));
                }
        }

        private Color Noisy(Point p) // Generates random values with some real basic parameters
        {
            int r = bip[bipDex].GetPixel(p.X, p.Y).R; // Get the current RGB values of the pixel
            int g = bip[bipDex].GetPixel(p.X, p.Y).G;
            int b = bip[bipDex].GetPixel(p.X, p.Y).B;
            switch (bip[bipDex].GetPixel(p.X, p.Y).ToArgb())
            {
                case int n when n == Color.White.ToArgb(): // If the pixel is part of a land tile
                    r = d.Next(0, 255);
                    g = d.Next(0, 255);
                    b = d.Next(0, 255);
                    break;
                case int n when n == Color.Black.ToArgb(): // if the pixel is part of sea tile.
                    r = 0;
                    g = 0;
                    b = d.Next(0, 255);
                    break;
                default: // In this case the pixel has already been randomlized... likely.
                    break;
            }
            return Color.FromArgb(r, g, b);
        }

        /*private void RandomAll() // This performs a similar function to NoiseAll above
        {
            for (int i = 20; i < bip[bipDex].Width - 20; i++) for (int j = 20; j < bip[bipDex].Height - 20; j++)
                {
                    bip[bipDex].SetPixel(i, j, Randomize(new Point(i, j)));
                }
        }

        private Color Randomize(Point p) // This performs a similar function to Noisy above
        {
            int r = bip[bipDex].GetPixel(p.X, p.Y).R;
            int g = bip[bipDex].GetPixel(p.X, p.Y).G;
            int b = bip[bipDex].GetPixel(p.X, p.Y).B;
            for (int i = 0; i < 3; i++)
            {
                switch(r)
                {
                    case int n when n < 170:
                        g = d.Next(0, 255);
                        b = d.Next(0, 255);
                        break;
                    default:
                        g = d.Next(0, 255);
                        b = d.Next(0, 255);
                        break;
                }
            }
            r = Math.Min(Math.Max(r, 0), 255);
            g = Math.Min(Math.Max(g, 0), 255);
            b = Math.Min(Math.Max(b, 0), 255);
            return Color.FromArgb(r, g, b);
        }*/
        
        private void Redo() // Partially functional redo/undo system
        {
            if (bipDex < bip.Count() - 1) bipDex++;
        }

        private void Undo(Boolean t = false) // Partially functional redo/undo system, if t = true, then it removes all the bips except for the first (black) one.
        {
            if(t)
            {
                switch (MessageBox.Show("Are you sure you want to start over? This gets rid of all of your maps so far!", "Start over from blank water?", MessageBoxButtons.YesNo))
                {
                    case DialogResult.Yes:
                        bip.RemoveRange(1, bip.Count() - 1);
                        bipDex = 0;
                        break;
                    default:
                        break;
                }
            }
            else if (bipDex > 0) bipDex--;
        }

        private void VaryAll() // Iterator
        {
            for (int i = 20; i < bip[bipDex].Width - 20; i++) for (int j = 20; j < bip[bipDex].Height - 20; j++)
                {
                    bip[bipDex].SetPixel(i, j, Variate(new Point(i, j)));
                }
        }

        private Color Variate(Point p) // Varies the individual RGBs of a pixel by set values at random.
        {
            int r = bip[bipDex].GetPixel(p.X, p.Y).R; // Get the original RGBs
            int g = bip[bipDex].GetPixel(p.X, p.Y).G;
            int b = bip[bipDex].GetPixel(p.X, p.Y).B;
            int m;
            for (int i = 1; i < 3; i++) // Cycle through RGB
            {
                switch (d.Next(1, 100)) // Pick the modification amount and direction
                {
                    case int n when n < 51:
                        m = 0;
                        break;
                    case int n when n < 66:
                        m = 5;
                        break;
                    case int n when n < 81:
                        m = -5;
                        break;
                    case int n when n < 88:
                        m = 10;
                        break;
                    case int n when n < 95:
                        m = -10;
                        break;
                    case int n when n < 98:
                        m = 20;
                        break;
                    default:
                        m = -20;
                        break;
                }
                switch (i) // Apply the modification to the appropriate value
                {
                    case 0: r += m; break;
                    case 1: g += m; break;
                    case 2: b += m; break;
                }
            }
            r = Math.Min(Math.Max(r, 0), 255); // Normalize the values
            g = Math.Min(Math.Max(g, 0), 255);
            b = Math.Min(Math.Max(b, 0), 255);
            return Color.FromArgb(r, g, b);
        }

        private void Voronoi(Tuple<int,int,int>[] p) // Does the Voronoi designation of tiles using the points presented
        {
            for (int i = 0; i < bip[bipDex].Width; i++) // Iterate through the pixels
            {
                for (int j = 0; j < bip[bipDex].Height; j++)
                {
                    double s = bip[bipDex].Height * bip[bipDex].Width * 255; // The Maximum possible distance
                    int t = 0; // Tracker for the closest point in the list
                    for (int q = 0; q < p.Count(); q++) // Iterate through each point in the list
                    {
                        if (s > Dist(new Tuple<int, int, int>(i, j, 128), (Tuple<int, int, int>)p[q])) // If the distance between the pixel and the point in the list is less than the current champ
                        {
                            s = Dist(new Tuple<int, int, int>(i, j, 128), p[q]); // Set the new shortest distance, Notice that the pixels themselves are at the middle height ~128, but the Voronoi points are at random heights
                            t = q; // Track the closest point's index
                        }
                    }
                    bip[bipDex].SetPixel(i, j, bip[bipDex].GetPixel(p[t].Item1, p[t].Item2)); // Set this pixel to the same color as the closest Voronoi point
                }
            }
        }

        private Tuple<int,int,int>[] VoronoiSeed(Boolean t = true) // This generates the list of points used in the Voronoi function above.
        {
            List<Tuple<int,int,int>> lSeed = new List<Tuple<int, int, int>>(); // Our list of points.
            for (int i = 0; i < bip[bipDex].Width; i += 20) for(int j = 0; j < bip[bipDex].Height; j += 20) // Choose a grid point every 20 pixels vertically and horizontally
            {
                lSeed.Add(new Tuple<int, int, int>(d.Next(i, i + 20), d.Next(j, j + 20), d.Next(20, (bip[bipDex].Height + bip[bipDex].Width) / 2))); // Add a random point within a 20x20 from the grid point, the Z property is random so that the Voronoi tiles are a little more complicated (I hope), and therefore generate more varied tiles (again, I hope).
                    Color c; // Set the color of the seed point to a random or set color based on some rules.
                    int x = lSeed.Last().Item1;
                    int y = lSeed.Last().Item2;
                    int w = bip[bipDex].Width - 60;
                    int h = bip[bipDex].Height - 60;
                    if (x < 39 || y < 39 || x > w || y > h) c = Color.Black; // Sets the color to a water tile around the border. I hate land on the edges of a map.
                    else if (t) c = Color.FromArgb(d.Next(0, 1) * 70 + 120, d.Next(0, 256) % 255, d.Next(0, 256) % 255); // Random colored land tile
                    else if (d.NextDouble() * 2.0 % 2 >= 1.4) c = Color.White; // Blank land tile (white) for my proper application of noise later
                    else c = Color.Black; // Also sets it to a water tile if it's not set to land
                bip[bipDex].SetPixel(lSeed[lSeed.Count() - 1].Item1, lSeed[lSeed.Count() - 1].Item2, c); // Sets the color
            }
            return lSeed.ToArray(); // Returns the Voronoi points/seeds
        }

       private void TotalCreation() // Automated total map making
        {
            DateTime n = DateTime.Now; // Time tracking for each element and the entire process
            DateTime s = DateTime.Now;
            this.Text = "Voronoi " + n; // Announces each step in the process, performs it, saves a copy of the work, and then proceeds
            Voronoi(VoronoiSeed(false));
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Simplex Noise " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            OSimplexAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Variation Pass 1 " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            VaryAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Variation Pass 2 " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            VaryAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Variation Pass 3 " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            VaryAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Variation Pass 4 " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            VaryAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Averaging Pass 1 " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            AverageAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Averaging Pass 2 " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            AverageAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Averaging Pass 3 " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            AverageAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Averaging Pass 4 " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            AverageAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Conversion " + (DateTime.Now - n) + "/" + (DateTime.Now - s);
            n = DateTime.Now;
            ConvertAll();
            bip[bipDex].Save(@"C:\Users\Jack\Desktop\" + this.Text.Split(' ')[0] + ".png", System.Drawing.Imaging.ImageFormat.Png);
            this.Text = "Done! " + (DateTime.Now - n) + "/" + (DateTime.Now - s); // Announces completion with the total time to complete, maybe superfluous with the KeyPress event changes.
        }

        private void LbW_Click(object sender, EventArgs e) // These click events are to set map defaults, these are not fully implemented yet.
        {
            tbW.Text = "600";
        }

        private void LbH_Click(object sender, EventArgs e)
        {
            tbH.Text = "600";
        }

        private void LbZ_Click(object sender, EventArgs e)
        {
            tbZ.Text = "1";
        }

        #endregion ;

        #region Noise implementation // For the borrowed Simplex Noise from u/KdotJRPG, THANKS AGAIN!

        public void OSimplexAll() // This is the main applying function, which is part iterator as well. I wrote this bit, the stol... er.. borrowed parts are later.
        {
            // I'm not really sure on what all effect slide and ColorScale really have on the map generation, as I haven't played around with it much.
            double colorScale = 1.0; // For adjusting the applied color values like slide below except as an exponent
            double scaleR = 0.1, scaleG = 0.02, scaleB = 0.02; // This makes the application of the noise patterns bigger/smaller so that the biome data doesn't match perfectly with height map and other data
                                                                // tl;dr: it makes the colors messy and pretty and different.
            double slide = 2.0; // This is for some position correction, so to speak, adjusts RGBs by a static value by addition.
            OpenSimplexNoise osn = new OpenSimplexNoise(), oso = new OpenSimplexNoise(d.Next(-2147483648, 2147483647) << 32 + d.Next(-2147483648, 2147483647)), osp = new OpenSimplexNoise(d.Next(-2147483648, 2147483647) << 32 + d.Next(-2147483648, 2147483647)); // Three different noise generators on three different seeds to make it as random as possible....

            for (int i = 0; i < bip[bipDex].Width; i++) for (int j = 0; j < bip[bipDex].Width; j++) // Iterator
                {
                    int r, g, b; // RGBs to be applied later
                    switch (bip[bipDex].GetPixel(i,j).ToArgb()) // Pick the color of the pixel to be modified
                    {
                        case int n when n == Color.White.ToArgb(): // If it's part of a white land tile randomize it for better land biome values
                            r = Math.Max(Math.Min((int)(Math.Pow(osn.Evaluate(i * scaleR, j * scaleR) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 90 + 165), 255), 0);
                            g = Math.Max(Math.Min((int)(Math.Pow(oso.Evaluate(i * scaleG, j * scaleG) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 255), 255), 0);
                            b = Math.Max(Math.Min((int)(Math.Pow(osp.Evaluate(i * scaleB, j * scaleB) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 255), 255), 0);
                            break;
                        case int n when n == Color.Black.ToArgb(): // If it's part of a black water tile, randomize it for better water values
                            r = Math.Max(Math.Min((int)(Math.Pow(osn.Evaluate(i * scaleR, j * scaleR) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 180), 255), 0);
                            g = Math.Max(Math.Min((int)(Math.Pow(oso.Evaluate(i * scaleG, j * scaleG) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 255), 255), 0);
                            b = Math.Max(Math.Min((int)(Math.Pow(osp.Evaluate(i * scaleB, j * scaleB) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 255), 255), 0);
                            break;
                        default: // if neither, just randomize it entirely!
                            r = Math.Max(Math.Min((int)(Math.Pow(osn.Evaluate(i * scaleR, j * scaleR) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 255), 255), 0);
                            g = Math.Max(Math.Min((int)(Math.Pow(oso.Evaluate(i * scaleG, j * scaleG) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 255), 255), 0);
                            b = Math.Max(Math.Min((int)(Math.Pow(osp.Evaluate(i * scaleB, j * scaleB) + slide, colorScale) / Math.Pow(slide + 1, colorScale) * 255), 255), 0);
                            break;
                    }
                    bip[bipDex].SetPixel(i, j, Color.FromArgb(r, g, b)); // set the color
                }
        }
    }

    public class OpenSimplexNoise // THANK YOU u/KdotJRPG! This is only the 2D portion of their great OpenSimplex work!
        {                                                           // This is where the heavy lifting is done. I left in u/KdotJRPG 's comments.
            private const double STRETCH = -0.211324865405187;    //(1/Math.sqrt(2+1)-1)/2;
            private const double SQUISH = 0.366025403784439;      //(Math.sqrt(2+1)-1)/2;
            private const double NORM = 1.0 / 47.0;

            private readonly byte[] perm;
            private readonly byte[] perm2D;

            private static readonly double[] gradients2D = new double[]
            {
             5,  2,    2,  5,
            -5,  2,   -2,  5,
             5, -2,    2, -5,
            -5, -2,   -2, -5,
            };

            private static readonly Contribution2[] lookup2D;
            
            static OpenSimplexNoise()
            {
                var base2D = new int[][]
                {
                new int[] { 1, 1, 0, 1, 0, 1, 0, 0, 0 },
                new int[] { 1, 1, 0, 1, 0, 1, 2, 1, 1 }
                };
                var p2D = new int[] { 0, 0, 1, -1, 0, 0, -1, 1, 0, 2, 1, 1, 1, 2, 2, 0, 1, 2, 0, 2, 1, 0, 0, 0 };
                var lookupPairs2D = new int[] { 0, 1, 1, 0, 4, 1, 17, 0, 20, 2, 21, 2, 22, 5, 23, 5, 26, 4, 39, 3, 42, 4, 43, 3 };

                var contributions2D = new Contribution2[p2D.Length / 4];
                for (int i = 0; i < p2D.Length; i += 4)
                {
                    var baseSet = base2D[p2D[i]];
                    Contribution2 previous = null, current = null;
                    for (int k = 0; k < baseSet.Length; k += 3)
                    {
                        current = new Contribution2(baseSet[k], baseSet[k + 1], baseSet[k + 2]);
                        if (previous == null)
                        {
                            contributions2D[i / 4] = current;
                        }
                        else
                        {
                            previous.Next = current;
                        }
                        previous = current;
                    }
                    current.Next = new Contribution2(p2D[i + 1], p2D[i + 2], p2D[i + 3]);
                }

                lookup2D = new Contribution2[64];
                for (var i = 0; i < lookupPairs2D.Length; i += 2)
                {
                    lookup2D[lookupPairs2D[i]] = contributions2D[lookupPairs2D[i + 1]];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int FastFloor(double x)
            {
                var xi = (int)x;
                return x < xi ? xi - 1 : xi;
            }

            public OpenSimplexNoise() : this(DateTime.Now.Ticks) { }

            public OpenSimplexNoise(long seed)
            {
                perm = new byte[256];
                perm2D = new byte[256];
                var source = new byte[256];
                for (int i = 0; i < 256; i++)
                {
                    source[i] = (byte)i;
                }
                seed = seed * 6364136223846793005L + 1442695040888963407L;
                seed = seed * 6364136223846793005L + 1442695040888963407L;
                seed = seed * 6364136223846793005L + 1442695040888963407L;
                for (int i = 255; i >= 0; i--)
                {
                    seed = seed * 6364136223846793005L + 1442695040888963407L;
                    int r = (int)((seed + 31) % (i + 1));
                    if (r < 0)
                    {
                        r += (i + 1);
                    }
                    perm[i] = source[r];
                    perm2D[i] = (byte)(perm[i] & 0x0E);
                    source[r] = source[i];
                }
            }

            public double Evaluate(double x, double y)
            {
                var stretchOffset = (x + y) * STRETCH;
                var xs = x + stretchOffset;
                var ys = y + stretchOffset;

                var xsb = FastFloor(xs);
                var ysb = FastFloor(ys);

                var squishOffset = (xsb + ysb) * SQUISH;
                var dx0 = x - (xsb + squishOffset);
                var dy0 = y - (ysb + squishOffset);

                var xins = xs - xsb;
                var yins = ys - ysb;

                var inSum = xins + yins;

                var hash =
                   (int)(xins - yins + 1) |
                   (int)(inSum) << 1 |
                   (int)(inSum + yins) << 2 |
                   (int)(inSum + xins) << 4;

                var c = lookup2D[hash];

                var value = 0.0;
                while (c != null)
                {
                    var dx = dx0 + c.dx;
                    var dy = dy0 + c.dy;
                    var attn = 2 - dx * dx - dy * dy;
                    if (attn > 0)
                    {
                        var px = xsb + c.xsb;
                        var py = ysb + c.ysb;

                        var i = perm2D[(perm[px & 0xFF] + py) & 0xFF];
                        var valuePart = gradients2D[i] * dx + gradients2D[i + 1] * dy;

                        attn *= attn;
                        value += attn * attn * valuePart;
                    }
                    c = c.Next;
                }
                return value * NORM;
            }

            private class Contribution2
            {
                public double dx, dy;
                public int xsb, ysb;
                public Contribution2 Next;

                public Contribution2(double multiplier, int xsb, int ysb)
                {
                    dx = -xsb - multiplier * SQUISH;
                    dy = -ysb - multiplier * SQUISH;
                    this.xsb = xsb;
                    this.ysb = ysb;
                }
            }
        }
        #endregion
}
