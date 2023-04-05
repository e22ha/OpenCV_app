using System;
using System.Windows;

namespace OpenCV2_photoapp;

public partial class WindowWithSlider : Window
{
    public int[] Value = { 1, 1 };

    public event EventHandler<RoutedPropertyChangedEventArgs<double>> ValueChanged = null!;

    public WindowWithSlider(string name)
    {
        InitializeComponent();
        Title = name;
        
        kSlider.Slider.Value = Value[0];
        kSlider.ValueChanged += Slider1_ValueChanged;

        jSlider.Slider.Value = Value[1];
        jSlider.ValueChanged += Slider2_ValueChanged;
    }

    private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Value[0] = (int)e.NewValue;
        ValueChanged?.Invoke(this, e);
    }

    private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Value[1] = (int)e.NewValue;
        ValueChanged?.Invoke(this, e);
    }
}