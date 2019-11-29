﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Starfield
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        readonly      Random          r         = new Random();
        private const int             MAX_DEPTH = 32;
        private const int             STAR_NBR  = 200;
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

        /// <summary>
        /// Create the stars 
        /// </summary>
        private void initStars()
        {
            var halfWidth  = canvas.ActualWidth  / 2;
            var halfHeight = canvas.ActualHeight / 2;

            for( var i = 0; i < STAR_NBR; i++ )
            {
                var star = new Star(X: r.Next(-20, 20),
                                    Y: r.Next(-20, 20),
                                    Z: r.Next(MAX_DEPTH/2,   MAX_DEPTH));

                var k  = kValue / star.z;
                var px = star.x * k + halfWidth;
                var py = star.y * k + halfHeight;

                var shade = Convert.ToInt32((1 - star.z / MAX_DEPTH) * 255);
                px = star.x * k + halfWidth;
                py = star.y * k + halfHeight;
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
            var halfWidth  = canvas.ActualWidth  / 2;
            var halfHeight = canvas.ActualHeight / 2;

            foreach (var star in stars) 
            { DrawStar(star, halfWidth, halfHeight); }
            
            framesExecuted++;
            clock.Stop();
            Debug.WriteLine($"Frame took {clock.ElapsedMilliseconds.ToString().PadLeft(5)}ms to execute ({clock.ElapsedTicks.ToString().PadLeft(10)}ticks)");
        }

        /// <summary>
        /// Draws a star on the 2d canvas
        /// </summary>
        /// <param name="star">Star to draw</param>
        /// <param name="halfWidth">Current half width of the canvas</param>
        /// <param name="halfHeight">Current half heigth of the canvas</param>
        private void DrawStar(Star star, double halfWidth, double halfHeight)
        {
            //star.z -= 0.2;
            var oldZ = star.z;
            star.z -= (512 - kValue) / 100;

            var k  = kValue / star.z;
            var px = star.x * k + halfWidth;
            var py = star.y * k + halfHeight;

            if (star.z <= 0 || (px <=0 || px >= canvas.ActualWidth) || (py <= 0 || py >= canvas.ActualHeight))
            {
                do
                {
                    star.x = r.Next(-20, 20);
                    star.y = r.Next(-20, 20);
                    star.z = r.Next(MAX_DEPTH/2,   MAX_DEPTH);
                }
                while (star.x == 0 || star.y == 0);
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
;
            (Dispatcher ?? throw new InvalidOperationException()).BeginInvoke(() =>
            {
                //star.tail.Stroke = new LinearGradientBrush(oldColor, color, new Point(1, 1), new Point(0, 0)); //TODO Make gradients that actually works
                star.tail.Stroke = new SolidColorBrush(color);
                star.tail.X1     = px;
                star.tail.Y1     = py;
                star.tail.X2     = oldPx;
                star.tail.Y2     = oldPy;

                Canvas.SetLeft(star.tail, 0);
                Canvas.SetTop(star.tail, 0);
            },DispatcherPriority.Send);
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
