﻿using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace OpenCV1_lern;

public class Filter
{
    public static ImageSource BitmapSourceFromHBitmap(Mat result)
    {
        var bitmap = result.ToBitmap();
        var bitmapSourceFromHBitmap = Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            nint.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        return bitmapSourceFromHBitmap;
    }
    

    
    public static int t1 { get; set; }
    public static int t2 { get; set; }
    public static int t { get; set; }
    public static int v { get; set; }
    
}