using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers;
using System.Globalization;
using ABB.Robotics.Controllers.RapidDomain;


namespace Painting
{
    public partial class MainWindow : Window
    {
        DrawingVisual visual;
        DrawingContext dc;
        double width, height;
        ABBRobot Robot;
        NetworkScanner Netscaner { get; set; }
        ControllerInfoCollection Controllers { get; set; }
        Tool tool;
        Point oldPos;
        bool isCartCathed;


        public MainWindow()
        {
            InitializeComponent();
            
            Robot = new ABBRobot();

            width = g.Width;
            height = g.Height;

            tool = new Tool(width/2, height/2);

            visual = new DrawingVisual();

            NetScan();

            Drawing();

            oldPos = new Point(0, 0);
        }

        private void NetScan()
        {
            Netscaner = new NetworkScanner();
            Netscaner.Scan();
            Controllers = Netscaner.Controllers;

            foreach (ControllerInfo c in Controllers)
            {
                cbox_Controllers.Items.Add(c);
            }
        }

        public void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                // Draw Axis
                DrawingAxis(dc);

                // Draw Cart
                tool.Draw(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }
        private void DrawingAxis(DrawingContext dc)
        {
            // axis X
            dc.DrawLine(new Pen(Brushes.Black, 1), new Point(10, 10), new Point(30, 10));

            // axis Y
            dc.DrawLine(new Pen(Brushes.Black, 1), new Point(10, 10), new Point(10, 30));

            // X ortha text
            FormattedText formattedText = new FormattedText("X", CultureInfo.GetCultureInfo("en-us"),
                                                FlowDirection.LeftToRight, new Typeface("Verdana"), 10, Brushes.Black,
                                                VisualTreeHelper.GetDpi(visual).PixelsPerDip);
            Point textPos = new Point(32, 5);
            dc.DrawText(formattedText, textPos);

            // Y ortha text
            formattedText = new FormattedText("Y", CultureInfo.GetCultureInfo("en-us"),
                                                FlowDirection.LeftToRight, new Typeface("Verdana"), 10, Brushes.Black,
                                                VisualTreeHelper.GetDpi(visual).PixelsPerDip);
            textPos = new Point(7, 30);
            dc.DrawText(formattedText, textPos);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) => Robot.StartExec();
        private void btnStop_Click(object sender, RoutedEventArgs e) => Robot.StopExec();

        private void g_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => isCartCathed = false;

        private void g_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Robot.SelectedController is null) return;

            if (Robot.SelectedController.Rapid.ExecutionStatus != ExecutionStatus.Running) return;

            var pos = e.GetPosition(g);
            if (pos.X > tool.pos.X && pos.X < tool.pos.X + tool.width &&
                pos.Y > tool.pos.Y && pos.Y < tool.pos.Y + tool.height)
            {
                oldPos = pos;
                isCartCathed = true;
            }
        }

        private void g_MouseMove(object sender, MouseEventArgs e)
        {
            if (Robot.SelectedController is null) return;
            
            if (Robot.SelectedController.Rapid.ExecutionStatus != ExecutionStatus.Running) return;

            if (isCartCathed)
            {
                var newPos = e.GetPosition(g);
                var distX = newPos.X - oldPos.X;
                var distY = newPos.Y - oldPos.Y;

                oldPos = newPos;

                tool.vector.X = Robot.vector.X = distX;
                tool.vector.Y = Robot.vector.Y = distY;

                tool.Move();
                Robot.Move();

                Drawing();
            }
        }

        private void cbox_Controllers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboBoxControllers = sender as ComboBox;
                Robot.Connect((ControllerInfo)comboBoxControllers?.SelectedItem);
                lbSystem.Content = Robot.SelectedController.SystemName;

                btnStart.IsEnabled = true;
                btnStop.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
