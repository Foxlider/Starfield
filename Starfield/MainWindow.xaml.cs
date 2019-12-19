using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

using Color = System.Windows.Media.Color;

namespace Starfield
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Random          r          = new Random();
        private const    int             MAX_DEPTH  = 64;
        private const    int             STAR_NBR   = 200;
        private const    int             TARGET_FPS = 60;
        private readonly List<Star>      stars      = new List<Star>();
        readonly         DispatcherTimer timer      = new DispatcherTimer();
        readonly         DispatcherTimer fpsTimer2  = new DispatcherTimer();
        private          int             framesExecuted;
        private          double          halfWidth;
        private          double          halfHeight;
        private          double          kValue = 128.0;

        private readonly Stopwatch clock;

        public MainWindow()
        {
            InitializeComponent();
            InitStars();
            timer.Interval = TimeSpan.FromMilliseconds(1000.0/TARGET_FPS);
            timer.Tick     += Loop;
            timer.Start();
            fpsTimer2.Interval =  new TimeSpan(0, 0, 0, 1);
            fpsTimer2.Tick += FpsCounter2;
            fpsTimer2.Start();

            clock = new Stopwatch();
            
            //ConsoleAllocator.ShowConsoleWindow();
        }

        private void FpsCounter2(object sender, EventArgs e)
        {
            fpsCounter2.Content = $"{framesExecuted.ToString().PadRight(5)} FPS (Processing)";
            framesExecuted = 0;
        }

        /// <summary>
        /// Create the stars 
        /// </summary>
        private void InitStars()
        {
            halfWidth  = canvas.ActualWidth  / 2;
            halfHeight = canvas.ActualHeight / 2;

            for( var i = 0; i < STAR_NBR; i++ )
            {
                var star = new Star(r.Next(-2000, 2000)/100.0,
                                    r.Next(-2000, 2000)/100.0,
                                    r.Next(1,   MAX_DEPTH));

                var k  = kValue / star.z;
                var px = star.x * k + halfWidth;
                var py = star.y * k + halfHeight;

                var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
                var color = Color.FromArgb(255, (byte) shade, (byte) shade, (byte) shade);

                (Dispatcher ?? throw new InvalidOperationException()).BeginInvoke(() =>
                {
                    var brush = new SolidColorBrush(color);
                    Line tail = new Line
                    {
                        Fill            = brush,
                        X1              = px,
                        X2              = px,
                        Y1              = py,
                        Y2              = py,
                        StrokeThickness = 1
                    };

                    star.tail = tail;
                    Canvas.SetLeft(star.tail, px);
                    Canvas.SetTop(star.tail, py);
                    canvas.Children.Add(star.tail);
                }, DispatcherPriority.Send);

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
            halfWidth  = canvas.ActualWidth  / 2;
            halfHeight = canvas.ActualHeight / 2;

            foreach (var star in stars)
            { DrawStar(star); }

            framesExecuted++;
            clock.Stop();
            //Debug.WriteLine($"Frame took {clock.ElapsedMilliseconds.ToString().PadLeft(5)}ms to execute ({clock.ElapsedTicks.ToString().PadLeft(10)}ticks)");
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

            if (star.z <= 0 || (posVals.px <=0 || posVals.px >= canvas.ActualWidth) || (posVals.py <= 0 || posVals.py >= canvas.ActualHeight))
            {
                star.x = r.Next(-2000, 2000) / 100.0;
                star.y = r.Next(-2000, 2000) / 100.0;
                star.z = r.Next(MAX_DEPTH /2, MAX_DEPTH);
                oldZ = star.z;
                posVals = CalcPos(star.x, star.y, kValue / star.z);
            }

            var oldVals = CalcPos(star.x, star.y, kValue / oldZ);
            
            var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
            var color = Color.FromArgb(255, (byte) shade, (byte) shade, (byte) shade);

            star.tail.Stroke = new SolidColorBrush(color);

            star.tail.X1 = posVals.px;
            star.tail.Y1 = posVals.py;
            star.tail.X2 = oldVals.px;
            star.tail.Y2 = oldVals.py;

            Canvas.SetLeft(star.tail, 0);
            Canvas.SetTop(star.tail, 0);
            //(Dispatcher ?? throw new InvalidOperationException()).BeginInvoke(() =>
            //{
            //    star.tail.Stroke = new SolidColorBrush(color);
            //    star.tail.X1     = posVals.px;
            //    star.tail.Y1     = posVals.py;
            //    star.tail.X2     = oldVals.px;
            //    star.tail.Y2     = oldVals.py;

            //    Canvas.SetLeft(star.tail, 0);
            //    Canvas.SetTop(star.tail, 0);
            //},DispatcherPriority.Send);
        }

        private (double px, double py) CalcPos(double origX, double origY, double k)
        {
            var px = origX * k + halfWidth;
            var py = origY * k + halfHeight;

            return (px, py);
        }

        /// <summary>
        /// Speed changer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        { kValue = e.NewValue; }
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

        /// <summary>
        /// Star and its tail represented by a line
        /// </summary>
        internal Line tail;

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
