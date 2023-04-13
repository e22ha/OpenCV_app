using System;
using System.Windows;

namespace OpenCV_photoapp;

public partial class WindowWithSlider : Window
{
    public double[] Value { get; }

    public event EventHandler<RoutedPropertyChangedEventArgs<double>> ValueChanged = null!;

    public WindowWithSlider(string name, double min, double max, double xVal, double yVal)
    {
        InitializeComponent();
        
        Title = name;
        // добавляем обработчик события Loaded для установки значений слайдеров
        Loaded += (sender, args) =>
        {
            xSlider.Slider.Minimum = min;
            xSlider.Slider.Value = xVal;
            xSlider.Slider.Maximum = max;
            xSlider.ValueChanged += Slider1_ValueChanged;

            ySlider.Slider.Minimum = min;
            ySlider.Slider.Value = yVal;
            ySlider.Slider.Maximum = max;
            ySlider.ValueChanged += Slider2_ValueChanged;
        };

        Value = new[] { xVal, yVal };
    }

    public WindowWithSlider(string name, int min, int max, int Val)
    {
        InitializeComponent();
        
        Title = name;
        // добавляем обработчик события Loaded для установки значений слайдеров
        Loaded += (_, _) =>
        {
            xSlider.Slider.Minimum = min;
            xSlider.Slider.Value = Val;
            xSlider.Slider.Maximum = max;
            xSlider.ValueChanged += Slider1_ValueChanged;

            ySlider.Visibility = Visibility.Collapsed;
            
        };

        Value = new [] { Val, 0.0 };    
    }


    private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Value[0] = (double)e.NewValue;
        ValueChanged?.Invoke(this, e);
    }

    private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Value[1] = (double)e.NewValue;
        ValueChanged?.Invoke(this, e);
    }
}