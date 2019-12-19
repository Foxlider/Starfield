using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarfieldWinForms
{
    public partial class Form1 : Form
    {
        private readonly Random r = new Random();
        private const int MAX_DEPTH = 64;
        private const int STAR_NBR = 200;
        private const int TARGET_FPS = 60;
        private readonly List<Star> stars = new List<Star>();
        readonly Timer timer = new Timer();
        readonly Timer fpsTimer = new Timer();
        readonly Timer fpsTimer2 = new Timer();
        private int framesExecuted;
        private int frameCount;
        private double halfWidth;
        private double halfHeight;
        private double kValue = 128.0;
        private double offsetH;
        private double offsetV;

        private Graphics canvas;

        private readonly Stopwatch clock;
        private readonly Stopwatch watch;

        readonly Timer offsetterDispatcher = new Timer();
        private readonly Stopwatch offsetterWatch;
        public Form1()
        {
            InitializeComponent();
            canvas = CreateGraphics();
            this.BackColor = Color.Black;

            InitStars();
            timer.Interval = (int)TimeSpan.FromMilliseconds(1000.0 / TARGET_FPS).TotalMilliseconds;
            timer.Tick += Loop;
            timer.Start();
            fpsTimer.Tick += FpsCounter;
            fpsTimer.Start();
            fpsTimer2.Interval = (int)new TimeSpan(0, 0, 0, 1).TotalMilliseconds;
            fpsTimer2.Tick += FpsCounter2;
            fpsTimer2.Start();

            offsetterDispatcher.Interval = (int)TimeSpan.FromMilliseconds(1).TotalMilliseconds;
            offsetterDispatcher.Tick += OffsetterDispatcher_Tick;
            offsetterWatch = new Stopwatch();
            offsetterWatch.Start();
            offsetterDispatcher.Start();

            clock = new Stopwatch();
            watch = new Stopwatch();
            watch.Start();
        }

        private int oldH;
        private int oldV;
        private int targetH;
        private int targetV;
        private void OffsetterDispatcher_Tick(object sender, EventArgs e)
        {
            if (offsetH == targetH || offsetV == targetV)
            {
                oldH = (int)offsetH;
                oldV = (int)offsetV;
                targetH = r.Next(-2000, 2000);
                targetV = r.Next(-2000, 2000);
                offsetterWatch.Restart();
            }
            //sliderHorOffset.Value += (targetH - oldH) / 100;
            //sliderVerOffset.Value += (targetV - oldV) / 100;
        }

        private void FpsCounter(object sender, EventArgs e)
        {
            frameCount++;
            if (watch.ElapsedMilliseconds <= 1000) return;
            watch.Restart();
            fpsCounter.Text = $"{frameCount.ToString().PadRight(5)} FPS (Render)";
            frameCount = 0;
        }

        private void FpsCounter2(object sender, EventArgs e)
        {
            fpsCounter2.Text = $"{framesExecuted.ToString().PadRight(5)} FPS (Processing)";
            framesExecuted = 0;
        }

        /// <summary>
        /// Create the stars 
        /// </summary>
        private void InitStars()
        {
            halfWidth = this.Width / 2;
            halfHeight = this.Height / 2;

            for (var i = 0; i < STAR_NBR; i++)
            {
                var star = new Star(X: r.Next(-2000, 2000) / 100.0,
                                    Y: r.Next(-2000, 2000) / 100.0,
                                    Z: r.Next(1, MAX_DEPTH));

                var k = kValue / star.z;
                var px = (star.x * k) + halfWidth;
                var py = (star.y * k) + halfHeight;

                var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
                var color = Color.FromArgb(255, (byte)shade, (byte)shade, (byte)shade);

                //Pen pen = new Pen(Color.White,5);
                //canvas.DrawLine(pen, (float)px, (float)py, (float)px, (float)py);

                //var brush = new SolidColorBrush(color);
                //Line tail = new Line
                //{
                //    Fill = brush,
                //    X1 = px,
                //    X2 = px,
                //    Y1 = py,
                //    Y2 = py,
                //    StrokeThickness = 1
                //};

                //star.tail = tail;
                //Canvas.SetLeft(star.tail, px);
                //Canvas.SetTop(star.tail, py);
                //canvas.Children.Add(star.tail);

                stars.Add(star);
            }
        }

        /// <summary>
        /// Process loop calculating each frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Loop(object sender, EventArgs e)
        {
            clock.Restart();
            halfWidth = this.Width / 2;
            halfHeight = this.Height / 2;
            canvas.Clear(Color.Black);
            foreach (var star in stars)
            { DrawStar(star); }

            framesExecuted++;
            clock.Stop();
            Debug.WriteLine($"Frame took {clock.ElapsedMilliseconds.ToString().PadLeft(5)}ms to execute ({clock.ElapsedTicks.ToString().PadLeft(10)}ticks)");
        }

        /// <summary>
        /// Draws a star on the 2d canvas
        /// </summary>
        /// <param name="obj">Star to draw as object</param>
        private void DrawStar(object obj)
        {
            if (!(obj is Star star)) return;
            var oldZ = star.z;
            star.z -= (512 - kValue) / 100;

            var posVals = CalcPos(star.x, star.y, kValue / star.z);

            if (star.z <= 0 || (posVals.px <= 0 || posVals.px >= this.Width) || (posVals.py <= 0 || posVals.py >= this.Height))
            {
                do
                {
                    star.x = r.Next(-2000, 2000) / 100.0;
                    star.y = r.Next(-2000, 2000) / 100.0;
                    star.z = r.Next(MAX_DEPTH / 2, MAX_DEPTH);
                }
                while (Math.Abs(star.x) < 0.0 || Math.Abs(star.y) < 0.0);
                oldZ = star.z;
                posVals = CalcPos(star.x, star.y, kValue / star.z);
            }

            var oldVals = CalcPos(star.x, star.y, kValue / oldZ);

            var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
            var color = Color.FromArgb(255, (byte)shade, (byte)shade, (byte)shade);


            Pen pen = new Pen(color, 1);
            canvas.DrawLine(pen, (float)posVals.px, (float)posVals.py, (float)oldVals.px, (float)oldVals.py);
        }

        private (double px, double py) CalcPos(double origX, double origY, double k)
        {
            var px = origX * k + halfWidth + offsetH / 10;
            var py = origY * k + halfHeight + offsetV / 10;

            return (px, py);
        }
        
        /// <summary>
        /// Speed changer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, System.EventArgs e)
        {  kValue = trackBar1.Value; }
    }


    /// <summary>
    /// Star class
    /// </summary>
    internal class Star
    {
        /// <summary>
        /// X coordinates of the star
        /// </summary>
        internal double x;

        /// <summary>
        /// Y coordinates of the star
        /// </summary>
        internal double y;

        /// <summary>
        /// Z coordinates of the star : Depth of the star in the 3d environment
        /// </summary>
        /// <remarks>
        /// Used to calculate the position of the star each frame and the length of its tail
        /// </remarks>
        internal double z;

        ///// <summary>
        ///// Star and its tail represented by a line
        ///// </summary>
        //internal Line tail;

        /// <summary>
        /// Creates a new Star
        /// </summary>
        /// <param name="X">X coordinates of the star at the start</param>
        /// <param name="Y">Y coordinates of the star at the start</param>
        /// <param name="Z">Z coordinates of the star => its current depth</param>
        public Star(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }
}
