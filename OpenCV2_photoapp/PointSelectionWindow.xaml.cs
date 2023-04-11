using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using Brushes = System.Drawing.Brushes;
using Point = System.Windows.Point;


namespace OpenCV2_photoapp;

public partial class PointSelectionWindow : Window
{
    private readonly Image<Bgr, byte> _image;
    private List<PointF> _sourcePoints;

    public PointSelectionWindow(Image<Bgr, byte> image)
    {
        InitializeComponent();

        _image = image;
        canvas.Background = new ImageBrush(Filter.BitmapSourceFromHBitmap(_image.ToBitmap().ToMat()));

        Width = _image.Width;
        Height = _image.Height;

        DrawInitialQuadrilateral();
    }

    private PointF _draggingPoint = PointF.Empty; // точка, которую перемещаем

    private bool _isDragging = false; // переменная для определения, идет ли перемещение точки

// обработчик события нажатия кнопки мыши на канвасе
    private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // определяем, какая точка была нажата
        foreach (var point in _sourcePoints)
        {
            if (Math.Abs(point.X - e.GetPosition(canvas).X) <= 50 && Math.Abs(point.Y - e.GetPosition(canvas).Y) <= 50)
            {
                _isDragging = true; // начинаем перемещение этой точки
                _draggingPoint = point;
                break;
            }
        }
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        // перемещаем точку, если есть активная точка перемещения
        if (!_isDragging) return;
        var index = _sourcePoints.IndexOf(_draggingPoint);
        _sourcePoints[index] = new PointF((float)e.GetPosition(canvas).X, (float)e.GetPosition(canvas).Y);
        _draggingPoint = _sourcePoints[index];

        // перерисовываем четырехугольник
        DrawQuadrilateral();
    }

// обработчик события отпускания кнопки мыши на канвасе
    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        // завершаем перемещение точки
        _isDragging = false;
        _draggingPoint = PointF.Empty;
    }

    private void DrawInitialQuadrilateral()
    {
        var imageWidth = _image.Width;
        var imageHeight = _image.Height;

        // Создаем список точек и добавляем в него координаты точек четырехугольника
        _sourcePoints = new List<PointF>
        {
            new(0, 0),
            new(0, imageHeight),
            new(imageWidth, imageHeight),
            new(imageWidth, 0)
        };

        DrawQuadrilateral();
    }

    private void DrawQuadrilateral()
    {
        var image = _image.ToBitmap();
        var graphics = Graphics.FromImage(image);
        // Отрисовываем точки
        foreach (var point in _sourcePoints)
        {
            graphics.FillEllipse(Brushes.CornflowerBlue, point.X - 10, point.Y - 10, 20, 20);
        }

        // Отрисовываем линии между точками
        graphics.DrawLine(Pens.DarkBlue, _sourcePoints[0], _sourcePoints[1]);
        graphics.DrawLine(Pens.DarkBlue, _sourcePoints[1], _sourcePoints[2]);
        graphics.DrawLine(Pens.DarkBlue, _sourcePoints[2], _sourcePoints[3]);
        graphics.DrawLine(Pens.DarkBlue, _sourcePoints[3], _sourcePoints[0]);

        canvas.Background = new ImageBrush(Filter.BitmapSourceFromHBitmap(image.ToMat()));
    }


    private static PointF PointToPointF(Point point)
    {
        return new PointF((float)point.X, (float)point.Y);
    }

    private static Point PointFToPoint(PointF point)
    {
        return new Point((int)point.X, (int)point.Y);
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }


    public PointF[]? GetSourcePoints()
    {
        return _sourcePoints.Count == 4 ? _sourcePoints.ToArray() : null;
    }
}