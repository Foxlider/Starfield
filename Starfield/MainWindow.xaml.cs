using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Starfield
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly      Random          r         = new Random();
        private const int             MAX_DEPTH = 32;
        private const int             STAR_NBR  = 500;
        private       List<Star>      stars     = new List<Star>();
        readonly      DispatcherTimer timer     = new DispatcherTimer();
        readonly      DispatcherTimer fpsTimer  = new DispatcherTimer();
        readonly      DispatcherTimer fpsTimer2 = new DispatcherTimer();
        private       int             framesExecuted;
        private       int             frameCount;
        private double kValue = 128.0;

        private Stopwatch clock;
        private Stopwatch watch;

        public MainWindow()
        {
            InitializeComponent();
            initStars();
            timer.Interval =  new TimeSpan(0, 0, 0, 0, 1);
            timer.Tick     += Loop;
            timer.Start();
            fpsTimer.Tick     += FpsCounter;
            fpsTimer.Start();
            fpsTimer2.Interval =  new TimeSpan(0, 0, 0, 1);
            fpsTimer2.Tick += FpsCounter2;
            fpsTimer2.Start();
            clock = new Stopwatch();
            watch = new Stopwatch();
            watch.Start();
            ConsoleAllocator.ShowConsoleWindow();
        }

        private void FpsCounter(object sender, EventArgs e)
        {
            frameCount ++;
            if (watch.ElapsedMilliseconds > 1000)
            {
                watch.Restart();
                fpsCounter.Content = $"{frameCount.ToString().PadRight(5)} FPS (Render)";
                frameCount         = 0;
            }
        }

        private void FpsCounter2(object sender, EventArgs e)
        {
            fpsCounter2.Content = $"{framesExecuted.ToString().PadRight(5)} FPS (Processing)";
            framesExecuted      = 0;
        }
        private void initStars()
        {
            var halfWidth  = canvas.ActualWidth  / 2;
            var halfHeight = canvas.ActualHeight / 2;

            for( var i = 0; i < STAR_NBR; i++ )
            {
                var star = new Star(X: r.Next(-20, 20),
                                    Y: r.Next(-20, 20),
                                    Z: r.Next(1,   MAX_DEPTH));


                var k = kValue / star.z;
                var size = (int)(1 - star.z / MAX_DEPTH) * 15 + 1;
                var px = star.x * k + halfWidth;
                var py = star.y * k + halfHeight;

                var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
                px = star.x * k + halfWidth;
                py = star.y * k + halfHeight;
                var color = Color.FromArgb(255, (byte)shade, (byte)shade, (byte)shade);

                (Dispatcher ?? throw new InvalidOperationException()).BeginInvoke(() =>
                {
                    Rectangle rec = new Rectangle { Fill = new SolidColorBrush(color), Height = 1, Width = 1 };
                    star.pixel = rec;
                    Canvas.SetLeft(rec, px);
                    Canvas.SetTop(rec, py);
                    canvas.Children.Add(rec);
                }, DispatcherPriority.Send);

                stars.Add(star);
            }
        }

        private void Loop(object sender, EventArgs e)
        {
            clock.Restart();
            var halfWidth = canvas.ActualWidth / 2;
            var halfHeight = canvas.ActualHeight / 2;

            //canvas.Children.Clear();
            foreach (var star in stars) { DrawStar(star, halfWidth, halfHeight); }
            
            framesExecuted++;
            clock.Stop();
            Console.WriteLine($"Frame took {clock.ElapsedMilliseconds.ToString().PadLeft(5)}ms to execute ({clock.ElapsedTicks.ToString().PadLeft(10)}ticks)");
        }

        private void DrawStar(Star star, double halfWidth, double halfHeight)
        {
            //star.z -= 0.2;
            star.z -= (256- kValue) / 100;

            var k  = kValue / star.z;
            var px = star.x * k + halfWidth;
            var py = star.y * k + halfHeight;

            if (star.z <= 0 || (px <=0 || px >= canvas.ActualWidth) || (py <= 0 || py >= canvas.ActualHeight))
            {
                do
                {
                    star.x = r.Next(-20, 20);
                    star.y = r.Next(-20, 20);
                    star.z = r.Next(1,   MAX_DEPTH);
                }
                while (star.x == 0 || star.y == 0);
            }


            //if (!(px > 0) || !(px < canvas.ActualWidth) || !(py > 0) || !(py < canvas.ActualHeight)) return;

            var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
            var color = Color.FromArgb(255, (byte) shade, (byte) shade, (byte) shade);
;
            (Dispatcher ?? throw new InvalidOperationException()).BeginInvoke(() =>
            {
                star.pixel.Fill = new SolidColorBrush(color);
                Canvas.SetLeft(star.pixel, px);
                Canvas.SetTop(star.pixel, py);
            },DispatcherPriority.Send);
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            kValue = e.NewValue;
        }
    }

    /// <summary>
    /// Star class
    /// </summary>
    internal class Star
    {
        internal double x;
        internal double y;
        internal double z;

        internal Rectangle pixel;

        public Star(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }
}
