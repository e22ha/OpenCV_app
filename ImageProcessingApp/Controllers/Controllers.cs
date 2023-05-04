using System;
using ImageProcessingApp.Interfaces;

namespace ImageProcessingApp.Controllers;

public class ImageProcessingController
{
    private readonly IImageProcessingModel _model;
    private readonly IImageProcessingView _view;

    public ImageProcessingController(IImageProcessingModel model, IImageProcessingView view)
    {
        _model = model;
        _view = view;
    }

    public void LoadImage(string imagePath)
    {
        try
        {
            _model.LoadImage(imagePath);
            _view.SetImage(_model.Image);
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage(ex.Message);
        }
    }

    public void DetectFaces()
    {
        try
        {
            var faces = _model.DetectFaces();
            _view.DisplayFaces(faces);
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage(ex.Message);
        }
    }

    public void RecognizeText()
    {
        try
        {
            var text = _model.RecognizeText();
            _view.DisplayText(text);
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage(ex.Message);
        }
    }
}

public class VideoProcessingController
{
    private readonly IVideoProcessingModel _model;
    private readonly IVideoProcessingView _view;

    public VideoProcessingController(IVideoProcessingModel model, IVideoProcessingView view)
    {
        _model = model;
        _view = view;
    }

    public void LoadVideo(string videoPath)
    {
        try
        {
            _model.LoadVideo(videoPath);
            _view.DisplayVideo(_model.Video);
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage(ex.Message);
        }
    }

    public void DetectFaces()
    {
        try
        {
            var faces = _model.DetectFaces();
            _view.DisplayFaces(faces);
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage(ex.Message);
        }
    }

    public void RecognizeText()
    {
        try
        {
            var text = _model.RecognizeText();
            _view.DisplayText(text);
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage(ex.Message);
        }
    }
}