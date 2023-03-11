using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OpenCV2_photoapp;

public partial class FilterControl : UserControl
{
    public delegate void SwitchChangedEventHandler(object sender, bool e);
    public event SwitchChangedEventHandler SwitchChanged = null!;
    public FilterControl()
    {
        InitializeComponent();
        DataContext = this;
        Switch.Checked += Switch_Ch;
        Switch.Unchecked += Switch_Ch;
         
    }
    
    private void Switch_Ch(object sender, RoutedEventArgs e)
    {
        SwitchChanged?.Invoke(this, Switch.IsChecked ?? false);
    }
    

    private static UIElement FindElementByName(DependencyObject? parent, string name)
    {
        switch (parent)
        {
            case null:
                return null;
            case FrameworkElement element when element.Name == name:
                return element;
        }

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = FindElementByName(child, name);
            if (result != null) return result;
        }

        return null;
    }

    private static T? GetParentOfType<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        return parent switch
        {
            null => null,
            T parentOfType => parentOfType,
            _ => GetParentOfType<T>(parent)
        };
    }


    private void RunAnim(int type)
    {
        var element = FindElementByName(GetParentOfType<Window>(this), TargetElementName);
        if (element == null) return;

        var fromSeconds = TimeSpan.FromSeconds(0.25);
        var calcHeight = 25*Convert.ToInt32(CountRow);
        var HideGrid = new DoubleAnimation
        {
            From = calcHeight,
            To = 0,
            Duration = new Duration(fromSeconds)
        };  
        var ShowGrid = new DoubleAnimation
        {
            From = 0,
            To = calcHeight,
            Duration = new Duration(fromSeconds)
        };

        var myStoryboard = new Storyboard();
        switch (type)
        {
            case 0:
                myStoryboard.Children.Add(ShowGrid);
                Storyboard.SetTarget(ShowGrid, element);
                Storyboard.SetTargetProperty(ShowGrid, new PropertyPath(HeightProperty));
                break;
            case 1:
                myStoryboard.Children.Add(HideGrid);
                Storyboard.SetTarget(HideGrid, element);
                Storyboard.SetTargetProperty(HideGrid, new PropertyPath(HeightProperty));
                break;
        }
        myStoryboard.Begin();
    }


    public static readonly DependencyProperty TargetElementNameProperty = DependencyProperty.Register(
        nameof(TargetElementName), typeof(string), typeof(FilterControl), new PropertyMetadata(null));

    public static readonly DependencyProperty CountRowProperty = DependencyProperty.Register(
        nameof(CountRow), typeof(string), typeof(FilterControl), new PropertyMetadata(null));

    public string TargetElementName
    {
        get => (string)GetValue(TargetElementNameProperty);
        set => SetValue(TargetElementNameProperty, value);
    }


    public string Title { get; set; } = null!;

    public string CountRow
    {
        get => (string)GetValue(CountRowProperty);
        set => SetValue(CountRowProperty, value);
    }

    private void Switch_On(object sender, RoutedEventArgs e)
    {
        RunAnim(0);
        
    }

    private void Switch_Off(object sender, RoutedEventArgs e)
    {
        RunAnim(1);
    }
}