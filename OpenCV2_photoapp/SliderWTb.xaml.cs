using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace OpenCV2_photoapp
{
    public partial class SliderWTb : UserControl
    {
        public static readonly DependencyProperty SliderValueProperty = DependencyProperty.Register(nameof(SliderValue), typeof(int), typeof(SliderWTb), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(SliderWTb), new PropertyMetadata(-100));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(SliderWTb), new PropertyMetadata(100));
        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(Step), typeof(int), typeof(SliderWTb), new PropertyMetadata(1));
        
        public delegate void SliderChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs);
        public event SliderChangedEventHandler ValueChanged;
        
        

        public SliderWTb()
        {
            InitializeComponent();
            Slider.ValueChanged += Slider_OnValueChanged;
        }

        public int SliderValue
        {
            get => (int)GetValue(SliderValueProperty);
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

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }
        

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ValueChanged?.Invoke(this, e);
        }
    }
}