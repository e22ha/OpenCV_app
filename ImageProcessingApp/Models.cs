using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using ImageProcessingApp.Interfaces;
using Tesseract;

namespace ImageProcessingApp.Models;

public class FaceModel
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
    
public class TextModel
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Text { get; set; }
}
    
public class ImageModel
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; }
    public List<FaceModel> DetectedFaces { get; set; }
    public List<TextModel> DetectedTexts { get; set; }
}
    
public class VideoModel
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; }
    public TimeSpan Duration { get; set; }
    public int FPS { get; set; }
    public List<FaceModel> DetectedFaces { get; set; }
    public List<TextModel> DetectedTexts { get; set; }
}
    
public class ImageProcessingModel : IImageProcessingModel
{
    private Image<Bgr, byte> _image;

    public Bitmap Image { get; }

    public void LoadImage(string imagePath)
    {
        _image = new Image<Bgr, byte>(imagePath);
    }

    IEnumerable<Rectangle> IImageProcessingModel.DetectFaces()
    {
        return DetectFaces();
    }

    public string RecognizeText()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Rectangle> DetectFaces()
    {
        using var cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
        var grayImage = _image.Convert<Gray, byte>();
        var faces = cascadeClassifier.DetectMultiScale(grayImage, 1.2, 3);
        return faces;
    }
        
}
    
public class VideoProcessingModel : IVideoProcessingModel
{
    private VideoCapture _videoCapture;
    private Frame _video;

    public void LoadVideo(string videoPath)
    {
        _videoCapture = new VideoCapture(videoPath);
    }

    public IEnumerable<Rectangle> DetectFaces()
    {
        if (_videoCapture == null)
        {
            throw new InvalidOperationException("No video loaded.");
        }

        var faces = new List<Rectangle>();
        using (var faceDetection = new CascadeClassifier("haarcascade_frontalface_default.xml"))
        {
            while (true)
            {
                using (var frame = _videoCapture.QueryFrame())
                {
                    if (frame == null)
                    {
                        break;
                    }

                    using (var imageFrame = frame.ToImage<Bgr, byte>())
                    {
                        var grayFrame = imageFrame.Convert<Gray, byte>();
                        var detectedFaces = faceDetection.DetectMultiScale(grayFrame, 1.1, 4);
                        foreach (var face in detectedFaces)
                        {
                            faces.Add(face);
                        }
                    }
                }
            }
        }
        return faces;
    }

    public Frame Video 
    { 
        get 
        {
            if (_video == null)
            {
                throw new InvalidOperationException("No video loaded.");
            }
            return _video; 
        } 
        set 
        {
            _video = value; 
        } 
    }

    public string RecognizeText()
    {
        var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
        using (var page = engine.Process(_video, PageSegMode.Auto))
        {
            return page.GetText();
        }
    }
}
