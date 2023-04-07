using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace OpenCV2_photoapp
{
    public partial class SliderWTb : UserControl
    {
        public delegate void SliderChangedEventHandler(object sender,
            RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs);

        public event SliderChangedEventHandler ValueChanged = null!;


        public SliderWTb()
        {
            InitializeComponent();
            Slider.ValueChanged += Slider_OnValueChanged;
        }


        public static readonly DependencyProperty SliderValueProperty = DependencyProperty.Register(nameof(SliderValue),
            typeof(double), typeof(SliderWTb), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum),
            typeof(int), typeof(SliderWTb), new PropertyMetadata(-10));

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum),
            typeof(int), typeof(SliderWTb), new PropertyMetadata(10));

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(double), typeof(SliderWTb), new PropertyMetadata(0.1));

        public double SliderValue
        {
            get => (double)GetValue(SliderValueProperty);
            set => SetValue(SliderValueProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }


        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ValueChanged?.Invoke(this, e);
        }
    }
}