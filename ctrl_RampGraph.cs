using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Free_Gamma
{

    public class ctrl_RampGraph : Canvas
    {

        public event PointChangedEventHandler PointChanged;

        public delegate void PointChangedEventHandler(object sender, EventArgs e);

        public static int DesiredPointsCount = 8;

        private List<Ellipse> iPoints = new List<Ellipse>();
        private List<Line> Lines = new List<Line>();

        private int PointWidth = 9;
        private int PointRadius;
        private object CurrentPoint;
        private int CurrentPointIndex = -1;
        private Point CurrentLocation;




        public ctrl_RampGraph()
        {
            PointRadius = (int) ((PointWidth - 1) / 2d);
            MouseMove += ctrl_RampGraph_MouseMove;
            PreviewKeyDown += ctrl_RampGraph_KeyDown;
            PreviewMouseDown += ctrl_RampGraph_MouseDown;
            Focusable = true;

            Width = 256d;
            Height = 256d;
            Background = Brushes.White;

            var L0 = new Line() { Stroke = Brushes.LightGray, X1 = 255d, Y1 = 0d, X2 = 0d, Y2 = 255d };
            Children.Add(L0);

            int dX = (int) Math.Round(Math.Round(256d / DesiredPointsCount));

            for (int i = 1; i <= DesiredPointsCount; i++) {
                var L = new Line() { StrokeThickness = 1d, Stroke = Brushes.Blue };
                Lines.Add(L);
                Children.Add(L);
            }

            var points = GetDefaultPoints();
            for (int i = 0; i < points.Count; i++) {
                var P = new Ellipse() { Width = PointWidth, Height = PointWidth, StrokeThickness = 2d, Stroke = Brushes.Blue, Fill = Brushes.White };
                iPoints.Add(P);
                Children.Add(P);

                int y = (int) Math.Round(256d - points[i].X);
                if (i == DesiredPointsCount)
                    y = 0;

                SetLeft(P, points[i].X - PointRadius);
                SetTop(P, y - PointRadius);
                P.Tag = i;
                P.MouseDown += iPoint_MouseDown;

                DrawLine(i);
            }
        }





        public static List<Point> GetDefaultPoints()
        {
            var points = new List<Point>();
            if (DesiredPointsCount == 0)
                return null;
            int dX = (int) Math.Round(Math.Round(256d / DesiredPointsCount));
            for (int i = 1; i <= DesiredPointsCount; i++) {
                int x = i * dX;
                if (i == DesiredPointsCount)
                    x = 256;
                points.Add(new Point(x, x));
            }
            return points;
        }








        private void ctrl_RampGraph_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(CurrentPoint is null) & Mouse.LeftButton == MouseButtonState.Pressed) {
                double x = Mouse.GetPosition(this).X;
                double y = Mouse.GetPosition(this).Y;

                int d = 1;

                if (CurrentPointIndex == DesiredPointsCount - 1) {
                    x = 256d;
                    if (y < 0d)
                        y = 0d;
                    if (y > GetTop(iPoints[CurrentPointIndex - 1]))
                        y = GetTop(iPoints[CurrentPointIndex - 1]);
                }
                else if (CurrentPointIndex == 0) {
                    if (x < d)
                        x = d;
                    if (y > 256d)
                        y = 256d;
                    if (x > get_Point(1).X - d)
                        x = get_Point(1).X - d;

                    if (y < GetTop(iPoints[1]) + d) y = GetTop(iPoints[1]) + d;
                }
                else {
                    int i = CurrentPointIndex - 1;
                    int k = CurrentPointIndex + 1;

                    if (x < GetLeft(iPoints[i]) + 2 * PointRadius + d)
                        x = GetLeft(iPoints[i]) + 2 * PointRadius + d;
                    if (x > GetLeft(iPoints[k]) - PointRadius - d)
                        x = GetLeft(iPoints[k]) - PointRadius - d;

                    if (y > GetTop(iPoints[i]) - d)
                        y = GetTop(iPoints[i]) - d;
                    if (y < GetTop(iPoints[k]) + PointRadius + d)
                        y = GetTop(iPoints[k]) + PointRadius + d;
                }

                SetLeft((UIElement) CurrentPoint, x - PointRadius);
                SetTop((UIElement) CurrentPoint, y - PointRadius);
                DrawLine(CurrentPointIndex);
                if (CurrentPointIndex != DesiredPointsCount - 1)
                    DrawLine(CurrentPointIndex + 1);
                PointChanged?.Invoke(this, new EventArgs());
            }
        }




        private void ctrl_RampGraph_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(CurrentPoint is null)) {

                int dx = 0;
                int dy = 0;
                if (e.Key == Key.Right) {
                    dx = 1;
                }
                else if (e.Key == Key.Left) {
                    dx = -1;
                }
                else if (e.Key == Key.Up) {
                    dy = -1;
                }
                else if (e.Key == Key.Down) {
                    dy = 1;
                }
                else {
                    return;
                }

                double x = GetLeft((UIElement) CurrentPoint) + dx;
                double y = GetTop((UIElement) CurrentPoint) + dy;

                if (CurrentPointIndex == DesiredPointsCount - 1) {
                    x = 256 - PointRadius;
                    if (y < 0 - PointRadius) y = 0 - PointRadius;

                    if (y > GetTop(iPoints[CurrentPointIndex - 1])) y = GetTop(iPoints[CurrentPointIndex - 1]);
                }
                else if (CurrentPointIndex == 0) {
                    if (x < 1 - PointRadius)
                        x = 1 - PointRadius;
                    if (x > get_Point(1).X - 1d - PointRadius)  x = get_Point(1).X - 1d - PointRadius;
                    if (y > 256 - PointRadius) y = 256 - PointRadius;
                    if (y < GetTop(iPoints[1]) + 1d) y = GetTop(iPoints[1]) + 1d;
                }
                else {
                    int i = CurrentPointIndex - 1;
                    int k = CurrentPointIndex + 1;

                    if (x < GetLeft(iPoints[i]) + PointRadius + 1d)  x = GetLeft(iPoints[i]) + PointRadius + 1d;
                    if (x > GetLeft(iPoints[k]) - 2 * PointRadius - 1d) x = GetLeft(iPoints[k]) - 2 * PointRadius - 1d;

                    if (y > GetTop(iPoints[i]) - PointRadius - 1d)  y = GetTop(iPoints[i]) - PointRadius - 1d;
                    if (y < GetTop(iPoints[k]) + 1d) y = GetTop(iPoints[k]) + 1d;
                }


                SetLeft((UIElement) CurrentPoint, x);
                SetTop((UIElement) CurrentPoint, y);
                for (int i = 0; i < Lines.Count; i++)
                    DrawLine(i);

                PointChanged?.Invoke(this, new EventArgs());

                Keyboard.Focus(this);
                e.Handled = true;
            }
        }









        private void DrawLine(int Index)
        {
            float x1, y1;
            if (Index == 0) {
                x1 = 0f;
                y1 = 256f;
            }
            else {
                x1 = (float) (GetLeft(iPoints[Index - 1]) + PointRadius);
                y1 = (float) (GetTop(iPoints[Index - 1]) + PointRadius);
            }
            Lines[Index].X1 = (double) x1;
            Lines[Index].Y1 = (double) y1;
            Lines[Index].X2 = GetLeft(iPoints[Index]) + PointRadius;
            Lines[Index].Y2 = GetTop(iPoints[Index]) + PointRadius;
        }






        public Point get_Point(int Index)
        {
            return new Point(GetLeft(iPoints[Index]) + PointRadius, 256d - GetTop(iPoints[Index]) - PointRadius);
        }

        public void set_Point(int Index, Point value)
        {
            SetLeft(iPoints[Index], value.X - PointRadius);
            SetTop(iPoints[Index], 256d - value.Y - PointRadius);
            DrawLine(Index);
            if (Index != DesiredPointsCount - 1)
                DrawLine(Index + 1);
        }


        public List<Point> Points
        {
            get {
                var p = new List<Point>();
                for (int i = 0; i < PointsCount; i++) {
                    var pp = get_Point(i);
                    p.Add(new Point(pp.X, pp.Y));
                }
                return p;
            }
        }



        public int PointsCount
        {
            get {
                return iPoints.Count;
            }
        }



        // llllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll




        private void iPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentPoint = sender;
            CurrentPointIndex = (int) ((Shape) sender).Tag;
            VisuallyUnSelectAllPoints();
            ((Shape) sender).Fill = Brushes.Blue;
            Keyboard.Focus(this);
        }



        private void iPoint_MouseDown2(object sender, MouseButtonEventArgs e)
        {

        }


        private void VisuallyUnSelectAllPoints()
        {
            for (int i = 0; i < iPoints.Count; i++)
                iPoints[i].Fill = Brushes.White;
        }


        private void ctrl_RampGraph_MouseDown(object sender, MouseButtonEventArgs e)
        {
            VisuallyUnSelectAllPoints();
            CurrentPoint = null;
        }





        public int get_y(int x)
        {
            float s, d, v;
            for (int k = 0; k < PointsCount; k++) {
                if (k == 0) {
                    if (x <= get_Point(0).X) {
                        s = (float) (get_Point(0).Y / get_Point(0).X);
                        d = 0f;
                        v = 256 * x * s;
                        if (v > ushort.MaxValue)
                            v = ushort.MaxValue;

                        return (int) Math.Round(v);
                        break;
                    }
                }
                else if (x <= get_Point(k).X) {
                    s = (float) ((get_Point(k).Y - get_Point(k - 1).Y) / (get_Point(k).X - get_Point(k - 1).X));
                    d = (float) (get_Point(k - 1).Y - (double) s * get_Point(k - 1).X);
                    v = 255f * (x * s + d);
                    if (v > ushort.MaxValue)
                        v = ushort.MaxValue;

                    return (int) Math.Round(v);
                    break;
                }
            }
            return -1;
        }




        public int get_yy(List<Point> Points, int x)
        {
            float s, d, v;
            for (int k = 0; k < PointsCount; k++) {
                if (k == 0) {
                    if (x <= Points[0].X) {
                        s = (float) (Points[0].Y / Points[0].X);
                        d = 0f;
                        v = 256 * x * s;
                        if (v > ushort.MaxValue)
                            v = ushort.MaxValue;

                        return (int) Math.Round(v);
                        break;
                    }
                }
                else if (x <= Points[k].X) {
                    s = (float) ((Points[k].Y - Points[k - 1].Y) / (Points[k].X - Points[k - 1].X));
                    d = (float) (Points[k - 1].Y - (double) s * Points[k - 1].X);
                    v = 255f * (x * s + d);
                    if (v > ushort.MaxValue)
                        v = ushort.MaxValue;

                    return (int) Math.Round(v);
                    break;
                }
            }
            return -1;
        }








    }
}