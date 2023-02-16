using System.Windows;
using System.Windows.Controls;

namespace OpenCV2_photoapp;

public partial class FilterControl : UserControl
{
    public FilterControl()
    {
        InitializeComponent();
        this.DataContext = this;

    }

    public string Title { get; set; }
}