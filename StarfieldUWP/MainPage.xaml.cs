using System;
using System.Collections.Generic;
using System.Diagnostics;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace StarfieldUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private readonly Random          r          = new Random();
        private const    int             MAX_DEPTH  = 64;
        private const    int             STAR_NBR   = 200;
        private const    int             TARGET_FPS = 60;
        private readonly List<Star>      stars      = new List<Star>();
        readonly         DispatcherTimer timer      = new DispatcherTimer();
        readonly         DispatcherTimer fpsTimer   = new DispatcherTimer();
        readonly         DispatcherTimer fpsTimer2  = new DispatcherTimer();
        private          int             framesExecuted;
        private          int             frameCount;
        private          double          halfWidth;
        private          double          halfHeight;
        private          double          kValue = 128.0;

        private readonly Stopwatch clock;
        private readonly Stopwatch watch;

        public MainPage()
        {
            this.InitializeComponent();
            Dispatcher.CurrentPriority = Windows.UI.Core.CoreDispatcherPriority.High;
            InitStars();
            timer.Interval =  TimeSpan.FromMilliseconds(1000.0 /TARGET_FPS);
            timer.Tick     += Loop;
            timer.Start();
            fpsTimer.Tick += FpsCounter;
            fpsTimer.Start();
            fpsTimer2.Interval =  new TimeSpan(0, 0, 0, 1);
            fpsTimer2.Tick     += FpsCounter2;
            fpsTimer2.Start();
            clock = new Stopwatch();
            watch = new Stopwatch();
            watch.Start();
        }

        private void FpsCounter(object sender, object e)
        {
            frameCount++;
            if (watch.ElapsedMilliseconds <= 1000) return;
            watch.Restart();
            fpsCounter.Text = $"{frameCount.ToString().PadRight(5)} FPS (Render)";
            frameCount = 0;
        }

        private void FpsCounter2(object sender, object e)
        {
            fpsCounter2.Text = $"{framesExecuted.ToString().PadRight(5)} FPS (Processing)";
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
                var star = new Star(X: r.Next(-2000, 2000)/100.0,
                                    Y: r.Next(-2000, 2000)/100.0,
                                    Z: r.Next(1,   MAX_DEPTH));

                var k  = kValue / star.z;
                var px = star.x * k + halfWidth;
                var py = star.y * k + halfHeight;

                var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
                var color = Color.FromArgb(255, (byte) shade, (byte) shade, (byte) shade);

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

                stars.Add(star);
            }
        }

        /// <summary>
        /// Process loop calculating each frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Loop(object sender, object e)
        {
            clock.Restart();
            halfWidth  = canvas.ActualWidth  / 2;
            halfHeight = canvas.ActualHeight / 2;

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

            var k  = kValue / star.z;
            var px = star.x * k + halfWidth;
            var py = star.y * k + halfHeight;

            if (star.z <= 0 || (px <=0 || px >= canvas.ActualWidth) || (py <= 0 || py >= canvas.ActualHeight))
            {
                do
                {
                    star.x = r.Next(-2000, 2000) / 100.0;
                    star.y = r.Next(-2000, 2000) / 100.0;
                    star.z = r.Next(MAX_DEPTH /2, MAX_DEPTH);
                }
                while (Math.Abs(star.x) < 0.0 || Math.Abs(star.y) < 0.0);
                oldZ = star.z;
                k    = kValue / star.z;
                px   = star.x * k + halfWidth;
                py   = star.y * k + halfHeight;
            }

            var oldK  = kValue / oldZ;
            var oldPx = star.x * oldK + halfWidth;
            var oldPy = star.y * oldK + halfHeight;
            
            var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
            var color = Color.FromArgb(255, (byte) shade, (byte) shade, (byte) shade);

            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                //star.tail.Stroke = new LinearGradientBrush(oldColor, color, new Point(1, 1), new Point(0, 0)); //TODO Make gradients that actually works
                star.tail.Stroke = new SolidColorBrush(color);
                star.tail.X1 = px;
                star.tail.Y1 = py;
                star.tail.X2 = oldPx;
                star.tail.Y2 = oldPy;

                Canvas.SetLeft(star.tail, 0);
                Canvas.SetTop(star.tail, 0);
            });
        }

        /// <summary>
        /// Speed changer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            kValue = e.NewValue;
        }
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
