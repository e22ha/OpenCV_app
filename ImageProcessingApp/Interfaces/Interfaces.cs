using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;

namespace ImageProcessingApp.Interfaces
{
    public interface IImageProcessingModel
    {
        Bitmap Image { get; }
        void LoadImage(string imagePath);
        IEnumerable<Rectangle> DetectFaces();
        string RecognizeText();
    }

    public interface IImageProcessingView
    {
        event EventHandler LoadImageClicked;
        event EventHandler DetectFacesClicked;
        event EventHandler RecognizeTextClicked;

        void SetImage(Bitmap image);
        void SetFaces(Rectangle[] faces);
        void SetText(string text);
        void ShowErrorMessage(string message);
        void DisplayErrorMessage(string exMessage);
        void DisplayFaces(IEnumerable<Rectangle> faces);
        void DisplayText(string text);
    }

    public interface IImageProcessingController
    {
        void Initialize();
        void LoadImage(string imagePath);
        void DetectFaces();
        void RecognizeText();
    }

    public interface IImageProcessingService
    {
        IImageProcessingModel GetImageProcessingModel();
    }
    
    public interface IVideoProcessingModel
    {
        void LoadVideo(string videoPath);
        IEnumerable<Rectangle> DetectFaces();
        Frame Video { get; }
        String RecognizeText();
    }
    
    public interface IVideoProcessingView
    {
        event EventHandler LoadVideoClicked;
        event EventHandler ProcessVideoClicked;

        void SetVideo(string videoPath);
        void SetProcessedVideo(string videoPath);
        void ShowErrorMessage(string message);
        void DisplayVideo(Frame frame);
        void DisplayErrorMessage(string exMessage);
        void DisplayFaces(IEnumerable<Rectangle> faces);
        void DisplayText(string text);
    }
}