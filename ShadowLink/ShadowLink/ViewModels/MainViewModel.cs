using System;
using System.Collections.Generic;
using ShadowLink.Services;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Bitmap = System.Drawing.Bitmap;


namespace ShadowLink.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private ImageCipher _imageCipher;
    private string _message;
    private string _seed;
    private Bitmap _image;
    private Avalonia.Media.Imaging.Bitmap _previewImage;
    public ICommand UploadImageCommand { get; }
    public ICommand EmbedTextCommand { get; }
    public ICommand ExtractTextCommand { get; }
    
    public MainViewModel()
    {
        _imageCipher = new ImageCipher();
        UploadImageCommand = ReactiveCommand.CreateFromTask(UploadImage);
        EmbedTextCommand = ReactiveCommand.Create(EmbedText);
        ExtractTextCommand = ReactiveCommand.Create(ExtractText);
    }

    public string Message
    {
        get { return _message; }
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    public string Seed
    {
        get { return _seed; }
        set
        {
            _seed = value;
            OnPropertyChanged();
        }
    }

    public Bitmap Image
    {
        get { return _image; }
        set
        {
            _image = value;
            OnPropertyChanged();
        }
    }

    public Avalonia.Media.Imaging.Bitmap PreviewImage
    {
        get { return _previewImage; }
        set
        {
            _previewImage = value;
            OnPropertyChanged();
        }
    }

    // TODO: Find a way to store Avalonia bitmap and System.Drawing bitmap in order to both operate and display the selected image. 

    public async Task UploadImage()
    {
        var dialog = new OpenFileDialog
        {
            AllowMultiple = false,
            Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "Images", Extensions = new List<string> { "png", "jpg", "jpeg", "bmp" } } }
        };
        
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var result = await dialog.ShowAsync(desktop.MainWindow);

            if (result != null && result.Length > 0)
            {
                using (var fs = new FileStream(result[0], FileMode.Open))
                {
                    var bitmap = new Bitmap(fs);
                    Image = bitmap;
                }
                
                using (var previewStream = new MemoryStream())
                {
                    Image.Save(previewStream, ImageFormat.Png);
                    previewStream.Seek(0, SeekOrigin.Begin);

                    PreviewImage = ConvertBitmapToAvalonia(new Bitmap(previewStream));
                }
            }
        }
        else
        {
            throw new Exception("Application is not a desktop application.");
        }
    }
    
    private Avalonia.Media.Imaging.Bitmap ConvertBitmapToAvalonia(Bitmap bitmap)
    {
        Console.WriteLine("POTATO");
        return new Avalonia.Media.Imaging.Bitmap(Stream.Null);
        // return (Avalonia.Media.Imaging.Bitmap)new ImageConverter().ConvertTo(bitmap, typeof(Avalonia.Media.Imaging.Bitmap));
    }
    
    public void EmbedText()
    {
        // Image = _imageCipher.EmbedText(Message, Seed, Image);
        Console.WriteLine("EmbedText");
    }

    public void ExtractText()
    {
        // Message = _imageCipher.ExtractText(Seed, Image);
        Console.WriteLine("ExtractText");
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}